# RA2IniEditor IDE Handoff v0.4.72 Dirty Navigation Decision Dialog

## 目标

v0.4.72 将 v0.4.71 的 dirty navigation 阻断提示升级为 IDE 风格三选一对话框：

- 保存
- 放弃修改
- 取消

本轮只处理当前 IDE Shell 的 dirty 导航编排，不修改保存底层、legacy 保存链路、Completion、Hover、Add Property 或 Project Explorer 布局。

## 新增结构

- `Ra2DirtyNavigationDecision`
  - `Save`
  - `Discard`
  - `Cancel`
- `IRa2DirtyNavigationDialogService`
  - 只负责显示 dirty navigation 对话框并返回用户选择。
- `Ra2DirtyNavigationDialogService`
  - WPF 实现，打开自定义中文 dialog。
- `Ra2DirtyNavigationDialog`
  - 标题：`未保存的修改`
  - 正文：`当前文件有未保存的修改。是否先保存？`
  - 按钮：`保存`、`放弃修改`、`取消`
  - AutomationId：
    - `DirtyNavigation.Dialog`
    - `DirtyNavigation.SaveButton`
    - `DirtyNavigation.DiscardButton`
    - `DirtyNavigation.CancelButton`

## Shell 行为

`ShellWindow` 中原本的 dirty guard 已替换为：

```text
TryResolveDirtyNavigationBeforeLeavingCurrentFile
```

调用场景：

- Open Folder
- automation open folder
- Project Explorer 选择另一个 INI 文件

行为：

- clean session：直接继续导航，不弹 dialog。
- dirty + Save：调用现有 `IRa2SaveCurrentFileService.Save(...)`。成功后继续导航；失败后停止导航并保留 dirty/editor text。
- dirty + Discard：调用现有 editor session Revert，清除当前内存修改，然后继续导航。
- dirty + Cancel：停止导航，保留当前 dirty session。

## 保持不变

- 未修改 `ProjectSaveService`。
- 未修改 legacy `IniFileService`。
- 未实现 Save All。
- 未修改 backup / rollback 核心服务。
- 未修改 Completion / Add Property / Hover 语义。
- 未移动 Project Explorer。
- 未改 INI 保存、dirty 或编辑底层模型。

## 测试覆盖

新增/调整边界测试：

- dirty dialog 中文文案与 AutomationId。
- dirty dialog service 不直接写文件、不接 ProjectSaveService、不做 SaveAll。
- Shell dirty navigation 通过 dialog service 分支到 Save / Discard / Cancel。
- Shell 保存仍只通过 `IRa2SaveCurrentFileService`。

## 手动验证建议

1. 启动 `RA2IniEditor.IDE`。
2. Open Folder。
3. 打开一个 INI 文件并编辑文本。
4. 在 Project Explorer 点击另一个 INI 文件。
5. 选择取消，确认仍停留在原文件且修改保留。
6. 再次切换，选择放弃修改，确认切换成功且原文件未写盘。
7. 再次编辑，切换时选择保存，确认保存成功后切换。
8. 用只读文件或写入失败场景验证保存失败时不切换且 dirty 保留。
