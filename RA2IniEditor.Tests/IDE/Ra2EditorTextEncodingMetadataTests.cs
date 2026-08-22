using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorTextEncodingMetadataTests
{
    [Fact]
    public void Unknown_IsReusableMetadataValue()
    {
        Ra2EditorTextEncodingMetadata metadata = Ra2EditorTextEncodingMetadata.Unknown;

        Assert.Equal(Ra2EditorTextEncodingKind.Unknown, metadata.Kind);
        Assert.False(metadata.HasBom);
        Assert.False(string.IsNullOrWhiteSpace(metadata.DisplayName));
        Assert.Null(metadata.CodePageName);
    }

    [Fact]
    public void Constructor_RejectsEmptyDisplayName()
    {
        Assert.Throws<ArgumentException>(() => new Ra2EditorTextEncodingMetadata(
            Ra2EditorTextEncodingKind.Utf8,
            string.Empty,
            hasBom: false));
    }
}
