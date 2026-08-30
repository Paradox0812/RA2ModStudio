# RA2IniEditor.IDE — Current Phase

更新时间：2026-08-31
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
Commit:     ab92d56b9b57f89f3c417b0b0f9a0fbf1086e66d
Upstream:   origin/codex/content-2d-baseline
```

该提交已同步到远端；最新提交 `TextUpdate` 固化文档治理结果，其父提交
`5a226ddf1f0dd04dd416bcbae549cc0a648e5d88` 固化 ASSET-VOX-4D 代码基线。当前交接入口是
`Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`。

### 最新完成阶段

`ASSET-VOX-4D Persistent Semantic Mask`：Completed / automated verified；用户已报告真实 Save/Import 通过，
其它物理验收项仍待确认。

- 体素语义工作区可显式保存和载入项目内 `<模型文件名>.semantic.json` v1。
- sidecar 分别保存已接受 Agent 建议、人工区域覆盖和稀疏人工体素覆盖。
- 恢复要求 working snapshot、deterministic evidence、manual layer 三重哈希精确匹配。
- 载入先完整验证临时状态；失败不改变当前会话，成功后一次性替换并清空旧画笔历史。
- 不保存几何、色板、RGB、相机、undo/redo 或临时笔划。
- 不支持自动保存/发现、跨 canonical hash 迁移、强制载入或部分恢复。

`ASSET-VOX-4E Mask-Driven Colour Materialization` Rev.3 已批准，`4E-1..4E-4` 已实现并通过聚焦自动化验证。
Rev.3 要求 DeepSeek 先根据有界几何/语义证据提出
Ground/Air/LargeSurface/Unknown，人工确认或纠正后，Host 只装载对应的一个专用 colouring Skill；模型提案不能
直接取得路由权。人工仍须从 active palette 选择 opaque/non-remap 基准 index，主体明暗家族以该 index 为不可
移动锚点，五个技法只决定层次。合同分离 classification/style 两阶段模型调用与 cache，并保留
BodyGeometryFamily、薄面 DualSurfacePolicy、policy-aware contrast 和多维质量/人工视觉门。
4E-1 已建立 evidence/proposal/confirmation、BaseColour、五个 technique、四个 class-derived adaptation、semantic
requirements/binding 和四个专用 Skill；4E-2 已增加独立 classifier cache、仅接受 confirmation 的 Host exact
single-Skill router、existing compiler 的 semantic binding/cache v2 入口和 normalization identity；4E-3 已接入
base-centred OKLab palette family、dual-surface/semantic/remap precedence、protected contrast 与多维质量门；4E-4 已在
现有 workspace 接入显式判型/人工确认、基准色、技法和质量警告 gate。真实 DeepSeek、截图/DPI、真实模型和游戏
视觉仍为 NotRun/Pending；4E-5 因一个 full-suite-only WPF resource isolation failure 未完成。

`ASSET-VOX-4E-R1 Ground/Air Colour Technique Research` 已完成：只读分析用户提供的 8 个 ZIP，按用户说明并
经 VXLSE III 源码复核采用 `RA2/unittem.pal`，结合公开教程和许可证明确的公开 VOX 模型提炼地面、空中及
大型水面单位上色规则。新增第 19 个 BuiltIn RA2 Skill `ra2-voxel-colour-techniques`，通过窄 Chat domain
路由供通用 DeepSeek 选择；它不授予模型写入权限，也尚未接入专用 4E style compiler。聚焦
`Ra2AgentSkillCatalogTests` 16/16 通过；全量测试、package、真实 DeepSeek 与 WPF 未运行。

权威证据：

- `Docs/ASSET-VOX-4D_PersistentSemanticMaskCodeFactAudit.md`
- `Docs/ASSET-VOX-4D_PersistentSemanticMaskFinalContract.md`
- `Docs/ASSET-VOX-4D_StageLedger.md`
- `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationCodeFactAudit.md`
- `Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md`（Approved）
- `Docs/ASSET-VOX-4E_StageLedger.md`
- `Docs/ASSET-VOX-4E_GroundAirColourTechniqueSourceStudy.md`

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
Physical WPF Save/Import: Passed (user-reported, 2026-08-30)
Wrong-model / dirty / DPI: NotRun or Unknown
```

这些数字是 4D 阶段账本的历史验证证据；文档维护任务不得把它们描述为重新运行。

4E-1 最新证据（来源：`Docs/ASSET-VOX-4E_StageLedger.md`）：

```text
New 4E-1 contract tests: 13/13 Passed
Affected Application:    45/45 Passed
Skill catalog:           18/18 Passed
Affected IDE:            88/88 Passed
Debug solution build:    Passed, 1 pre-existing CS8602 warning / 0 error
Full suites/package:     NotRun (4E-5 gate)
Real DeepSeek/WPF/model: NotRun
```

4E-2 最新证据（同一 Stage Ledger）：classifier/router/compiler/cache focused 26/26，affected Application 49/49，
affected IDE 107/107。4E-3：new materialization 35/35、affected Application 77/77、affected IDE 89/89。4E-4：
workspace UI/ViewModel 25/25，XAML/Debug build 0 warning/0 error。4E-5：Application 350/350、AssetHost 50/50；IDE
full suite 两次均 2913/2914，唯一失败的 WPF visual-resource test 单独 1/1 通过。测试只使用 fake clients；真实
DeepSeek、WPF screenshot/model visual 和 clean package 均未完成。

4E-4 判型兼容修复：真实 Provider 的精确五字段提案允许 enum token 大小写差异与多行 reason 的等价空白归一化；
未知 enum、额外字段、伪造 FactId、越界或 stale evidence 仍 fail closed；安全解包字符串化/arguments 包装后仍执行
精确五字段验证。classifier + workspace ViewModel Release 隔离输出测试 30/30 通过；用户正在运行的 Debug IDE 锁定
对应 DLL，因此需重启/重新构建后才能在当前程序中生效。

## 5. 当前关键边界

- Legacy root solution、legacy MainWindow 和旧表格编辑器不得恢复。
- Shell 全局布局、项目 Apply/Save、Provider、public API、VOX/VXL/HVA writer 未因 4D 改变。
- 模型不直接写文件；Application Preview 是候选权威，IDE Host 是 Apply/Undo 权威，Save pipeline 是磁盘权威。
- 当前语义 mask 是审阅/上色输入，不等于最终游戏 palette、VXL/HVA 或 GameReady 证明。
- 未经独立契约不得修改 parser、Field Registry priority、diagnostics、completion、Save、rollback 或持久化格式。

## 6. 当前不足与风险

- 4D 真实 WPF Save/Import 已由用户报告通过；错模型拒绝和未保存确认框尚未确认。
- 连续画笔的真实鼠标、100%/125% DPI 与视觉体验仍需人工确认。
- 尚无项目级素材 Apply/Save、Artifact Registry 或自动 INI 注册。
- 尚无直接 VXL/HVA writer、多部件 Body/Turret/Barrel 最终 materialization 或游戏内验收。
- DeepSeek 正式文本模型只消费文本化几何证据，不能宣称可靠图像材质理解。
- Shell 关闭/项目切换 dirty guard、sidecar merge/迁移仍在 Deferred Governance Queue。

## 7. 下一安全入口

`ASSET-VOX-4E-1..4E-4` 已完成实现与聚焦自动化验证。下一安全入口是收口 `4E-5`：

1. 先为 full-suite-only 的 `IdeVisualSystemBoundaryTests` WPF DeferredAppResource/Popup dispatcher 隔离失败单独立约
   诊断或修复；不得把 focused 1/1 通过冒充 full-suite pass。
2. 通过完整 IDE suite 后运行 IdeOnly clean package。
3. 请求并记录真实 WPF 截图、100%/125% DPI 和 ground/air/large-surface 样本人工验收；真实 DeepSeek 调用需另行
   明确付费授权。

4D 的错模型拒绝、未保存确认和 100%/125% DPI 指针体验仍应补测，但不得把这些未确认项写成通过；
4E 仍不得宣称 VXL/HVA 或 GameReady。

## 8. 最小继续阅读集

1. `AGENTS.md`
2. `Docs/README.md`
3. `Docs/Codex_CurrentPhase.md`
4. `Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md`
5. `Docs/ASSET-VOX-4D_StageLedger.md`
6. `Docs/ASSET-VOX-4E_StageLedger.md`
7. 当前任务直接相关的 CodeFactAudit / FinalContract

较早阶段细节按需读取对应 Contract、Stage Ledger 或 `Docs/Archive/`，不要把历史
“next phase” 当成当前指令。
