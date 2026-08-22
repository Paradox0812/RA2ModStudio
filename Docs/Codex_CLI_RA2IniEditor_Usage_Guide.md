# Codex CLI 使用指南 — RA2IniEditor.IDE

## 1. 适用场景

Codex App 出现闪退或窗口消失时，建议用 Codex CLI 作为主要执行入口。

CLI 适合：

```text
明确文档驱动的小范围实现
只读诊断并输出计划
执行 restore/build/test/package
小范围 XAML / ViewModel / 测试改动
```

## 2. 启动方式

从项目根目录启动：

```powershell
cd C:\Users\PC\Desktop\RA2IniEditor_IDE
codex
```

不要从这些目录启动：

```text
C:\Users\PC
C:\Users
Desktop
Downloads
```

## 3. 每个新会话第一步

先让 Codex 读上下文，不改文件：

```text
请先读取 AGENTS.md、Docs/RA2IniEditor_IDE_Full_Codex_Context.md，以及当前任务文档。

不要修改任何文件。请只复述：当前任务目标、允许修改文件、禁止修改文件、禁止改变的语义、必须保留的 AutomationId、验证命令、是否需要确认后实现。
```

## 4. 确认后执行

复述正确后再发：

```text
确认。现在严格按当前任务文档执行有限实现。不得超出文档范围。完成后运行要求的验证命令并报告。
```

## 5. 常用验证命令

完整验证：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 6. UI 失败时不要连续返工

如果 UI 截图不合格，发：

```text
不要继续修改文件。请只诊断当前 UI 与任务文档验收标准不一致的原因，并列出最小修复计划。停下等待确认。
```
