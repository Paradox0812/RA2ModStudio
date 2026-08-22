using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry;

internal sealed class LocalFieldRegistryLoadedDefinition
{
    public LocalFieldRegistryLoadedDefinition(
        Ra2FieldDefinition definition,
        string sourceFileName,
        string sourceFilePath)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        SourceFileName = string.IsNullOrWhiteSpace(sourceFileName)
            ? throw new ArgumentException("Source file name cannot be empty.", nameof(sourceFileName))
            : sourceFileName;
        SourceFilePath = string.IsNullOrWhiteSpace(sourceFilePath)
            ? throw new ArgumentException("Source file path cannot be empty.", nameof(sourceFilePath))
            : sourceFilePath;
    }

    public Ra2FieldDefinition Definition { get; }

    public string SourceFileName { get; }

    public string SourceFilePath { get; }
}
