namespace RA2IniEditor.Core;

/// <summary>INI 文档写回。</summary>
public static class IniSerializer
{
    public static string Serialize(IniDocument document)
    {
        string newLine = string.IsNullOrEmpty(document.NewLine) ? Environment.NewLine : document.NewLine;
        return string.Join(newLine, document.Lines.Select(line => line.ToOutputText()));
    }
}
