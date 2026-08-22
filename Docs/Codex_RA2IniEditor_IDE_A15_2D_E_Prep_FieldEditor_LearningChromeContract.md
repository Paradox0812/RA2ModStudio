# Codex Task: RA2IniEditor.IDE A15-2D/E-Prep Field Editor / Learning Wizard Custom Chrome Contract

## 0. Context

User feedback:

```text
编辑 / 学习字段界面还是原生 WPF 风格，应该去掉外边框和默认系统图标。
```

These surfaces belong to the tertiary Field Registry window family and should not be freely redesigned together with Manager.

This task is **contract only**.

Do not implement UI changes in this task.

---

## 1. Goal

Prepare a focused custom chrome + visual direction contract for:

```text
1. Field Editor / New Field Editor
2. Field Learning Wizard
```

The contract should clarify how to remove native WPF outer frame while preserving safe workflow behavior.

---

## 2. Hard Boundaries

Do not modify:

```text
XAML
code-behind
ViewModels
tests
scripts
field registry services
solution / project files
legacy files
```

Do not change semantics:

```text
field editor validation
save/apply behavior
field learning parse behavior
build apply plan behavior
apply behavior
target scope behavior
diagnostics/completion/hover behavior
```

This is documentation / planning only.

---

## 3. Required Inspection

Inspect and report for both surfaces:

```text
1. Window XAML path
2. code-behind path
3. ViewModel / DataContext
4. Open/show path
5. Modal / non-modal
6. WindowStyle
7. ShowInTaskbar
8. ResizeMode
9. SizeToContent
10. Width / Height / MinWidth / MinHeight
11. Existing AutomationIds
12. Existing tests
13. Whether the window writes state
14. Current UX problems
```

---

## 4. Required Output

Create or update:

```text
Docs/FieldEditorAndLearningChromeContract.md
```

Suggested structure:

```markdown
# Field Editor and Learning Wizard Chrome Contract

## 1. Scope and Baseline
## 2. Inventory
## 3. Current Window Properties
## 4. Current UX Problems
## 5. Proposed Custom Chrome Rules
## 6. Editor Window Layout Direction
## 7. Learning Wizard Workflow Direction
## 8. AutomationIds to Preserve
## 9. Tests to Add / Update
## 10. Semantic Boundaries
## 11. Risks
## 12. Recommended Implementation Split
```

---

## 5. Proposed Future Direction

The contract should aim for:

```text
1. No default WPF icon.
2. No default system title bar.
3. No normal outer system border.
4. Custom lightweight header.
5. Close button in custom header.
6. Keep move ability.
7. Preserve resize if currently needed.
8. Chinese-first labels.
```

But it must also distinguish:

```text
Field Editor -> structured editor dialog
Field Learning Wizard -> workflow surface
```

So they must not be forced into the same final internal layout.

---

## 6. Recommended Split

The contract should recommend:

```text
A15-2D: Field Editor / Allowed Values Editor
A15-2E: Field Learning Wizard
```

with chrome consistency rules shared, but workflow/layout implementation kept separate.

---

## 7. Validation Commands

For this documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If needed, run full validation.

---

## 8. Final Report Format

Report:

```text
1. Phase completed: A15-2D/E-Prep contract.
2. Files changed.
3. Surfaces inspected.
4. Current chrome problems.
5. Proposed split.
6. Commands run.
7. Test result.
8. Package result.
9. Confirmation no source/UI behavior changed.
10. Confirmation semantics unchanged.
11. Confirmation legacy not restored.
12. Recommended next phase.
```
