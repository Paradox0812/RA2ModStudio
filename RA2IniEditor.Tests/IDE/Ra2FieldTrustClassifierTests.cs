using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldTrustClassifierTests
{
    [Fact]
    public void Classify_SourceVerified_DoesNotShowInHover()
    {
        Ra2FieldTrustInfo info = Ra2FieldTrustClassifier.Classify("source-verified-modenc-test", Ra2FieldSourceKind.Yuri);

        Assert.Equal(Ra2FieldTrustLevel.Verified, info.Level);
        Assert.False(info.ShouldShowInHover);
        Assert.Null(info.HoverFootnote);
    }

    [Fact]
    public void Classify_Inferred_ShowsLightweightHoverFootnote()
    {
        Ra2FieldTrustInfo info = Ra2FieldTrustClassifier.Classify("community-reference-inferred-yuri-test", Ra2FieldSourceKind.BuiltIn);

        Assert.Equal(Ra2FieldTrustLevel.Inferred, info.Level);
        Assert.True(info.ShouldShowInHover);
        Assert.Contains("推断", info.HoverFootnote, StringComparison.Ordinal);
    }

    [Fact]
    public void Classify_Guardrail_ShowsWarningFootnote()
    {
        Ra2FieldTrustInfo info = Ra2FieldTrustClassifier.Classify("source-verified-guardrail-general-test", Ra2FieldSourceKind.Yuri);

        Assert.Equal(Ra2FieldTrustLevel.VerifiedGuardrail, info.Level);
        Assert.True(info.ShouldShowWarningStyle);
        Assert.Contains("上下文", info.DetailText, StringComparison.Ordinal);
    }

    [Fact]
    public void Classify_NonExistent_TakesPrecedenceOverGuardrail()
    {
        Ra2FieldTrustInfo info = Ra2FieldTrustClassifier.Classify("source-verified-non-existent-guardrail-test", Ra2FieldSourceKind.Yuri);

        Assert.Equal(Ra2FieldTrustLevel.NonExistent, info.Level);
        Assert.Contains("未实现", info.ShortLabel, StringComparison.Ordinal);
    }
}
