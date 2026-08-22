using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2FieldValueCompletionRequest
{
    public Ra2FieldValueCompletionRequest(
        Ra2SectionKind sectionKind,
        string key,
        Ra2FieldDefinition? fieldDefinition,
        Ra2ValueCompletionContext context)
    {
        SectionKind = sectionKind;
        Key = key ?? string.Empty;
        FieldDefinition = fieldDefinition;
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Ra2SectionKind SectionKind { get; }

    public string Key { get; }

    public Ra2FieldDefinition? FieldDefinition { get; }

    public Ra2ValueCompletionContext Context { get; }
}
