using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiToolContractTests
{
    private const string Schema = """
        {
          "type": "object",
          "properties": {
            "summary": { "type": "string" }
          }
        }
        """;

    [Fact]
    public void Request_DefaultsToAdvisoryOnly()
    {
        Ra2AiRequest request = new(
            Ra2AiIntent.Auto,
            "user",
            "prompt");

        Assert.Empty(request.Tools);
        Assert.Equal(Ra2AiToolChoiceMode.None, request.ToolChoice);
    }

    [Fact]
    public void AuthoringToolSchema_DeclaresFlatDiscriminatedOutcomes()
    {
        Ra2AiToolDefinition tool = Assert.Single(
            Ra2AiAuthoringToolCatalog.GetTools(Ra2AiCapabilityMode.CurrentDocumentEditPreview));
        using System.Text.Json.JsonDocument document =
            System.Text.Json.JsonDocument.Parse(tool.ParametersJsonSchema);
        System.Text.Json.JsonElement root = document.RootElement;

        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal("outcome", Assert.Single(root.GetProperty("required").EnumerateArray()).GetString());
        Assert.True(root.GetProperty("properties").TryGetProperty("message", out _));
        Assert.True(root.GetProperty("properties").TryGetProperty("operations", out _));
    }

    [Fact]
    public void Request_AcceptsOneUniqueAutoTool()
    {
        Ra2AiToolDefinition tool = CreateTool();

        Ra2AiRequest request = new(
            Ra2AiIntent.Auto,
            "user",
            "prompt",
            tools: [tool],
            toolChoice: Ra2AiToolChoiceMode.Auto);

        Assert.Same(tool, Assert.Single(request.Tools));
        Assert.Equal(Ra2AiToolChoiceMode.Auto, request.ToolChoice);
    }

    [Fact]
    public void Request_AcceptsOneRequiredTool()
    {
        Ra2AiToolDefinition tool = CreateTool();

        Ra2AiRequest request = new(
            Ra2AiIntent.Auto,
            "user",
            "prompt",
            tools: [tool],
            toolChoice: Ra2AiToolChoiceMode.Required);

        Assert.Same(tool, Assert.Single(request.Tools));
        Assert.Equal(Ra2AiToolChoiceMode.Required, request.ToolChoice);
    }

    [Fact]
    public void Request_RejectsToolChoiceMismatchAndDuplicateNames()
    {
        Ra2AiToolDefinition tool = CreateTool();

        Assert.Throws<ArgumentException>(() => new Ra2AiRequest(
            Ra2AiIntent.Auto,
            "user",
            "prompt",
            tools: [tool]));
        Assert.Throws<ArgumentException>(() => new Ra2AiRequest(
            Ra2AiIntent.Auto,
            "user",
            "prompt",
            toolChoice: Ra2AiToolChoiceMode.Auto));
        Assert.Throws<ArgumentException>(() => new Ra2AiRequest(
            Ra2AiIntent.Auto,
            "user",
            "prompt",
            tools: [tool, CreateTool()],
            toolChoice: Ra2AiToolChoiceMode.Auto));
    }

    [Fact]
    public void ToolDefinition_RejectsInvalidNameAndSchema()
    {
        Assert.Throws<ArgumentException>(() => new Ra2AiToolDefinition(
            "bad name",
            "description",
            Schema));
        Assert.ThrowsAny<Exception>(() => new Ra2AiToolDefinition(
            "valid_name",
            "description",
            "[]"));
        Assert.ThrowsAny<Exception>(() => new Ra2AiToolDefinition(
            "valid_name",
            "description",
            "{"));
    }

    [Fact]
    public void ToolCall_PreservesUnparsedArguments()
    {
        Ra2AiToolCall call = new(
            "call-1",
            "preview_ini_edit_plan",
            "{ not valid json yet }");

        Assert.Equal("call-1", call.Id);
        Assert.Equal("preview_ini_edit_plan", call.Name);
        Assert.Equal("{ not valid json yet }", call.ArgumentsJson);
    }

    [Fact]
    public void ToolCallDelta_RequiresAtLeastOneFragment()
    {
        Assert.Throws<ArgumentException>(() => new Ra2AiToolCallDelta(0, null, null, null));

        Ra2AiToolCallDelta delta = new(0, "call", null, "{\"");

        Assert.Equal(0, delta.Index);
        Assert.Equal("call", delta.IdFragment);
        Assert.Empty(delta.NameFragment);
        Assert.Equal("{\"", delta.ArgumentsFragment);
    }

    [Fact]
    public void ToolCallResponse_IsSuccessfulTerminalButNotTextSuccess()
    {
        Ra2AiToolCall call = new(
            "call-1",
            "preview_ini_edit_plan",
            "{}");

        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([call], "explanation");

        Assert.Equal(Ra2AiResponseKind.ToolCalls, response.Kind);
        Assert.False(response.IsSuccess);
        Assert.True(response.IsSuccessfulTerminal);
        Assert.Equal(Ra2AiStreamFinishKind.ToolCalls, response.FinishKind);
        Assert.Equal("explanation", response.Text);
        Assert.Same(call, Assert.Single(response.ToolCalls));
    }

    [Fact]
    public void ToolCallResponse_WithDiagnosticsPreservesCalls()
    {
        Ra2AiToolCall call = new("call-1", "preview_ini_edit_plan", "{}");
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([call]);
        Ra2AiRequestDiagnostics diagnostics = new(
            "0123456789abcdef0123456789abcdef",
            "deepseek-v4-flash",
            10,
            null,
            null,
            TimeSpan.FromMilliseconds(1),
            0,
            0,
            200);

        Ra2AiResponse withDiagnostics = response.WithDiagnostics(diagnostics);

        Assert.Same(diagnostics, withDiagnostics.Diagnostics);
        Assert.Same(call, Assert.Single(withDiagnostics.ToolCalls));
    }

    [Fact]
    public void StreamEvent_SeparatesTextAndToolDeltaPayloads()
    {
        Ra2AiStreamEvent text = Ra2AiStreamEvent.CreateContentDelta("a");
        Ra2AiToolCallDelta delta = new(0, "id", "name", "{}");
        Ra2AiStreamEvent tool = Ra2AiStreamEvent.CreateToolCallDelta(delta);

        Assert.Equal(Ra2AiStreamEventKind.ContentDelta, text.Kind);
        Assert.Equal("a", text.Text);
        Assert.Equal(Ra2AiStreamEventKind.ToolCallDelta, tool.Kind);
        Assert.Empty(tool.Text);
        Assert.Equal(delta, tool.ToolCallDelta);
    }

    private static Ra2AiToolDefinition CreateTool()
        => new("preview_ini_edit_plan", "description", Schema);
}
