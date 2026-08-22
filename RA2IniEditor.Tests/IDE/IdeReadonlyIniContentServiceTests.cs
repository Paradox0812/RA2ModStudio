using System.Text;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IdeReadonlyIniContentServiceTests
{
    [Fact]
    public void ReadFileReadonly_ForNormalFileReadsTextAndMetadata()
    {
        RecordingIniFileStore fileStore = new("[Rules]\r\nName=Rules", new UTF8Encoding(false), "\r\n");
        ReadonlyIniContentService service = new(fileStore);
        ReadonlyIniFileDescriptor descriptor = new("rules.ini", @"C:\Test\rules.ini", 128);

        ReadonlyIniContentResult result = service.ReadFileReadonly(descriptor);

        Assert.Equal(1, fileStore.ReadCount);
        Assert.Equal("rules.ini", result.FileName);
        Assert.Contains("Name=Rules", result.Text);
        Assert.Contains("Encoding: UTF-8", result.MetadataText);
        Assert.Contains("Newline: CRLF", result.MetadataText);
        Assert.Equal(Ra2EditorTextEncodingKind.Utf8, result.EncodingMetadata.Kind);
        Assert.False(result.IsLargeFileDeferred);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ReadFileReadonly_ForUtf8BomMapsEncodingMetadata()
    {
        RecordingIniFileStore fileStore = new("[Rules]\r\nName=Rules", new UTF8Encoding(true), "\r\n");
        ReadonlyIniContentService service = new(fileStore);

        ReadonlyIniContentResult result = service.ReadFileReadonly(new ReadonlyIniFileDescriptor(
            "rules.ini",
            @"C:\Test\rules.ini",
            128));

        Assert.Equal(Ra2EditorTextEncodingKind.Utf8Bom, result.EncodingMetadata.Kind);
        Assert.True(result.EncodingMetadata.HasBom);
        Assert.Contains("UTF-8 BOM", result.MetadataText);
    }

    [Fact]
    public void ReadFileReadonly_ForLargeFileShowsMetadataWarningButStillReads()
    {
        RecordingIniFileStore fileStore = new("[Rules]\nName=Rules", Encoding.UTF8, "\n");
        ReadonlyIniContentService service = new(fileStore);
        ReadonlyIniFileDescriptor descriptor = new(
            "rules.ini",
            @"C:\Test\rules.ini",
            ReadonlyIniContentService.LargeFileWarningThresholdBytes + 1);

        ReadonlyIniContentResult result = service.ReadFileReadonly(descriptor);

        Assert.Equal(1, fileStore.ReadCount);
        Assert.Contains("Large file", result.MetadataText);
        Assert.False(result.IsLargeFileDeferred);
    }

    [Fact]
    public void ReadFileReadonly_ForVeryLargeFileDefersPreviewWithoutReadingText()
    {
        RecordingIniFileStore fileStore = new("[Rules]\nName=Rules", Encoding.UTF8, "\n");
        ReadonlyIniContentService service = new(fileStore);
        ReadonlyIniFileDescriptor descriptor = new(
            "rules.ini",
            @"C:\Test\rules.ini",
            ReadonlyIniContentService.VeryLargeFilePreviewThresholdBytes + 1);

        ReadonlyIniContentResult result = service.ReadFileReadonly(descriptor);

        Assert.Equal(0, fileStore.ReadCount);
        Assert.True(result.IsLargeFileDeferred);
        Assert.Contains("Full preview is deferred", result.Text);
        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, result.EncodingMetadata);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ReadFileReadonly_WhenReadFailsReturnsErrorResult()
    {
        RecordingIniFileStore fileStore = new("[Rules]\nName=Rules", Encoding.UTF8, "\n")
        {
            ThrowOnRead = true
        };
        ReadonlyIniContentService service = new(fileStore);
        ReadonlyIniFileDescriptor descriptor = new("rules.ini", @"C:\Test\rules.ini", 128);

        ReadonlyIniContentResult result = service.ReadFileReadonly(descriptor);

        Assert.Equal(1, fileStore.ReadCount);
        Assert.Equal("Synthetic read failure.", result.ErrorMessage);
        Assert.Contains("Failed to read rules.ini", result.Text);
        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, result.EncodingMetadata);
        Assert.False(result.IsLargeFileDeferred);
    }

    private sealed class RecordingIniFileStore : IIniFileStore
    {
        private readonly Encoding _encoding;
        private readonly string _newLine;
        private readonly string _text;

        public RecordingIniFileStore(string text, Encoding encoding, string newLine)
        {
            _text = text;
            _encoding = encoding;
            _newLine = newLine;
        }

        public int ReadCount { get; private set; }

        public bool ThrowOnRead { get; init; }

        public IniTextReadResult ReadText(string path)
        {
            ReadCount++;
            if (ThrowOnRead)
                throw new IOException("Synthetic read failure.");

            return new IniTextReadResult(path, _text, _encoding, _newLine);
        }

        public IniTextWriteResult WriteText(string path, string text, Encoding encoding)
        {
            throw new NotSupportedException("Readonly IDE tests must not write text.");
        }
    }
}
