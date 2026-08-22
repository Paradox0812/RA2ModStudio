# Codex Task: RA2IniEditor.IDE A15-2B-P2 Field Registry Custom Chrome

## 0. Context

Field Registry Center / Manager still show default WPF title icon, title bar, and outer system border.

User wants these management windows to align better with the borderless inspector direction.

This task is about secondary-window chrome consistency, not Field Registry behavior.

---

## 1. Goal

For Field Registry Center and Field Registry Manager:

```text
remove default WPF icon
remove default system title bar
remove normal outer system border
use custom lightweight IDE tool-window chrome
preserve move behavior
preserve resize behavior if possible
```

These are large management tool windows, not tiny inspector popups.

---

## 2. Target Surfaces

```text
Field Registry Center
Field Registry Manager / Advanced Tools
```

---

## 3. Files Allowed

Allowed:

```text
FieldRegistryCenterWindow.xaml
FieldRegistryCenterWindow.xaml.cs
FieldRegistryManagerWindow.xaml
FieldRegistryManagerWindow.xaml.cs
IdeSecondaryWindowStyles.xaml or scoped local styles
WpfAutomationHarnessBoundaryTests.cs
```

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
Field Registry loader/writer/apply/rollback services
parser
diagnostics
completion
hover
quick peek
save preflight
BuiltIn field registry JSON
solution/project files
legacy files
```

---

## 4. Hard Semantic Boundaries

Do not change:

```text
Project > Global > BuiltIn priority
load order
active pack load/reload semantics
import preview semantics
cleanup plan semantics
apply cleanup behavior
rollback behavior
field learning behavior
backup manifest behavior
diagnostics/completion/hover behavior
save/dirty behavior
```

---

## 5. Chrome Contract

Required:

```xml
WindowStyle="None"
```

Recommended for large management windows:

```text
use WindowChrome to preserve resizing and drag behavior
custom header with title/subtitle and close button
```

If using `WindowChrome`, use a pattern like:

```xml
<WindowChrome.WindowChrome>
    <shell:WindowChrome
        CaptionHeight="40"
        ResizeBorderThickness="6"
        CornerRadius="0"
        GlassFrameThickness="0"
        UseAeroCaptionButtons="False" />
</WindowChrome.WindowChrome>
```

Add namespace if needed:

```xml
xmlns:shell="clr-namespace:System.Windows.Shell;assembly=PresentationFramework"
```

Do not make these windows tiny `SizeToContent` popups.

---

## 6. Custom Header Requirements

Header should contain:

```text
left: Chinese-first title and compact subtitle/status chips
right: close button
optional: minimize/maximize only if needed and approved
```

Required close button AutomationIds:

```text
FieldRegistryCenter.CloseButton
FieldRegistryManager.CloseButton
```

Close button closes only the current window.

---

## 7. Drag / Resize

The borderless window must remain movable.

Allowed strategies:

```text
WindowChrome CaptionHeight
or local header DragMove handler
```

Prefer WindowChrome if it preserves resize better.

Do not implement global drag behavior.

---

## 8. Tests

Add/update tests for:

```text
FieldRegistryCenterWindow has WindowStyle=None.
FieldRegistryManagerWindow has WindowStyle=None.
Close button AutomationIds exist.
Existing key AutomationIds remain present.
WindowChrome exists if used.
```

Avoid pixel-perfect tests.

---

## 9. Validation

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 10. Manual Smoke

```text
Open Field Registry Center.
Confirm default icon/title bar/system buttons are gone.
Confirm custom header is readable.
Confirm close button works.
Confirm window can move.
Confirm window can resize if resize was preserved.
Open Field Registry Manager and repeat.
Confirm Field Registry behavior is unchanged.
```
