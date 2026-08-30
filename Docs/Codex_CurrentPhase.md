# RA2IniEditor.IDE — Current Phase

更新时间：2026-08-30
状态类型：CurrentStatus / concise index

## 1. 当前产品目标

RA2IniEditor.IDE 是面向 Red Alert 2 / Yuri's Revenge / Ares / Phobos 的
IDE-only、source-first INI 开发环境。长期目标是让 Agent 在显式权限和确定性验证边界内编排
INI、Cameo/Icon、VOX/VXL 与 SHP 内容生产。

当前安全默认仍是 Preview、显式 Apply/Save 和本地验证；模型输出不是编辑、持久化或游戏正确性的权威。

## 2. 最新可信状态

### 最新已提交 Git 基线

```text
Repository: H:\RA2\RA2IniEditor_IDE
Remote:     https://github.com/Paradox0812/RA2ModStudio.git
Branch:     codex/content-2d-baseline
Commit:     5a226ddf1f0dd04dd416bcbae549cc0a648e5d88
Upstream:   origin/codex/content-2d-baseline
```

该提交已同步到远端，并以 `feat: checkpoint asset voxel workflow through ASSET-VOX-4D`
固化当前代码基线。当前交接入口是
`Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`。

### 最新完成阶段

`ASSET-VOX-4D Persistent Semantic Mask`：Completed / automated verified / physical WPF acceptance pending。

- 体素语义工作区可显式保存和载入项目内 `<模型文件名>.semantic.json` v1。
- sidecar 分别保存已接受 Agent 建议、人工区域覆盖和稀疏人工体素覆盖。
- 恢复要求 working snapshot、deterministic evidence、manual layer 三重哈希精确匹配。
- 载入先完整验证临时状态；失败不改变当前会话，成功后一次性替换并清空旧画笔历史。
- 不保存几何、色板、RGB、相机、undo/redo 或临时笔划。
- 不支持自动保存/发现、跨 canonical hash 迁移、强制载入或部分恢复。

权威证据：

- `Docs/ASSET-VOX-4D_PersistentSemanticMaskCodeFactAudit.md`
- `Docs/ASSET-VOX-4D_PersistentSemanticMaskFinalContract.md`
- `Docs/ASSET-VOX-4D_StageLedger.md`

## 3. 当前主要能力

### IDE / INI

- .NET 8、WPF、AvalonEdit、AvalonDock；唯一 solution 为 `RA2IniEditor.IDE.sln`。
- source editor、Project Explorer、导航、Dirty、Undo/Redo、Save Preflight、backup/rollback。
- Completion、轻量 Hover、Quick Peek、引用查询、当前/项目诊断和项目 Search。
- Field Registry 保持 `Project > Global > BuiltIn`。

### AI / Agent

- Chat 与 Work 分权；Work 使用结构化 Preview 和显式 Apply，不自动 Save。
- 当前文件与项目 rules/art 多文档 Proposal、Diff、原子内存 Apply、compound Undo 已实现。
- Gateway、受限语义检索、BuiltIn RA2 Skills 和一次性结构化 repair 已实现。
- Provider/model output 始终是不可信提案输入。

### Voxel authoring

- GLB、VOX、单 Section VXL + 显式 PAL 可进入统一 canonical voxel snapshot。
- 已有确定性 voxelization、质量候选、working-geometry continuity、Agent 稀疏几何建议和中轴短缝连接。
- 已有 Agent 初始语义、人工区域/体素覆盖、连续表面笔划、部件/材质审阅配色。
- 可显式固化候选并导出经 canonical codec 回读验证的 MagicaVoxel `.vox` 副本。
- 4D 已补齐项目内语义 sidecar 的显式保存与载入。

## 4. 最新验证证据

来源：`Docs/ASSET-VOX-4D_StageLedger.md`。

```text
Focused 4D tests:      10/10 Passed
RA2IniEditor.Tests:    2892/2892 Passed
Application tests:     302/302 Passed
AssetHost tests:       50/50 Passed
Debug solution build:  Passed, 0 warning / 0 error
IdeOnly clean package: Passed, 1422 files
Real DeepSeek/Tencent: NotRun in 4D
Physical WPF 4D smoke: NotRun
```

这些数字是 4D 阶段账本的历史验证证据；文档维护任务不得把它们描述为重新运行。

## 5. 当前关键边界

- Legacy root solution、legacy MainWindow 和旧表格编辑器不得恢复。
- Shell 全局布局、项目 Apply/Save、Provider、public API、VOX/VXL/HVA writer 未因 4D 改变。
- 模型不直接写文件；Application Preview 是候选权威，IDE Host 是 Apply/Undo 权威，Save pipeline 是磁盘权威。
- 当前语义 mask 是审阅/上色输入，不等于最终游戏 palette、VXL/HVA 或 GameReady 证明。
- 未经独立契约不得修改 parser、Field Registry priority、diagnostics、completion、Save、rollback 或持久化格式。

## 6. 当前不足与风险

- 4D 真实 WPF Save/Open、错模型拒绝和未保存确认框尚未完成物理烟测。
- 连续画笔的真实鼠标、100%/125% DPI 与视觉体验仍需人工确认。
- 尚无项目级素材 Apply/Save、Artifact Registry 或自动 INI 注册。
- 尚无直接 VXL/HVA writer、多部件 Body/Turret/Barrel 最终 materialization 或游戏内验收。
- DeepSeek 正式文本模型只消费文本化几何证据，不能宣称可靠图像材质理解。
- Shell 关闭/项目切换 dirty guard、sidecar merge/迁移仍在 Deferred Governance Queue。

## 7. 下一安全入口

第一步是完成 4D 物理验收：

1. 在真实项目 VOX 上创建 Agent、区域和体素三层分划。
2. 保存 sidecar，关闭/重开工作区并载入，确认三层恢复。
3. 用另一模型载入同一 sidecar，确认明确拒绝且当前状态不变。
4. 制造未保存修改后执行载入/切换，确认只出现一次可理解的提示。

验收通过后，单独选择并立约一个方向：

- 推荐：`ASSET-VOX-4E Mask-Driven Colour Materialization`。
- 备选：`ASSET-VOX-5A Multipart VXL/HVA Materialization`。

不得跳过 4D 物理验收或直接宣称 GameReady。

## 8. 最小继续阅读集

1. `AGENTS.md`
2. `Docs/README.md`
3. `Docs/Codex_CurrentPhase.md`
4. `Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`
5. `Docs/ASSET-VOX-4D_StageLedger.md`
6. 当前任务直接相关的 CodeFactAudit / FinalContract

较早阶段细节按需读取对应 Contract、Stage Ledger 或 `Docs/Archive/`，不要把历史
“next phase” 当成当前指令。
