# Codex Task: RA2IniEditor.IDE A15-2B-P4 Cleanup Preview Demotion / Layout Fix

## 0. Context

User screenshot review shows that the `字段库概括清理预览 / Cleanup Preview` section in `Field Registry Manager / 字段库高级工具` consumes too much vertical space when no cleanup plan has been built.

User question:

```text
高级工具的清理预览高度有问题，评估它的作用，如果不大的话能否直接删去？
```

Decision:

```text
Do not delete the cleanup preview workflow in this phase.
Demote it into a collapsed / compact advanced cleanup section.
```

Reason:

```text
The cleanup preview is a potentially useful but niche and risky maintenance workflow.
It can build a read-only cleanup plan and then apply cleanup to active field packs.
Deleting it would require removing commands, tests, docs, and workflow semantics, and could remove a safety preview before a write operation.
The current issue is layout weight, not necessarily feature invalidity.
```

---

## 1. Goal

Fix the poor height behavior of the Cleanup Preview area in `Field Registry Manager` without deleting the feature or changing cleanup semantics.

Target surface:

```text
Field Registry Manager / 字段库高级工具
```

Primary goal:

```text
Make cleanup preview visually demoted and compact by default.
```

---

## 2. Allowed Files

Allowed:

```text
RA2IniEditor.IDE/Views/FieldRegistryManagerWindow.xaml
RA2IniEditor.IDE/ViewModels/FieldRegistryManagerViewModel.cs, display-only only if needed
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
RA2IniEditor.Tests/IDE/FieldRegistryManagerViewModelTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project paths.

---

## 3. Forbidden Changes

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
Field Registry loader / writer / apply / rollback / import / learning services
cleanup plan builder service
cleanup apply service
parser / normalization / validation logic
BuiltIn field registry JSON
solution / project files
legacy files
```

Do not change behavior:

```text
BuildCleanupPlan behavior
ApplyCleanupPlan behavior
cleanup plan semantics
cleanup apply semantics
backup/confirmation behavior
rollback behavior
warnings generation behavior
open folder behavior
Field Registry priority
```

---

## 4. Required Pre-Implementation Inspection

Before editing, inspect and report:

```text
1. Current CleanupPreviewPanel XAML structure.
2. Current BuildCleanupPlanButton binding / click handler.
3. Current ApplyCleanupPlanButton binding / click handler.
4. Current cleanup plan row collection / empty state properties.
5. Current RepairPreviewTabs / cleanup DataGrids.
6. Current tests that assert cleanup preview UI.
```

Then implement only if all current command paths can be preserved.

---

## 5. Required Layout Changes

### 5.1 Default compact state

When no cleanup plan has been built, the cleanup section should be compact.

Acceptable patterns:

```text
1. Collapsed Expander style section:
   标题：高级清理 / 字段库清理预览
   摘要：尚未构建清理预览；此功能仅用于整理 active 字段库，不会修改 INI。
   Buttons: 构建清理计划

2. Compact card with a collapsed details area:
   Summary row + Build button
   Details visible only when plan rows or warnings exist
```

Preferred:

```text
Use Expander or equivalent compact section.
Default IsExpanded=false if no plan exists.
```

### 5.2 After build

When cleanup plan exists:

```text
1. Show summary counts.
2. Show preview table inside a bounded scroll area.
3. Keep Apply Cleanup visually separated as a write/risk action.
4. Do not let preview table dominate the full window height.
```

### 5.3 Height constraints

Requirements:

```text
1. Cleanup preview area must not create a large blank region when empty.
2. Preview table must have a bounded MaxHeight.
3. If content is large, internal scroll handles it.
4. Warnings area remains separate and scrollable.
```

---

## 6. Visual / Text Requirements

Use Chinese-first wording.

Recommended labels:

```text
高级清理
字段库清理预览
构建清理计划
应用清理
预览为空
构建清理计划后，将显示候选、摘要、将删除、跳过、警告等结果。
应用清理会写入 active 字段库，并保留原有确认与备份流程。
```

Avoid:

```text
large empty DataGrid
always-expanded cleanup area
write action mixed with read-only build action
unclear risk wording
```

Do not translate actual keys, sections, paths, file names, or data values.

---

## 7. AutomationId Requirements

Preserve existing:

```text
FieldRegistryManager.CleanupPreviewPanel
FieldRegistryManager.BuildCleanupPlanButton
FieldRegistryManager.ApplyCleanupPlanButton
FieldRegistryManager.RepairPreviewTabs
FieldRegistryManager.CleanupPlanGrid
FieldRegistryManager.RepairPreviewSummary
FieldRegistryManager.RepairPreviewAbstractFieldsGrid
FieldRegistryManager.RepairPreviewRemovedConcreteGrid
FieldRegistryManager.RepairPreviewSkippedGrid
FieldRegistryManager.RepairPreviewWarningsList
FieldRegistryManager.CleanupStatusText
```

Allowed additions:

```text
FieldRegistryManager.CleanupPreviewExpander
FieldRegistryManager.CleanupPreviewSummaryCard
FieldRegistryManager.CleanupPreviewEmptyState
FieldRegistryManager.CleanupPreviewScrollHost
FieldRegistryManager.CleanupWriteWarning
```

Do not remove or rename existing AutomationIds.

---

## 8. Tests

Update boundary tests only.

Required checks:

```text
1. CleanupPreviewPanel still exists.
2. BuildCleanupPlanButton still exists.
3. ApplyCleanupPlanButton still exists.
4. CleanupPreviewExpander or compact host exists.
5. Empty state exists.
6. Existing cleanup grids/tabs remain present.
7. Apply cleanup remains in a write/risk warning area.
```

Avoid pixel-perfect tests.

---

## 9. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 10. Manual Smoke Checklist

After implementation:

```text
1. Open Field Registry Manager.
2. Confirm cleanup preview no longer takes a large blank area when no plan exists.
3. Confirm Build Cleanup Plan button is still visible.
4. Build cleanup plan.
5. Confirm preview appears in a bounded scroll region.
6. Confirm Apply Cleanup remains clearly marked as a write/risk action.
7. Confirm Apply Cleanup still uses the existing confirmation/backup flow.
8. Confirm Warnings section still scrolls independently.
9. Confirm Field Registry behavior is unchanged.
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: A15-2B-P4.
2. Files changed.
3. Cleanup preview layout changes.
4. Existing commands preserved.
5. AutomationIds preserved/added.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation cleanup semantics unchanged.
11. Confirmation Shell unchanged.
12. Confirmation legacy not restored.
13. Manual smoke steps or result.
14. Remaining risks.
```
