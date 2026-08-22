using RA2IniEditor.IDE.Models;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Loads readonly INI text for the currently selected file.
/// </summary>
public sealed class ReadonlyIniContentService
{
    public const long LargeFileWarningThresholdBytes = 2 * 1024 * 1024;
    public const long VeryLargeFilePreviewThresholdBytes = 8 * 1024 * 1024;

    private readonly IIniFileStore _fileStore;
    private readonly Ra2EditorEncodingMetadataAdapter _encodingMetadataAdapter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadonlyIniContentService"/> class.
    /// </summary>
    public ReadonlyIniContentService(IIniFileStore fileStore)
        : this(fileStore, new Ra2EditorEncodingMetadataAdapter())
    {
    }

    internal ReadonlyIniContentService(
        IIniFileStore fileStore,
        Ra2EditorEncodingMetadataAdapter encodingMetadataAdapter)
    {
        _fileStore = fileStore;
        _encodingMetadataAdapter = encodingMetadataAdapter;
    }

    /// <summary>
    /// Reads readonly INI content for a selected file descriptor.
    /// </summary>
    public ReadonlyIniContentResult ReadFileReadonly(ReadonlyIniFileDescriptor descriptor)
    {
        if (descriptor.FileSizeBytes > VeryLargeFilePreviewThresholdBytes)
            return ReadonlyIniContentResult.LargeFileDeferred(descriptor.FileName, descriptor.FilePath, descriptor.FileSizeBytes);

        try
        {
            IniTextReadResult result = _fileStore.ReadText(descriptor.FilePath);
            var encodingMetadata = _encodingMetadataAdapter.FromReadResult(result);
            string metadata = BuildMetadataText(encodingMetadata.DisplayName, FormatNewLine(result.NewLine), descriptor.FileSizeBytes);
            return ReadonlyIniContentResult.Loaded(descriptor.FileName, result.FilePath, result.Text, metadata, encodingMetadata);
        }
        catch (Exception ex)
        {
            return ReadonlyIniContentResult.Failed(descriptor.FileName, descriptor.FilePath, ex.Message);
        }
    }

    private static string BuildMetadataText(string encodingName, string newLine, long fileSizeBytes)
    {
        string sizeText = ReadonlyIniContentResult.FormatBytes(fileSizeBytes);
        string metadata = $"Encoding: {encodingName} | Newline: {newLine} | Size: {sizeText}";
        return fileSizeBytes > LargeFileWarningThresholdBytes ? $"{metadata} | Large file" : metadata;
    }

    private static string FormatNewLine(string newLine) => newLine switch
    {
        "\r\n" => "CRLF",
        "\n" => "LF",
        "\r" => "CR",
        _ => "Default"
    };
}
