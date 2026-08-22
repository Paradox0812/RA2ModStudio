# RA2IniEditor.IDE FR-DQ-3I Release Icon and User Docs Report

## 基线

基于 `RA2IniEditor_IDE_FR_DQ_3H_Fix2_TestExpectationAlignment_SourceClean.zip`。

该基线已由用户本地确认：

```text
dotnet build 通过
dotnet test 全绿
```

## 本阶段目标

补齐 v0.5.0-preview 交付前缺失的发布资产：

- 应用图标 / 任务栏图标 / exe 图标。
- 用户说明文档。
- Release Notes。
- Known Issues。
- 字段可信度说明。
- 打包说明。
- 发布前烟测清单。

## 修改内容

### 图标

新增：

```text
RA2IniEditor.IDE/Assets/AppIcon.ico
RA2IniEditor.IDE/Assets/AppIcon.png
```

修改：

```text
RA2IniEditor.IDE/RA2IniEditor.IDE.csproj
RA2IniEditor.IDE/Views/ShellWindow.xaml
```

处理内容：

- 设置 `<ApplicationIcon>Assets\AppIcon.ico</ApplicationIcon>`。
- 将 `AppIcon.ico` 作为 WPF Resource。
- 将 `AppIcon.png` 作为 Content 复制到输出 / 发布目录。
- 为 `ShellWindow` 增加 `Icon="/Assets/AppIcon.ico"`。

### 文档

新增：

```text
README.md
Docs/UserGuide_v0.5.0-preview.md
Docs/ReleaseNotes_v0.5.0-preview.md
Docs/KnownIssues_v0.5.0-preview.md
Docs/FieldRegistryTrustLevels.md
Docs/PackagingGuide_v0.5.0-preview.md
Docs/SmokeChecklist_v0.5.0-preview.md
```

发布包会复制以下文档：

```text
README.md
ReleaseNotes_v0.5.0-preview.md
KnownIssues_v0.5.0-preview.md
FieldRegistryTrustLevels.md
SmokeChecklist_v0.5.0-preview.md
```

## 未修改范围

本阶段没有修改：

- BuiltIn 字段库内容。
- Hover 核心逻辑。
- Diagnostics 核心逻辑。
- 保存链路。
- Completion / Add Property 行为。
- Field Registry runtime 逻辑。

## 静态检查

已完成：

```text
ICO 可由 PIL 识别，尺寸 256x256，RGBA。
csproj XML 可解析。
ShellWindow.xaml XML 可解析。
SourceClean 包不包含 bin/obj/.vs/TestResults/artifacts。
```

当前环境没有 dotnet CLI，未在本环境运行 build/test。请在本地执行：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```

## 建议发布版本

```text
RA2IniEditor.IDE v0.5.0-preview
```
