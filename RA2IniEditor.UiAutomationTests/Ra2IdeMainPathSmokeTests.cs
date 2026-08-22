using System.Diagnostics;
using System.Runtime.InteropServices;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Xunit;
using FlaUITextBox = FlaUI.Core.AutomationElements.TextBox;
using FlaUIApplication = FlaUI.Core.Application;

namespace RA2IniEditor.UiAutomationTests;

public sealed class Ra2IdeMainPathSmokeTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void SearchTool_OpenHideAndReopen_UsesFloatingHostWithoutMockResults()
    {
        using FlaUIApplication app = FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = ResolveIdeExePath(),
            UseShellExecute = false
        });
        using UIA3Automation automation = new();

        try
        {
            Window shell = WaitForWindow(app, automation, "Shell.Window");
            Click(shell, automation, "Shell.MainToolbar.SearchButton");
            Window searchHost = TryWaitForFloatingHost(app, automation, DefaultTimeout) ??
                                throw new InvalidOperationException("Search did not open in a top-level floating host.");

            Assert.NotSame(shell, searchHost);
            Assert.True(searchHost.BoundingRectangle.Width >= 420);
            Assert.True(searchHost.BoundingRectangle.Height >= 420);
            Click(searchHost, automation, "Shell.Dock.FloatingHost.CloseButton");
            Assert.Null(TryWaitForFloatingHost(app, automation, TimeSpan.FromSeconds(3)));

            Click(shell, automation, "Shell.MainToolbar.SearchButton");
            Window reopenedHost = TryWaitForFloatingHost(app, automation, DefaultTimeout) ??
                                  throw new InvalidOperationException("Search did not reopen after AvalonDock hide.");
            Assert.NotSame(shell, reopenedHost);

            Click(reopenedHost, automation, "Shell.Dock.FloatingHost.CloseButton");
            Assert.Null(TryWaitForFloatingHost(app, automation, TimeSpan.FromSeconds(3)));
            app.Close();
            Assert.True(Retry.WhileFalse(
                () => app.HasExited,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMilliseconds(100)).Result);
        }
        finally
        {
            if (!app.HasExited)
                app.Close();

            if (!app.HasExited)
                app.Kill();
        }

        using FlaUIApplication restoredApp = FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = ResolveIdeExePath(),
            UseShellExecute = false
        });
        using UIA3Automation restoredAutomation = new();
        try
        {
            Window restoredShell = WaitForWindow(restoredApp, restoredAutomation, "Shell.Window");
            Assert.Null(TryWaitForFloatingHost(restoredApp, restoredAutomation, TimeSpan.FromSeconds(3)));
            Click(restoredShell, restoredAutomation, "Shell.MainToolbar.SearchButton");
            Window restoredHost = TryWaitForFloatingHost(restoredApp, restoredAutomation, DefaultTimeout) ??
                                  throw new InvalidOperationException("Persisted hidden Search did not reopen as floating.");
            Click(restoredHost, restoredAutomation, "Shell.Dock.FloatingHost.CloseButton");
        }
        finally
        {
            if (!restoredApp.HasExited)
                restoredApp.Close();
            if (!restoredApp.HasExited)
                restoredApp.Kill();
        }
    }

    private static Window? TryWaitForFloatingHost(
        FlaUIApplication app,
        UIA3Automation automation,
        TimeSpan timeout)
        => Retry.WhileNull(
            () => FindFloatingHost(app.ProcessId, automation),
            timeout,
            TimeSpan.FromMilliseconds(100),
            ignoreException: true).Result;

    private static Window? FindFloatingHost(int processId, UIA3Automation automation)
    {
        Window? match = null;
        EnumWindows((handle, _) =>
        {
            GetWindowThreadProcessId(handle, out int windowProcessId);
            if (windowProcessId != processId || !IsWindowVisible(handle))
                return true;

            try
            {
                Window candidate = automation.FromHandle(handle).AsWindow();
                if (string.Equals(SafeAutomationId(candidate), "Shell.Window", StringComparison.Ordinal))
                    return true;
                if (candidate.FindFirstDescendant(
                        automation.ConditionFactory.ByAutomationId("Shell.Dock.FloatingHost.CloseButton")) is null)
                    return true;

                match = candidate;
                return false;
            }
            catch
            {
                return true;
            }
        }, IntPtr.Zero);
        return match;
    }

    private delegate bool EnumWindowsProc(IntPtr handle, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr handle, out int processId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr handle);

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void M6A_ShellResponsiveLandmarksAndKeyboardPaths_RemainReachable()
    {
        using TempIdeProject tempProject = TempIdeProject.Create();
        using FlaUIApplication app = FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = ResolveIdeExePath(),
            Arguments = $"--automation-open-folder \"{tempProject.Path}\"",
            UseShellExecute = false
        });
        using UIA3Automation automation = new();

        try
        {
            Window shell = WaitForWindow(app, automation, "Shell.Window");
            WaitForText(shell, automation, "Shell.OutputTextBox", "1 个 INI 文件");
            Find(shell, automation, "Shell.Dock.Tab.Tool.SectionExplorer").AsTabItem().Select();
            SelectProjectExplorerItem(shell, automation, "rulesmd.ini");
            WaitForText(shell, automation, "StatusCurrentFileText", "rulesmd.ini");

            IntPtr windowHandle = new(shell.Properties.NativeWindowHandle.Value);
            uint dpi = GetDpiForWindow(windowHandle);
            Assert.True(dpi >= 96, $"Unexpected Shell DPI: {dpi}.");

            VerifyResponsiveProfile(shell, automation, windowHandle, dpi, widthDip: 1920, heightDip: 1040);
            VerifyResponsiveProfile(shell, automation, windowHandle, dpi, widthDip: 1280, heightDip: 800);
            VerifyToolbarKeyboardRoundTrip(shell, automation);
            VerifyModelSelectorKeyboardToggle(shell, automation);
        }
        finally
        {
            if (!app.HasExited)
                app.Close();
            if (!app.HasExited)
                app.Kill();
        }
    }

    [UiAutomationFact]
    [Trait("Category", "UiAutomation")]
    public void IdeMainPath_OpenFolder_EditCompletionAddPropertyAndRevert_DoesNotWriteSourceIni()
    {
        using TempIdeProject tempProject = TempIdeProject.Create();
        string originalIniText = File.ReadAllText(tempProject.RulesPath);
        using FlaUIApplication app = FlaUIApplication.Launch(new ProcessStartInfo
        {
            FileName = ResolveIdeExePath(),
            Arguments = $"--automation-open-folder \"{tempProject.Path}\"",
            UseShellExecute = false
        });
        using UIA3Automation automation = new();

        try
        {
            Window shell = WaitForWindow(app, automation, "Shell.Window");
            WaitForText(shell, automation, "Shell.OutputTextBox", "1 个 INI 文件");
            SelectProjectExplorerItem(shell, automation, "rulesmd.ini");
            WaitForEnabled(shell, automation, "Shell.SourceEditor.RevertInMemoryChangesButton");

            AutomationElement editor = Find(shell, automation, "Shell.SourceEditor");
            editor.Click();
            Keyboard.Type("\r\n; UIA smoke edit");

            TryOpenCompletionFromContextMenu(automation, editor);
            TryCommitCompletionIfOpen(automation);

            TryOpenAddPropertyFromContextMenu(automation, editor);
            Window? addPropertyWindow = TryWaitForWindow(app, automation, "AddProperty.Window", TimeSpan.FromSeconds(5));
            if (addPropertyWindow is not null)
            {
                SetText(addPropertyWindow, automation, "AddProperty.SearchTextBox", "Strength");
                SetText(addPropertyWindow, automation, "AddProperty.ValueTextBox", "500");
                if (!TryClickIfEnabled(addPropertyWindow, automation, "AddProperty.AddSelectedButton"))
                    Click(addPropertyWindow, automation, "AddProperty.CancelButton");
            }

            Click(shell, automation, "Shell.SourceEditor.RevertInMemoryChangesButton");

            Assert.Equal(originalIniText, File.ReadAllText(tempProject.RulesPath));
        }
        finally
        {
            if (!app.HasExited)
                app.Close();

            if (!app.HasExited)
                app.Kill();
        }
    }

    private static void TryOpenCompletionFromContextMenu(UIA3Automation automation, AutomationElement editor)
    {
        editor.RightClick();
        AutomationElement? menuItem = TryFindDesktopElement(automation, "Shell.SourceEditor.ShowCompletionPreviewMenuItem", TimeSpan.FromSeconds(3));
        menuItem?.AsMenuItem().Invoke();
    }

    private static void TryCommitCompletionIfOpen(UIA3Automation automation)
    {
        AutomationElement? itemsList = TryFindDesktopElement(automation, "Ra2CompletionDropdown.ItemsList", TimeSpan.FromSeconds(3));
        if (itemsList is null)
            return;

        Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
    }

    private static void TryOpenAddPropertyFromContextMenu(UIA3Automation automation, AutomationElement editor)
    {
        editor.RightClick();
        AutomationElement? menuItem = TryFindDesktopElement(automation, "Shell.SourceEditor.AddPropertyMenuItem", TimeSpan.FromSeconds(3));
        menuItem?.AsMenuItem().Invoke();
    }

    private static void VerifyResponsiveProfile(
        Window shell,
        UIA3Automation automation,
        IntPtr windowHandle,
        uint dpi,
        int widthDip,
        int heightDip)
    {
        int widthPixels = ScaleDip(widthDip, dpi);
        int heightPixels = ScaleDip(heightDip, dpi);
        IntPtr monitor = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
        MonitorInfo monitorInfo = new() { Size = Marshal.SizeOf<MonitorInfo>() };
        Assert.True(GetMonitorInfo(monitor, ref monitorInfo));

        int workWidth = monitorInfo.WorkArea.Right - monitorInfo.WorkArea.Left;
        int workHeight = monitorInfo.WorkArea.Bottom - monitorInfo.WorkArea.Top;
        Assert.True(
            widthPixels <= workWidth && heightPixels <= workHeight,
            $"The current monitor work area {workWidth}x{workHeight}px cannot host the {widthDip}x{heightDip} DIP M6-A profile at {dpi} DPI.");

        int left = monitorInfo.WorkArea.Left + ((workWidth - widthPixels) / 2);
        int top = monitorInfo.WorkArea.Top + ((workHeight - heightPixels) / 2);
        ShowWindow(windowHandle, ShowRestore);
        Assert.True(SetWindowPos(windowHandle, IntPtr.Zero, left, top, widthPixels, heightPixels, SetWindowPosFlags));

        bool resized = Retry.WhileFalse(
            () => Math.Abs(shell.BoundingRectangle.Width - widthPixels) <= 4 &&
                  Math.Abs(shell.BoundingRectangle.Height - heightPixels) <= 4,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        Assert.True(
            resized,
            $"Shell did not reach the {widthDip}x{heightDip} DIP profile. Actual={shell.BoundingRectangle.Width}x{shell.BoundingRectangle.Height}px, DPI={dpi}.");

        AutomationElement openFolderButton = Find(shell, automation, "Shell.MainToolbar.OpenFolderButton");
        AutomationElement layoutButton = Find(shell, automation, "Shell.MainToolbar.WindowLayoutButton");
        AutomationElement documentStatus = Find(shell, automation, "StatusCurrentFileText");
        AutomationElement bottomTools = Find(shell, automation, "Shell.OutputTextBox");
        AutomationElement rightTools = Find(shell, automation, "Shell.ProjectExplorer");

        AssertInsideShell(shell, openFolderButton, "Shell.MainToolbar.OpenFolderButton");
        AssertInsideShell(shell, layoutButton, "Shell.MainToolbar.WindowLayoutButton");
        AssertInsideShell(shell, documentStatus, "StatusCurrentFileText");
        AssertInsideShell(shell, bottomTools, "Shell.OutputTextBox");
        AssertInsideShell(shell, rightTools, "Shell.ProjectExplorer");
        Assert.True(
            layoutButton.BoundingRectangle.Right > openFolderButton.BoundingRectangle.Right,
            "Window Layout must remain the right-most command after responsive layout.");
    }

    private static void VerifyToolbarKeyboardRoundTrip(Window shell, UIA3Automation automation)
    {
        AutomationElement layoutButton = Find(shell, automation, "Shell.MainToolbar.WindowLayoutButton");
        shell.Focus();
        bool focused = Retry.WhileFalse(
            () =>
            {
                layoutButton.Focus();
                return string.Equals(SafeAutomationId(automation.FocusedElement()), "Shell.MainToolbar.WindowLayoutButton", StringComparison.Ordinal);
            },
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        Assert.True(focused, $"Keyboard focus did not reach Window Layout. Actual='{SafeAutomationId(automation.FocusedElement())}'.");

        Keyboard.TypeSimultaneously(VirtualKeyShort.SHIFT, VirtualKeyShort.TAB);
        AssertFocused(automation, "Shell.MainToolbar.ProjectExplorerButton");
        Keyboard.Press(VirtualKeyShort.TAB);
        AssertFocused(automation, "Shell.MainToolbar.WindowLayoutButton");
    }

    private static void VerifyModelSelectorKeyboardToggle(Window shell, UIA3Automation automation)
    {
        Find(shell, automation, "Shell.Dock.Tab.Tool.AiAssistant").AsTabItem().Select();
        FlaUI.Core.AutomationElements.ComboBox modelSelector = Find(shell, automation, "AiAssistant.ModelSelector").AsComboBox();
        shell.Focus();
        modelSelector.Focus();
        AssertFocused(automation, "AiAssistant.ModelSelector");

        Keyboard.Press(VirtualKeyShort.F4);
        bool expanded = Retry.WhileFalse(
            () => modelSelector.ExpandCollapseState == ExpandCollapseState.Expanded,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        Assert.True(expanded, "F4 did not expand the AI model selector.");

        Keyboard.Press(VirtualKeyShort.ESCAPE);
        bool collapsed = Retry.WhileFalse(
            () => modelSelector.ExpandCollapseState == ExpandCollapseState.Collapsed,
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        Assert.True(collapsed, "ESC did not collapse the AI model selector.");
    }

    private static void AssertFocused(UIA3Automation automation, string expectedAutomationId)
    {
        bool focused = Retry.WhileFalse(
            () => string.Equals(SafeAutomationId(automation.FocusedElement()), expectedAutomationId, StringComparison.Ordinal),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        Assert.True(focused, $"Keyboard focus did not reach '{expectedAutomationId}'. Actual='{SafeAutomationId(automation.FocusedElement())}'.");
    }

    private static void AssertInsideShell(Window shell, AutomationElement element, string automationId)
    {
        System.Drawing.Rectangle innerBounds = element.BoundingRectangle;
        System.Drawing.Rectangle shellBounds = shell.BoundingRectangle;
        Assert.True(innerBounds.Width > 0 && innerBounds.Height > 0, $"'{automationId}' has an empty bounding rectangle.");
        Assert.True(
            innerBounds.Left >= shellBounds.Left - 1 &&
            innerBounds.Top >= shellBounds.Top - 1 &&
            innerBounds.Right <= shellBounds.Right + 1 &&
            innerBounds.Bottom <= shellBounds.Bottom + 1,
            $"'{automationId}' bounds {innerBounds} escape Shell bounds {shellBounds}.");
    }

    private static int ScaleDip(int value, uint dpi)
        => checked((int)Math.Round(value * dpi / 96d, MidpointRounding.AwayFromZero));

    private static void SetText(Window window, UIA3Automation automation, string automationId, string text)
    {
        FlaUITextBox textBox = Find(window, automation, automationId).AsTextBox();
        textBox.Text = text;
    }

    private static void SelectProjectExplorerItem(Window shell, UIA3Automation automation, string expectedName)
    {
        AutomationElement? projectExplorer = shell.FindFirstDescendant(
            automation.ConditionFactory.ByAutomationId("Shell.ProjectExplorer"));
        if (projectExplorer is null)
        {
            Find(shell, automation, "Shell.Dock.Tab.Tool.SectionExplorer").AsTabItem().Select();
            projectExplorer = Find(shell, automation, "Shell.ProjectExplorer");
        }
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

    private static AutomationElement Find(AutomationElement root, UIA3Automation automation, string automationId)
    {
        ConditionFactory cf = automation.ConditionFactory;
        AutomationElement? element = Retry.WhileNull(
            () => root.FindFirstDescendant(cf.ByAutomationId(automationId)) ??
                  root.FindAllDescendants().FirstOrDefault(candidate =>
                      string.Equals(SafeAutomationId(candidate), automationId, StringComparison.Ordinal)),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        return element ?? throw new InvalidOperationException(
            $"Could not find AutomationId '{automationId}'. Tree: {FormatAutomationTree(root, maxNodes: 80)}");
    }

    private static Window WaitForWindow(FlaUIApplication app, UIA3Automation automation, string automationId)
        => TryWaitForWindow(app, automation, automationId, DefaultTimeout) ??
           throw new InvalidOperationException($"Could not find top-level window '{automationId}'. AppExited={app.HasExited}.");

    private static Window? TryWaitForWindow(
        FlaUIApplication app,
        UIA3Automation automation,
        string automationId,
        TimeSpan timeout)
        => Retry.WhileNull(
            () => TryFindWindow(app, automation, automationId),
            timeout,
            TimeSpan.FromMilliseconds(100)).Result;

    private static Window? TryFindWindow(FlaUIApplication app, UIA3Automation automation, string automationId)
    {
        Window? appWindow = app.GetAllTopLevelWindows(automation).FirstOrDefault(candidate =>
            string.Equals(SafeAutomationId(candidate), automationId, StringComparison.Ordinal) ||
            candidate.FindFirstDescendant(automation.ConditionFactory.ByAutomationId(automationId)) is not null);
        if (appWindow is not null)
            return appWindow;

        Window? match = null;
        EnumWindows((handle, _) =>
        {
            GetWindowThreadProcessId(handle, out int windowProcessId);
            if (windowProcessId != app.ProcessId || !IsWindowVisible(handle))
                return true;

            try
            {
                Window candidate = automation.FromHandle(handle).AsWindow();
                if (string.Equals(SafeAutomationId(candidate), automationId, StringComparison.Ordinal) ||
                    candidate.FindFirstDescendant(
                        automation.ConditionFactory.ByAutomationId(automationId)) is not null)
                {
                    match = candidate;
                    return false;
                }
            }
            catch
            {
                // 窗口可能在枚举期间销毁；交给 Retry 处理下一次快照。
            }

            return true;
        }, IntPtr.Zero);
        return match;
    }

    private static AutomationElement? TryFindDesktopElement(
        UIA3Automation automation,
        string automationId,
        TimeSpan timeout)
        => Retry.WhileNull(
            () => automation.GetDesktop().FindFirstDescendant(automation.ConditionFactory.ByAutomationId(automationId)),
            timeout,
            TimeSpan.FromMilliseconds(100)).Result;

    private static void Click(AutomationElement root, UIA3Automation automation, string automationId)
        => Find(root, automation, automationId).AsButton().Invoke();

    private static bool TryClickIfEnabled(AutomationElement root, UIA3Automation automation, string automationId)
    {
        AutomationElement element = Find(root, automation, automationId);
        if (!element.IsEnabled)
            return false;

        element.AsButton().Invoke();
        return true;
    }

    private static string WaitForText(Window window, UIA3Automation automation, string automationId, string expectedText)
    {
        AutomationElement element = Find(window, automation, automationId);
        bool reached = Retry.WhileFalse(
            () => SafeText(element).Contains(expectedText, StringComparison.OrdinalIgnoreCase),
            DefaultTimeout,
            TimeSpan.FromMilliseconds(100)).Result;
        if (!reached)
            throw new InvalidOperationException($"Timed out waiting for '{automationId}' to contain '{expectedText}'. Actual: '{SafeText(element)}'.");

        return SafeText(element);
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

    private static string SafeText(AutomationElement element)
    {
        string name = SafeName(element);
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        try
        {
            return element.AsTextBox().Text;
        }
        catch
        {
            return name;
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
        string configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Name ?? "Release";
        string exePath = Path.Combine(root, "RA2IniEditor.IDE", "bin", configuration, "net8.0-windows", "RA2IniEditor.IDE.exe");
        if (!File.Exists(exePath))
            throw new FileNotFoundException($"RA2IniEditor.IDE.exe was not found. Build the IDE project in {configuration} before running UI automation.", exePath);

        return exePath;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RA2IniEditor.IDE.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate RA2IniEditor.IDE.sln from test output directory.");
    }

    private const uint MonitorDefaultToNearest = 0x00000002;
    private const int ShowRestore = 9;
    private const uint SetWindowPosFlags = 0x0004 | 0x0010 | 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect WorkArea;
        public uint Flags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr windowHandle, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo monitorInfo);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr windowHandle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr windowHandle, int command);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr windowHandle,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    private sealed class TempIdeProject : IDisposable
    {
        private TempIdeProject(string path)
        {
            Path = path;
            RulesPath = System.IO.Path.Combine(path, "rulesmd.ini");
        }

        public string Path { get; }

        public string RulesPath { get; }

        public static TempIdeProject Create()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RA2IniEditor_UiaSmoke_" + Guid.NewGuid().ToString("N"));
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

            string annotationDirectory = System.IO.Path.Combine(path, ".ra2ide");
            Directory.CreateDirectory(annotationDirectory);
            File.WriteAllText(
                System.IO.Path.Combine(annotationDirectory, "field-annotations.zh-CN.json"),
                """
                {
                  "version": 1,
                  "language": "zh-CN",
                  "entries": [
                    {
                      "sectionKind": "Vehicle",
                      "key": "Strength",
                      "displayName": "HP",
                      "aliases": ["Health"],
                      "note": "Maximum unit health."
                    },
                    {
                      "sectionKind": "Vehicle",
                      "key": "Armor",
                      "displayName": "Armor type",
                      "aliases": ["Protection"],
                      "note": "Armor category used by this unit."
                    },
                    {
                      "sectionKind": "Vehicle",
                      "key": "Primary",
                      "displayName": "Primary weapon",
                      "aliases": ["Main weapon"],
                      "note": "Default primary weapon."
                    }
                  ]
                }
                """);

            return new TempIdeProject(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
