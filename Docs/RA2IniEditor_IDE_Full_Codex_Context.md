# RA2IniEditor.IDE — Compact Codex Context

更新时间：2026-08-31
用途：为新任务恢复最小充分工程上下文；阶段历史不在此重复。

当前工作树状态（2026-08-31）：ASSET-VOX-4E Rev.4 UI-R1 已实现并通过聚焦自动验证，正在等待用户手动 UI
验收。活动上色链不再调用 DeepSeek 判型；用户人工选择单位类型，Host 确定性路由唯一 colouring Skill。Voxel
Style workspace 已改为五阶段任务流。不得在人工验收前声称 4E-5 或 clean package 完成。详见
`Docs/ASSET-VOX-4E_ManualUnitClassAndWorkspaceUiAmendmentRev4.md`。

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
- `4D`：项目内 `.semantic.json` v1 的显式保存/载入与三重哈希绑定；用户已报告真实 Save/Import 通过。
- `4E`：FinalContract Rev.3 已批准；`4E-1..4E-4` 已实现并聚焦验证。DeepSeek
  将根据有界证据提出
  Ground/Air/LargeSurface/Unknown，人工确认/纠正后 Host 只装载对应的一个专用 colouring Skill。人工还必须从
  active palette 选择 opaque/non-remap 基准 index，主体明暗家族以该 index 为不可移动锚点；五个技法只决定
  层次。4E-1 已提供 evidence/proposal/confirmation、BaseColour、technique/adaptation、requirements/binding、
  classifier/Ground/Air/LargeSurface Skills；4E-2 已提供独立 classification/style cache、confirmation-only exact route、
  semantic binding cache v2；4E-3 已提供 base-centred family/materialization/contrast/quality；4E-4 已接入现有
  workspace 的显式判型确认、基准色、技法和质量警告 gate。4E-5 的完整 IDE suite、package 和物理验证未完成。
- `4E-R1`：8 个用户 ZIP + VXLSE III `RA2/unittem.pal` + 公开教程/许可明确 VOX 模型的上色研究已完成；
  新增 Chat-only `ra2-voxel-colour-techniques` 供通用 DeepSeek 选择，聚焦测试 16/16 通过。专用 4E style
  compiler 尚未接入该 Skill；全量测试、package 和真实模型调用未运行。

## 5. ASSET-VOX-4D 当前基线

状态：Completed / automated verified；Save/Import physical acceptance passed (user-reported)，
错模型/dirty/DPI 项仍为 NotRun/Unknown。

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
- `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationCodeFactAudit.md`
- `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md`（Approved）
- `Docs/ASSET-VOX-4E_StageLedger.md`

## 6. Git 基线

```text
Remote:   https://github.com/Paradox0812/RA2ModStudio.git
Branch:   codex/content-2d-baseline
Commit:   ab92d56b9b57f89f3c417b0b0f9a0fbf1086e66d
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
Physical WPF Save/Import: Passed (user-reported, 2026-08-30)
Wrong-model / dirty / DPI: NotRun or Unknown
```

这些是阶段完成时的可信证据，不代表后续文档任务重新运行了测试或 Provider。

4E-1 最新验证（`Docs/ASSET-VOX-4E_StageLedger.md`）：13/13 新 contract、45/45 affected Application、
18/18 Skill catalog、88/88 affected IDE。4E-2：26/26 classifier/router/compiler/cache、49/49 affected Application、
107/107 affected IDE。4E-3：35/35 new materialization、77/77 affected Application、89/89 affected IDE。4E-4：
workspace UI/ViewModel 25/25，Debug/XAML build 0 warning/0 error。4E-5 full Application 350/350、AssetHost 50/50，
IDE full 2913/2914；唯一 WPF deferred-resource test 单独 1/1 通过但全套仍失败。package、真实 DeepSeek、截图/DPI
和真实模型视觉均未完成。

## 8. 当前明确不足

- 4D 真实 WPF Save/Import 已由用户报告通过；错模型拒绝和未保存确认框尚未确认。
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

`ASSET-VOX-4E-1..4E-4` 已完成。下一安全阶段是收口 `4E-5`：先单独处理 full-suite-only 的 WPF
DeferredAppResource/Popup dispatcher 测试隔离失败；完整 IDE suite 通过后再 clean package；随后记录现有 workspace
截图、100%/125% DPI 和真实 ground/air/large-surface 样本人工验收。真实 DeepSeek 仍需单独付费授权。

4D 的错模型、dirty confirmation 和 DPI 指针项仍需补验，不得推断为通过。4E 不包含 VXL/HVA、
项目 Apply/Save 或 GameReady。

## 11. 新任务最小读取集

1. `AGENTS.md`
2. 本文件
3. `Docs/Codex_CurrentPhase.md`
4. `Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`
5. 当前任务直接相关的 CodeFactAudit / FinalContract / Stage Ledger

历史细节位于 `Docs/Archive/` 和各阶段文档；不要继续向本文件追加完整阶段日志。
