namespace RA2IniEditor.IDE.Classification;

internal sealed class Ra2SectionClassificationWarning
{
    public Ra2SectionClassificationWarning(string sectionName, string message, int lineNumber)
    {
        SectionName = sectionName;
        Message = message;
        LineNumber = lineNumber;
    }

    public string SectionName { get; }

    public string Message { get; }

    public int LineNumber { get; }
}
