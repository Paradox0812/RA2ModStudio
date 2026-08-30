# RA2IniEditor.IDE — ASSET-VOX-4D Git Baseline Context Capsule

更新时间：2026-08-30  
用途：在新 Codex 对话中恢复当前 Git 基线、体素流水线状态和下一安全入口。  
状态：Current / evidence-based capsule

## 1. 新对话开场提示词

将下面内容作为新对话的第一条消息：

```text
请继续 H:\RA2\RA2IniEditor_IDE 项目。

开始前完整读取：
1. AGENTS.md
2. Docs/ContextCapsule_ASSET_VOX_4D_GIT_BASELINE.md
3. Docs/ASSET-VOX-4D_StageLedger.md
4. 当前任务直接相关的 FinalContract / CodeFactAudit

当前远程基线为 commit 5a226dd，分支 codex/content-2d-baseline，远程
https://github.com/Paradox0812/RA2ModStudio.git。先核对 HEAD、upstream 和工作树，
不得从旧 Handoff 推断当前能力。

ASSET-VOX-4D 已实现并自动验证；物理 WPF Save/Open、错模型拒绝、未保存确认框
烟测仍待人工完成。先确认这个验收结果。若验收通过，再为下一阶段做代码事实审计
和详细契约；不要直接修改 Shell、项目 Apply/Save、Provider、public API 或
VOX/VXL/HVA writer。
```

## 2. Git 与仓库事实

```text
Repository root: H:\RA2\RA2IniEditor_IDE
Remote:          https://github.com/Paradox0812/RA2ModStudio.git
Branch:          codex/content-2d-baseline
HEAD:            5a226dd feat: checkpoint asset voxel workflow through ASSET-VOX-4D
Upstream:        origin/codex/content-2d-baseline
Ahead/behind:    0 / 0（本胶囊创建前）
Prior tag:       content-2d01-verified -> 9bee2e1
```

确认事实：

- `5a226dd` 已存在于本地和远程，`origin/HEAD` 指向该分支。
- 本胶囊创建前工作树干净。
- `.gitignore` 已排除 `artifacts/`、`.verify-*`、构建输出和本机密钥文件。
- `.gitattributes` 已将 VOX/VXL/HVA/PAL/GLB 等资产标记为 binary。
- 不要再次 `git init`、Clone 到新目录或重新添加 `origin`。

## 3. 当前体素能力

### 已实现并有自动证据

- 参考图经受控 Tencent Hunyuan Provider 生成 GLB 候选，并在会话内进入本地体素审阅链路。
- GLB/VOX/单 Section VXL 可解码为统一 canonical voxel snapshot；VXL 要求显式 Westwood PAL。
- 已有确定性的 GLB-to-voxel、质量候选、表面平滑、结构差异和三维交互审阅。
- Working Geometry Continuity 已完成：显式采纳后的几何成为后续质量、Agent、上色、固化和导出的唯一工作基线，旧 GLB 不再把模型复原。
- DeepSeek 可基于文本化几何证据提出稀疏几何修复；双轮分歧时进行第三轮仲裁；Host 只执行绑定坐标并保留最低几何安全线。
- 支持主体对称修复和受限中轴短缝连接；不自动填任意孔洞。
- 支持 Agent 初始部件/材质建议，以及人工区域覆盖和人工 cell 覆盖。
- 3D 语义编辑支持左键连续绘制/擦除、单笔划单撤销项、画笔大小、镜像、精确外露表面命中。
- 支持部件/材质两种审阅配色；这些颜色只是标注色，不直接成为游戏内 VOX palette。
- 可显式固化当前候选，并导出经过 canonical codec 回读验证的 MagicaVoxel `.vox` 副本。
- ASSET-VOX-4D 可显式保存和载入项目内 `<模型文件名>.semantic.json` v1 sidecar。

### ASSET-VOX-4D 持久化语义

- sidecar 分开保存：已接受 Agent 建议、人工区域覆盖、稀疏人工 cell 覆盖。
- 使用 working snapshot hash、evidence package hash、manual layer hash 三重绑定。
- 保存采用稳定排序、UTF-8、32 MiB 上限和原子替换。
- 载入先在临时状态完整校验，成功后一次替换；失败不改变当前会话。
- 不保存几何、截图、着色结果或 undo/redo 历史。
- 不支持跨 canonical hash 迁移、强制载入、部分恢复、自动保存或自动发现。

## 4. 当前明确不足

- 4D 的真实 WPF Save/Open、错模型拒绝、未保存确认框尚未由用户完成物理烟测。
- 连续画笔的真实鼠标、100%/125% DPI 和视觉体验仍以用户验收为最终证据。
- 当前语义标注只是上色基础；尚未形成完整、权威的 mask-driven 最终游戏着色流水线。
- 尚无项目级素材 Apply/Save、Artifact Registry 或自动 INI 注册。
- 尚无直接 VXL/HVA writer、多部件 Body/Turret/Barrel 最终 materialization 和简单 HVA 动画写出。
- 尚无游戏内视觉/阴影/法线/炮塔轴心验收，不能宣称 GameReady。
- DeepSeek 正式文本模型只消费文本化几何证据；不要把它描述成可靠的真实图像材质视觉模型。
- Shell 全局关闭/项目切换 dirty guard、sidecar merge/迁移均在 Deferred Governance Queue。

## 5. 最新验证证据

权威来源：`Docs/ASSET-VOX-4D_StageLedger.md`。

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

不同阶段的测试数量不同。不得用 4D 的全量数字倒推所有旧阶段均重新完成真实 Provider 或游戏验收。

## 6. 核心代码与文档入口

### 当前状态与治理

- `AGENTS.md`
- `Docs/README.md`
- `Docs/Codex_CurrentPhase.md`
- `Docs/CurrentCapabilities.md`
- `Docs/DevelopmentRoadmap.md`
- `Docs/DecisionLog.md`
- `Docs/PublicApiLedger.md`

### 最新体素实现证据

- `Docs/ASSET-VOX-4D_PersistentSemanticMaskCodeFactAudit.md`
- `Docs/ASSET-VOX-4D_PersistentSemanticMaskFinalContract.md`
- `Docs/ASSET-VOX-4D_StageLedger.md`
- `Docs/ASSET-VOX-4B-STROKE-1_StageLedger.md`
- `Docs/ASSET-VOX-4B-FIX2_StageLedger.md`
- `Docs/ASSET-VOX-4A_StageLedger.md`
- `Docs/ASSET-VOX-3C_StageLedger.md`
- `Docs/ASSET-VOX-3D_StageLedger.md`
- `Docs/ASSET-VOX-3B_StageLedger.md`

### 关键实现

- `RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml.cs`
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelSemanticSidecarStore.cs`
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelSemanticMaskCompiler.cs`
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelRefinementAiCoordinator.cs`
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelWorkingGeometryState.cs`
- `RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelVoxExportService.cs`
- `RA2IniEditor.Application/Automation/Experimental/VoxelAuthoring/`

## 7. 不得重复的错误路径

- 不让 Host 的旧 enum/schema 或启发式判断取代 Agent 的领域判断；Host 只保留格式、资源和最低安全边界。
- 不把只读候选生成建立在旧 GLB 上并覆盖已采纳 working geometry。
- 不用“单一连通组件”或任意指标改善冒充可靠几何质量。
- 不把保护区、结构区或差异审阅颜色写入最终 palette。
- 不把 VOX 中间产物表述为已生成 VXL/HVA。
- 不在没有独立契约时修改 Shell、项目 Apply/Save、public API 或持久化格式。
- 不通过重复全量测试、重复真实模型调用或多 Agent 堆叠制造成本。

## 8. 下一安全入口

第一步不是继续编码，而是完成 4D 物理验收：

1. 在一个真实项目 VOX 上创建多种部件/材质人工分划。
2. 保存 `.semantic.json`，关闭并重新打开工作区，再载入验证三层恢复。
3. 用另一个模型尝试载入同一 sidecar，确认明确拒绝且当前状态不变。
4. 制造未保存修改后执行载入/切换，确认只出现一次可理解的确认提示。

验收通过后，下一阶段只能单独选择并立约其中一个方向：

- `ASSET-VOX-4E Mask-Driven Colour Materialization`：将已确认的部件/材质 mask 与自然语言风格、PAL/VOX palette 确定性组合为可审阅着色候选；仍不写 VXL/HVA，不接项目 Apply/Save。
- `ASSET-VOX-5A Multipart VXL/HVA Materialization`：调查并冻结 Body/Turret/Barrel、pivot、normal、HVA 和 VXLSE 兼容写出边界；不得跳过 4D 物理验收或直接宣称 GameReady。

若用户没有选择，推荐先进入 `ASSET-VOX-4E`，因为当前人工语义工具已经提供可靠的上色输入基础，而 VXL/HVA 写出仍需要更大的格式和游戏验收契约。

## 9. 本胶囊的证据边界

- `Confirmed`：Git/remote 状态、4D Stage Ledger、现有文件与当前状态文档。
- `NotRun`：本胶囊任务没有重新构建、测试、调用 Provider 或启动 WPF；验证数字来自已提交的 4D Stage Ledger。
- `No runtime impact`：本胶囊只新增文档，不修改代码、测试、UI、API 或持久化实现。
