using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiProposalPreparationRunnerTests
{
    [Fact]
    public async Task PrepareAsync_PreCancelledRequestReturnsTypedTerminalResult()
    {
        Fixture fixture = new();
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AiEditProposalResult result = await fixture.Runner.PrepareAsync(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            fixture.Response,
            source.Token);

        Assert.Equal(Ra2AiEditProposalFailureKind.PreviewCancelled, result.FailureKind);
        Assert.Null(result.Proposal);
    }

    [Fact]
    public async Task PrepareAsync_ValidCallReturnsPreparedProposal()
    {
        Fixture fixture = new();

        Ra2AiEditProposalResult result = await fixture.Runner.PrepareAsync(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            fixture.Response,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Proposal);
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            Ra2EditableDocumentSessionService sessionService = new(
                new Ra2IniTextDocumentParser(),
                new Ra2DirtyStateService());
            Ra2EditableDocumentSession session = sessionService.StartEditing(
                "rulesmd.ini",
                "[InfantryTypes]\n1=E1\n\n[E1]\nStrength=100\n");
            Snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
                Ra2AuthoringSnapshot.Capture(
                    session,
                    session.DocumentState.CurrentText,
                    string.Empty,
                    new Ra2FieldRegistryProviderSnapshot(
                        new BuiltInRa2FieldDefinitionProvider(),
                        revision: 17)).Snapshot);
            Ra2IniAuthoringWorkspace workspace = new(
                new Ra2IniEditPreviewService(
                    new Ra2IniLanguageAnalysisService(),
                    new Ra2AddPropertyInsertPlanner()),
                new RecordingTransactionPort());
            Runner = new Ra2AiProposalPreparationRunner(
                new Ra2AiAuthoringCoordinator(
                    new Ra2AiAuthoringToolAdapter(),
                    workspace));
            Response = Ra2AiResponse.CreateToolCalls(
            [
                new Ra2AiToolCall(
                    "call-1",
                    Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                    """
                    {
                      "outcome":"proposal",
                      "summary":"Update Strength",
                      "operations":[{
                        "kind":"replace_field_value",
                        "section":"E1",
                        "key":"Strength",
                        "value":"125"
                      }]
                    }
                    """)
            ]);
        }

        public Ra2AuthoringSnapshot Snapshot { get; }

        public Ra2AiProposalPreparationRunner Runner { get; }

        public Ra2AiResponse Response { get; }
    }

    private sealed class RecordingTransactionPort : IRa2EditorTransactionPort
    {
        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
            => throw new InvalidOperationException("Runner preparation must not apply.");
    }
}
