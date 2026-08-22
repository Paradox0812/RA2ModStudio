using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniLanguageAnalysisServiceTests
{
    [Fact]
    public void Analyze_MapsExistingDiagnosticsPropertyByPropertyWithoutReordering()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Name=GI
            Name=Duplicate
            ArmorX=paper
            Primary=MissingWeapon
            """;
        Ra2LanguageAnalysisRequest request = new(
            "C:\\mod",
            "C:\\mod\\rules.ini",
            "rules.ini",
            text,
            23,
            CreateRegistrySnapshot(9));
        CurrentSourceSnapshot sourceSnapshot = new(
            request.ProjectRootPath,
            request.FilePath,
            request.FileName,
            request.Text,
            request.AnalysisVersion,
            SourceEditorState.Loaded);
        IReadOnlyList<IdeDiagnosticIssueViewModel> expected = new CurrentFileReadonlyDiagnosticService()
            .Analyze(sourceSnapshot, request.FieldRegistry.Provider);

        Ra2IniLanguageAnalysisResult result = new Ra2IniLanguageAnalysisService().Analyze(request);

        Assert.True(result.Succeeded);
        Assert.Equal(expected.Count, result.Diagnostics.Count);
        for (int index = 0; index < expected.Count; index++)
        {
            IdeDiagnosticIssueViewModel expectedIssue = expected[index];
            Ra2DiagnosticFact actualIssue = result.Diagnostics[index];
            Assert.Equal(expectedIssue.Code, actualIssue.Code);
            Assert.Equal(expectedIssue.SourceKind, actualIssue.SourceKind);
            Assert.Equal(expectedIssue.Severity, actualIssue.Severity);
            Assert.Equal(expectedIssue.Message, actualIssue.Message);
            Assert.Equal(expectedIssue.FilePath, actualIssue.FilePath);
            Assert.Equal(expectedIssue.LineNumber, actualIssue.LineNumber);
            Assert.Equal(expectedIssue.ColumnNumber, actualIssue.ColumnNumber);
            Assert.Equal(expectedIssue.SectionId, actualIssue.SectionId);
            Assert.Equal(expectedIssue.Key, actualIssue.Key);
            Assert.Equal(expectedIssue.Version, actualIssue.AnalysisVersion);
        }
    }

    [Fact]
    public void Analyze_NonFatalFailureReturnsSafeResultWithoutExceptionText()
    {
        Ra2IniLanguageAnalysisService service = new(
            new ThrowingTextDocumentParser(new InvalidOperationException("sensitive detail")),
            new Ra2DocumentSemanticModelBuilder(),
            new CurrentFileReadonlyDiagnosticService());

        Ra2IniLanguageAnalysisResult result = service.Analyze(CreateRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2LanguageAnalysisFailureKind.UnexpectedFailure, result.FailureKind);
        Assert.Equal("Language analysis failed.", result.FailureMessage);
        Assert.DoesNotContain("sensitive", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.TextDocument);
        Assert.Null(result.SemanticModel);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Analyze_FatalFailureIsNotConvertedToOrdinaryResult()
    {
        Ra2IniLanguageAnalysisService service = new(
            new ThrowingTextDocumentParser(new OutOfMemoryException("fatal")),
            new Ra2DocumentSemanticModelBuilder(),
            new CurrentFileReadonlyDiagnosticService());

        Assert.Throws<OutOfMemoryException>(() => service.Analyze(CreateRequest()));
    }

    [Fact]
    public void Analyze_AfterRuntimeReloadKeepsRequestProviderAndRevisionStable()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), $"ra2-a1-{Guid.NewGuid():N}");
        string globalActive = Path.Combine(tempRoot, "global-active");
        Directory.CreateDirectory(globalActive);

        try
        {
            FieldRegistryRuntimeService runtimeService = new(new LocalFieldRegistryLoader(), globalActive);
            Ra2FieldRegistryProviderSnapshot capturedRegistry = runtimeService.CaptureProviderSnapshot();
            const string text = """
                [InfantryTypes]
                0=E1

                [E1]
                SnapshotOnlyKey=1
                """;
            Ra2LanguageAnalysisRequest request = new(
                tempRoot,
                Path.Combine(tempRoot, "rules.ini"),
                "rules.ini",
                text,
                31,
                capturedRegistry);
            WritePack(globalActive, "SnapshotOnlyKey");

            runtimeService.Reload(null);
            Ra2IniLanguageAnalysisResult result = new Ra2IniLanguageAnalysisService().Analyze(request);

            Assert.True(result.Succeeded);
            Assert.Equal(capturedRegistry.Revision, result.FieldRegistryRevision);
            Assert.Equal(1, result.FieldRegistryRevision);
            Assert.Equal(2, runtimeService.CaptureProviderSnapshot().Revision);
            Assert.Same(capturedRegistry.Provider, request.FieldRegistry.Provider);
            Ra2KeyValueSymbol keyValue = Assert.Single(
                result.SemanticModel!.KeyValues,
                item => item.Key == "SnapshotOnlyKey");
            Assert.False(keyValue.IsKnownKey);
            Assert.True(runtimeService.CurrentProvider.IsKnownField(
                Ra2SectionKind.Infantry,
                "SnapshotOnlyKey"));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static Ra2LanguageAnalysisRequest CreateRequest()
        => new(
            "project",
            "rules.ini",
            "rules.ini",
            "[E1]\nOwner=Americans",
            12,
            CreateRegistrySnapshot(7));

    private static Ra2FieldRegistryProviderSnapshot CreateRegistrySnapshot(long revision)
        => new(new BuiltInRa2FieldDefinitionProvider(), revision);

    private static void WritePack(string directoryPath, string key)
    {
        File.WriteAllText(Path.Combine(directoryPath, "global.fields.json"), $$"""
            {
              "fields": [
                {
                  "key": "{{key}}",
                  "appliesTo": ["Infantry"],
                  "editorKind": "Text",
                  "sourceKind": "External"
                }
              ]
            }
            """);
    }

    private sealed class ThrowingTextDocumentParser : IRa2IniTextDocumentParser
    {
        private readonly Exception _exception;

        public ThrowingTextDocumentParser(Exception exception)
        {
            _exception = exception;
        }

        public Ra2IniTextDocument Parse(string text)
            => throw _exception;
    }
}
