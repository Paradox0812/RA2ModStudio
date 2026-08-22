namespace RA2IniEditor.Core;

/// <summary>INI Section，持有该 Section 下的 Key-Value 行。</summary>
public sealed class IniSection
{
    public IniSection(string name, IniSectionLine headerLine)
    {
        Name = name;
        HeaderLine = headerLine;
    }

    public string Name { get; set; }
    public IniSectionLine HeaderLine { get; }
    public List<IniKeyValueLine> KeyValues { get; } = new();
}
