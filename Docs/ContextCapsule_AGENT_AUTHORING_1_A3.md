# Context Capsule — AGENT-AUTHORING-1-R1 A3

## 1. 当前状态

`AGENT-AUTHORING-1-R1 A3` 已完成实现与自动化验证。项目现在具备一个内部、
单文档、单次消费的结构化编辑事务路径，但没有 A3 用户入口，AI 也不能直接调用它。

## 2. 当前架构事实

- `Ra2IniAuthoringWorkspace` 是活动 Preview 的唯一所有者，只保留一个槽位。
- 调用 Apply 只能提供 PreviewId 和显式确认；实时状态不能由调用方注入。
- 确认后的匹配 Preview 在进入事务端口前被 claim，之后不可重放。
- `ShellEditorTransactionPort` 在提交瞬间读取 Session、Editor、Registry Revision 和 Caret。
- `Ra2IniEditPreviewCurrencyEvaluator` 必须在实时提交前再次通过。
- `Ra2EditorSessionController.ApplyProgrammaticText` 只生成一个新 Session revision。
- 成功 Apply 更新内存 Session、AvalonEdit 文本和一个语义 Undo 单元，但不保存。

## 3. 必须保持的边界

- 不允许绕开 Workspace，直接把 A2 Preview 写进 Editor。
- 不允许调用方提供实时 Session、文本或 Registry Revision。
- 不允许自动保存或调用 Writer/Backup/Rollback。
- A3 类型保持 internal / Experimental。
- 现有单状态程序化 Undo 不是多级事务栈。
- `ShellWindow.xaml`、Dock、AutomationId、AI、Search、Field Registry 和 parser 语义保持不变。

## 4. 关键文件

- `Docs/AGENT-AUTHORING-1-R1_A3_EditorTransactionPortContract.md`
- `Docs/AGENT-AUTHORING-1-R1_A3_StageLedger.md`
- `RA2IniEditor.IDE/Editing/Ra2IniEditApplyResult.cs`
- `RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs`
- `RA2IniEditor.IDE/Editing/IRa2EditorTransactionPort.cs`
- `RA2IniEditor.IDE/Controllers/EditorSession/Ra2EditorSessionController.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`

## 5. 验证基线

- IDE-only Debug build：0 warnings / 0 errors。
- A3 定向及受影响边界：23/23。
- 完整非 UI 测试：2436/2436。
- UIA：NotRun；A3 没有新增用户入口或控件。
- 最终 IdeOnly clean package：`artifacts/RA2IniEditor.IDE.SourceClean.AGENT-AUTHORING-A3.Final.zip`。

## 6. 开放风险

- 编辑器同步异常补偿没有为 AvalonEdit 人工引入故障注入钩子；静态边界锁定了恢复和只读降级路径。
- 提交后的非语义 Shell 状态刷新若异常，不会把已完成的语义提交误报为失败；后续普通 UI 状态刷新会重新同步。
- A3 只保留一个程序化语义 Undo 状态，连续结构化事务会替换上一事务的语义 Undo。
- `AGENT-AUTHORING-A1-TD-001` 仍为 Open / Controlled。

## 7. 下一推荐阶段

`AGENT-AUTHORING-1-R1 A4`

允许范围：

- 单独设计用户可见 Preview/确认入口，并复用现有 Workspace。

禁止范围：

- AI 自动提交、自动保存、多文件事务、绕开 PreviewId、改变 Save/Undo 生命周期、
  parser/diagnostics/Field Registry/Completion 重构。

停止条件：

- 需要新增外部 public API、无法保留显式确认、或需要改变 Shell/Dock/XAML 范围而未获用户确认。

## 8. 新上下文必读顺序

1. `AGENTS.md`
2. `Docs/Codex_CurrentPhase.md`
3. `Docs/AGENT-AUTHORING-1-R1_A3_EditorTransactionPortContract.md`
4. `Docs/AGENT-AUTHORING-1-R1_A3_StageLedger.md`
5. 本胶囊

不要从 A3 内部端口存在推断“AI 已经能修改文件”或“A4 用户界面已完成”。
