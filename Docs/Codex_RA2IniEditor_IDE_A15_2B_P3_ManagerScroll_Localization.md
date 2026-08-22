# Codex Task: RA2IniEditor.IDE A15-2B-P3 Field Registry Manager Layout Refinement

## 0. Context

A15-2B-P / A15-2B-P2 improved Field Registry Center / Manager visual hierarchy and custom chrome.

User feedback after screenshot review:

```text
1. 当前 active 字段库和警告区域应该可以上下滑动，目前设计不合理。
2. 仍有部分中英文混杂。
3. 当前问题主要集中在 Field Registry Manager / 高级工具界面。
```

This task is a **limited layout refinement** for `Field Registry Manager` only.

Do not touch Field Import Preview, Field Learning Wizard, Field Editor, Allowed Values Editor, or Shell.

---

## 1. Goal

Refine `Field Registry Manager` so that:

```text
1. 当前 active 字段库区域可独立纵向滚动。
2. 警告区域可独立纵向滚动。
3. 长内容区域不会把整窗体撑成不合理的大空白布局。
4. 中英文 UI 文案进一步统一。
5. 现有命令、语义、按钮行为完全保持不变。
```

---

## 2. Allowed Files

Only these files may be modified:

```text
FieldRegistryManagerWindow.xaml
FieldRegistryManagerWindow.xaml.cs, only if strictly needed for local UI wiring with no semantic change
FieldRegistryManagerViewModel.cs, display-only only
WpfAutomationHarnessBoundaryTests.cs
FieldRegistryManagerViewModelTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project paths.

---

## 3. Forbidden Files / Non-goals

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
FieldRegistryCenterWindow.xaml
FieldRegistryHarvestPreviewWindow.xaml
FieldLearningWizardWindow.xaml
FieldEditorWindow.xaml
AllowedValuesEditorWindow.xaml
Field Registry loader / writer / apply / rollback / import / learning services
parser / normalization / validation logic
BuiltIn field registry JSON
solution / project files
legacy files
```

Do not change semantics:

```text
Project > Global > BuiltIn priority
reload behavior
cleanup preview behavior
apply cleanup behavior
rollback behavior
warnings generation behavior
open folder behavior
confirmation flow
```

---

## 4. Required Layout Changes

### 4.1 Current Active Registry section

The section currently titled similar to:

```text
当前 active 字段库
```

must become a bounded content region with vertical scrolling.

Requirements:

```text
1. The active packs/source list must be placed inside a ScrollViewer or an internal scrolling host.
2. The section must have a reasonable max height so it does not over-expand.
3. Long lists should scroll vertically.
4. The header and helper text stay visible above the scroll region.
```

Preserve the existing DataGrid/list behavior and bindings.

### 4.2 Warnings section

The section currently titled similar to:

```text
警告
```

must become a bounded content region with vertical scrolling.

Requirements:

```text
1. Warning list / warning box must be placed in a bounded vertical region.
2. Large warning output should scroll vertically.
3. Empty state should remain compact.
4. The warnings section should not create a huge empty blank region.
```

### 4.3 General vertical sizing

Requirements:

```text
1. Replace layout that relies on oversized fixed vertical regions.
2. Use Grid row sizing more appropriately.
3. Keep main workflow sections readable without forcing excessive whitespace.
4. Preserve resize behavior from the custom chrome window.
```

---

## 5. Localization Cleanup

Continue the Chinese-first UI text cleanup **inside Field Registry Manager only**.

Examples of mixed text that may be adjusted:

```text
active fields -> active 字段库 / 当前 active 字段库
fallback -> fallback / 保底
Loaded -> 已加载
Warnings -> 警告
Project / Global / BuiltIn -> 项目 / 全局 / 内置, or bilingual where useful
```

Rules:

```text
1. User-facing labels should prefer Chinese-first wording.
2. Technical tokens such as Key, Section, INI, file names, paths, and actual field values should not be translated blindly.
3. Keep stable AutomationIds unchanged.
```

---

## 6. AutomationIds

Preserve all existing Field Registry Manager AutomationIds.

Add only if needed:

```text
FieldRegistryManager.ActivePacksScrollHost
FieldRegistryManager.WarningsScrollHost
FieldRegistryManager.ActivePacksSection
FieldRegistryManager.WarningsSection
```

Do not remove or rename existing AutomationIds.

---

## 7. Tests

Update/add only boundary-style tests.

Required checks:

```text
1. Active packs section scroll host exists.
2. Warnings section scroll host exists.
3. Existing key buttons still exist.
4. Existing apply/rollback buttons still exist.
5. New display-only localization text does not mutate state.
```

Avoid pixel-perfect tests.

---

## 8. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Open Field Registry Manager.
2. Confirm “当前 active 字段库” can scroll vertically when content grows.
3. Confirm “警告” can scroll vertically when content grows.
4. Confirm no giant blank areas appear.
5. Confirm reload / import preview / relearn / rollback / cleanup actions still work visually.
6. Confirm mixed-language labels are reduced.
7. Confirm no semantic behavior changed.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: A15-2B-P3.
2. Files changed.
3. Layout sections changed.
4. Localization changes.
5. Tests updated.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation semantics unchanged.
11. Confirmation Shell unchanged.
12. Confirmation legacy not restored.
13. Remaining risks.
14. Recommended next phase.
```
