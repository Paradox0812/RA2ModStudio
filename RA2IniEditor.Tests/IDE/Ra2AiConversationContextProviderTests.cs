using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiConversationContextProviderTests
{
    [Fact]
    public void BuildContext_ExtractsRecentUserAndAssistantTurns()
    {
        Ra2AiConversationContext context = Build([
            User("Generate a unit."),
            Assistant("Draft unit response.")
        ]);

        Assert.Equal(2, context.Turns.Count);
        Assert.Equal(Ra2AiConversationRole.User, context.Turns[0].Role);
        Assert.Equal("Generate a unit.", context.Turns[0].Text);
        Assert.False(context.Turns[0].IsDraftResponse);
        Assert.Equal(Ra2AiConversationRole.Assistant, context.Turns[1].Role);
        Assert.Equal("Draft unit response.", context.Turns[1].Text);
        Assert.True(context.Turns[1].IsDraftResponse);
        Assert.False(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_KeepsOnlyLastNTurns()
    {
        Ra2AiConversationContext context = Build(
            [
                User("turn 1"),
                Assistant("turn 2"),
                User("turn 3"),
                Assistant("turn 4"),
                User("turn 5")
            ],
            lastTurns: 3);

        Assert.Equal(3, context.Turns.Count);
        Assert.Equal(["turn 3", "turn 4", "turn 5"], context.Turns.Select(turn => turn.Text));
        Assert.True(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_EnforcesMaxTotalCharacterCount()
    {
        Ra2AiConversationContext context = Build(
            [
                User("first older message"),
                Assistant("second middle message"),
                User("third newest message")
            ],
            maxCharacters: 35,
            maxSingleTurnCharacters: 100);

        Assert.True(context.TotalCharacterCount <= 35);
        Assert.True(context.WasTruncated);
        Assert.Equal("third newest message", context.Turns[^1].Text);
    }

    [Fact]
    public void BuildContext_TruncatesOversizedAssistantResponseSafely()
    {
        string longResponse = new('A', 100);

        Ra2AiConversationContext context = Build(
            [Assistant(longResponse)],
            maxSingleTurnCharacters: 30);

        Assert.Single(context.Turns);
        Assert.True(context.Turns[0].Text.Length <= 30);
        Assert.Contains("[truncated]", context.Turns[0].Text);
        Assert.True(context.Turns[0].IsDraftResponse);
        Assert.True(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_MarksAssistantTurnsAsDraftResponses()
    {
        Ra2AiConversationContext context = Build([
            Assistant("This is an advisory draft.")
        ]);

        Assert.True(context.Turns[0].IsDraftResponse);
    }

    [Fact]
    public void BuildContext_RedactsHiddenProviderMetadata()
    {
        Ra2AiConversationContext context = Build([
            Assistant("Visible text\nProvider metadata: internal route\nMore visible text")
        ]);

        Assert.DoesNotContain("Provider metadata", context.Turns[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internal route", context.Turns[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[redacted sensitive content]", context.Turns[0].Text);
    }

    [Fact]
    public void BuildContext_RedactsApiKeyLikeText()
    {
        Ra2AiConversationContext context = Build([
            User("My key is sk-testsecret123456 and DEEPSEEK_API_KEY=secret.")
        ]);

        Assert.DoesNotContain("sk-testsecret123456", context.Turns[0].Text, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", context.Turns[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[redacted sensitive content]", context.Turns[0].Text);
    }

    [Fact]
    public void BuildContext_RedactsAuthorizationHeaderAndRawPayloads()
    {
        Ra2AiConversationContext context = Build([
            Assistant("Authorization: Bearer sk-testsecret123456\nRaw request payload: full prompt\nRaw response payload: full body")
        ]);

        Assert.DoesNotContain("Authorization", context.Turns[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer", context.Turns[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Raw request payload", context.Turns[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Raw response payload", context.Turns[0].Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildContext_UsesSharedOutboundSanitizerBehavior()
    {
        const string token = "ds-sharedtoken123456";

        Ra2AiConversationContext context = Build([User($"safe prefix {token} safe suffix")]);
        Ra2AiOutboundTextSanitizationResult direct =
            Ra2AiOutboundTextSanitizer.Sanitize($"safe prefix {token} safe suffix");

        Assert.Equal(direct.Text, context.Turns[0].Text);
        Assert.True(direct.WasRedacted);
    }

    [Fact]
    public void BuildContext_EmptyChatReturnsEmptyContext()
    {
        Ra2AiConversationContext context = Build([]);

        Assert.Empty(context.Turns);
        Assert.Equal(0, context.TotalCharacterCount);
        Assert.False(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_ExcludesIncompleteAndErrorTurns()
    {
        Ra2AiConversationContext context = Build([
            User("completed user message"),
            Assistant("partial stream", Ra2AiConversationTurnState.Incomplete),
            Assistant("provider failure", Ra2AiConversationTurnState.Error),
            Assistant("completed response")
        ]);

        Assert.Equal(2, context.Turns.Count);
        Assert.Equal(["completed user message", "completed response"], context.Turns.Select(turn => turn.Text));
        Assert.All(context.Turns, turn => Assert.Equal(Ra2AiConversationTurnState.Completed, turn.State));
        Assert.False(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_ExcludesCompletedButIneligibleTurns()
    {
        Ra2AiConversationContext context = Build([
            User("failed request prompt", isContextEligible: false),
            Assistant("successful response")
        ]);

        Assert.Single(context.Turns);
        Assert.Equal("successful response", context.Turns[0].Text);
        Assert.True(context.Turns[0].IsContextEligible);
        Assert.False(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_ExcludedTurnsDoNotCauseRecentTurnTruncation()
    {
        Ra2AiConversationContext context = Build(
            [
                User("only eligible turn"),
                Assistant("cancelled 1", Ra2AiConversationTurnState.Incomplete),
                Assistant("failed 2", Ra2AiConversationTurnState.Error)
            ],
            lastTurns: 1);

        Assert.Single(context.Turns);
        Assert.Equal("only eligible turn", context.Turns[0].Text);
        Assert.False(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_IneligibleTurnsDoNotCauseRecentTurnTruncation()
    {
        Ra2AiConversationContext context = Build(
            [
                User("only eligible turn"),
                User("failed request prompt 1", isContextEligible: false),
                User("failed request prompt 2", isContextEligible: false)
            ],
            lastTurns: 1);

        Assert.Single(context.Turns);
        Assert.Equal("only eligible turn", context.Turns[0].Text);
        Assert.False(context.WasTruncated);
    }

    [Fact]
    public void BuildContext_DoesNotModifySourceChatMessages()
    {
        Ra2AiConversationTurn[] turns =
        [
            User("Original user text"),
            Assistant("Original assistant text")
        ];
        string[] originalTexts = turns.Select(turn => turn.Text).ToArray();
        Ra2AiConversationTurnState[] originalStates = turns.Select(turn => turn.State).ToArray();
        bool[] originalEligibility = turns.Select(turn => turn.IsContextEligible).ToArray();

        _ = Build(turns, maxSingleTurnCharacters: 10);

        Assert.Equal(originalTexts[0], turns[0].Text);
        Assert.Equal(originalTexts[1], turns[1].Text);
        Assert.False(turns[0].IsDraftResponse);
        Assert.False(turns[1].IsDraftResponse);
        Assert.Equal(originalStates, turns.Select(turn => turn.State));
        Assert.Equal(originalEligibility, turns.Select(turn => turn.IsContextEligible));
    }

    [Fact]
    public void BuildContext_DoesNotModifyEditorTextOrDirtyState()
    {
        string editorText = "[HTNK]\nStrength=400";
        bool isDirty = false;

        _ = Build([
            User("Explain current unit."),
            Assistant("Draft response.")
        ]);

        Assert.Equal("[HTNK]\nStrength=400", editorText);
        Assert.False(isDirty);
    }

    private static Ra2AiConversationContext Build(
        IReadOnlyList<Ra2AiConversationTurn> turns,
        int lastTurns = Ra2AiConversationContextRequest.DefaultLastTurns,
        int maxCharacters = Ra2AiConversationContextRequest.DefaultMaxCharacters,
        int maxSingleTurnCharacters = Ra2AiConversationContextRequest.DefaultMaxSingleTurnCharacters)
    {
        Ra2AiConversationContextProvider provider = new();
        return provider.BuildContext(new Ra2AiConversationContextRequest
        {
            Turns = turns,
            LastTurns = lastTurns,
            MaxCharacters = maxCharacters,
            MaxSingleTurnCharacters = maxSingleTurnCharacters
        });
    }

    private static Ra2AiConversationTurn User(string text, bool isContextEligible = true)
        => new()
        {
            Role = Ra2AiConversationRole.User,
            Text = text,
            IsContextEligible = isContextEligible
        };

    private static Ra2AiConversationTurn Assistant(
        string text,
        Ra2AiConversationTurnState state = Ra2AiConversationTurnState.Completed)
        => new()
        {
            Role = Ra2AiConversationRole.Assistant,
            Text = text,
            State = state
        };
}
