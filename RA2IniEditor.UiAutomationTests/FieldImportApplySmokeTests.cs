using System.Diagnostics;
using FlaUI.Core;
using System.Runtime.InteropServices;
using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Xunit;
using FlaUIButton = FlaUI.Core.AutomationElements.Button;
using FlaUIComboBox = FlaUI.Core.AutomationElements.ComboBox;
using FlaUITextBox = FlaUI.Core.AutomationElements.TextBox;
using FlaUIApplication = FlaUI.Core.Application;

namespace RA2IniEditor.UiAutomationTests;

public sealed class FieldImportApplySmokeTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void FieldImportApply_ProjectTarget_WritesActivePackAndManifest()
    {
        using TempProject tempProject = TempProject.Create(registryAfterObject: false);
        string importedKey = CreateSmokeFieldKey("MyImportedSmokeKey");
        string exePath = ResolveIdeExePath();
        using FlaUIApplication app = FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"--automation-open-folder \"{tempProject.Path}\"",
            UseShellExecute = false
        });
        using UIA3Automation automation = new();

        try
        {
            Window shell = WaitForWindow(app, automation, "Shell.Window");
            WaitForText(shell, automation, "Shell.OutputTextBox", "Found 1 INI file");
            Click(shell, automation, "Shell.MainToolbar.FieldRegistryButton");

            Window manager = WaitForWindow(app, automation, "FieldRegistryManager.Window");
            Click(manager, automation, "FieldRegistryManager.OpenFieldImportPreviewButton");

            Window preview = WaitForWindow(app, automation, "FieldImportPreview.Window");
            FlaUITextBox rawTextBox = Find(preview, automation, "FieldImportPreview.RawTextBox").AsTextBox();
            rawTextBox.Text = $$"""
                | Key | AppliesTo | Type | Description |
                | --- | --- | --- | --- |
                | {{importedKey}} | Infantry | Text | UI automation smoke imported field |
                """;

            Click(preview, automation, "FieldImportPreview.ParsePreviewButton");
            SelectComboBoxItem(preview, automation, "FieldImportPreview.TargetScopeComboBox", "Project");
            SelectComboBoxItem(preview, automation, "FieldImportPreview.ApplyModeComboBox", "AppendOrUpdate");
            Click(preview, automation, "FieldImportPreview.BuildApplyPlanButton");

            FlaUIButton applyButton = Find(preview, automation, "FieldImportPreview.ApplyButton").AsButton();
            WaitForApplyButtonEnabled(preview, automation, applyButton);
            applyButton.Invoke();
            ConfirmMessageBox(app, automation);

            string applyStatus = WaitForText(preview, automation, "FieldImportPreview.ApplyStatusText", "Apply completed");
            Assert.Contains("Apply completed", applyStatus, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(tempProject.ActivePackPath), $"Expected active pack at {tempProject.ActivePackPath}");
            Assert.True(Directory.Exists(tempProject.BackupsPath), $"Expected backup directory at {tempProject.BackupsPath}");
            Assert.Contains(Directory.EnumerateFiles(tempProject.BackupsPath, "manifest.json", SearchOption.AllDirectories), File.Exists);

            string managerStatus = WaitForText(manager, automation, "FieldRegistryManager.StatusText", "local field");
            Assert.Contains("local field", managerStatus, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!app.HasExited)
                app.Close();
        }
    }

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void FieldImportApply_ProjectTargetWithForwardRegistry_WritesActivePack()
    {
        using TempProject tempProject = TempProject.Create(registryAfterObject: true);
        string importedKey = CreateSmokeFieldKey("MyImportedSmokeKey");
        string exePath = ResolveIdeExePath();
        using FlaUIApplication app = FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"--automation-open-folder \"{tempProject.Path}\"",
            UseShellExecute = false
        });
        using UIA3Automation automation = new();

        try
        {
            Window shell = WaitForWindow(app, automation, "Shell.Window");
            WaitForText(shell, automation, "Shell.OutputTextBox", "Found 1 INI file");
            Click(shell, automation, "Shell.MainToolbar.FieldRegistryButton");

            Window manager = WaitForWindow(app, automation, "FieldRegistryManager.Window");
            Click(manager, automation, "FieldRegistryManager.OpenFieldImportPreviewButton");

            Window preview = WaitForWindow(app, automation, "FieldImportPreview.Window");
            Find(preview, automation, "FieldImportPreview.RawTextBox").AsTextBox().Text = $$"""
                | Key | AppliesTo | Type | Description |
                | --- | --- | --- | --- |
                | {{importedKey}} | Infantry | Text | UI automation smoke imported field |
                """;

            Click(preview, automation, "FieldImportPreview.ParsePreviewButton");
            SelectComboBoxItem(preview, automation, "FieldImportPreview.TargetScopeComboBox", "Project");
            Click(preview, automation, "FieldImportPreview.BuildApplyPlanButton");
            FlaUIButton applyButton = Find(preview, automation, "FieldImportPreview.ApplyButton").AsButton();
            WaitForApplyButtonEnabled(preview, automation, applyButton);
            applyButton.Invoke();
            ConfirmMessageBox(app, automation);

            Assert.Contains(
                "Apply completed",
                WaitForText(preview, automation, "FieldImportPreview.ApplyStatusText", "Apply completed"),
                StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(tempProject.ActivePackPath));
        }
        finally
        {
            if (!app.HasExited)
                app.Close();
        }
    }

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void FieldImportApplyRollback_ProjectTargetDeletesCreatedPack()
    {
        using TempProject tempProject = TempProject.Create(registryAfterObject: false);
        string rollbackKey = CreateSmokeFieldKey("MyRollbackSmokeKey");
        string exePath = ResolveIdeExePath();
        using FlaUIApplication app = FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"--automation-open-folder \"{tempProject.Path}\"",
            UseShellExecute = false
        });
        using UIA3Automation automation = new();

        try
        {
            Window shell = WaitForWindow(app, automation, "Shell.Window");
            WaitForText(shell, automation, "Shell.OutputTextBox", "Found 1 INI file");
            Click(shell, automation, "Shell.MainToolbar.FieldRegistryButton");

            Window manager = WaitForWindow(app, automation, "FieldRegistryManager.Window");
            Click(manager, automation, "FieldRegistryManager.OpenFieldImportPreviewButton");

            Window preview = WaitForWindow(app, automation, "FieldImportPreview.Window");
            Find(preview, automation, "FieldImportPreview.RawTextBox").AsTextBox().Text = $$"""
                | Key | AppliesTo | Type | Description |
                | --- | --- | --- | --- |
                | {{rollbackKey}} | Infantry | Text | UI automation rollback smoke imported field |
                """;

            Click(preview, automation, "FieldImportPreview.ParsePreviewButton");
            SelectComboBoxItem(preview, automation, "FieldImportPreview.TargetScopeComboBox", "Project");
            Click(preview, automation, "FieldImportPreview.BuildApplyPlanButton");
            FlaUIButton applyButton = Find(preview, automation, "FieldImportPreview.ApplyButton").AsButton();
            WaitForApplyButtonEnabled(preview, automation, applyButton);
            applyButton.Invoke();
            ConfirmMessageBox(app, automation, "Apply Field Registry Import");
            WaitForText(preview, automation, "FieldImportPreview.ApplyStatusText", "Apply completed");
            Assert.True(File.Exists(tempProject.ActivePackPath), $"Expected active pack at {tempProject.ActivePackPath}");

            Click(manager, automation, "FieldRegistryManager.RefreshRollbackManifestsButton");
            SelectFirstDataItem(manager, automation, "FieldRegistryManager.RollbackManifestsGrid");
            Click(manager, automation, "FieldRegistryManager.RollbackSelectedButton");
            ConfirmMessageBox(app, automation, "Rollback Field Registry Import");

            Retry.WhileTrue(
                () => File.Exists(tempProject.ActivePackPath),
                DefaultTimeout,
                TimeSpan.FromMilliseconds(100));
            Assert.False(File.Exists(tempProject.ActivePackPath));
            string rollbackStatus = WaitForText(manager, automation, "FieldRegistryManager.RollbackStatusText", "Rollback completed");
            Assert.Contains("Rollback completed", rollbackStatus, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Operation:", rollbackStatus, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Target:", rollbackStatus, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Manifest:", rollbackStatus, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!app.HasExited)
                app.Close();
        }
    }

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
    {
        Window? window = Retry.WhileNull(
            () => TryFindWindow(app, automation, automationId),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        window ??= TryFindWindow(app, automation, automationId);
        return window ?? throw new InvalidOperationException($"Could not find top-level window '{automationId}'. AppExited={app.HasExited}. Windows: {FormatTopLevelWindows(app, automation)}");
    }

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

    private static void SelectComboBoxItem(Window window, UIA3Automation automation, string automationId, string itemText)
    {
        FlaUIComboBox comboBox = Find(window, automation, automationId).AsComboBox();
        comboBox.Select(itemText);
    }

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

    private static void WaitForApplyButtonEnabled(Window preview, UIA3Automation automation, FlaUIButton applyButton)
    {
        bool isEnabled = Retry.WhileFalse(
            () => applyButton.IsEnabled,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (isEnabled)
            return;

        string applyStatus = Find(preview, automation, "FieldImportPreview.ApplyStatusText").Name;
        string disabledReason = Find(preview, automation, "FieldImportPreview.ApplyDisabledReasonText").Name;
        string summary = Find(preview, automation, "FieldImportPreview.ApplySummaryText").Name;
        throw new InvalidOperationException($"Apply button did not become enabled. Status: {applyStatus} DisabledReason: {disabledReason} Summary: {summary}");
    }

    private static void ConfirmMessageBox(FlaUIApplication app, UIA3Automation automation)
        => ConfirmMessageBox(app, automation, "Apply Field Registry Import");

    private static void ConfirmMessageBox(FlaUIApplication app, UIA3Automation automation, string confirmationTitle)
    {
        Window? dialog = Retry.WhileNull(
            () => FindConfirmationDialog(app, automation, confirmationTitle),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(100)).Result;
        if (dialog is not null)
        {
            InvokeConfirmButton(dialog, automation, confirmationTitle);
            return;
        }

        if (TryConfirmWin32MessageBox(app.ProcessId, confirmationTitle))
            return;

        Keyboard.Press(VirtualKeyShort.ENTER);
        Thread.Sleep(250);

        dialog = FindConfirmationDialog(app, automation, confirmationTitle);
        if (dialog is not null)
        {
            InvokeConfirmButton(dialog, automation, confirmationTitle);
            return;
        }

        if (TryConfirmWin32MessageBox(app.ProcessId, confirmationTitle))
            return;

        Keyboard.Press(VirtualKeyShort.ENTER);
    }

    private static void InvokeConfirmButton(Window dialog, UIA3Automation automation, string confirmationTitle)
    {
        FlaUIButton[] buttons = dialog.FindAllDescendants()
            .Where(element => element.ControlType == ControlType.Button)
            .Select(element => element.AsButton())
            .ToArray();
        FlaUIButton? yesButton = buttons.FirstOrDefault(button =>
            SafeNameContains(button, "Yes") ||
            SafeNameContains(button, "\u662f"));
        if (yesButton is null)
        {
            string buttonNames = string.Join(", ", buttons.Select(SafeName));
            throw new InvalidOperationException($"Confirmation dialog '{confirmationTitle}' did not expose a Yes button. Buttons: {buttonNames}");
        }

        yesButton.Invoke();
    }

    private static void SelectFirstDataItem(Window window, UIA3Automation automation, string automationId)
    {
        AutomationElement grid = Find(window, automation, automationId);
        AutomationElement? row = Retry.WhileNull(
            () => grid.FindAllDescendants(automation.ConditionFactory.ByControlType(ControlType.DataItem)).FirstOrDefault(),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (row is null)
            throw new InvalidOperationException($"Could not find any row in '{automationId}'.");

        row.Click();
    }

    private static Window? FindConfirmationDialog(FlaUIApplication app, UIA3Automation automation, string title)
    {
        Window? appWindow = app.GetAllTopLevelWindows(automation)
            .FirstOrDefault(window => SafeNameEquals(window, title));
        if (appWindow is not null)
            return appWindow;

        ConditionFactory cf = automation.ConditionFactory;
        return automation.GetDesktop()
            .FindAllChildren(cf.ByControlType(ControlType.Window))
            .Select(element => element.AsWindow())
            .FirstOrDefault(window => SafeNameEquals(window, title));
    }

    private static bool TryConfirmWin32MessageBox(int processId, string title)
    {
        IntPtr handle = Retry.WhileNull(
            () => FindTopLevelWindowHandle(processId, title),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(100)).Result;
        if (handle == IntPtr.Zero)
            return false;

        SetForegroundWindow(handle);
        Thread.Sleep(100);
        Keyboard.Press(VirtualKeyShort.ENTER);
        return true;
    }

    private static IntPtr FindTopLevelWindowHandle(int processId, string title)
    {
        IntPtr match = IntPtr.Zero;
        EnumWindows((handle, _) =>
        {
            GetWindowThreadProcessId(handle, out int windowProcessId);
            if (windowProcessId != processId || !IsWindowVisible(handle))
                return true;

            string windowTitle = GetWindowTitle(handle);
            if (string.Equals(windowTitle, title, StringComparison.OrdinalIgnoreCase))
            {
                match = handle;
                return false;
            }

            return true;
        }, IntPtr.Zero);
        return match;
    }

    private static string GetWindowTitle(IntPtr handle)
    {
        int length = GetWindowTextLength(handle);
        if (length <= 0)
            return string.Empty;

        StringBuilder builder = new(length + 1);
        _ = GetWindowText(handle, builder, builder.Capacity);
        return builder.ToString();
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

    private static bool SafeNameEquals(AutomationElement element, string value)
    {
        try
        {
            return string.Equals(element.Name, value, StringComparison.OrdinalIgnoreCase);
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

    private static string CreateSmokeFieldKey(string prefix)
        => prefix + Guid.NewGuid().ToString("N");

    private delegate bool EnumWindowsProc(IntPtr handle, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowText(IntPtr handle, StringBuilder text, int maxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr handle, out int processId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr handle);

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

    private sealed class TempProject : IDisposable
    {
        private TempProject(string path)
        {
            Path = path;
            ActivePackPath = System.IO.Path.Combine(path, ".ra2inieditor", "field-registry", "active", "user-import.fields.json");
            BackupsPath = System.IO.Path.Combine(path, ".ra2inieditor", "field-registry", "backups");
        }

        public string Path { get; }

        public string ActivePackPath { get; }

        public string BackupsPath { get; }

        public static TempProject Create(bool registryAfterObject)
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RA2IniEditor.UiAutomation", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            string rulesText = registryAfterObject
                ? """
                  [NEWINF]
                  Name=NEWINF
                  MyImportedSmokeKey=test

                  [InfantryTypes]
                  0=NEWINF
                  """
                : """
                  [InfantryTypes]
                  0=NEWINF

                  [NEWINF]
                  Name=NEWINF
                  MyImportedSmokeKey=test
                  """;
            File.WriteAllText(System.IO.Path.Combine(path, "rulesmd.ini"), rulesText);
            return new TempProject(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
