# Codex CLI Runbook — A15-2B-P / P2

## 1. Start CLI

```powershell
cd C:\Users\PC\Desktop\RA2IniEditor_IDE
codex
```

## 2. First message: read only

```text
请先读取 AGENTS.md、Docs/RA2IniEditor_IDE_Full_Codex_Context.md、Docs/FieldRegistrySurfacesUiContract.md、Docs/Codex_RA2IniEditor_IDE_A15_2B_P_VisualPolish_Implementation.md，以及 Docs/Codex_RA2IniEditor_IDE_A15_2B_P2_FieldRegistry_CustomChrome.md。

不要修改任何文件。请先复述 A15-2B-P / P2 的任务目标、允许修改文件、禁止修改文件、必须保留和新增的 AutomationId、display-only ViewModel 属性、测试要求和验证命令。复述完成后停下，等待我确认。
```

## 3. Confirm implementation

```text
确认。现在严格按 A15-2B-P / P2 文档执行有限实现。不得修改 ShellWindow.xaml / ShellWindow.xaml.cs，不得改变 Field Registry 语义，不得修改 deferred surfaces，不得恢复 legacy。完成后运行 restore/build/test/package 全验证，并更新 Docs/Codex_CurrentPhase.md 与 Docs/RA2IniEditor_IDE_Full_Codex_Context.md 中的当前阶段状态。
```

## 4. Expected validation

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 5. Manual smoke checklist

```text
Open Field Registry Center.
Confirm custom chrome if P2 was implemented.
Confirm no default icon/system title bar if P2 was implemented.
Confirm Project > Global > BuiltIn is readable.
Confirm Project / Global / BuiltIn status cards are compact.
Confirm search and field count are grouped.
Open Field Registry Manager.
Confirm sections are separated: Status / Entry Actions / Rollback / Cleanup / Warnings.
Confirm Apply Cleanup and Rollback are visibly risk/write actions.
Confirm existing confirmation flow remains.
```
