using System.IO;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Maintains the IDE readonly field registry provider and local load status.
/// </summary>
public sealed class FieldRegistryRuntimeService
{
    private readonly LocalFieldRegistryLoader _loader;
    private readonly BuiltInFieldRegistryPackLoader _builtInLoader;
    private readonly string? _globalActiveDirectoryOverride;
    private readonly FieldRegistryProvenanceSnapshotBuilder _provenanceSnapshotBuilder = new();
    private readonly object _providerPublishGate = new();
    private Ra2FieldRegistryProviderSnapshot _currentProviderSnapshot;

    public FieldRegistryRuntimeService()
        : this(new LocalFieldRegistryLoader(), new BuiltInFieldRegistryPackLoader(), null)
    {
    }

    public FieldRegistryRuntimeService(LocalFieldRegistryLoader loader, string? globalActiveDirectoryOverride = null)
        : this(loader, new BuiltInFieldRegistryPackLoader(), globalActiveDirectoryOverride)
    {
    }

    internal FieldRegistryRuntimeService(
        LocalFieldRegistryLoader loader,
        BuiltInFieldRegistryPackLoader builtInLoader,
        string? globalActiveDirectoryOverride = null)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _builtInLoader = builtInLoader ?? throw new ArgumentNullException(nameof(builtInLoader));
        _globalActiveDirectoryOverride = globalActiveDirectoryOverride;
        CurrentState = FieldRegistryRuntimeState.Empty(GetGlobalActiveDirectoryPath(), null);
        IRa2FieldDefinitionProvider builtInProvider = CreateBuiltInProvider();
        _currentProviderSnapshot = new Ra2FieldRegistryProviderSnapshot(builtInProvider, 1);
        CurrentProvenanceProvider = _provenanceSnapshotBuilder.Build(
            new LocalFieldRegistryLoadResult([], []),
            null,
            builtInProvider);
    }

    public FieldRegistryRuntimeState CurrentState { get; private set; }

    public IRa2FieldDefinitionProvider CurrentProvider => CaptureProviderSnapshot().Provider;

    internal IFieldRegistryProvenanceProvider CurrentProvenanceProvider { get; private set; }

    public IRa2FieldDefinitionProvider Reload(string? projectRootPath)
    {
        string globalDirectory = GetGlobalActiveDirectoryPath();
        string? projectDirectory = GetProjectActiveDirectoryPath(projectRootPath);

        LocalFieldRegistryLoadResult globalResult = _loader.LoadDirectory(globalDirectory);
        LocalFieldRegistryLoadResult? projectResult = projectDirectory is null
            ? null
            : _loader.LoadDirectory(projectDirectory);

        List<IRa2FieldDefinitionProvider> providers = new();
        if (projectResult is not null && projectResult.Definitions.Count > 0)
            providers.Add(new LocalRa2FieldDefinitionProvider(projectResult.Definitions));

        if (globalResult.Definitions.Count > 0)
            providers.Add(new LocalRa2FieldDefinitionProvider(globalResult.Definitions));

        IRa2FieldDefinitionProvider builtInProvider = CreateBuiltInProvider();
        providers.Add(builtInProvider);
        IRa2FieldDefinitionProvider nextProvider = new CompositeRa2FieldDefinitionProvider(providers);
        IFieldRegistryProvenanceProvider nextProvenanceProvider = _provenanceSnapshotBuilder.Build(
            globalResult,
            projectResult,
            builtInProvider);
        FieldRegistryRuntimeState nextState = FieldRegistryRuntimeState.FromLoadResults(
            globalDirectory,
            globalResult,
            projectDirectory,
            projectResult);

        lock (_providerPublishGate)
        {
            long nextRevision = checked(_currentProviderSnapshot.Revision + 1);
            CurrentProvenanceProvider = nextProvenanceProvider;
            CurrentState = nextState;
            _currentProviderSnapshot = new Ra2FieldRegistryProviderSnapshot(nextProvider, nextRevision);
        }

        return nextProvider;
    }

    /// <summary>
    /// 捕获当前字段库 Provider 与对应修订号，供一次只读分析全程复用。
    /// </summary>
    internal Ra2FieldRegistryProviderSnapshot CaptureProviderSnapshot()
    {
        lock (_providerPublishGate)
            return _currentProviderSnapshot;
    }

    private IRa2FieldDefinitionProvider CreateBuiltInProvider()
    {
        LocalFieldRegistryLoadResult builtInResult = _builtInLoader.Load();
        return builtInResult.Definitions.Count > 0
            ? new LocalRa2FieldDefinitionProvider(builtInResult.Definitions)
            : new BuiltInRa2FieldDefinitionProvider();
    }

    public string GetGlobalActiveDirectoryPath()
    {
        if (!string.IsNullOrWhiteSpace(_globalActiveDirectoryOverride))
            return _globalActiveDirectoryOverride;

        return Path.Combine(GetGlobalRootDirectoryPath(), "active");
    }

    public string GetGlobalRootDirectoryPath()
    {
        if (!string.IsNullOrWhiteSpace(_globalActiveDirectoryOverride))
        {
            DirectoryInfo? parent = Directory.GetParent(_globalActiveDirectoryOverride);
            return parent?.FullName ?? _globalActiveDirectoryOverride;
        }

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "RA2IniEditor", "FieldRegistry");
    }

    public string? GetProjectActiveDirectoryPath(string? projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
            return null;

        return Path.Combine(projectRootPath, ".ra2inieditor", "field-registry", "active");
    }
}

public sealed class FieldRegistryRuntimeState
{
    private FieldRegistryRuntimeState(
        FieldRegistryPackLoadStatus global,
        FieldRegistryPackLoadStatus? project,
        IReadOnlyList<string> warnings)
    {
        Global = global;
        Project = project;
        Warnings = warnings;
    }

    public FieldRegistryPackLoadStatus Global { get; }

    public FieldRegistryPackLoadStatus? Project { get; }

    public int TotalLocalFieldCount => Global.FieldCount + (Project?.FieldCount ?? 0);

    public IReadOnlyList<string> Warnings { get; }

    public static FieldRegistryRuntimeState Empty(string globalDirectory, string? projectDirectory)
    {
        FieldRegistryPackLoadStatus global = FieldRegistryPackLoadStatus.FromResult(
            "Global",
            globalDirectory,
            new LocalFieldRegistryLoadResult([], []));
        FieldRegistryPackLoadStatus? project = projectDirectory is null
            ? null
            : FieldRegistryPackLoadStatus.FromResult("Project", projectDirectory, new LocalFieldRegistryLoadResult([], []));
        return new FieldRegistryRuntimeState(global, project, []);
    }

    public static FieldRegistryRuntimeState FromLoadResults(
        string globalDirectory,
        LocalFieldRegistryLoadResult globalResult,
        string? projectDirectory,
        LocalFieldRegistryLoadResult? projectResult)
    {
        FieldRegistryPackLoadStatus global = FieldRegistryPackLoadStatus.FromResult("Global", globalDirectory, globalResult);
        FieldRegistryPackLoadStatus? project = projectDirectory is null || projectResult is null
            ? null
            : FieldRegistryPackLoadStatus.FromResult("Project", projectDirectory, projectResult);

        List<string> warnings = new();
        warnings.AddRange(globalResult.Warnings);
        if (projectResult is not null)
            warnings.AddRange(projectResult.Warnings);

        return new FieldRegistryRuntimeState(global, project, Array.AsReadOnly(warnings.ToArray()));
    }
}

public sealed class FieldRegistryPackLoadStatus
{
    private FieldRegistryPackLoadStatus(
        string scope,
        string directoryPath,
        bool directoryExists,
        int fieldCount,
        int warningCount)
    {
        Scope = scope;
        DirectoryPath = directoryPath;
        DirectoryExists = directoryExists;
        FieldCount = fieldCount;
        WarningCount = warningCount;
    }

    public string Scope { get; }

    public string DirectoryPath { get; }

    public bool DirectoryExists { get; }

    public int FieldCount { get; }

    public int WarningCount { get; }

    public string StatusText
    {
        get
        {
            if (!DirectoryExists)
                return "No active field pack";

            if (WarningCount > 0)
                return "Loaded with warnings";

            return FieldCount > 0 ? "Loaded" : "No fields";
        }
    }

    public static FieldRegistryPackLoadStatus FromResult(
        string scope,
        string directoryPath,
        LocalFieldRegistryLoadResult result)
    {
        return new FieldRegistryPackLoadStatus(
            scope,
            directoryPath,
            Directory.Exists(directoryPath),
            result.Definitions.Count,
            result.Warnings.Count);
    }
}
