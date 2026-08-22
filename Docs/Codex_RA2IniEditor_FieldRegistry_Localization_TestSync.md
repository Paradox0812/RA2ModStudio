# Codex Task: RA2IniEditor.IDE Field Registry Localization Test Sync

## 0. Context

The last documentation-only task created:

```text
Docs/FieldEditorAndLearningChromeContract.md
```

No source/XAML/ViewModel changes were intended in that task.

However, `dotnet test --no-build` currently fails because the working tree already contains manual UI text/localization edits from an earlier step.

Reported failures:

```text
1. FieldRegistryRollbackUiBoundaryTests still expects:
   回滚会根据备份清单恢复或删除目标 active pack

2. WpfAutomationHarnessBoundaryTests still expects:
   Project > Global > BuiltIn
```

The user confirmed the failure is caused by manual changes to mixed Chinese/English UI text.

This task is a **test synchronization / boundary assertion cleanup** task.

Do not change product behavior.

---

## 1. Goal

Make tests align with the current approved UI text/localization direction without weakening behavioral coverage.

The intent is:

```text
1. Keep the localized UI text.
2. Update tests that assert stale exact UI strings.
3. Prefer stable AutomationIds and semantic boundary checks over brittle exact long English/mixed-language text.
4. Preserve Field Registry behavior semantics.
```

---

## 2. Allowed Files

Allowed:

```text
RA2IniEditor.Tests/IDE/FieldRegistryRollbackUiBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Only update docs if the current phase/status needs to record that localization test sync was performed.

Do not modify unless absolutely necessary:

```text
FieldRegistryCenterWindow.xaml
FieldRegistryManagerWindow.xaml
FieldRegistryManagerViewModel.cs
```

If source/UI text must be touched to restore consistency, stop and report first.

---

## 3. Forbidden Changes

Do not modify:

```text
ShellWindow.xaml
ShellWindow.xaml.cs
Field Registry loader/writer/apply/rollback/import/learning services
parser / normalization / validation logic
BuiltIn field registry JSON
solution/project files
legacy files
```

Do not change behavior:

```text
Project > Global > BuiltIn priority
reload behavior
cleanup preview behavior
apply cleanup behavior
rollback behavior
warnings generation behavior
open folder behavior
confirmation flow
save / dirty behavior
```

---

## 4. Required Inspection

Before editing, inspect the two failing tests and report:

```text
1. Exact failing assertion.
2. Current UI/XAML text or bound display property being checked.
3. Whether the test should:
   - update expected localized text,
   - switch to AutomationId-based check,
   - or check semantic token presence rather than full sentence.
```

Then apply the minimal fix.

---

## 5. Test Adjustment Rules

### 5.1 For priority display

If UI now uses Chinese-first text such as:

```text
项目 > 全局 > 内置
```

or bilingual chips such as:

```text
项目 Project
全局 Global
内置 BuiltIn
```

then update `WpfAutomationHarnessBoundaryTests` to avoid requiring the exact old string:

```text
Project > Global > BuiltIn
```

Preferred checks:

```text
1. Priority strip AutomationId exists.
2. Project / Global / BuiltIn priority chips AutomationIds exist.
3. Or assert Chinese localized text plus source-order semantics.
```

Acceptable tokens:

```text
项目
全局
内置
Project
Global
BuiltIn
```

Do not weaken the test to only check that "some text exists".

### 5.2 For rollback description

If rollback help text was localized or reworded, update `FieldRegistryRollbackUiBoundaryTests` to assert the current approved meaning.

Preferred checks:

```text
1. Rollback panel AutomationId exists.
2. RollbackSelectedButton exists.
3. Rollback disabled/status text exists.
4. Help text still communicates that rollback restores/deletes target active pack according to backup manifest.
```

Avoid brittle assertion on the exact old sentence if the UI now uses localized/chinese-first wording.

---

## 6. Validation Commands

Run full or at least test validation.

Preferred:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is stale or XAML changed unexpectedly, run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 7. Acceptance Criteria

Accepted when:

```text
1. Tests pass.
2. No product behavior changed.
3. No Field Registry services changed.
4. UI text localization remains consistent.
5. Tests still verify priority/rollback semantic boundaries.
6. IdeOnly package passes.
7. Legacy is not restored.
```

---

## 8. Final Report Format

Report:

```text
1. Files changed.
2. Failing assertions found.
3. How each assertion was updated.
4. Commands run.
5. Test result.
6. Package result.
7. Confirmation product behavior unchanged.
8. Confirmation Field Registry semantics unchanged.
9. Confirmation legacy not restored.
10. Remaining risks.
```
