# RA2IniEditor.IDE — Compact Codex Context

更新时间：2026-08-30
用途：为新任务恢复最小充分工程上下文；阶段历史不在此重复。

## 1. 产品与仓库身份

RA2IniEditor.IDE 是面向 RA2 / YR / Ares / Phobos 的 source-first INI IDE，技术栈为
.NET 8、WPF、AvalonEdit 和 AvalonDock。唯一构建入口是 `RA2IniEditor.IDE.sln`；旧表格编辑器、
legacy root solution 和 legacy MainWindow 不属于当前产品。

长期目标是自然语言驱动的 Mod 内容生产 Agent，在受控边界内编排 INI、Cameo/Icon、
VOX/VXL 和 SHP 的创建、预览、绑定与验证。

## 2. 文档权威顺序

1. `AGENTS.md`：稳定授权、IDE-only 和验证规则。
2. `Docs/ProductVisionAndRequirements.md`：已接受的最终目标。
3. `Docs/CurrentCapabilities.md`：实现/部分/未实现能力矩阵。
4. `Docs/Codex_CurrentPhase.md`：最新状态、风险和下一安全入口。
5. 当前任务的 Contract / Stage Ledger / Context Capsule。

历史 Handoff、旧 CurrentPhase/Context 快照和旧 “next phase” 只作证据，不是当前指令。

## 3. Solution 与所有权

```text
RA2IniEditor.Core              INI model/parser/schema/validation primitives
RA2IniEditor.Infrastructure    Field Registry、BuiltIn 数据、IO helpers
RA2IniEditor.Application       UI-neutral Query/Diagnostics/Edit Preview 与 voxel core
RA2IniEditor.IDE               WPF Shell、editing/AI/search/asset authoring Host
RA2IniEditor.AssetHost         bounded provider process/workspace/artifact Host
RA2IniEditor.Application.Tests headless contract tests
RA2IniEditor.Tests             IDE/non-UI integration tests
RA2IniEditor.AssetHost.Tests   provider Host tests
```

权威分层：

```text
Provider/model output = untrusted proposal
Application Preview   = deterministic candidate authority
IDE Host              = active session, Apply and Undo authority
Save pipeline         = disk/encoding/backup/rollback authority
User/policy           = external cost, overwrite and final commit authority
```

## 4. 当前已完成能力

### INI IDE

- 源码编辑、项目浏览、导航、Dirty、Undo/Redo、Save Preflight 和 backup/rollback。
- Completion、轻量 Hover、Quick Peek、Find References 和 current/project diagnostics。
- Field Registry `Project > Global > BuiltIn`、Manager、学习/导入预览和已验证的数据质量收口。
- 项目 Search；当前文件 Preview-first Replace All，不自动保存。
- AvalonDock 工作区、Search 浮窗、布局恢复与持久化。

### AI / authoring

- DeepSeek Flash/Pro、SSE streaming、取消/超时、失败分类和资源/隐私边界。
- Chat/Work 显式分权；Work 使用 required structured tool、canonical Preview 和显式 Apply。
- 当前文件与项目 rules/art 多文档 Proposal、Diff、原子内存 Apply/rollback 和 compound Undo/Redo。
- Gateway Query/Validate/Edit Preview、受限语义检索、BuiltIn RA2 Skills 和一次结构化 repair。
- 已有 direct-fire、Projectile/Warhead、SuperWeapon 等受限 source-backed profile；不等于任意对象自动制作。

### Voxel authoring

- `ASSET-VOX-1A/1B`：分离式装配契约、canonical snapshot、受限 VOX/VXL reader、VOX/PNG/SliceStack codec。
- `1C/P2`：独立 AssetHost 与 Tencent Hunyuan remote adapter；真实调用曾生成经 Host 验证的 GLB。
- `1D/1E`：GLB-to-canonical voxel 与自然语言样式计划/确定性 colourizer。
- `1E-UI/3D`：中央体素工作区、VOX 或单 Section VXL/PAL 输入、原生 WPF 3D 审阅。
- `2A/2B/2C/3D`：质量候选、结构证据、Agent 稀疏几何修复、对称修复和受限中轴短缝连接。
- `3A/3B/3C`：生成编排、显式固化/VOX 导出、working-geometry continuity。
- `4A/4B/FIX2/STROKE-1`：Agent 初始语义、人工区域/体素覆盖、精确表面命中和连续笔划。
- `4D`：项目内 `.semantic.json` v1 的显式保存/载入与三重哈希绑定。

## 5. ASSET-VOX-4D 当前基线

状态：Completed / automated verified / physical WPF acceptance pending。

- sidecar 分开保存已接受 Agent 建议、人工区域覆盖和稀疏人工 cell 覆盖。
- 使用 working snapshot hash、evidence package hash、manual layer hash 三重绑定。
- 保存使用稳定排序、严格 UTF-8、32 MiB 上限和原子替换。
- 载入在临时状态完整验证，成功后一次替换；失败不改变当前会话。
- 不保存几何、截图、着色结果、相机或 undo/redo 历史。
- 不支持跨 canonical hash 迁移、强制载入、部分恢复、自动保存或自动发现。

权威文档：

- `Docs/ASSET-VOX-4D_PersistentSemanticMaskCodeFactAudit.md`
- `Docs/ASSET-VOX-4D_PersistentSemanticMaskFinalContract.md`
- `Docs/ASSET-VOX-4D_StageLedger.md`

## 6. Git 基线

```text
Remote:   https://github.com/Paradox0812/RA2ModStudio.git
Branch:   codex/content-2d-baseline
Commit:   5a226ddf1f0dd04dd416bcbae549cc0a648e5d88
Upstream: origin/codex/content-2d-baseline
Tag:      content-2d01-verified -> 9bee2e1a064fadd7dc3e4992037b09393ce2540e
```

当前交接胶囊：`Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`。

不要再次 `git init`、Clone 到新目录、重新添加 `origin` 或从旧 Handoff 推断能力。

## 7. 最新可信验证

来源：`Docs/ASSET-VOX-4D_StageLedger.md`。

```text
Focused 4D tests:       10/10 Passed
RA2IniEditor.Tests:     2892/2892 Passed
Application tests:      302/302 Passed
AssetHost tests:        50/50 Passed
Debug solution build:   Passed, 0 warning / 0 error
IdeOnly clean package:  Passed, 1422 files
Real DeepSeek/Tencent:  NotRun in 4D
Physical WPF 4D smoke:  NotRun
```

这些是阶段完成时的可信证据，不代表后续文档任务重新运行了测试或 Provider。

## 8. 当前明确不足

- 4D 真实 WPF Save/Open、错模型拒绝、未保存确认框尚未完成物理烟测。
- 连续画笔的真实鼠标、100%/125% DPI 和视觉体验仍需用户验收。
- 语义 mask 是上色输入，不是最终游戏 palette 或 GameReady 证明。
- 尚无项目级素材 Apply/Save、Artifact Registry、自动 INI 注册。
- 尚无直接 VXL/HVA writer、多部件最终 materialization 或简单 HVA 动画写出。
- 尚无游戏内视觉、阴影、法线、炮塔轴心验收。
- DeepSeek 正式文本模型只消费文本化几何证据，不应描述成可靠图像材质模型。

## 9. 禁止越界

- 不恢复 legacy solution、MainWindow、表格编辑器或旧对象工作流。
- 不让 Provider/Agent 绕过 canonical Preview、显式 Apply/Save 或 Host 安全边界。
- 不把 VOX 中间产物写成 VXL/HVA 或 GameReady。
- 不在没有独立契约时修改 Shell、项目 Apply/Save、public API、持久化格式或 writer。
- 不让旧 GLB 覆盖已经显式采纳的 working geometry。
- 不把结构/审阅标注色直接写入最终 palette。

## 10. 下一安全入口

先完成 4D 物理验收：

1. 保存并重载一个包含 Agent、区域和 cell 三层的真实 sidecar。
2. 用错误模型载入并确认拒绝且状态不变。
3. 验证未保存修改的单次确认提示。

通过后，只选择一个独立方向：

- 推荐 `ASSET-VOX-4E Mask-Driven Colour Materialization`；或
- `ASSET-VOX-5A Multipart VXL/HVA Materialization`。

两者都不得跳过代码事实审计、详细契约和相应人工/游戏验证。

## 11. 新任务最小读取集

1. `AGENTS.md`
2. 本文件
3. `Docs/Codex_CurrentPhase.md`
4. `Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`
5. 当前任务直接相关的 CodeFactAudit / FinalContract / Stage Ledger

历史细节位于 `Docs/Archive/` 和各阶段文档；不要继续向本文件追加完整阶段日志。
