using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Models;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Represents a file or section node in the IDE Project Explorer.
/// </summary>
public sealed class ProjectExplorerItemViewModel : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isCurrentFile;
    private bool _isCurrentSection;
    private int _sectionCount;

    public ProjectExplorerItemViewModel(
        ProjectExplorerItemKind kind,
        string displayText,
        string? filePath,
        long fileSizeBytes = 0,
        int? lineNumber = null,
        string? sectionId = null,
        string? iconText = null)
    {
        Kind = kind;
        DisplayText = displayText;
        FilePath = filePath;
        FileSizeBytes = fileSizeBytes;
        LineNumber = lineNumber;
        SectionId = sectionId;
        IconText = string.IsNullOrWhiteSpace(iconText)
            ? GetDefaultIconText(kind)
            : iconText;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    public ProjectExplorerItemKind Kind { get; }

    public string DisplayText { get; }

    public string FileName => DisplayText;

    public string IconText { get; }

    /// <summary>
    /// Compact visual glyph used by the Project Explorer tree. IconText is kept as a stable semantic badge for tests and automation.
    /// </summary>
    public string IconGlyph => GetIconGlyph(Kind, IconText);

    /// <summary>
    /// Resource key for the compact vector icon used by the Project Explorer tree.
    /// </summary>
    public string IconKey => GetIconKey(Kind, IconText);

    public string AutomationId => $"Shell.ProjectExplorer.{Kind}.{DisplayText}";

    public string? FilePath { get; }

    public long FileSizeBytes { get; }

    public int? LineNumber { get; }

    public string? SectionId { get; }

    public ObservableCollection<ProjectExplorerItemViewModel> Children { get; } = [];

    public int SectionCount
    {
        get => _sectionCount;
        private set
        {
            if (SetProperty(ref _sectionCount, value))
            {
                OnPropertyChanged(nameof(DisplayTextWithCount));
                OnPropertyChanged(nameof(ToolTipText));
            }
        }
    }

    public string DisplayTextWithCount =>
        Kind is ProjectExplorerItemKind.TypeGroup or ProjectExplorerItemKind.FactionGroup
            ? SectionCount > 0 ? $"{DisplayText} ({SectionCount})" : DisplayText
            : DisplayText;

    public string ToolTipText => Kind switch
    {
        ProjectExplorerItemKind.File => string.IsNullOrWhiteSpace(FilePath) ? DisplayText : FilePath,
        ProjectExplorerItemKind.TypeGroup => SectionCount > 0 ? $"{DisplayText}: {SectionCount} section(s)" : DisplayText,
        ProjectExplorerItemKind.FactionGroup => SectionCount > 0 ? $"{DisplayText}: {SectionCount} section(s)" : DisplayText,
        ProjectExplorerItemKind.Section => LineNumber is > 0 ? $"{DisplayText}{Environment.NewLine}Line {LineNumber}" : DisplayText,
        _ => DisplayText
    };

    public bool CanNavigateToSource => Kind == ProjectExplorerItemKind.Section &&
                                       LineNumber is > 0 &&
                                       !string.IsNullOrWhiteSpace(FilePath);

    public bool IsCurrentFile
    {
        get => _isCurrentFile;
        set => SetProperty(ref _isCurrentFile, value);
    }

    public bool IsCurrentSection
    {
        get => _isCurrentSection;
        set => SetProperty(ref _isCurrentSection, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public ReadonlyIniFileDescriptor ToDescriptor()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new InvalidOperationException("Project Explorer file nodes must have a file path.");

        return new ReadonlyIniFileDescriptor(DisplayText, FilePath, FileSizeBytes);
    }

    public void SetSectionCount(int sectionCount)
    {
        SectionCount = Math.Max(0, sectionCount);
    }

    private static string GetDefaultIconText(ProjectExplorerItemKind kind) => kind switch
    {
        ProjectExplorerItemKind.File => "INI",
        ProjectExplorerItemKind.TypeGroup => "Type",
        ProjectExplorerItemKind.FactionGroup => "Faction",
        ProjectExplorerItemKind.Section => "Sec",
        ProjectExplorerItemKind.Placeholder => "...",
        _ => "?"
    };


    private static string GetIconKey(ProjectExplorerItemKind kind, string iconText)
    {
        if (kind == ProjectExplorerItemKind.File)
            return "Icon.FileIni";

        if (kind == ProjectExplorerItemKind.FactionGroup)
        {
            return iconText switch
            {
                "A" => "Icon.Country.Allied",
                "S" => "Icon.Country.Soviet",
                "Y" => "Icon.Country.Yuri",
                "N" => "Icon.Country.Custom",
                "C" => "Icon.Country.Common",
                "?" => "Icon.Country.Unknown",
                _ => "Icon.Country.Custom"
            };
        }

        return iconText switch
        {
            "INI" => "Icon.FileIni",
            "Reg" => "Icon.Registry",
            "Inf" => "Icon.Infantry",
            "Veh" => "Icon.Vehicle",
            "Air" => "Icon.Aircraft",
            "Bld" => "Icon.Building",
            "Wpn" => "Icon.Weapon",
            "WH" => "Icon.Warhead",
            "Proj" => "Icon.Projectile",
            "Anim" => "Icon.Animation",
            "Vxl" => "Icon.VoxelAnimation",
            "Ptc" => "Icon.Particle",
            "SW" => "Icon.SuperWeapon",
            "AI" => "Icon.AI",
            "Ter" => "Icon.Terrain",
            "?" => "Icon.Section",
            "..." => "Icon.Section",
            _ => "Icon.Section"
        };
    }

    private static string GetIconGlyph(ProjectExplorerItemKind kind, string iconText)
    {
        if (kind == ProjectExplorerItemKind.FactionGroup)
        {
            return iconText switch
            {
                "A" => "★",
                "S" => "◆",
                "Y" => "Ψ",
                "N" => "○",
                "C" => "●",
                _ => "◇"
            };
        }

        return iconText switch
        {
            "INI" => "▤",
            "Reg" => "☷",
            "Inf" => "♟",
            "Veh" => "▰",
            "Air" => "✈",
            "Bld" => "▥",
            "Wpn" => "⚔",
            "WH" => "✹",
            "Proj" => "➤",
            "Anim" => "◌",
            "Vxl" => "◆",
            "Ptc" => "✣",
            "SW" => "☄",
            "AI" => "◇",
            "Ter" => "△",
            "?" => "?",
            "..." => "…",
            _ => "•"
        };
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
