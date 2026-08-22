# Codex CLI Prompt Templates — RA2IniEditor.IDE

## Template A: Context rebuild

```text
请先读取 AGENTS.md、Docs/RA2IniEditor_IDE_Full_Codex_Context.md，以及 Docs/<当前任务文档>.md。

不要修改任何文件。请只复述：
1. 当前任务目标；
2. 允许修改的文件；
3. 禁止修改的文件；
4. 禁止改变的业务语义；
5. 必须保留的 AutomationId；
6. 需要运行的验证命令；
7. 是否需要用户确认后才能实现。

复述完成后停下，等待我确认。
```

## Template B: Approved implementation

```text
确认。现在严格按 Docs/<当前任务文档>.md 执行有限实现。

必须遵守：
1. 不得超出允许文件范围；
2. 不得修改 ShellWindow.xaml / ShellWindow.xaml.cs，除非任务文档明确授权；
3. 不得改变业务语义；
4. 不得恢复 legacy；
5. XAML / ViewModel 改动后必须运行 restore/build/test/package 全验证；
6. 最终报告必须列出文件变更、命令结果、文档更新、风险和下一步建议。
```

## Template C: Diagnostic only

```text
不要修改任何文件。

请只读检查：相关 XAML / code-behind / ViewModel、打开路径、当前测试覆盖、风险、最小实现计划。

输出计划后停下，等待确认。
```

## Template D: Stop failed UI loop

```text
不要继续修改文件。

当前 UI 结果不符合任务验收标准。请只诊断：
1. 哪条验收标准未满足；
2. 当前源码中对应位置；
3. 为什么前一次实现没有命中目标；
4. 最小修复方案；
5. 需要改哪些文件；
6. 哪些文件仍然禁止修改。

输出诊断后停下，等待确认。
```

## Template E: Current A15-2B-P context rebuild

```text
请先读取 AGENTS.md、Docs/RA2IniEditor_IDE_Full_Codex_Context.md、Docs/FieldRegistrySurfacesUiContract.md、Docs/Codex_RA2IniEditor_IDE_A15_2B_P_VisualPolish_Implementation.md，以及 Docs/Codex_RA2IniEditor_IDE_A15_2B_P2_FieldRegistry_CustomChrome.md。

不要修改任何文件。请先复述 A15-2B-P / P2 的任务目标、允许修改文件、禁止修改文件、必须保留和新增的 AutomationId、display-only ViewModel 属性、测试要求和验证命令。复述完成后停下，等待我确认。
```
