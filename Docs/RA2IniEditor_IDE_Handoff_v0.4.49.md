# RA2IniEditor IDE Handoff v0.4.49

## Scope

v0.4.49 extracts current-document language navigation orchestration from
`ShellWindow.xaml.cs` into `Ra2LanguageNavigationController`.

Covered features:

- Go To Definition.
- Peek Definition.
- Find All References.
- Definition provider and reference finder routing.
- Navigation messages for jump, preview, and references.

## What Changed

- Added `RA2IniEditor.IDE/Controllers/Language/Ra2LanguageNavigationController.cs`.
- `ShellWindow` now builds the current document language request and delegates:
  - `_languageNavigationController.GoToDefinition(...)`
  - `_languageNavigationController.PeekDefinition(...)`
  - `_languageNavigationController.FindReferences(...)`
- `ShellWindow` still owns WPF-only work:
  - Reading AvalonEdit text and caret offset.
  - Scrolling editor/caret.
  - Opening Peek Definition and Find References windows.
  - Output messages.

## Controller Boundary

`Ra2LanguageNavigationController` is IDE-internal and pure orchestration. It does
not reference WPF windows, AvalonEdit `TextEditor`, disk IO, save services, or
ObjectAggregator.

The controller returns result DTOs. UI actions remain in `ShellWindow`.

## Guardrails

- No Save Current File.
- No Save / Save All.
- No ProjectSaveService or legacy save dependency.
- No Completion behavior change.
- No Hover tooltip lifecycle change.
- No Add Property behavior change.
- No Edit / Revert / dirty behavior change.
- No Core or Infrastructure public API change.

## Verification

- `dotnet test -c Release`: 780 passed.
- `dotnet build -c Release --no-incremental`: passed, 0 errors, 26 existing warnings.

## Manual Smoke

1. Open RA2IniEditor.IDE.
2. Open an INI with `Primary=120mm` and `[120mm]`.
3. Put caret on `120mm`, use Go To Definition, and confirm the editor jumps to `[120mm]`.
4. Put caret on a known key such as `Strength`, use Peek Definition, and confirm preview opens.
5. Put caret on a section header or reference value, use Find All References, and confirm references window opens.
6. Smoke Completion, Hover, Add Property, Edit Mode, and Revert to confirm no unrelated behavior changed.
