# Codex Task: RA2IniEditor.IDE A15-2C Field Import Preview Workflow Contract

## 0. Current Baseline

A15-2T has been completed.

Reported state:

```text
Phase: A15-2T Field Registry tertiary surfaces audit / UI Contract
Files changed: Docs/FieldRegistryTertiarySurfacesUiContract.md
Tests: 1298 passed / 0 failed
IdeOnly package: passed
Packaged file count: 651
Shell: unchanged
Field Registry semantics: unchanged
Legacy table-style editor: not restored
```

Discovered tertiary surfaces:

```text
1. Field Import Preview
2. Field Learning Wizard
3. Field Editor / New Field Editor
4. Allowed Values Editor
5. Remote Preset Editor
6. Apply / Rollback / destructive MessageBox confirmations
```

Next phase:

```text
A15-2C: Field Import Preview workflow contract / implementation
```

This task is the **contract/planning stage first**.

Do not implement UI changes in this task.

---

## 1. Goal

Prepare a strict implementation contract for the Field Import Preview workflow.

Field Import Preview is a workflow surface, not a simple status window.

The contract must clarify:

```text
1. Current source input path / pasted content flow.
2. Current parse / normalize / validate flow.
3. Current target scope selection: Project / Global.
4. Current preview diff model.
5. Added / Changed / Same / Invalid classification.
6. Warning and disabled reason display.
7. Apply boundary and confirmation behavior.
8. Backup manifest behavior.
9. Reload behavior after apply.
10. Existing tests and AutomationIds.
```

The goal is to make the workflow clearer later without changing its semantics.

---

## 2. Hard Boundaries

Do not modify source files in this planning task.

Do not change:

```text
XAML
code-behind
ViewModels
tests
scripts
field registry JSON
solution/project files
```

Do not change behavior in any future implementation unless explicitly approved:

```text
harvest parser behavior
normalization behavior
validation behavior
preview draft generation
diff classification
target scope behavior
apply writer behavior
backup manifest behavior
rollback behavior
reload behavior
field registry priority
diagnostics
completion
hover
quick peek
save preflight
undo / redo
dirty state
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

## 3. Documents to Read First

Before inspecting source, read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/FieldRegistrySurfacesUiContract.md
Docs/FieldRegistryTertiarySurfacesUiContract.md
```

Then inspect the current Field Import Preview implementation.

---

## 4. Required Inspection

Find and report the exact implementation paths for:

```text
Field Import Preview window XAML
Field Import Preview code-behind
related ViewModel/DataContext
preview draft/result model
target scope model
import/apply command handlers
parser / normalizer / validator entry points, read-only only
backup manifest/apply integration entry points, read-only only
existing tests
existing AutomationIds
```

For the target window, report:

```text
WindowStyle
ShowInTaskbar
ResizeMode
SizeToContent
WindowStartupLocation
Width
Height
MinWidth
MinHeight
Owner behavior
Show / ShowDialog path
```

Do not edit files during inspection.

---

## 5. Workflow Classification

Classify the current Field Import Preview workflow into these sections:

```text
Step 1: Source
  - pasted text / selected file / current INI / remote preset, if supported
  - source name
  - source status

Step 2: Parse and Validate
  - parse result
  - normalized fields
  - validation issues
  - warnings

Step 3: Target
  - Project active registry
  - Global active registry
  - target path
  - disabled reason

Step 4: Preview Diff
  - Added
  - Changed
  - Same
  - Invalid
  - Skipped
  - Warnings

Step 5: Review
  - preview rows
  - grouped/filterable status
  - row details / reason / source

Step 6: Apply
  - apply target
  - apply mode
  - confirmation boundary
  - backup manifest behavior
  - reload behavior
```

If any section does not exist in the current implementation, report it as missing rather than inventing it.

---

## 6. Proposed Future UI Direction

The future UI should be a step-based workflow dialog.

Preferred visual language:

```text
custom lightweight tool-window chrome if consistent with Field Registry windows
Chinese-first labels
step headers
status chips
summary cards
warnings/invalid area
preview result table only where useful
clear disabled reason
explicit write/apply boundary
```

Avoid:

```text
large unstructured WPF form
ambiguous button clusters
mixing read-only parse/preview actions with apply actions
full paths as primary visual text
default WPF chrome unless intentionally retained
English-heavy user-facing labels
hidden target scope
hidden disabled reasons
```

---

## 7. Output Required

Create or update:

```text
Docs/FieldImportPreviewWorkflowContract.md
```

Use this structure:

```markdown
# Field Import Preview Workflow Contract

## 1. Scope and Baseline

## 2. Current Implementation Inventory

| Area | File / Type | Current Role | Writes State | Risk |

## 3. Current Workflow

### 3.1 Source
### 3.2 Parse and Validate
### 3.3 Target
### 3.4 Preview Diff
### 3.5 Review
### 3.6 Apply

## 4. Current UX Problems

## 5. Proposed Future Layout

## 6. Required AutomationIds

## 7. Display-only ViewModel Properties Needed

## 8. Commands / Handlers to Reuse

## 9. Semantic Boundaries

## 10. Tests to Add or Update

## 11. Risks

## 12. Recommended Implementation Plan

## 13. Acceptance Criteria
```

---

## 8. Proposed Future Implementation Phases

The contract should propose implementation phases, but do not implement them yet.

Recommended split:

```text
A15-2C-1: Import Preview source/target/status layout
A15-2C-2: Preview diff summary cards and warning/invalid display
A15-2C-3: Review table grouping/filtering polish
A15-2C-4: Apply boundary / confirmation text consistency
A15-2C-5: Custom chrome consistency if not already covered
```

If inspection shows a smaller safe implementation is possible, propose it.

---

## 9. Tests to Plan

Plan tests for:

```text
1. Existing AutomationIds remain.
2. New workflow section AutomationIds exist.
3. Added / Changed / Same / Invalid counts display correctly.
4. Project / Global target display is correct.
5. Disabled reason display is correct.
6. Opening preview does not write files.
7. Apply still uses existing command/confirmation flow.
8. Backup/reload behavior is not changed by layout-only work.
```

Avoid pixel-perfect tests.

---

## 10. Validation Commands

For this documentation-only contract task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If no prior build output exists:

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
1. Phase completed: A15-2C Field Import Preview workflow contract.
2. Files changed.
3. Files inspected.
4. Current workflow summary.
5. High-risk points.
6. Proposed implementation phases.
7. Commands run.
8. Test result.
9. Package result.
10. Confirmation no XAML/code/ViewModel behavior changed.
11. Confirmation Field Registry semantics unchanged.
12. Confirmation legacy not restored.
13. Recommended next phase.
```
