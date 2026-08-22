using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiOutboundTextSanitizerTests
{
    [Theory]
    [InlineData("Authorization: Bearer value")]
    [InlineData("DEEPSEEK_API_KEY=value")]
    [InlineData("Provider Metadata: internal route")]
    [InlineData("RAW_RESPONSE: body")]
    public void Sanitize_RedactsSensitiveMarkerLineCaseInsensitively(string source)
    {
        Ra2AiOutboundTextSanitizationResult result =
            Ra2AiOutboundTextSanitizer.Sanitize(source);

        Assert.True(result.WasRedacted);
        Assert.Equal(Ra2AiOutboundTextSanitizer.RedactedText, result.Text);
    }

    [Fact]
    public void Sanitize_RedactsMultipleApiKeyLikeTokensWithoutLeakingOriginals()
    {
        const string firstToken = "sk-12345678ABCDEFG";
        const string secondToken = "DS-abcdefgh87654321";

        Ra2AiOutboundTextSanitizationResult result =
            Ra2AiOutboundTextSanitizer.Sanitize($"first={firstToken}; second={secondToken}");

        Assert.True(result.WasRedacted);
        Assert.DoesNotContain(firstToken, result.Text, StringComparison.Ordinal);
        Assert.DoesNotContain(secondToken, result.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, result.Text.Split(Ra2AiOutboundTextSanitizer.RedactedText).Length - 1);
    }

    [Fact]
    public void Sanitize_NormalizesCrLfAndLoneCrWithoutChangingSafeLines()
    {
        Ra2AiOutboundTextSanitizationResult result =
            Ra2AiOutboundTextSanitizer.Sanitize("safe one\r\nsafe two\rsafe three");

        Assert.False(result.WasRedacted);
        Assert.Equal(
            string.Join(Environment.NewLine, ["safe one", "safe two", "safe three"]),
            result.Text);
    }

    [Theory]
    [InlineData("sk-short")]
    [InlineData("desk-123456789")]
    [InlineData("risk-123456789")]
    [InlineData("A normal token budget is safe text.")]
    public void Sanitize_DoesNotRedactFalseBoundaryMatches(string source)
    {
        Ra2AiOutboundTextSanitizationResult result =
            Ra2AiOutboundTextSanitizer.Sanitize(source);

        Assert.False(result.WasRedacted);
        Assert.Equal(source, result.Text);
    }

    [Fact]
    public void Sanitize_EmptyAndNullReturnEmptyWithoutRedaction()
    {
        Assert.Equal(
            new Ra2AiOutboundTextSanitizationResult(string.Empty, WasRedacted: false),
            Ra2AiOutboundTextSanitizer.Sanitize(null));
        Assert.Equal(
            new Ra2AiOutboundTextSanitizationResult(string.Empty, WasRedacted: false),
            Ra2AiOutboundTextSanitizer.Sanitize(string.Empty));
    }
}
