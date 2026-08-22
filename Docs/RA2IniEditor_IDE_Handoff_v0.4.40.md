# RA2IniEditor IDE Handoff v0.4.40

## 目标

v0.4.40 在 IDE Shell 的 AvalonEdit Source Editor 上加入最小内存编辑预览。用户必须显式点击 `Enter Edit Mode` 才能编辑当前已加载文件内容；本轮不实现保存、不写磁盘、不接 legacy 保存链路。

## 本轮新增行为

- 默认状态仍为只读预览，状态文案为 `Editor State: Read-only Preview`。
- 点击 `Enter Edit Mode` 后，`SourceTextEditor.IsReadOnly` 切换为 `false`，状态进入 `Editor State: Editing`。
- 用户修改文本后，AvalonEdit `TextChanged` 只同步到 IDE 内部 `Ra2EditableDocumentSession`，状态显示为 `Editor State: Modified`。
- 点击 `Revert In-memory Changes` 会恢复进入编辑模式时的原始文本，并回到只读预览。
- 切换文件或打开项目文件夹会清理内存编辑会话并回到只读预览。

## 明确未实现

- 未实现 Save / Save All / Ctrl+S。
- 未接 `ProjectSaveService`、`ProjectLoader`、`ObjectAggregator`。
- 未实现 completion commit，Enter / Tab 仍只关闭 completion dropdown。
- 未实现自定义 undo/redo 栈。
- 未实现多文件 dirty 状态。

## 关键文件

- `RA2IniEditor.IDE/Editing/IRa2EditableDocumentSessionService.cs`
- `RA2IniEditor.IDE/Editing/Ra2EditableDocumentSessionService.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.Tests/IDE/Ra2EditableDocumentSessionTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2EditableBufferUiBoundaryTests.cs`

## 验证重点

1. 打开项目后 Source Editor 默认为只读。
2. 点击 `Enter Edit Mode` 后可以修改文本。
3. 修改文本后状态显示 `Editor State: Modified`。
4. 点击 `Revert In-memory Changes` 后文本恢复并重新只读。
5. 文件内容没有写入磁盘。
6. Completion Preview 的 Enter / Tab 仍不写入文本。
