using System.Security.Cryptography;

namespace RA2IniEditor.AssetHost.Tests;

internal static class AssetHostTestFixture
{
    internal const string ProviderId = "ra2-fixture-provider";
    internal const string ProviderVersion = "1.0.0";
    internal const string ModelId = "fixture-image-to-mesh";
    internal const string ModelRevision = "fixture-r1";

    internal static Ra2GenerationProviderConfiguration CreateConfiguration(
        string workspaceRoot,
        string? expectedHash = null,
        string expectedProviderId = ProviderId,
        bool licenseAccepted = true,
        IEnumerable<string>? forbiddenRoots = null,
        TimeSpan? probeTimeout = null,
        TimeSpan? orphanTtl = null,
        long maximumWorkspaceRootBytes = Ra2GenerationLimits.DefaultWorkspaceRootBytes,
        string? executablePath = null) =>
        new(
            executablePath ?? FindFixtureExecutable(),
            expectedHash ?? ComputeExecutableHash(executablePath ?? FindFixtureExecutable()),
            expectedProviderId,
            ProviderVersion,
            ModelId,
            ModelRevision,
            Ra2GenerationCapability.ReferenceImageToMesh,
            licenseAccepted,
            workspaceRoot,
            forbiddenRoots,
            probeTimeout,
            orphanTtl,
            maximumWorkspaceRootBytes);

    internal static Ra2GenerationRequest CreateRequest(
        Guid? runId = null,
        string prompt = "Create one deterministic fixture mesh.",
        int seed = 17,
        int candidateCount = 1,
        bool includePreviewPng = false,
        TimeSpan? timeout = null) =>
        new(
            runId ?? Guid.NewGuid(),
            prompt,
            string.Empty,
            new[] { new Ra2GenerationReferenceImage("front.png", Ra2GenerationMediaKind.Png, MinimalPng) },
            seed,
            candidateCount,
            includePreviewPng,
            ProviderId,
            ModelRevision,
            timeout ?? TimeSpan.FromSeconds(20));

    internal static string CreateUnusedWorkspacePath() =>
        Path.Combine(Path.GetTempPath(), "ra2-asset-host-tests", Guid.NewGuid().ToString("N"));

    internal static string FindFixtureExecutable()
    {
        string fileName = OperatingSystem.IsWindows()
            ? "RA2IniEditor.AssetHost.Tests.exe"
            : "RA2IniEditor.AssetHost.Tests";
        string path = Path.Combine(AppContext.BaseDirectory, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The managed fixture provider apphost was not produced.", path);
        }

        return Path.GetFullPath(path);
    }

    internal static string ComputeFixtureExecutableHash()
        => ComputeExecutableHash(FindFixtureExecutable());

    internal static string ComputeExecutableHash(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    internal static FixtureProviderSandbox CreateProviderSandbox(string probeMode)
    {
        string sourceRoot = AppContext.BaseDirectory;
        string sandboxRoot = Path.Combine(Path.GetTempPath(), "ra2-asset-host-provider-fixtures", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(sandboxRoot);
        foreach (string fileName in new[]
                 {
                     "RA2IniEditor.AssetHost.Tests.exe",
                     "RA2IniEditor.AssetHost.Tests.dll",
                     "RA2IniEditor.AssetHost.Tests.deps.json",
                     "RA2IniEditor.AssetHost.Tests.runtimeconfig.json",
                     "RA2IniEditor.AssetHost.dll"
                 })
        {
            File.Copy(Path.Combine(sourceRoot, fileName), Path.Combine(sandboxRoot, fileName));
        }

        string executablePath = Path.Combine(sandboxRoot, "RA2IniEditor.AssetHost.Tests.exe");
        File.WriteAllText(executablePath + ".probe-mode", probeMode);
        return new FixtureProviderSandbox(sandboxRoot, executablePath);
    }

    private static readonly byte[] MinimalPng =
    {
        137, 80, 78, 71, 13, 10, 26, 10,
        0, 0, 0, 13, 73, 72, 68, 82,
        0, 0, 0, 1, 0, 0, 0, 1,
        8, 6, 0, 0, 0, 31, 21, 196, 137,
        0, 0, 0, 13, 73, 68, 65, 84,
        8, 215, 99, 248, 207, 192, 240, 31,
        0, 5, 0, 1, 255, 137, 153, 61, 29,
        0, 0, 0, 0, 73, 69, 78, 68,
        174, 66, 96, 130
    };
}

internal sealed class FixtureProviderSandbox : IDisposable
{
    internal FixtureProviderSandbox(string root, string executablePath)
    {
        Root = root;
        ExecutablePath = executablePath;
    }

    internal string Root { get; }
    internal string ExecutablePath { get; }

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
