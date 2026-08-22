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

public sealed class Ra2IdeDirtyNavigationSmokeTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void DirtyNavigation_CancelPreservesDirtyTextAndDiscardSwitchesWithoutWriting()
    {
        using TempDirtyNavigationProject tempProject = TempDirtyNavigationProject.Create();
        string originalRulesText = File.ReadAllText(tempProject.RulesPath);
        using FlaUIApplication app = LaunchIde(tempProject.Path);
        using UIA3Automation automation = new();

        try
        {
            Window shell = OpenRulesFile(app, automation, tempProject);
            AutomationElement editor = Find(shell, automation, "Shell.SourceEditor");
            AutomationElement textArea = Find(shell, automation, "Shell.SourceEditor.TextArea");

            TypeIntoEditor(textArea, "\r\n; Dirty navigation cancel branch\r\nStrength=500");
            WaitForEditorTextContains(editor, "Dirty navigation cancel branch");
            Thread.Sleep(250);

            Task cancelSelection = BeginSelectProjectExplorerItem(shell, automation, tempProject.ArtFileName);
            Window cancelDialog = WaitForDirtyNavigationDialog(
                app,
                automation,
                shell,
                editor,
                cancelSelection,
                "cancel branch");
            Click(cancelDialog, automation, "DirtyNavigation.CancelButton");
            WaitForSelectionTask(cancelSelection, "cancel branch artmd.ini selection");

            WaitForEditorTextContains(editor, "Dirty navigation cancel branch");
            Assert.Equal(originalRulesText, File.ReadAllText(tempProject.RulesPath));

            Task discardSelection = BeginSelectProjectExplorerItem(shell, automation, tempProject.ArtFileName);
            Window discardDialog = WaitForDirtyNavigationDialog(
                app,
                automation,
                shell,
                editor,
                discardSelection,
                "discard branch");
            Click(discardDialog, automation, "DirtyNavigation.DiscardButton");
            WaitForSelectionTask(discardSelection, "discard branch artmd.ini selection");

            WaitForEditorTextContains(editor, "Voxel=yes");
            Assert.Equal(originalRulesText, File.ReadAllText(tempProject.RulesPath));

            SelectProjectExplorerItem(shell, automation, tempProject.RulesFileName);
            WaitForEditorTextContains(editor, "Strength=400");
            Assert.DoesNotContain("Dirty navigation cancel branch", TryReadEditorText(editor) ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CloseApp(app);
        }
    }

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void DirtyNavigation_SaveWritesCurrentFileCreatesBackupAndSwitches()
    {
        using TempDirtyNavigationProject tempProject = TempDirtyNavigationProject.Create();
        using FlaUIApplication app = LaunchIde(tempProject.Path);
        using UIA3Automation automation = new();

        try
        {
            Window shell = OpenRulesFile(app, automation, tempProject);
            AutomationElement editor = Find(shell, automation, "Shell.SourceEditor");
            AutomationElement textArea = Find(shell, automation, "Shell.SourceEditor.TextArea");

            TypeIntoEditor(textArea, "\r\n; Dirty navigation save branch\r\nStrength=700");
            WaitForEditorTextContains(editor, "Dirty navigation save branch");
            Thread.Sleep(250);

            Task saveSelection = BeginSelectProjectExplorerItem(shell, automation, tempProject.ArtFileName);
            Window saveDialog = WaitForDirtyNavigationDialog(
                app,
                automation,
                shell,
                editor,
                saveSelection,
                "save branch");
            Click(saveDialog, automation, "DirtyNavigation.SaveButton");
            WaitForSelectionTask(saveSelection, "save branch artmd.ini selection");

            WaitForEditorTextContains(editor, "Voxel=yes");
            string savedRulesText = File.ReadAllText(tempProject.RulesPath);
            Assert.Contains("Dirty navigation save branch", savedRulesText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Strength=700", savedRulesText, StringComparison.OrdinalIgnoreCase);

            string backupPath = WaitForSingleBackupFile(tempProject);
            string backupText = File.ReadAllText(backupPath);
            Assert.Contains("Strength=400", backupText, StringComparison.Ordinal);
            Assert.DoesNotContain("Dirty navigation save branch", backupText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CloseApp(app);
        }
    }

    private static Window OpenRulesFile(
        FlaUIApplication app,
        UIA3Automation automation,
        TempDirtyNavigationProject tempProject)
    {
        Window shell = WaitForWindow(app, automation, "Shell.Window");
        WaitForText(shell, automation, "Shell.OutputTextBox", "2 个 INI 文件");
        SelectProjectExplorerItem(shell, automation, tempProject.RulesFileName);
        WaitForEditorTextContains(Find(shell, automation, "Shell.SourceEditor"), "Strength=400");
        return shell;
    }

    private static FlaUIApplication LaunchIde(string projectPath)
        => FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = ResolveIdeExePath(),
            Arguments = $"--automation-open-folder \"{projectPath}\"",
            UseShellExecute = false
        });

    private static Window WaitForDirtyNavigationDialog(
        FlaUIApplication app,
        UIA3Automation automation,
        Window shell,
        AutomationElement editor,
        Task selectionTask,
        string branchName)
    {
        try
        {
            return WaitForWindow(app, automation, "DirtyNavigation.Dialog");
        }
        catch (Exception ex)
        {
            string diagnostics = CollectDirtyNavigationDiagnostics(
                app,
                automation,
                shell,
                editor,
                selectionTask,
                branchName);
            throw new InvalidOperationException(
                $"Dirty navigation dialog did not appear for {branchName}.{Environment.NewLine}{diagnostics}",
                ex);
        }
    }

    private static string CollectDirtyNavigationDiagnostics(
        FlaUIApplication app,
        UIA3Automation automation,
        Window shell,
        AutomationElement editor,
        Task selectionTask,
        string branchName)
    {
        string editorText = TryReadEditorText(editor) ?? "<unavailable>";
        string editorSnippet = editorText.Length > 500 ? editorText[..500] : editorText;
        string projectExplorerTree = TryFormatAutomationTree(shell, automation, "Shell.ProjectExplorer", 80);

        return string.Join(
            Environment.NewLine,
            $"Branch: {branchName}",
            $"AppExited: {app.HasExited}",
            $"SelectionTaskStatus: {selectionTask.Status}",
            $"SelectionTaskException: {selectionTask.Exception?.GetBaseException().Message ?? "<none>"}",
            $"ShellName: {SafeName(shell)}",
            $"EditorStateText: {TryReadElementName(shell, automation, "Shell.SourceEditor.EditorStateText")}",
            $"SaveHintText: {TryReadElementName(shell, automation, "Shell.SourceEditor.SaveHintText")}",
            $"OutputText: {TryReadElementName(shell, automation, "Shell.OutputTextBox")}",
            $"EditorTextSnippet: {editorSnippet.Replace(Environment.NewLine, "\\n")}",
            $"ProjectExplorerTree: {projectExplorerTree}",
            $"TopLevelWindows: {FormatTopLevelWindows(app, automation)}");
    }

    private static string WaitForSingleBackupFile(TempDirtyNavigationProject tempProject)
    {
        bool reached = Retry.WhileFalse(
            () => CountBackupFiles(tempProject) == 1,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (!reached)
            throw new InvalidOperationException($"Expected exactly one backup file under '{tempProject.BackupRootPath}', found {CountBackupFiles(tempProject)}.");

        return Directory.EnumerateFiles(tempProject.BackupRootPath, tempProject.RulesFileName, SearchOption.AllDirectories).Single();
    }

    private static int CountBackupFiles(TempDirtyNavigationProject tempProject)
        => Directory.Exists(tempProject.BackupRootPath)
            ? Directory.EnumerateFiles(tempProject.BackupRootPath, tempProject.RulesFileName, SearchOption.AllDirectories).Count()
            : 0;

    private static void SelectProjectExplorerItem(Window shell, UIA3Automation automation, string expectedName)
    {
        Task selectionTask = BeginSelectProjectExplorerItem(shell, automation, expectedName);
        WaitForSelectionTask(selectionTask, $"{expectedName} selection");
    }

    private static Task BeginSelectProjectExplorerItem(Window shell, UIA3Automation automation, string expectedName)
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

        return Task.Run(() => SelectTreeItemOrClick(item, expectedName));
    }

    private static void WaitForSelectionTask(Task selectionTask, string description)
    {
        if (!selectionTask.Wait(TimeSpan.FromSeconds(5)))
            throw new InvalidOperationException($"Timed out waiting for {description} to complete.");

        if (selectionTask.Exception is not null)
            throw selectionTask.Exception.GetBaseException();
    }

    private static void SelectTreeItemOrClick(AutomationElement item, string expectedName)
    {
        try
        {
            if (item.Patterns.SelectionItem.IsSupported)
            {
                item.Patterns.SelectionItem.Pattern.Select();
                Thread.Sleep(100);
                return;
            }
        }
        catch
        {
            // Fall back to clicking the visible TreeItem area.
        }

        ClickTreeItemCenterOrText(item, expectedName);
    }

    private static void ClickTreeItemCenterOrText(AutomationElement item, string expectedName)
    {
        System.Drawing.Rectangle bounds = item.BoundingRectangle;
        if (bounds.Width > 0 && bounds.Height > 0)
        {
            Mouse.Click(new System.Drawing.Point(bounds.Left + Math.Min(bounds.Width / 2, 120), bounds.Top + bounds.Height / 2));
            Thread.Sleep(100);
            return;
        }

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
            TryFindFirstDescendant(candidate, cf.ByAutomationId(automationId)) is not null ||
            string.Equals(SafeAutomationId(candidate), automationId, StringComparison.Ordinal));
        if (appWindow is not null)
            return appWindow;

        try
        {
            return automation.GetDesktop()
                .FindAllChildren(cf.ByControlType(ControlType.Window))
                .Select(element => element.AsWindow())
                .FirstOrDefault(window => string.Equals(SafeAutomationId(window), automationId, StringComparison.Ordinal));
        }
        catch
        {
            return null;
        }
    }

    private static AutomationElement? TryFindFirstDescendant(AutomationElement root, FlaUI.Core.Conditions.ConditionBase condition)
    {
        try
        {
            return root.FindFirstDescendant(condition);
        }
        catch
        {
            return null;
        }
    }

    private static AutomationElement? TryFind(AutomationElement root, UIA3Automation automation, string automationId)
    {
        try
        {
            return root.FindFirstDescendant(automation.ConditionFactory.ByAutomationId(automationId));
        }
        catch
        {
            return null;
        }
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

    private static void WaitForEditorTextContains(AutomationElement editor, string expectedText)
    {
        bool reached = Retry.WhileFalse(
            () => (TryReadEditorText(editor) ?? string.Empty).Contains(expectedText, StringComparison.OrdinalIgnoreCase),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (!reached)
            throw new InvalidOperationException($"Timed out waiting for editor text to contain '{expectedText}'. Actual: '{TryReadEditorText(editor) ?? "<unavailable>"}'.");
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
        try
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
        catch (Exception ex)
        {
            return $"<top-level window scan failed: {ex.Message}>";
        }
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

    private static string TryFormatAutomationTree(
        AutomationElement root,
        UIA3Automation automation,
        string automationId,
        int maxNodes)
    {
        AutomationElement? element = TryFind(root, automation, automationId);
        if (element is null)
            return $"<{automationId} unavailable>";

        try
        {
            return FormatAutomationTree(element, maxNodes);
        }
        catch (Exception ex)
        {
            return $"<{automationId} tree unavailable: {ex.Message}>";
        }
    }

    private static string TryReadElementName(AutomationElement root, UIA3Automation automation, string automationId)
    {
        AutomationElement? element = TryFind(root, automation, automationId);
        return element is null ? $"<{automationId} unavailable>" : SafeName(element);
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

    private sealed class TempDirtyNavigationProject : IDisposable
    {
        private TempDirtyNavigationProject(string path)
        {
            Path = path;
            RulesFileName = "rulesmd.ini";
            ArtFileName = "artmd.ini";
            RulesPath = System.IO.Path.Combine(path, RulesFileName);
            ArtPath = System.IO.Path.Combine(path, ArtFileName);
            BackupRootPath = System.IO.Path.Combine(path, "backup");
        }

        public string Path { get; }

        public string RulesFileName { get; }

        public string ArtFileName { get; }

        public string RulesPath { get; }

        public string ArtPath { get; }

        public string BackupRootPath { get; }

        public static TempDirtyNavigationProject Create()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RA2IniEditor_DirtyNavigationSmoke_" + Guid.NewGuid().ToString("N"));
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
                """);
            File.WriteAllText(
                System.IO.Path.Combine(path, "artmd.ini"),
                """
                [HTNK]
                Image=HTNK
                Voxel=yes
                """);

            return new TempDirtyNavigationProject(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
