using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Provides readonly project tree state for INI files and current-file sections.
/// </summary>
public sealed class ProjectExplorerViewModel : INotifyPropertyChanged
{
    private ProjectExplorerItemViewModel? _selectedItem;
    private string _statusText = "No project opened.";

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ProjectExplorerItemViewModel> Items { get; } = [];

    public ProjectExplorerItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public void Clear()
    {
        Items.Clear();
        SelectedItem = null;
        StatusText = "No project opened.";
    }

    public void ShowFiles(IEnumerable<ReadonlyIniFileDescriptor> files)
    {
        Items.Clear();
        SelectedItem = null;

        foreach (ReadonlyIniFileDescriptor file in files)
            Items.Add(new ProjectExplorerItemViewModel(ProjectExplorerItemKind.File, file.FileName, file.FilePath, file.FileSizeBytes));

        StatusText = Items.Count == 0 ? "No INI files found." : $"{Items.Count} INI file(s)";
    }

    public void ShowGroupedSectionsForCurrentFile(string filePath, IEnumerable<ReadonlySectionClassificationResult> sections)
    {
        ProjectExplorerItemViewModel? fileItem = FindFileItem(filePath);
        if (fileItem is null)
            return;

        IReadOnlyList<ReadonlySectionClassificationResult> sectionList = sections.ToList();
        fileItem.Children.Clear();
        foreach (IGrouping<string, ReadonlySectionClassificationResult> typeGroup in sectionList.GroupBy(section => section.TypeGroup).OrderBy(group => GetTypeGroupOrder(group.Key)))
        {
            ProjectExplorerItemViewModel typeNode = new(
                ProjectExplorerItemKind.TypeGroup,
                typeGroup.Key,
                filePath,
                iconText: GetTypeIconText(typeGroup.Key));
            typeNode.SetSectionCount(typeGroup.Count());
            fileItem.Children.Add(typeNode);

            var factionGroups = typeGroup
                .GroupBy(section => section.FactionGroup)
                .OrderBy(group => GetFactionGroupOrder(group.Key));
            foreach (IGrouping<string?, ReadonlySectionClassificationResult> factionGroup in factionGroups)
            {
                if (string.IsNullOrWhiteSpace(factionGroup.Key))
                {
                    AddSectionNodes(typeNode, filePath, factionGroup, typeGroup.Key);
                    continue;
                }

                ProjectExplorerItemViewModel factionNode = new(
                    ProjectExplorerItemKind.FactionGroup,
                    factionGroup.Key,
                    filePath,
                    iconText: GetFactionIconText(factionGroup.Key));
                factionNode.SetSectionCount(factionGroup.Count());
                typeNode.Children.Add(factionNode);
                AddSectionNodes(factionNode, filePath, factionGroup, typeGroup.Key);
            }
        }

        fileItem.IsExpanded = true;
        int sectionCount = sectionList.Count;
        StatusText = sectionCount == 1 ? "1 section in current file" : $"{sectionCount} sections in current file";
    }

    public void ShowPlaceholderForCurrentFile(string filePath, string message)
    {
        ProjectExplorerItemViewModel? fileItem = FindFileItem(filePath);
        if (fileItem is null)
            return;

        fileItem.Children.Clear();
        fileItem.Children.Add(new ProjectExplorerItemViewModel(
            ProjectExplorerItemKind.Placeholder,
            message,
            filePath));
        fileItem.IsExpanded = true;
        StatusText = message;
    }

    public void ClearAllSectionNodes()
    {
        foreach (ProjectExplorerItemViewModel item in Items)
            item.Children.Clear();
    }

    public void MarkCurrentFile(string filePath)
    {
        foreach (ProjectExplorerItemViewModel item in Items)
        {
            item.IsCurrentFile = item.Kind == ProjectExplorerItemKind.File &&
                                 string.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase);
        }

        ClearCurrentSection();
        ProjectExplorerItemViewModel? fileItem = FindFileItem(filePath);
        if (fileItem is not null)
            fileItem.IsExpanded = true;
    }

    public void MarkCurrentSection(string filePath, string sectionId)
    {
        ProjectExplorerItemViewModel? matchingSection = null;
        foreach (ProjectExplorerItemViewModel fileItem in Items)
        {
            foreach (ProjectExplorerItemViewModel sectionNode in EnumerateDescendants(fileItem))
            {
                if (sectionNode.Kind != ProjectExplorerItemKind.Section)
                    continue;

                bool isMatch = string.Equals(sectionNode.FilePath, filePath, StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(sectionNode.SectionId, sectionId, StringComparison.OrdinalIgnoreCase);
                if (isMatch)
                    matchingSection = sectionNode;
            }
        }

        if (matchingSection is null)
            return;

        ClearCurrentSection();
        matchingSection.IsCurrentSection = true;
        ExpandAncestors(matchingSection);
    }

    private void ClearCurrentSection()
    {
        foreach (ProjectExplorerItemViewModel item in Items)
        {
            item.IsCurrentSection = false;
            foreach (ProjectExplorerItemViewModel descendant in EnumerateDescendants(item))
                descendant.IsCurrentSection = false;
        }
    }

    private static void AddSectionNodes(
        ProjectExplorerItemViewModel parent,
        string filePath,
        IEnumerable<ReadonlySectionClassificationResult> sections,
        string? typeGroup = null)
    {
        foreach (ReadonlySectionClassificationResult section in sections.OrderBy(section => section.LineNumber))
        {
            string displayText = string.IsNullOrWhiteSpace(section.DisplayName)
                ? $"[{section.SectionId}]"
                : $"[{section.SectionId}]  {section.DisplayName}";
            parent.Children.Add(new ProjectExplorerItemViewModel(
                ProjectExplorerItemKind.Section,
                displayText,
                filePath,
                lineNumber: section.LineNumber,
                sectionId: section.SectionId,
                iconText: GetTypeIconText(typeGroup ?? section.TypeGroup)));
        }
    }

    private static string GetTypeIconText(string? typeGroup) => typeGroup switch
    {
        "Global / Registry" => "Reg",
        "Infantry" => "Inf",
        "Vehicle" => "Veh",
        "Aircraft" => "Air",
        "Building" => "Bld",
        "Weapon" => "Wpn",
        "Warhead" => "WH",
        "Projectile" => "Proj",
        "Animation" => "Anim",
        "VoxelAnim" => "Vxl",
        "Particle" => "Ptc",
        "SuperWeapon" => "SW",
        "AI" => "AI",
        "Terrain / Overlay" => "Ter",
        "Unknown" => "?",
        _ => "Sec"
    };

    private static string GetFactionIconText(string? factionGroup) => factionGroup switch
    {
        "Allied" => "A",
        "Soviet" => "S",
        "Yuri" => "Y",
        "Neutral" => "N",
        "Common" => "C",
        "Unknown" => "?",
        _ => "?"
    };

    private static int GetTypeGroupOrder(string typeGroup) => typeGroup switch
    {
        "Global / Registry" => 0,
        "Infantry" => 10,
        "Vehicle" => 20,
        "Aircraft" => 30,
        "Building" => 40,
        "Weapon" => 50,
        "Warhead" => 60,
        "Projectile" => 70,
        "Animation" => 80,
        "VoxelAnim" => 90,
        "Particle" => 100,
        "SuperWeapon" => 110,
        "AI" => 120,
        "Terrain / Overlay" => 130,
        "Unknown" => 900,
        _ => 800
    };

    private static int GetFactionGroupOrder(string? factionGroup) => factionGroup switch
    {
        null => 0,
        "Allied" => 10,
        "Soviet" => 20,
        "Yuri" => 30,
        "Neutral" => 40,
        "Common" => 50,
        "Unknown" => 60,
        _ => 70
    };

    private ProjectExplorerItemViewModel? FindFileItem(string filePath)
    {
        return Items.FirstOrDefault(item =>
            item.Kind == ProjectExplorerItemKind.File &&
            string.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    }

    private void ExpandAncestors(ProjectExplorerItemViewModel target)
    {
        foreach (ProjectExplorerItemViewModel fileItem in Items)
        {
            if (TryExpandAncestors(fileItem, target))
                return;
        }
    }

    private static bool TryExpandAncestors(ProjectExplorerItemViewModel current, ProjectExplorerItemViewModel target)
    {
        if (ReferenceEquals(current, target))
            return true;

        foreach (ProjectExplorerItemViewModel child in current.Children)
        {
            if (!TryExpandAncestors(child, target))
                continue;

            current.IsExpanded = true;
            return true;
        }

        return false;
    }

    private static IEnumerable<ProjectExplorerItemViewModel> EnumerateDescendants(ProjectExplorerItemViewModel item)
    {
        foreach (ProjectExplorerItemViewModel child in item.Children)
        {
            yield return child;
            foreach (ProjectExplorerItemViewModel descendant in EnumerateDescendants(child))
                yield return descendant;
        }
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
