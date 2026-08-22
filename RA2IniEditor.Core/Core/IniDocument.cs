using System.Text;

namespace RA2IniEditor.Core;

/// <summary>完整 INI 文档，保留行级结构。</summary>
public sealed class IniDocument
{
    public string? FilePath { get; set; }
    public Encoding Encoding { get; set; } = new UTF8Encoding(false);
    public string NewLine { get; set; } = Environment.NewLine;
    public string OriginalText { get; set; } = string.Empty;
    public List<IniLine> Lines { get; } = new();
    public List<IniSection> Sections { get; } = new();
    public List<IniIssue> ParseIssues { get; } = new();
    public List<IniIssue> Issues { get; } = new();

    public IniSection? FindSection(string name)
    {
        return Sections.FirstOrDefault(section => string.Equals(section.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
