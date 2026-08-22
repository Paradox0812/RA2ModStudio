namespace RA2IniEditor.IDE.Models;

/// <summary>
/// Represents the readonly result of opening an INI project folder.
/// </summary>
public sealed class ProjectOpenResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectOpenResult"/> class.
    /// </summary>
    public ProjectOpenResult(string projectFolderPath, IReadOnlyList<ReadonlyIniFileDescriptor> files)
    {
        ProjectFolderPath = projectFolderPath;
        Files = files;
    }

    /// <summary>
    /// Gets the opened project folder path.
    /// </summary>
    public string ProjectFolderPath { get; }

    /// <summary>
    /// Gets readonly INI file descriptors discovered in the root folder.
    /// </summary>
    public IReadOnlyList<ReadonlyIniFileDescriptor> Files { get; }

    /// <summary>
    /// Gets the total number of INI files discovered in the root folder.
    /// </summary>
    public int TotalIniFileCount => Files.Count;

    /// <summary>
    /// Gets a value indicating whether no INI files were discovered.
    /// </summary>
    public bool IsEmpty => TotalIniFileCount == 0;
}
