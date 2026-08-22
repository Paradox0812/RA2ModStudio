# Codex Task: RA2IniEditor.IDE A15-2T Field Registry Tertiary Windows Audit / UI Contract

## 0. Context

A15-2B-P/P2 improved Field Registry Center / Manager. The primary Field Registry windows are now closer to IDE management surfaces.

User feedback:

```text
字段库中的三级界面仍然是原生 WPF 风格。
```

This means Field Registry child/workflow/editor windows still need attention, but they must not be redesigned freely or all at once.

This task is **audit + UI contract only**.

Do not implement UI changes in this task.

---

## 1. Goal

Audit all Field Registry tertiary surfaces and prepare a strict staged UI contract.

Tertiary surfaces are windows/dialogs opened from Field Registry Center / Manager, such as:

```text
Field Import Preview
Field Learning Wizard
Field Editor / New Field Editor
Allowed Values Editor
Remote Preset Editor
Apply / Rollback / destructive confirmations
other Field Registry child dialogs found in source
```

The goal is to identify which windows still use default WPF chrome/form layout and define a safe redesign order.

---

## 2. Hard Boundaries

Do not modify source files in this task.

Do not modify:

```text
XAML
code-behind
ViewModels
tests
scripts
field registry JSON
solution/project files
```

Do not modify semantics:

```text
Field Registry load order
Project > Global > BuiltIn priority
Import preview diff semantics
Apply writer behavior
Rollback behavior
Backup manifest behavior
Field learning behavior
Field editor validation behavior
Allowed values behavior
Remote preset behavior
Diagnostics / Completion / Hover / Quick Peek
Save / Dirty behavior
```

Do not restore:

```text
RA2IniEditor.sln
RA2IniEditor.csproj
legacy MainWindow
legacy table-style editor
legacy object workbench
```

---

## 3. Surfaces to Discover

Search the project for Field Registry child windows/dialogs.

Likely targets:

```text
FieldImportPreviewWindow
FieldLearningWizardWindow
FieldEditorWindow
AllowedValuesEditorWindow
RemotePresetEditorWindow
Rollback / Apply confirmation MessageBox flows
Cleanup preview child surfaces
Any FieldRegistry-related Window / Dialog / Popup / UserControl
```

Use actual discovered source paths.

Do not invent surfaces.

If a surface is not found, report:

```text
Not found in current IDE-only package.
```

---

## 4. Required Inspection Per Surface

For each discovered tertiary surface, report:

```text
1. Name
2. XAML path
3. code-behind path
4. ViewModel/DataContext type
5. Open/show entry point
6. Owner assignment
7. Modal/non-modal
8. WindowStyle
9. ShowInTaskbar
10. ResizeMode
11. SizeToContent
12. WindowStartupLocation
13. Width / Height / MinWidth / MinHeight
14. Existing AutomationIds
15. Existing tests
16. Whether it reads/writes registry state
17. Current UX problems
18. Recommended future phase
```

---

## 5. Classification Rules

Classify each window into one of these categories.

### 5.1 Workflow Dialog

For multi-step processes:

```text
Field Import Preview
Field Learning Wizard
```

Future style:

```text
custom lightweight tool-window chrome
step-based layout
source -> parse/preview -> target -> review -> apply
explicit write boundary
compact warnings/invalid state
```

### 5.2 Editor Dialog

For editing structured field definitions:

```text
Field Editor
Allowed Values Editor
New Field Editor
```

Future style:

```text
compact editor card
validation summary
clear required/optional fields
no huge default form layout
no raw WPF DataGrid unless needed
```

### 5.3 Preset / Remote Dialog

For remote or preset selection:

```text
Remote Preset Editor
Remote import/preset selection window
```

Future style:

```text
source/status card
preview before apply
network/remote boundary clear
no automatic apply
```

### 5.4 Confirmation Dialog

For dangerous or write operations:

```text
Apply cleanup
Rollback selected
Overwrite
Discard changes
```

Future style:

```text
short summary
risk list
primary/secondary actions
no large form layout
Chinese-first text
```

---

## 6. UI Direction for Future Implementation

All tertiary Field Registry surfaces should eventually align with the current IDE direction:

```text
custom lightweight chrome where appropriate
Chinese-first UI text
compact header
clear workflow sections
status chips
explicit disabled reasons
clear write/apply boundary
less default WPF form/grid feeling
stable AutomationIds
```

Avoid:

```text
default WPF icon/titlebar for child windows
large blank regions
unstructured form grids
ambiguous button clusters
English-heavy user-facing labels
full paths as primary content
write actions mixed with read-only actions
```

---

## 7. Proposed Staged Plan

Unless inspection proves otherwise, propose this order:

```text
A15-2C: Field Import Preview workflow contract / implementation
A15-2D: Field Editor + Allowed Values Editor contract / implementation
A15-2E: Field Learning Wizard workflow contract / implementation
A15-2F: Apply / Rollback / destructive confirmation consistency
A15-2G: Remote Preset / optional tertiary surfaces
```

Do not merge all tertiary surfaces into one large implementation task.

---

## 8. Output Required

Create or update:

```text
Docs/FieldRegistryTertiarySurfacesUiContract.md
```

Document structure:

```markdown
# Field Registry Tertiary Surfaces UI Contract

## 1. Scope and Baseline

## 2. Inventory Summary

| Surface | Files | Type | Entry Point | Writes State | Current Chrome | Current UX Problem | Future Phase | Risk |

## 3. Detailed Surface Notes

### 3.1 <Surface Name>
- XAML:
- Code-behind:
- ViewModel:
- Entry point:
- Window properties:
- Current UX problems:
- Proposed category:
- Future contract:
- Non-goals:
- Tests needed:

## 4. Cross-cutting Problems

## 5. Common Chrome / Style Rules

## 6. Localization Rules

## 7. Recommended Redesign Order

## 8. Risk Matrix

## 9. Acceptance Criteria for Future Phases
```

---

## 9. Required Stop Point

After creating/updating the contract document, stop.

Do not implement any UI change.

Do not modify XAML.

Do not modify ViewModels.

Do not modify tests unless explicitly instructed.

---

## 10. Validation Commands

For this documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If `--no-build` cannot run due to missing output, run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: A15-2T tertiary surfaces audit / UI contract.
2. Files changed.
3. Surfaces discovered and count.
4. High-risk surfaces.
5. Recommended redesign order.
6. Commands run.
7. Test result.
8. Package result.
9. Confirmation no source/XAML/ViewModel behavior changed.
10. Confirmation Field Registry semantics unchanged.
11. Confirmation legacy not restored.
12. Recommended next phase.
```
