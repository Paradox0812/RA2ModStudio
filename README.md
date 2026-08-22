# RA2IniEditor.IDE v0.5.0-preview

这是 RA2IniEditor.IDE 的技术预览版源码包，重点验证 RA2/YR INI 字段库、Hover、Quick Peek、Issues 诊断和字段库管理能力。

## 快速验证

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```

## 打包

详见：

```text
Docs/PackagingGuide_v0.5.0-preview.md
```

## 用户说明

详见：

```text
Docs/UserGuide_v0.5.0-preview.md
Docs/ReleaseNotes_v0.5.0-preview.md
Docs/KnownIssues_v0.5.0-preview.md
```

本版本是 preview，不保证所有字段说明完全权威。字段库用于辅助 Hover、Completion 和 Diagnostics，不作为强制保存规则。
