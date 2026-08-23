using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiAuthoringToolAdapterTests
{
    private readonly Ra2EditableDocumentSessionService _sessionService = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());

    [Fact]
    public void TryCreatePlan_BindsTrustedLocalSnapshotAndMapsOperations()
    {
        Ra2AuthoringSnapshot snapshot = CreateSnapshot();
        Ra2AiToolCall call = Call(
            """
            {
              "outcome": "proposal",
              "summary": "Update unit",
              "operations": [
                {
                  "kind": "replace_field_value",
                  "section": "E1",
                  "key": "Strength",
                  "value": "125"
                },
                {
                  "kind": "upsert_field",
                  "section": "E1",
                  "key": "Armor",
                  "value": "light"
                }
              ]
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter()
            .TryCreatePlan(call, new Ra2AiAuthoringRequestContext(snapshot));

        Assert.True(result.Succeeded);
        Ra2IniEditPlan plan = Assert.IsType<Ra2IniEditPlan>(result.Plan);
        Assert.Equal(snapshot.DocumentId, plan.ExpectedDocumentId);
        Assert.Equal(snapshot.EditRevision, plan.ExpectedVersion);
        Assert.Equal(snapshot.FieldRegistry.Revision, plan.ExpectedFieldRegistryRevision);
        Assert.Equal(Ra2AiAuthoringToolCatalog.TrustedPlanOrigin, plan.Origin);
        Assert.Equal("Update unit", plan.Summary);
        Assert.Equal(2, plan.Operations.Count);
        Assert.Equal(Ra2IniEditOperationKind.ReplaceFieldValue, plan.Operations[0].Kind);
        Assert.Equal(Ra2IniEditOperationKind.UpsertField, plan.Operations[1].Kind);
    }

    [Fact]
    public void TryCreatePlan_NormalizesUnambiguousNonStrictProviderDrift()
    {
        Ra2AiEditPlanCreationResult result = Parse(
            """
            {
              "operations": {
                "kind": "replace_field_value",
                "section": "E1",
                "key": "Strength",
                "value": 150,
              },
            }
            """);

        Assert.True(result.Succeeded, result.Message);
        Ra2IniEditPlan plan = Assert.IsType<Ra2IniEditPlan>(result.Plan);
        Ra2IniEditOperation operation = Assert.Single(plan.Operations);
        Assert.Equal("150", operation.Value);
        Assert.Equal("AI 结构化修改建议", plan.Summary);
    }

    [Fact]
    public void TryCreatePlan_InfersClarificationOnlyFromUnambiguousMessageShape()
    {
        Ra2AiEditPlanCreationResult result = Parse(
            """{"message":"请提供目标 Section。"}""");

        Assert.True(result.NeedsClarification);
        Assert.Equal("请提供目标 Section。", result.Message);
    }

    [Fact]
    public void TryCreatePlan_RejectsUnsupportedTool()
    {
        Ra2AiToolCall call = new("call-1", "apply_file", "{}");

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter()
            .TryCreatePlan(call, new Ra2AiAuthoringRequestContext(CreateSnapshot()));

        Assert.Equal(Ra2AiEditProposalFailureKind.UnsupportedTool, result.FailureKind);
        Assert.Null(result.Plan);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("""{"summary":"x"}""")]
    [InlineData("""{"outcome":"proposal","summary":1,"operations":[]}""")]
    [InlineData("""{"outcome":"proposal","summary":"x","operations":{}}""")]
    public void TryCreatePlan_RejectsInvalidArgumentShape(string arguments)
    {
        Ra2AiEditPlanCreationResult result = Parse(arguments);

        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, result.FailureKind);
    }

    [Fact]
    public void TryCreatePlan_RejectsUnknownRootAndOperationProperties()
    {
        Ra2AiEditPlanCreationResult root = Parse(
            """{"outcome":"proposal","summary":"x","operations":[],"documentId":"model-owned"}""");
        Ra2AiEditPlanCreationResult operation = Parse(
            """
            {
              "outcome":"proposal",
              "summary":"x",
              "operations":[{
                "kind":"upsert_field",
                "section":"E1",
                "key":"Strength",
                "value":"125",
                "apply":true
              }]
            }
            """);

        Assert.Equal(Ra2AiEditProposalFailureKind.UnknownArgumentProperty, root.FailureKind);
        Assert.Equal(Ra2AiEditProposalFailureKind.UnknownArgumentProperty, operation.FailureKind);
    }

    [Fact]
    public void TryCreatePlan_RejectsDuplicateProperties()
    {
        Ra2AiEditPlanCreationResult root = Parse(
            """{"outcome":"proposal","summary":"x","summary":"y","operations":[]}""");
        Ra2AiEditPlanCreationResult operation = Parse(
            """
            {
              "outcome":"proposal",
              "summary":"x",
              "operations":[{
                "kind":"upsert_field",
                "section":"E1",
                "key":"Strength",
                "key":"Armor",
                "value":"125"
              }]
            }
            """);

        Assert.Equal(Ra2AiEditProposalFailureKind.DuplicateArgumentProperty, root.FailureKind);
        Assert.Equal(
            Ra2AiEditProposalFailureKind.DuplicateArgumentProperty,
            operation.FailureKind);
    }

    [Fact]
    public void TryCreatePlan_RejectsUnsupportedOperationAndExcessiveValue()
    {
        Ra2AiEditPlanCreationResult unsupported = Parse(
            ValidOperationJson("delete_field", "1"));
        Ra2AiEditPlanCreationResult oversized = Parse(
            ValidOperationJson("upsert_field", new string('x', 8193)));

        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidOperation, unsupported.FailureKind);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, oversized.FailureKind);
    }

    [Fact]
    public void TryCreatePlan_RejectsEmptyOrExcessiveOperationLists()
    {
        Ra2AiEditPlanCreationResult empty = Parse(
            """{"outcome":"proposal","summary":"x","operations":[]}""");
        string operations = string.Join(
            ",",
            Enumerable.Range(0, 129)
                .Select(_ =>
                    """{"kind":"upsert_field","section":"E1","key":"Strength","value":"1"}"""));
        Ra2AiEditPlanCreationResult excessive = Parse(
            $$"""{"outcome":"proposal","summary":"x","operations":[{{operations}}]}""");

        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidOperation, empty.FailureKind);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidOperation, excessive.FailureKind);
    }

    [Fact]
    public void TryCreatePlan_ReturnsClarificationWithoutPlanOrFailure()
    {
        Ra2AiEditPlanCreationResult result = Parse(
            """{"outcome":"needs_clarification","message":"请提供 Section、Key 和目标值。"}""");

        Assert.True(result.NeedsClarification);
        Assert.Equal(Ra2AiToolAdaptationOutcomeKind.NeedsClarification, result.OutcomeKind);
        Assert.Equal(Ra2AiEditProposalFailureKind.None, result.FailureKind);
        Assert.Null(result.Plan);
    }

    [Theory]
    [InlineData("""{"outcome":"needs_clarification","message":"x","summary":"y"}""")]
    [InlineData("""{"outcome":"needs_clarification","message":"x","operations":[]}""")]
    [InlineData("""{"outcome":"proposal","summary":"x","operations":[],"message":"mixed"}""")]
    [InlineData("""{"outcome":"unknown","message":"x"}""")]
    public void TryCreatePlan_RejectsMixedOrUnknownOutcomeShapes(string arguments)
    {
        Ra2AiEditPlanCreationResult result = Parse(arguments);

        Assert.Equal(Ra2AiToolAdaptationOutcomeKind.Failed, result.OutcomeKind);
        Assert.NotEqual(Ra2AiEditProposalFailureKind.None, result.FailureKind);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void TryCreatePlan_StillRejectsAmbiguousOrCompositeValues(string valueJson)
    {
        Ra2AiEditPlanCreationResult result = Parse(
            $$"""
            {
              "outcome":"proposal",
              "summary":"x",
              "operations":[{
                "kind":"replace_field_value",
                "section":"E1",
                "key":"Strength",
                "value":{{valueJson}}
              }]
            }
            """);

        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, result.FailureKind);
        Assert.Contains("value", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.Plan);
    }

    [Fact]
    public void TryCreatePlan_ReportsSanitizedStructuralFailureWithoutEchoingArguments()
    {
        const string secret = "ds-do-not-echo-this";
        Ra2AiEditPlanCreationResult result = Parse(
            $$"""{"outcome":"proposal","summary":"{{secret}}"}""");

        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, result.FailureKind);
        Assert.Contains("operations", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(secret, result.Message, StringComparison.Ordinal);
    }

    private Ra2AiEditPlanCreationResult Parse(string arguments)
        => new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            Call(arguments),
            new Ra2AiAuthoringRequestContext(CreateSnapshot()));

    private static Ra2AiToolCall Call(string arguments)
        => new(
            "call-1",
            Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
            arguments);

    private static string ValidOperationJson(string kind, string value)
        => $$"""
            {
              "outcome":"proposal",
              "summary":"x",
              "operations":[{
                "kind":"{{kind}}",
                "section":"E1",
                "key":"Strength",
                "value":{{System.Text.Json.JsonSerializer.Serialize(value)}}
              }]
            }
            """;

    private Ra2AuthoringSnapshot CreateSnapshot()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing(
            "rulesmd.ini",
            "[E1]\nStrength=100");
        Ra2AuthoringSnapshotCaptureResult result = Ra2AuthoringSnapshot.Capture(
            session,
            session.DocumentState.CurrentText,
            @"C:\Project",
            new Ra2FieldRegistryProviderSnapshot(
                new BuiltInRa2FieldDefinitionProvider(),
                revision: 7));
        return Assert.IsType<Ra2AuthoringSnapshot>(result.Snapshot);
    }
}
