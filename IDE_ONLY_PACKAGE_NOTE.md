# RA2IniEditor.IDE.SourceClean

此包由 Full clean source package 派生，用于 RA2IniEditor.IDE-only 开发与验证。legacy 表格式编辑器已主动分离，不应在本包中恢复。

当前包的基础设施目标是可以独立执行 restore、build、test，并可重新生成 IDE-only clean package。

保留：

- `RA2IniEditor.Core`
- `RA2IniEditor.Infrastructure`
- `RA2IniEditor.IDE`
- `RA2IniEditor.Tests`
- `RA2IniEditor.UiAutomationTests`
- `RA2IniEditor.IDE.sln`
- `tools/`
- 精选 `Docs/`
- BuiltIn v3.2 字段库

排除：

- legacy 根 `RA2IniEditor.csproj`
- legacy `RA2IniEditor.sln`
- old table-style editor application
- legacy `Analysis/RA2/Services/ViewModels/Views` 等旧表格式编辑器代码

测试处理：

- `RA2IniEditor.Tests.csproj` 不再引用 legacy `RA2IniEditor.csproj`。
- 测试根目录定位接受 `RA2IniEditor.IDE.sln`。
- 仍依赖 legacy 根项目或已裁剪历史文档的测试，应改成 IDE-only 语义，或显式视为 Full-only 残留。

推荐验证：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
.\tools\package-source-clean.ps1 -Profile IdeOnly -PackageName RA2IniEditor.IDE.SourceClean.zip
```

Full-only 残留说明：

- 不允许为了通过 IDE-only 验证而恢复 legacy 表格式编辑器。
- `Docs/UserGuide.md` 仍提到 Key-Value 表格工作流，后续需要人工改写或归档。
- `Docs/ReleaseChecklist.md` 仍有偏通用 DataGrid UI 检查项，后续需要按 IDE Shell 体验复核。
- `Docs/FeatureOverview.md` 和 `Docs/DeveloperNotes.md` 本轮未重写。
