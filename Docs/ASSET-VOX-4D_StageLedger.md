# ASSET-VOX-4D — Stage Result Ledger

日期：2026-08-30  
状态：Completed / automated verified / physical WPF acceptance pending

## 4D-0 — Code-fact audit

- 确认三层权威状态分别位于 Agent suggestions、human region overrides、sparse human cell layer。
- 确认 `Ra2VoxelSceneSnapshot.CanonicalHash`、evidence `PackageHash`、manual `LayerHash` 均可直接复用。
- 确认 Infrastructure 已向 IDE 开放 internal `AtomicTextFileWriter`，无需创建平行写入器。

## 4D-1 — Strict sidecar store

- 新增项目内 `.semantic.json` v1 存储器，严格 UTF-8、32 MiB 上限、未知/重复字段拒绝。
- 保存使用稳定排序和原子替换；载入完整验证路径、schema、枚举、资源、三组哈希后才返回临时状态。
- 项目外、目录缺失、reparse point、错模型、错 evidence、错 layer hash 均返回 typed failure。

## 4D-2 — Workspace state integration

- 引入 authoring/saved revision dirty model，覆盖接受/丢弃 Agent、区域编辑/清除、笔划提交和画笔 undo/redo。
- 保存捕获不可变状态；保存期间继续编辑则文件保留旧快照且 UI 仍为 dirty。
- 载入成功后原子替换三层、清空画笔历史、切回 Browse/Semantics 并只失效一次着色/固化候选。
- 载入失败或工作几何竞态时不替换任何语义层。

## 4D-3 — Existing-workspace UI

- 新增 `VoxelStyle.Semantics.SaveSidecar`、`LoadSidecar`、`PersistenceStatus`。
- 只使用现有语义工具栏与原生文件对话框，没有新增复杂面板或 Shell 修改。
- 未保存确认仅在用户真正选定破坏性本地操作后显示；取消文件对话框不会产生多余提示。

## 4D-4 — Verification

- 定向：10/10（store、ViewModel、UI contract）。
- IDE 全量：2892/2892。
- Application 全量：302/302。
- AssetHost 全量：50/50。
- 最终 Debug solution build：0 warning / 0 error；首次测试编译曾报告 1 个既有
  `BuiltInFieldRegistryPackLoaderTests.cs` nullable warning。
- 未进行真实 DeepSeek/Tencent 调用。

## 4D-5 — Package and stop state

- `tools/package-source-clean.ps1 -Profile IdeOnly` 成功。
- 产物：`artifacts/RA2IniEditor.IDE.SourceClean.zip`，1422 files。
- legacy 未恢复；Shell、Apply/Save、Provider、public C# API、VOX/VXL/HVA writer、几何/色板语义均未修改。
- 剩余验证：用户执行一次物理 WPF Save/Open、错模型拒绝与未保存确认框烟测。

## Deferred Governance Queue

- Shell 关闭/项目切换全局 dirty guard：需要 Shell-specific 新批准。
- 跨 canonical hash 迁移、merge、autosave/auto-discovery：不属于 v1，需新格式/兼容性契约。
- 将语义嵌入 VOX/VXL/HVA 或接入项目 Apply/Save：明确禁止，需独立阶段。
