using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniLanguageAnalysisContractTests
{
    [Fact]
    public void Request_PreservesNeutralInputAndCapturedRegistry()
    {
        Ra2FieldRegistryProviderSnapshot registry = CreateRegistrySnapshot(7);

        Ra2LanguageAnalysisRequest request = new(
            "project",
            "rules.ini",
            "rules.ini",
            "[E1]\nOwner=Americans",
            12,
            registry);

        Assert.Equal("project", request.ProjectRootPath);
        Assert.Equal("rules.ini", request.FilePath);
        Assert.Equal("rules.ini", request.FileName);
        Assert.Equal("[E1]\nOwner=Americans", request.Text);
        Assert.Equal(12, request.AnalysisVersion);
        Assert.Same(registry, request.FieldRegistry);
    }

    [Fact]
    public void Result_SuccessPreservesModelsAndDefensivelyCopiesDiagnostics()
    {
        Ra2LanguageAnalysisRequest request = CreateRequest();
        Ra2IniTextDocument textDocument = new Ra2IniTextDocumentParser().Parse(request.Text);
        Ra2DocumentSemanticModel semanticModel = new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot(request.FilePath, request.Text, request.AnalysisVersion),
            request.FieldRegistry.Provider);
        List<Ra2DiagnosticFact> diagnostics =
        [
            new Ra2DiagnosticFact(
                "TEST",
                "Test",
                IniIssueSeverity.Warning,
                "message",
                request.FilePath,
                1,
                1,
                "E1",
                "Owner",
                request.AnalysisVersion)
        ];

        Ra2IniLanguageAnalysisResult result = new(
            request,
            Ra2LanguageAnalysisFailureKind.None,
            null,
            textDocument,
            semanticModel,
            diagnostics);
        diagnostics.Clear();

        Assert.True(result.Succeeded);
        Assert.Equal(Ra2LanguageAnalysisFailureKind.None, result.FailureKind);
        Assert.Null(result.FailureMessage);
        Assert.Same(textDocument, result.TextDocument);
        Assert.Same(semanticModel, result.SemanticModel);
        Assert.Single(result.Diagnostics);
        Assert.Equal(7, result.FieldRegistryRevision);
    }

    [Fact]
    public void Result_FailureRejectsPartialModelsAndDiagnostics()
    {
        Ra2LanguageAnalysisRequest request = CreateRequest();
        Ra2IniTextDocument textDocument = new Ra2IniTextDocumentParser().Parse(request.Text);

        Assert.Throws<ArgumentException>(() => new Ra2IniLanguageAnalysisResult(
            request,
            Ra2LanguageAnalysisFailureKind.UnexpectedFailure,
            "safe failure",
            textDocument,
            null,
            []));
        Assert.Throws<ArgumentException>(() => new Ra2IniLanguageAnalysisResult(
            request,
            Ra2LanguageAnalysisFailureKind.UnexpectedFailure,
            null,
            null,
            null,
            []));
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
}
