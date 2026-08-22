using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using WinForms = System.Windows.Forms;
using Xunit;
using FlaUIApplication = FlaUI.Core.Application;

namespace RA2IniEditor.UiAutomationTests;

public sealed class Ra2IdeSaveSmokeTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void SaveButton_DirtyFileWritesBackupClearsDirtyAndUpdatesRevertBaseline()
    {
        using TempSaveProject tempProject = TempSaveProject.Create();
        using FlaUIApplication app = LaunchIde(tempProject.Path);
        using UIA3Automation automation = new();

        try
        {
            Window shell = OpenRulesFile(app, automation, tempProject);
            AutomationElement editor = Find(shell, automation, "Shell.SourceEditor");
            AutomationElement textArea = Find(shell, automation, "Shell.SourceEditor.TextArea");
            TypeIntoEditor(textArea, "\r\n; Saved via button\r\nstrength=500");
            WaitForText(shell, automation, "Shell.SourceEditor.EditorStateText", "内存中已修改");

            Click(shell, automation, "Shell.SourceEditor.SaveCurrentFileButton");
            WaitForText(shell, automation, "Shell.OutputTextBox", "保存当前文件成功");
            WaitForText(shell, automation, "Shell.SourceEditor.EditorStateText", "已保存");

            Assert.Contains("strength=500", File.ReadAllText(tempProject.RulesPath), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("saved via button", File.ReadAllText(tempProject.RulesPath), StringComparison.OrdinalIgnoreCase);
            string backupPath = WaitForSingleBackupFile(tempProject);
            Assert.Contains("Strength=400", File.ReadAllText(backupPath), StringComparison.Ordinal);

            int backupCountAfterSave = CountBackupFiles(tempProject);
            string savedText = File.ReadAllText(tempProject.RulesPath);
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_S);
            WaitForText(shell, automation, "Shell.OutputTextBox", "没有未保存");
            Assert.Equal(savedText, File.ReadAllText(tempProject.RulesPath));
            Assert.Equal(backupCountAfterSave, CountBackupFiles(tempProject));

            Click(shell, automation, "Shell.SourceEditor.RevertInMemoryChangesButton");
            WaitForDisabled(shell, automation, "Shell.SourceEditor.RevertInMemoryChangesButton");
            Assert.Equal(savedText, File.ReadAllText(tempProject.RulesPath));
            Assert.Contains("saved via button", TryReadEditorText(editor) ?? savedText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CloseApp(app);
        }
    }

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void CtrlS_SavesDirtyFileAndReadonlyCtrlSDoesNotWrite()
    {
        using TempSaveProject tempProject = TempSaveProject.Create();
        using FlaUIApplication app = LaunchIde(tempProject.Path);
        using UIA3Automation automation = new();

        try
        {
            Window shell = OpenRulesFile(app, automation, tempProject);
            string originalText = File.ReadAllText(tempProject.RulesPath);

            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_S);
            WaitForText(shell, automation, "Shell.OutputTextBox", "没有未保存");
            Assert.Equal(originalText, File.ReadAllText(tempProject.RulesPath));
            Assert.Equal(0, CountBackupFiles(tempProject));

            AutomationElement editor = Find(shell, automation, "Shell.SourceEditor");
            AutomationElement textArea = Find(shell, automation, "Shell.SourceEditor.TextArea");
            TypeIntoEditor(textArea, "\r\n; Saved via Ctrl+S\r\nstrength=600");
            WaitForText(shell, automation, "Shell.SourceEditor.EditorStateText", "内存中已修改");

            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_S);
            WaitForText(shell, automation, "Shell.OutputTextBox", "保存当前文件成功");
            WaitForText(shell, automation, "Shell.SourceEditor.EditorStateText", "已保存");

            string savedText = File.ReadAllText(tempProject.RulesPath);
            Assert.Contains("strength=600", savedText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("saved via Ctrl+S", savedText, StringComparison.OrdinalIgnoreCase);
            string backupPath = WaitForSingleBackupFile(tempProject);
            Assert.Contains("Strength=400", File.ReadAllText(backupPath), StringComparison.Ordinal);
        }
        finally
        {
            CloseApp(app);
        }
    }

    private static Window OpenRulesFile(
        FlaUIApplication app,
        UIA3Automation automation,
        TempSaveProject tempProject)
    {
        Window shell = WaitForWindow(app, automation, "Shell.Window");
        WaitForText(shell, automation, "Shell.OutputTextBox", "1 个 INI 文件");
        SelectProjectExplorerItem(shell, automation, tempProject.RulesFileName);
        WaitForEnabled(shell, automation, "Shell.SourceEditor.SaveCurrentFileButton");
        return shell;
    }

    private static FlaUIApplication LaunchIde(string projectPath)
        => FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = ResolveIdeExePath(),
            Arguments = $"--automation-open-folder \"{projectPath}\"",
            UseShellExecute = false
        });

    private static string WaitForSingleBackupFile(TempSaveProject tempProject)
    {
        bool reached = Retry.WhileFalse(
            () => CountBackupFiles(tempProject) == 1,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (!reached)
            throw new InvalidOperationException($"Expected exactly one backup file under '{tempProject.BackupRootPath}', found {CountBackupFiles(tempProject)}.");

        return Directory.EnumerateFiles(tempProject.BackupRootPath, "rulesmd.ini", SearchOption.AllDirectories).Single();
    }

    private static int CountBackupFiles(TempSaveProject tempProject)
        => Directory.Exists(tempProject.BackupRootPath)
            ? Directory.EnumerateFiles(tempProject.BackupRootPath, "rulesmd.ini", SearchOption.AllDirectories).Count()
            : 0;

    private static void SelectProjectExplorerItem(Window shell, UIA3Automation automation, string expectedName)
    {
        AutomationElement projectExplorer = Find(shell, automation, "Shell.ProjectExplorer");
        string fileAutomationId = $"Shell.ProjectExplorer.File.{expectedName}";
        AutomationElement? item = Retry.WhileNull(
            () => projectExplorer.FindFirstDescendant(automation.ConditionFactory.ByAutomationId(fileAutomationId)) ??
                  projectExplorer
                      .FindAllDescendants(automation.ConditionFactory.ByControlType(ControlType.TreeItem))
                      .FirstOrDefault(element => SafeNameContains(element, expectedName) ||
                                                 element.FindAllDescendants()
                                                     .Any(child => SafeNameContains(child, expectedName))),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (item is null)
            throw new InvalidOperationException(
                $"Could not find Project Explorer item '{expectedName}'. Tree: {FormatAutomationTree(projectExplorer, maxNodes: 80)}");

        AutomationElement? text = item.FindAllDescendants()
            .FirstOrDefault(child => SafeNameContains(child, expectedName));
        (text ?? item).Click();
    }

    private static void TypeIntoEditor(AutomationElement editor, string text)
    {
        WinForms.IDataObject? previousClipboard = GetClipboardDataObject();
        try
        {
            ClickInsideEditor(editor);
            SetClipboardText(text);
            Thread.Sleep(100);
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.END);
            Thread.Sleep(100);
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_V);
            Thread.Sleep(100);
        }
        finally
        {
            RestoreClipboard(previousClipboard);
        }
    }

    private static void ClickInsideEditor(AutomationElement editor)
    {
        System.Drawing.Rectangle bounds = editor.BoundingRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            editor.Click();
            return;
        }

        int x = bounds.Left + Math.Clamp(bounds.Width / 4, 12, 120);
        int y = bounds.Top + Math.Clamp(bounds.Height / 4, 12, 60);
        Mouse.Click(new System.Drawing.Point(x, y));
        Thread.Sleep(100);
    }

    private static WinForms.IDataObject? GetClipboardDataObject()
        => RunSta(() =>
        {
            try
            {
                return WinForms.Clipboard.GetDataObject();
            }
            catch
            {
                return null;
            }
        });

    private static void SetClipboardText(string text)
        => RunSta(() => WinForms.Clipboard.SetText(text));

    private static void RestoreClipboard(WinForms.IDataObject? dataObject)
        => RunSta(() =>
        {
            try
            {
                if (dataObject is null)
                    WinForms.Clipboard.Clear();
                else
                    WinForms.Clipboard.SetDataObject(dataObject, copy: true);
            }
            catch
            {
                // Clipboard restoration is best effort; it must not hide the UIA smoke failure.
            }
        });

    private static T RunSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? exception = null;
        Thread thread = new(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception is not null)
            throw exception;

        return result!;
    }

    private static void RunSta(Action action)
        => RunSta(() =>
        {
            action();
            return true;
        });

    private static AutomationElement Find(AutomationElement root, UIA3Automation automation, string automationId)
    {
        ConditionFactory cf = automation.ConditionFactory;
        AutomationElement? element = Retry.WhileNull(
            () => root.FindFirstDescendant(cf.ByAutomationId(automationId)),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        return element ?? throw new InvalidOperationException(
            $"Could not find AutomationId '{automationId}'. Tree: {FormatAutomationTree(root, maxNodes: 80)}");
    }

    private static Window WaitForWindow(FlaUIApplication app, UIA3Automation automation, string automationId)
        => Retry.WhileNull(
            () => TryFindWindow(app, automation, automationId),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result ??
           throw new InvalidOperationException($"Could not find top-level window '{automationId}'. AppExited={app.HasExited}. Windows: {FormatTopLevelWindows(app, automation)}");

    private static Window? TryFindWindow(FlaUIApplication app, UIA3Automation automation, string automationId)
    {
        ConditionFactory cf = automation.ConditionFactory;
        Window? appWindow = app.GetAllTopLevelWindows(automation).FirstOrDefault(candidate =>
            candidate.FindFirstDescendant(cf.ByAutomationId(automationId)) is not null ||
            string.Equals(SafeAutomationId(candidate), automationId, StringComparison.Ordinal));
        if (appWindow is not null)
            return appWindow;

        return automation.GetDesktop()
            .FindAllChildren(cf.ByControlType(ControlType.Window))
            .Select(element => element.AsWindow())
            .FirstOrDefault(window => string.Equals(SafeAutomationId(window), automationId, StringComparison.Ordinal));
    }

    private static void Click(AutomationElement root, UIA3Automation automation, string automationId)
        => Find(root, automation, automationId).AsButton().Invoke();

    private static string WaitForText(Window window, UIA3Automation automation, string automationId, string expectedText)
    {
        AutomationElement element = Find(window, automation, automationId);
        bool reached = Retry.WhileFalse(
            () => SafeNameContains(element, expectedText),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (!reached)
            throw new InvalidOperationException($"Timed out waiting for '{automationId}' to contain '{expectedText}'. Actual: '{SafeName(element)}'.");

        return element.Name;
    }

    private static void WaitForEnabled(Window window, UIA3Automation automation, string automationId)
    {
        AutomationElement element = Find(window, automation, automationId);
        bool reached = Retry.WhileFalse(
            () => element.IsEnabled,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (!reached)
            throw new InvalidOperationException($"Timed out waiting for '{automationId}' to become enabled.");
    }

    private static void WaitForDisabled(Window window, UIA3Automation automation, string automationId)
    {
        AutomationElement element = Find(window, automation, automationId);
        bool reached = Retry.WhileFalse(
            () => !element.IsEnabled,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (!reached)
            throw new InvalidOperationException($"Timed out waiting for '{automationId}' to become disabled.");
    }

    private static string? TryReadEditorText(AutomationElement editor)
    {
        try
        {
            if (!editor.Patterns.Text.IsSupported)
                return null;

            return editor.Patterns.Text.Pattern.DocumentRange.GetText(-1);
        }
        catch
        {
            return null;
        }
    }

    private static string FormatTopLevelWindows(FlaUIApplication app, UIA3Automation automation)
    {
        string[] appWindowNames = app.GetAllTopLevelWindows(automation)
            .Select(window => $"AppWindow='{SafeName(window)}' AutomationId='{SafeAutomationId(window)}'")
            .ToArray();
        string[] desktopWindowNames = automation.GetDesktop()
            .FindAllChildren(automation.ConditionFactory.ByControlType(ControlType.Window))
            .Select(element => $"DesktopWindow='{SafeName(element)}' AutomationId='{SafeAutomationId(element)}'")
            .ToArray();
        return string.Join("; ", appWindowNames.Concat(desktopWindowNames));
    }

    private static string FormatAutomationTree(AutomationElement root, int maxNodes)
    {
        List<string> nodes = new();
        Queue<(AutomationElement Element, int Depth)> queue = new();
        queue.Enqueue((root, 0));
        while (queue.Count > 0 && nodes.Count < maxNodes)
        {
            (AutomationElement element, int depth) = queue.Dequeue();
            nodes.Add($"{new string(' ', depth * 2)}{element.ControlType}:{SafeName(element)}#{SafeAutomationId(element)}");
            foreach (AutomationElement child in element.FindAllChildren())
                queue.Enqueue((child, depth + 1));
        }

        return string.Join(" | ", nodes);
    }

    private static bool SafeNameContains(AutomationElement element, string value)
    {
        try
        {
            return element.Name.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string SafeName(AutomationElement element)
    {
        try
        {
            return element.Name;
        }
        catch
        {
            return "<unsupported>";
        }
    }

    private static string SafeAutomationId(AutomationElement element)
    {
        try
        {
            return element.AutomationId;
        }
        catch
        {
            return "<unsupported>";
        }
    }

    private static string ResolveIdeExePath()
    {
        string root = FindRepositoryRoot();
        string exePath = Path.Combine(root, "RA2IniEditor.IDE", "bin", "Release", "net8.0-windows", "RA2IniEditor.IDE.exe");
        if (!File.Exists(exePath))
            throw new FileNotFoundException("RA2IniEditor.IDE.exe was not found. Build the IDE project in Release before running UI automation.", exePath);

        return exePath;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RA2IniEditor.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate RA2IniEditor.sln from test output directory.");
    }

    private static void CloseApp(FlaUIApplication app)
    {
        if (!app.HasExited)
            app.Close();

        if (!app.HasExited)
            app.Kill();
    }

    private sealed class TempSaveProject : IDisposable
    {
        private TempSaveProject(string path)
        {
            Path = path;
            RulesFileName = "rulesmd.ini";
            RulesPath = System.IO.Path.Combine(path, RulesFileName);
            BackupRootPath = System.IO.Path.Combine(path, "backup");
        }

        public string Path { get; }

        public string RulesFileName { get; }

        public string RulesPath { get; }

        public string BackupRootPath { get; }

        public static TempSaveProject Create()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RA2IniEditor_SaveSmoke_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            File.WriteAllText(
                System.IO.Path.Combine(path, "rulesmd.ini"),
                """
                [VehicleTypes]
                0=HTNK

                [HTNK]
                Name=HTNK
                Strength=400
                Armor=heavy
                Primary=120mm

                [120mm]
                Damage=90
                Projectile=Cannon
                Warhead=AP

                [Cannon]
                Image=120MM

                [AP]
                Verses=100%,100%,100%
                """);

            return new TempSaveProject(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
