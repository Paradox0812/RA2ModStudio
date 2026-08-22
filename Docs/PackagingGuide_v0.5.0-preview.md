# RA2IniEditor.IDE v0.5.0-preview 打包说明

## 1. 还原、构建、测试

在解决方案根目录执行：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```

每次解压新的 SourceClean 包后，都要先执行 `dotnet restore`。不要在全新 clean 包中直接使用 `--no-restore`。

## 2. 自包含单文件发布包

适合发给普通测试用户：

```powershell
dotnet publish .\RA2IniEditor.IDE\RA2IniEditor.IDE.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
```

输出目录通常位于：

```text
RA2IniEditor.IDE\bin\Release\net8.0-windows\win-x64\publish
```

## 3. 压缩发布包

进入 publish 目录后执行：

```powershell
Compress-Archive -Path .\* -DestinationPath ..\RA2IniEditor_IDE_v0.5.0-preview_win-x64.zip -Force
```

## 4. 发布包应包含

```text
RA2IniEditor.IDE.exe
README.md
ReleaseNotes_v0.5.0-preview.md
KnownIssues_v0.5.0-preview.md
FieldRegistryTrustLevels.md
SmokeChecklist_v0.5.0-preview.md
```

## 5. 发布前建议

发布前至少完成一次真实项目烟测，确认 Hover、Issues、Quick Peek、保存和备份流程没有明显问题。
