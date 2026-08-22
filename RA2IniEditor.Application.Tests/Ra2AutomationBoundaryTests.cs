using System.Reflection;
using System.Runtime.CompilerServices;
using RA2IniEditor.Application.Automation.Experimental;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationBoundaryTests
{
    private static readonly string[] ExpectedExportedTypes =
    [
        "IRa2AutomationDocumentQueryService",
        "Ra2AutomationDocumentQueryService",
        "Ra2AutomationDocumentSnapshot",
        "Ra2AutomationFieldRegistrySnapshot",
        "Ra2AutomationTextSpan",
        "Ra2AutomationSectionQuery",
        "Ra2AutomationSectionQueryResult",
        "Ra2AutomationSectionQueryFailureKind",
        "Ra2AutomationSectionFact",
        "Ra2AutomationFieldFact",
        "Ra2AutomationReferenceQuery",
        "Ra2AutomationReferenceQueryResult",
        "Ra2AutomationReferenceQueryFailureKind",
        "Ra2AutomationReferenceTargetFact",
        "Ra2AutomationReferenceFact"
    ];

    [Fact]
    public void ApplicationAssembly_ExportsExactlyTheContractAllowlist()
    {
        Type[] exportedTypes = typeof(Ra2AutomationDocumentQueryService).Assembly
            .GetExportedTypes()
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ExpectedExportedTypes.OrderBy(name => name, StringComparer.Ordinal),
            exportedTypes.Select(type => type.Name));
        Assert.Equal(ExpectedExportedTypes.Length, exportedTypes.Length);
    }

    [Fact]
    public void ApplicationAssembly_ExposesOnlyTheExactFriendAssemblies()
    {
        string[] friends = typeof(Ra2AutomationDocumentQueryService).Assembly
            .GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => attribute.AssemblyName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "RA2IniEditor.Application.Tests",
                "RA2IniEditor.IDE",
                "RA2IniEditor.Tests"
            },
            friends);
    }

    [Fact]
    public void ApplicationAssembly_ReferencesCoreButNoUiOrInfrastructureAssembly()
    {
        string[] referencedNames = typeof(Ra2AutomationDocumentQueryService).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name!)
            .ToArray();

        Assert.Contains("RA2IniEditor.Core", referencedNames);
        Assert.DoesNotContain("RA2IniEditor.IDE", referencedNames);
        Assert.DoesNotContain("RA2IniEditor.Infrastructure", referencedNames);
    }

    [Fact]
    public void ApplicationProject_IsNet8AndProductionSourceIsHeadless()
    {
        string root = FindRepositoryRoot();
        string project = File.ReadAllText(Path.Combine(root, "RA2IniEditor.Application", "RA2IniEditor.Application.csproj"));

        Assert.Contains("<TargetFramework>net8.0</TargetFramework>", project, StringComparison.Ordinal);
        Assert.DoesNotContain("UseWPF", project, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "<ProjectReference Include=\"..\\RA2IniEditor.IDE",
            project,
            StringComparison.Ordinal);
        Assert.DoesNotContain("RA2IniEditor.Infrastructure", project, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", project, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonDock", project, StringComparison.Ordinal);

        string sourceRoot = Path.Combine(root, "RA2IniEditor.Application");
        string[] forbiddenTokens =
        [
            "System.Windows",
            "ICSharpCode.AvalonEdit",
            "RA2IniEditor.IDE",
            "RA2IniEditor.Infrastructure",
            "FieldRegistryRuntimeService",
            "File.Read",
            "File.Write",
            "Directory.",
            "Environment.",
            "Process.",
            "Dispatcher",
            "Clipboard"
        ];

        foreach (string path in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                           !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
        {
            string source = File.ReadAllText(path);
            foreach (string token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PublicContracts_AreImmutableAndValidateStructure()
    {
        Assert.True(typeof(Ra2AutomationTextSpan).IsValueType);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationTextSpan(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationTextSpan(1, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationTextSpan(int.MaxValue, 1));
        Assert.Equal(int.MaxValue, new Ra2AutomationTextSpan(0, int.MaxValue).End);

        Assert.Throws<ArgumentNullException>(() => new Ra2AutomationFieldRegistrySnapshot(null!, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationFieldRegistrySnapshot(
            new AutomationTestSupport.EmptyFieldDefinitionProvider(),
            0));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationDocumentSnapshot(
            Guid.Empty,
            0,
            "rulesmd.ini",
            string.Empty,
            false,
            new Ra2AutomationFieldRegistrySnapshot(
                new AutomationTestSupport.EmptyFieldDefinitionProvider(),
                1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationDocumentSnapshot(
            Guid.NewGuid(),
            -1,
            "rulesmd.ini",
            string.Empty,
            false,
            new Ra2AutomationFieldRegistrySnapshot(
                new AutomationTestSupport.EmptyFieldDefinitionProvider(),
                1)));
        Assert.Throws<ArgumentException>(() => new Ra2AutomationDocumentSnapshot(
            Guid.NewGuid(),
            0,
            " ",
            string.Empty,
            false,
            new Ra2AutomationFieldRegistrySnapshot(
                new AutomationTestSupport.EmptyFieldDefinitionProvider(),
                1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationSectionQuery("E1", -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationReferenceQuery(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationReferenceQuery(
            0,
            new Ra2AutomationTextSpan(0, 0)));

        Type[] contractTypes = typeof(Ra2AutomationDocumentQueryService).Assembly
            .GetExportedTypes()
            .ToArray();
        foreach (PropertyInfo property in contractTypes.SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)))
            Assert.Null(property.SetMethod);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RA2IniEditor.IDE.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
