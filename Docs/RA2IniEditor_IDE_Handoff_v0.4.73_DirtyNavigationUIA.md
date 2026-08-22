# RA2IniEditor IDE Handoff v0.4.73 Dirty Navigation Dialog UIA Smoke

## 目标

v0.4.73 为 v0.4.72 的 dirty navigation 三选一弹窗补充 opt-in UIA smoke。普通 `dotnet test` 默认仍跳过 UI 自动化，不启动 IDE、不弹窗。

## 新增 UIA 覆盖

新增测试文件：

- `RA2IniEditor.UiAutomationTests/Ra2IdeDirtyNavigationSmokeTests.cs`

覆盖场景：

- dirty 后切换 INI 文件会出现 `DirtyNavigation.Dialog`。
- Cancel 分支：不切换文件，编辑器中 dirty 文本保留，磁盘文件不变。
- Discard 分支：放弃当前内存修改并切换到目标 INI，磁盘文件不变。
- Save 分支：保存当前文件、生成 backup、切换到目标 INI。
- Save 分支验证 backup 中仍是旧内容，源文件中是新内容。

## AutomationId

UIA smoke 依赖以下 AutomationId：

- `Shell.Window`
- `Shell.ProjectExplorer`
- `Shell.SourceEditor`
- `Shell.SourceEditor.TextArea`
- `Shell.SourceEditor.RevertInMemoryChangesButton`
- `Shell.OutputTextBox`
- `DirtyNavigation.Dialog`
- `DirtyNavigation.SaveButton`
- `DirtyNavigation.DiscardButton`
- `DirtyNavigation.CancelButton`

说明：现有 Shell output AutomationId 仍是 `Shell.OutputTextBox`，本轮没有为文档中的 `Shell.OutputText` 重命名，避免破坏已有 UIA smoke。

## 运行方式

普通测试：

```powershell
dotnet test -c Release
```

Dirty navigation UIA：

```powershell
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test RA2IniEditor.UiAutomationTests -c Release --no-restore --filter FullyQualifiedName~DirtyNavigation
```

## 保持不变

- 未修改 dirty dialog 业务逻辑。
- 未修改 `ProjectSaveService`。
- 未修改 legacy save。
- 未修改 backup / rollback 核心。
- 未修改 Completion / Add Property / Hover。
- 未修改 Project Explorer 布局。
- UIA 仅使用临时目录 `%TEMP%\RA2IniEditor_DirtyNavigationSmoke_<guid>`。

## 注意

UIA 会操作真实交互桌面。若 UIA 连续失败 1-3 次，应停止反复运行，改为输出失败步骤和日志，交由手动确认。
