using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class CurrentSourceSnapshotEncodingTests
{
    [Fact]
    public void PublicConstructor_DefaultsEncodingMetadataToUnknown()
    {
        CurrentSourceSnapshot snapshot = new("C:\\mod", "C:\\mod\\rules.ini", "rules.ini", "[Rules]", 1, SourceEditorState.Loaded);

        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, snapshot.EncodingMetadata);
    }

    [Fact]
    public void InternalConstructor_CarriesEncodingMetadata()
    {
        Ra2EditorTextEncodingMetadata metadata = new(Ra2EditorTextEncodingKind.Utf8Bom, "UTF-8 BOM", true);

        CurrentSourceSnapshot snapshot = new("C:\\mod", "C:\\mod\\rules.ini", "rules.ini", "[Rules]", 1, SourceEditorState.Loaded, metadata);

        Assert.Same(metadata, snapshot.EncodingMetadata);
    }
}
