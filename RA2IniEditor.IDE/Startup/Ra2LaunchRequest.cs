namespace RA2IniEditor.IDE.Startup;

internal enum Ra2LaunchTargetKind
{
    None = 0,
    ProjectFolder,
    IniFile,
    Invalid
}

/// <summary>一次进程启动的有界打开请求；不序列化、不持久化。</summary>
internal sealed record Ra2LaunchRequest
{
    private Ra2LaunchRequest(
        Ra2LaunchTargetKind kind,
        string? projectFolderPath,
        string? targetFilePath,
        string? errorMessage)
    {
        Kind = kind;
        ProjectFolderPath = projectFolderPath;
        TargetFilePath = targetFilePath;
        ErrorMessage = errorMessage;
    }

    public Ra2LaunchTargetKind Kind { get; }

    public string? ProjectFolderPath { get; }

    public string? TargetFilePath { get; }

    public string? ErrorMessage { get; }

    public static Ra2LaunchRequest None()
        => new(Ra2LaunchTargetKind.None, null, null, null);

    public static Ra2LaunchRequest ProjectFolder(string folderPath)
        => new(
            Ra2LaunchTargetKind.ProjectFolder,
            folderPath ?? throw new ArgumentNullException(nameof(folderPath)),
            null,
            null);

    public static Ra2LaunchRequest IniFile(string projectFolderPath, string filePath)
        => new(
            Ra2LaunchTargetKind.IniFile,
            projectFolderPath ?? throw new ArgumentNullException(nameof(projectFolderPath)),
            filePath ?? throw new ArgumentNullException(nameof(filePath)),
            null);

    public static Ra2LaunchRequest Invalid(string message)
        => new(
            Ra2LaunchTargetKind.Invalid,
            null,
            null,
            string.IsNullOrWhiteSpace(message) ? "启动参数无效。" : message.Trim());
}
