namespace RA2IniEditor.IDE.FieldTrust;

internal sealed record Ra2FieldTrustInfo(
    Ra2FieldTrustLevel Level,
    string ShortLabel,
    string? HoverFootnote,
    string DetailText,
    bool ShouldShowInHover,
    bool ShouldShowWarningStyle);
