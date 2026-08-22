# RA2IniEditor IDE Handoff v0.4.19

目标版本：v0.4.19 Field Registry Manager Local Status UI  
稳定化补丁：v0.4.19.1 Field Registry Manager Local Status Stabilization

## 1. v0.4.19 已完成内容

v0.4.19 为 IDE Shell 增加了本地字段库状态窗口 `Field Registry Manager`。

已完成：

1. 顶部工具栏新增 `Field Registry` 入口。
2. 新增非模态 `FieldRegistryManagerWindow`，重复点击会激活已有窗口。
3. 展示 Global active 字段库状态。
4. 展示 Project active 字段库状态。
5. 展示每个 scope 的 field count、warning count 和状态文案。
6. 展示 loader warnings。
7. 支持 `Reload Local Field Registry`。
8. 支持 `Open Global Folder`。
9. 支持有项目时 `Open Project Folder`。
10. Reload 后重建 readonly highlighter provider 并刷新 AvalonEdit。

## 2. 本地字段库路径

Global active 字段库：

```text
%AppData%/RA2IniEditor/FieldRegistry/active/
```

Project active 字段库：

```text
<ProjectRoot>/.ra2inieditor/field-registry/active/
```

本阶段只读取 `active/*.fields.json`，不读取 raw、normalized、cache、backups。

## 3. Provider 链路

v0.4.19.1 后 provider 优先级为：

```text
Project Local
  > Global Local
  > BuiltIn
```

说明：

1. Project provider 和 Global provider 必须是独立 provider。
2. 不应把 Project / Global definitions 合并进同一个 `LocalRa2FieldDefinitionProvider`。
3. `LocalRa2FieldDefinitionProvider` 内部仍保持 `sectionKind -> Global -> Unknown` fallback。
4. `CompositeRa2FieldDefinitionProvider` 负责来源优先级。

## 4. Highlighter Reload

Reload local field registry 后：

1. `FieldRegistryRuntimeService.Reload(projectRootPath)` 重新读取本地 active 字段库。
2. 重新构造 composite provider。
3. ShellWindow 移除旧 `Ra2KnownFieldHighlightingTransformer`。
4. ShellWindow 创建新的 `ReadonlyIniHighlightTokenizer` 和 transformer。
5. AvalonEdit `TextView.Redraw()` 刷新只读高亮。

Highlighter / tokenizer 本身不读取文件、不访问网络。

## 5. v0.4.19.1 稳定化修复

v0.4.19.1 修复：

1. Project provider 优先级修正，确保 Project Local 覆盖 Global Local 和 BuiltIn。
2. Global provider 仍覆盖 BuiltIn。
3. `[ParticleSystems]` 注册表条目映射修正为 `Ra2SectionKind.ParticleSystem`。
4. 补充回归测试，防止 provider 合并策略退化。
5. 确认源码包脚本排除 `.vs/`、`bin/`、`obj/`、`TestResults/`、`artifacts/` 和常见用户文件。

## 6. 未做事项

v0.4.19 / v0.4.19.1 明确未实现：

- GitHub fetch。
- GitHub raw docs parser。
- harvest parser。
- normalize / validate pipeline。
- preview / apply / rollback。
- 字段库编辑器。
- Completion。
- 字段候选项。
- 保存。
- dirty。
- 编辑。
- TextChanged 编辑链路。
- legacy 字段库接入。
- `ObjectAggregator`。
- `ProjectLoader`。
- `ProjectSaveService`。

## 7. 打包注意事项

源码交付包应使用：

```powershell
.\tools\package-source.ps1 -OutputDirectory artifacts -PackageName RA2IniEditor-source-v0.4.19.1.zip
```

脚本应排除：

```text
.git/
.vs/
bin/
obj/
TestResults/
artifacts/
*.user
*.suo
*.vsidx
*.DotSettings.user
```

不要手动删除用户本地目录；打包时排除即可。

## 8. 后续建议

完成 v0.4.19.1 后，再进入 v0.4.20 Harvest Parser Contract / Prototype。不要在 v0.4.19.1 中顺手实现 GitHub、normalize、apply、Completion 或字段库编辑。
