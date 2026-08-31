# ASSET-VOX-4E — Manual Unit Class and Task-Oriented Workspace Amendment Rev.4

日期：2026-08-31
状态：Approved / implemented / focused automated verification passed / physical UI acceptance pending
风险：R3（内部路由身份与既有 WPF workspace 重构）
治理：StopForReview
被修订合同：`Docs/ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md` Rev.3

## 1. 修订原因与结论

真实 Provider 判型连续暴露结构化提案兼容问题，而且单位类型本来就必须由人工确认。Rev.4 删除活动工作流中的
DeepSeek 自动判型阶段，改为用户直接选择 `Ground / Air / LargeSurface / Unknown`。Host 从当前 working snapshot
和 semantic composition 生成本地 evidence identity，并以 `HumanManualSelection` 确认来源确定性路由恰好一个
class-specific colouring Skill。分类模型、classifier Skill、classifier cache 和 proposal 均不再是活动上色链依赖。

同时，既有 workspace 的模型载入、几何候选、分划生成、画笔、sidecar、上色和导出入口过度分散。Rev.4 将其整理为
五个互斥任务阶段：`模型 → 几何 → 分划与标注 → 上色 → 审阅与导出`。详情区只显示审阅事实；所有写操作放回对应
阶段任务面板；预览模式改为单一 selector。

## 2. 权威与数据契约

- `SelectedWorkflowStage`：IDE ViewModel session-only enum，不序列化，不进入项目设置或 sidecar。
- `HumanManualSelection`：Application internal confirmation source，要求 proposal 为 null，并绑定当前 EvidenceHash。
- 只有人工 confirmation 可以进入 active colouring router；旧 proposal confirmation 不再获活动路由权。
- Host 只装载 adaptation 指定的一个 colouring Skill；不存在 classifier Skill availability 前置门。
- `Ground / Air / LargeSurface / Unknown` 继续复用 Rev.3 已验证的 adaptation policy；Unknown 继续强制 NeedsReview。
- 人工基准色仍是 active RA2 palette 的 opaque/non-remap exact index，BodyBase 锚点、palette family、semantic/remap
  precedence、quality gate、freeze/export 均不改变。
- classification proposal/cache 类型可作为未接线历史兼容实现暂存，但不得从 workspace、compiler、router、
  materializer 或 quality evaluator调用。

## 3. 五阶段 UI 精确契约

### 3.1 模型

- 载入项目内 VOX/VXL；VXL 继续要求显式 PAL。
- 参考图生成归入本阶段，仍保留显式确认和原 Provider 成本边界。

### 3.2 几何

- 展示 current working geometry、GLB source/provenance。
- 集中 `载入 GLB / 生成候选 / AI 识别结构 / 采用当前候选`。
- 几何处理仍是可选项，不阻止用户直接进入分划。

### 3.3 分划与标注

- 集中 `创建人工区域 / AI 建议 / 接受 / 丢弃 / 载入分划 / 保存分划`。
- 集中浏览、画笔、擦除、部件目标、材质目标、remap、笔刷大小、镜像、undo/redo 和审阅维度。
- sidecar schema/store、原子载入、dirty、哈希和权威优先级完全不变。

### 3.4 上色

- 第一步人工选择并确认单位类型；不得显示或调用“AI 判断单位类型”。
- 第二步选择人工主体基准色和上色规则/技法模板。
- 第三步显示 style sources/override 并显式编译上色预览。

### 3.5 审阅与导出

- 显示 colour quality 状态、metrics、warnings 和当前 generation acknowledgement。
- 集中 `固化最终候选 / 导出 VOX`。
- 详情 tabs 只显示几何摘要、区域清单、上色计划和审阅问题，不承载写操作。

## 4. AutomationId 变更

移除活动 UI：

```text
VoxelStyle.UnitClass.Analyze
VoxelStyle.UnitClass.Evidence
```

保留：

```text
VoxelStyle.UnitClass.Selector
VoxelStyle.UnitClass.Confirm
VoxelStyle.UnitClass.Status
VoxelStyle.UnitClass.Skill
VoxelStyle.Semantics.*
VoxelStyle.BaseColour.*
VoxelStyle.Template.*
VoxelStyle.ColourQuality.*
```

新增：

```text
VoxelStyle.Workflow.StageNavigator
VoxelStyle.Workflow.Model
VoxelStyle.Workflow.Geometry
VoxelStyle.Workflow.Semantics
VoxelStyle.Workflow.Colour
VoxelStyle.Workflow.Review
VoxelStyle.Workflow.NextAction
VoxelStyle.Preview.ModeSelector
VoxelStyle.Semantics.BrushPart
VoxelStyle.Semantics.BrushMaterial
VoxelStyle.Semantics.BrushRemap
```

Shell AutomationId 与全局布局不变。

## 5. 允许与禁止范围

允许修改 Application internal confirmation/materialization/quality identity、IDE router/compiler/coordinator/ViewModel、
既有 workspace XAML/code-behind、直接测试和本合同/状态文档。

禁止修改 Shell、4D sidecar schema/store、Provider/AssetHost protocol、项目 Apply/Save、INI/Field Registry、VOX/VXL/HVA
writer、public .NET API、依赖和 build configuration。不得真实调用付费模型。

## 6. 自动验证结果

```text
Release solution build before manual launch: 0 warning / 0 error
Isolated Release build after launch: 0 error / 1 existing CS8602 warning outside diff
Application focused: 39/39 Passed
IDE focused router/compiler/ViewModel/UI: 39/39 Passed
git diff --check: Passed
```

### UI-R1-FIX1 — selector display binding

用户首次截图验收发现单位类型与基准色下拉列表存在条目和滚动条但文字为空。根因是 XAML 对
`Ra2VoxelUnitClassOption`、`Ra2VoxelPaletteColourOption` 使用了不存在的 `DisplayName`，而两个 record 的真实属性均为
`Display`；技法选项的 `DisplayName` 正确。修复只更正两个 `DisplayMemberPath`，并增加 XAML 属性路径与三类选项
非空显示文本回归测试。最终隔离 Release XAML build 0 warning/0 error，workspace UI/ViewModel 26/26 passed。

正在运行的 Release EXE 锁定常规输出后，未关闭用户程序；最终 XAML build 使用系统临时目录隔离输出完成。临时输出
导致 repository-root UI tests 无法定位源码，因此 UI tests 随后以 `BuildProjectReferences=false` 更新测试程序集并在
仓库标准输出位置复跑，最终 39/39 通过。该环境过程不是产品失败。

## 7. 人工 UI 验收门

用户明确要求自行验收，不允许自动电脑操控。UI-R1-FIX1 后需重新构建并重启程序；正式 4E-5/package 前至少确认：

1. 五个阶段在 100% DPI 下完整可见，当前阶段高亮，左侧只显示该阶段操作；
2. 载入模型、载入/保存分划、画笔目标和导出入口无需在多个 tabs 间寻找；
3. 上色阶段没有 AI 判型按钮或判型证据框，人工选择类型后显示唯一对应 Skill；
4. 基准色与技法 selector、编译按钮、质量警告确认、固化与导出均可操作；
5. 预览 selector、3D viewport、底部四个只读详情 tabs 无遮挡或异常压缩；
6. 125% DPI 再检查一次横向阶段导航、左侧滚动和底部详情高度；
7. 使用一个真实模型完成：载入 → 分划 → 人工类型 → 基准色 → 编译 → 固化 → 导出。

截图和结果由用户提供。未收到通过结论前，本阶段状态保持 `PhysicalUiAcceptance = Pending`，不生成 clean package。

## 8. 当前裁决

```text
Rev.4 contract: Approved
Implementation: Completed
Focused automated verification: Passed
Physical UI acceptance: Pending (user-owned)
Full suites: Not rerun in this amendment
Clean package: NotRun
GameReady: OutOfScope
```
