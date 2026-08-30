# CONTENT-PROJECT-UI-1 — Work Project Proposal End-to-End Final Contract

日期：2026-08-24
状态：Completed / verified（CPUI-1B -> CPUI-1F）
风险：R4
前置：`AGENT-MODE-2`、`CONTENT-UI-1`、`CONTENT-2D-2`、`CONTENT-2D-3`、
`FIELD-REGISTRY-ART-1` completed

## 1. 结论与用户可见目标

本阶段不重新实现多文档事务、rules/art 模板或 Diff 算法，而是把已有能力接入 Work 模式的
唯一生产链：

```text
Work 请求
  -> 第一次 DeepSeek：意图分析包
  -> 本地选择 project capability
  -> 第二次 DeepSeek：固定项目模板参数
  -> 本地 Project Snapshot currency check
  -> ExpandProjectTemplate
  -> PreviewProject
  -> AI 项目提案卡 + 主工作区 Project Diff
  -> 用户显式“应用到项目”
  -> ApplyProject 原子更新 rules/art 内存 session
  -> 一次 compound Undo/Redo
```

完成后，用户可以在 Work 模式要求为一个**现有 TechnoType**建立 rules/art 图像绑定，IDE 将在
同一提案中展示并可原子应用：

- `rulesmd.ini`（或 classic 配对的 `rules.ini`）：`[ownerSectionId] Image=artSectionId`；
- `artmd.ini`（或 classic 配对的 `art.ini`）：创建/更新 `[artSectionId]`，写入
  `Image=bodyAssetId` 与 `Cameo=cameoAssetId`；
- 非阻塞 Asset Manifest 摘要：列出后续可能需要的 body SHP 和 Cameo 文件。

本阶段不创建完整 Techno，不生成或复制素材，不检查素材是否存在，不自动 Apply，不自动 Save。

## 2. 代码事实回归

### 2.1 已完成且必须复用

| 能力 | 现有权威实现 | 当前状态 |
|---|---|---|
| rules/art 项目模板 | `Ra2AutomationTemplateService.ExpandProjectTemplate` | production / verified |
| Project Plan + Manifest | `Ra2ContentProjectTemplateCompiler` | production / verified |
| Project Preview | `Ra2AutomationProjectEditPreviewService` | production / verified |
| 项目 session owner | `Ra2ProjectDocumentSessionStore` | production / verified |
| 原子内存 Apply/rollback | `Ra2ProjectEditorTransactionCoordinator` | production / verified |
| compound Undo/Redo | 同一 coordinator + Shell command path | production / verified |
| Project Diff 投影 | `Ra2AuthoringDiffProjectionBuilder.Build(Ra2ProjectEditPreview)` | implemented / test-only consumer |
| Shell transaction port | `ShellEditorTransactionPort.ApplyProject` | wired / no AI caller |

### 2.2 当前缺口

1. Work intent capability 白名单没有 `techno-rules-art-binding`。
2. AI Tool Catalog 只声明单文档工具，没有项目模板工具。
3. `Ra2AiAuthoringRequestContext` 只能绑定一个 `Ra2AuthoringSnapshot`。
4. `Ra2AiAuthoringToolAdapter` 只调用 `ExpandTemplate`，不调用 `ExpandProjectTemplate`。
5. `Ra2AiAuthoringCoordinator` 只调用 `Workspace.Preview/Apply`。
6. `Ra2AiEditProposal`、Apply result、Proposal ViewModel 只表达单文档 Preview。
7. Shell 发送链只捕获当前文档；生产代码没有调用 Project Store 的 `CaptureSnapshot`。
8. Diff ViewModel 只调用单文档 Builder overload；现有 Project overload 只有测试消费者。
9. Project Preview 缺少按 PreviewId 定向 Dismiss 的 Workspace 入口。
10. User Guide 已明确记录 AI panel 尚未调用 project template。

因此本阶段是 R4 端到端权威链接入，不是新增底层能力。

## 3. 架构不变量

1. `Ra2ProjectDocumentSessionStore` 继续是活动与非活动 INI 内存状态的唯一 owner。
2. `Ra2IniAuthoringWorkspace` 继续是单文档/项目 Preview 的唯一 active preview gate。
3. `Ra2ProjectEditorTransactionCoordinator` 继续是项目 Apply/rollback/compound Undo 的唯一实现。
4. `Ra2AutomationProjectEditPlan` 继续是 INI 修改唯一真相；Manifest 不是 Apply 输入。
5. `Ra2AuthoringDiffProjectionBuilder` 继续是 Diff 唯一算法；UI 只选择正确 overload。
6. AI 只返回固定模板参数，不返回 raw project plan、raw INI、路径、Apply、Save 或文件系统指令。
7. 同一时刻仍只有一个 active AI proposal；单文档和项目提案不能并存。
8. Work 仍固定为两次模型调用；项目能力不增加第三次调用。
9. Chat 模式继续不暴露任何编辑工具。
10. custom endpoint 继续是 advisory-only，不获得项目编辑能力。

禁止建立第二个 coordinator、第二个 Workspace、隐藏 project session cache、顺序执行两个单文档
Apply，或从 UI 直接调用 TransactionPort。

## 4. 支持范围与明确非目标

### 4.1 v1 唯一支持意图

```text
capability_id: techno-rules-art-binding
template_id: techno-rules-art-asset-binding
template_version: 1
domain_intent_id: art-animation
completion_level: field
```

适用条件：

- 已打开真实项目；
- 项目中存在唯一完整 `rulesmd.ini + artmd.ini` 或 `rules.ini + art.ini` 配对；
- 两个目标文档都属于当前 Project Store membership、可编辑且未超资源上限；
- `ownerSectionId` 是 rules 文档中已存在且可被当前分类器确认为 Techno 的 Section；
- 五个模板参数均存在并通过现有 Application 编译门禁。

活动文档可以是该项目中的任意可编辑 INI；但当当前主题不能唯一提供 Techno ID，模型必须返回
clarification，不能从文件名、最近对话或 art Section 猜 owner。

### 4.2 不属于本阶段

- 新建/注册完整 Techno、Building、Aircraft、Infantry；
- 武器链与 art binding 合并为一个更大模板；
- SuperWeapon、Faction、AI tuple、sound/EVA、particle/radiation 跨文档 profile；
- SHP/Cameo/VXL/HVA 生成、读取、复制、落盘或格式解析；
- Asset Provider 自动调用；
- 素材缺失诊断升级为 error/save blocker；
- Save All、项目级磁盘事务、退出确认新 UI；
- 逐文件或逐 hunk Apply；
- 新 Dock、面板、菜单、工具栏、布局 schema 或默认比例变化。

## 5. Reuse Contract

### 5.1 必须直接复用

- 项目模板：Gateway `ExpandProjectTemplate`；
- 项目预览：Workspace `PreviewProject`；
- 项目应用：Workspace `ApplyProject`；
- 项目事务：现有 `ShellEditorTransactionPort.ApplyProject`；
- 项目撤销：现有 compound Undo/Redo command path；
- Project Diff：现有 Builder project overload；
- 叶风险事实：现有 document preview 的 diagnostics、field trust 与 section disposition；
- 模板参数 JSON 兼容：现有 bounded scalar、argument object/name-value array、clarification 解析器。

### 5.2 允许的最小扩展

- IDE 内部 project authoring availability/request context；
- IDE 内部 project tool definition；
- 现有 Proposal 的严格 document/project discriminated payload；
- Workspace 定向丢弃 active project preview 的 internal 方法；
- Proposal ViewModel 与 Diff ViewModel 的 project branch；
- Shell 的 rules/art pair admission、Project Snapshot 捕获和 UI 事件接线。

### 5.3 明确拒绝

- 复制 `ResolveRulesArtPair` 作为新的最终语义权威。IDE 只做用户体验 admission；Application
  compiler 必须再次执行最终 pairing 校验。
- 为复用方便把 Project Store、Apply 或文件路径开放成 public Agent API。
- 新增 NuGet/Diff 控件/序列化依赖。

## 6. 数据模型契约

### 6.1 Authoring availability snapshot

单次发送前创建内部不可变 availability：

```text
DocumentEditAvailability : existing Ra2AiEditAvailabilityKind
RulesArtProjectAvailability : Available / NoProject / PairMissing /
                              PairAmbiguous / SnapshotUnavailable /
                              ReadOnly / ResourceLimitExceeded
```

第一阶段 intent 分析完成后，按 capability 选择对应维度。项目能力不可用时不发送第二次模型请求，
直接返回本地中文失败信息；普通单文档 Work 能力不受 project availability 影响。

### 6.2 Request Context

现有 request context 扩展为严格二选一：

| Scope | 必有 | 必空 |
|---|---|---|
| Document | `Ra2AuthoringSnapshot` | Project Snapshot |
| Project | `Ra2AutomationProjectSnapshot` + 固定 target paths | Document Snapshot |

Project Context 生命周期从发送前捕获开始，直到 response 被接受、取消或失效；不持久化、不序列化、
不进入对话历史。

### 6.3 Tool adaptation result

适配结果同样严格二选一：

```text
Document -> Ra2IniEditPlan
Project  -> Ra2AutomationProjectEditPlan + Ra2AutomationAssetManifest
```

项目 expansion 失败时 Plan、Manifest 均为空；不得产生 partial proposal。

### 6.4 Unified proposal

`Ra2AiEditProposal` 扩展为 internal discriminated proposal：

```text
Scope            : Document | Project
ProposalId       : non-empty
DocumentPreview  : exactly one with ProjectPreview
ProjectPreview   : exactly one with DocumentPreview
AssetManifest    : required only for Project in this stage
ApplyPolicy      : Normal | Caution | Blocked
RiskSummary      : bounded non-empty display text
```

禁止 nullable payload 同时为空或同时存在。proposal 不保存，不写 layout，不进入 Application public API。

### 6.5 Apply result

现有 UI apply result 扩展为严格二选一：

```text
Document result -> Ra2IniEditApplyResult
Project result  -> Ra2ProjectEditApplyResult
```

成功态必须恰有一个成功 payload；失败态不得伪造成功 payload。Shell 只根据 scope 执行展示后处理，
不再次修改文档。

### 6.6 Ownership table

| 数据 | Owner | 生命周期 | 持久化 |
|---|---|---|---|
| User prompt / UserMode | AI panel | request/session | existing conversation only |
| availability | Shell request preparation | one request | no |
| project request snapshot | Project Store capture | one request/proposal | no |
| intent package | Pipeline | two-call bridge | no |
| Project Plan/Manifest | Application expansion result | proposal | no |
| Project Preview | Workspace | single-use active preview | no |
| Proposal UI state | existing Proposal ViewModel | proposal card/view | no |
| committed document state | Project Store | project session | existing explicit Save only |
| compound Undo | Project coordinator | latest project transaction | no |

## 7. Work 两阶段路由契约

### 7.1 Intent analysis schema

`Ra2AiIntentAnalysisStage` 增加且只增加：

```text
capability_id enum += techno-rules-art-binding
```

一致性规则：

- `outcome=authoring`；
- `capability_id=techno-rules-art-binding`；
- `domain_intent_id=art-animation`；
- `completion_level=field`；
- 用户只是询问 art/rules 原理时必须为 advisory；
- 用户要求完整单位但未明确只做图像绑定时，不能误路由到本 profile；
- 用户要求生成真实 SHP/Cameo 时，本 profile 只能处理明确可分离的 INI binding 部分；若用户要求
  一次完成素材本体，当前能力返回 unsupported/clarification，不能声称素材已创建。

### 7.2 Capability mode

内部 `Ra2AiCapabilityMode` 增加：

```text
ProjectRulesArtBindingPreview
```

该 mode 只暴露项目模板工具；不得同时暴露 raw single-document operations tool。

### 7.3 第二次调用工具

工具名：

```text
expand_ini_project_content_template
```

固定 schema：

```json
{
  "outcome": "proposal | needs_clarification",
  "template_id": "techno-rules-art-asset-binding",
  "template_version": 1,
  "arguments": {
    "ownerSectionId": "existing techno id",
    "artSectionId": "art section id",
    "bodyAssetId": "future body SHP stem",
    "cameoAssetId": "future cameo SHP stem",
    "assetBrief": "bounded user-visible asset brief"
  },
  "message": "optional bounded display message"
}
```

继续复用现有非严格 provider 兼容边界：唯一可解释的 missing outcome、arguments object 与
name/value array 可规范化；未知属性、重复参数、复合 value、空 ID、路径字符和歧义 shape 继续
fail closed。错误信息不得回显完整参数或敏感内容。

## 8. Project Snapshot 与 currency

### 8.1 发送前捕获

Shell 从当前 Project Explorer membership 中查找精确文件名候选：

- `rulesmd.ini` / `artmd.ini`；
- `rules.ini` / `art.ini`。

IDE admission 只决定是否值得捕获，不取代 Application 最终 pairing 权威。目标路径全部交给现有
Project Store `CaptureSnapshot`；活动编辑器文本必须与 store active session 一致。允许读取尚未缓存的
目标文档进入内存 session，但不写盘。

### 8.2 response 后复核

在 tool call 进入 adapter/coordinator 前，按原 target paths 再捕获一次 current Project Snapshot，
并逐项比较：

- ProjectSessionId、ProjectRevision、ProjectRootPath；
- 文档数量与顺序；
- DocumentId、FilePath、Version、Text；
- Field Registry provider/revision。

任一变化返回 `RequestContextStale`，不调用 expansion/preview，不保留 partial payload。

### 8.3 Preview/Apply currency

Proposal 创建后继续依赖现有 Project Preview identity 和 Project transaction coordinator 的
validate-all/prepare-all/commit-all currency gate。UI 不新增第二个 currency evaluator。

## 9. Adapter 与 Coordinator 契约

### 9.1 Adapter

Adapter 按 tool name + request scope 分派：

- 单文档工具 + Document Context：保持现有行为；
- 项目工具 + Project Context：解析固定参数并调用 Gateway `ExpandProjectTemplate`；
- scope/tool 不匹配：`UnsupportedTool`；
- expansion 失败：映射为本地 typed failure；
- expansion 成功：返回 Plan + Manifest；不调用 Preview/Apply。

### 9.2 Coordinator

Coordinator 仍持有一个 active proposal slot：

1. invalidate 旧 Workspace preview；
2. 校验 response 恰好一个 tool call；
3. 校验 request/current snapshot currency；
4. 调用 Adapter；
5. Document 分支调用现有 `Preview`；Project 分支调用现有 `PreviewProject`；
6. 聚合 risk policy 后发布一个 proposal；
7. generation 不匹配时定向丢弃对应 preview。

### 9.3 Project risk policy

对全部 document previews 聚合：

- 任一叶新增 error：`Blocked`；
- 任一叶新增 warning、未知字段、非 Verified/ManualCurated trust 或非 Normal Section disposition：
  `Caution`；
- 否则 `Normal`。

Asset Manifest requirement 或素材不存在不是 error/warning，不改变 ApplyPolicy。

### 9.4 Apply / Dismiss / Invalidate

- Document proposal：行为完全不变；
- Project proposal：`ApplyConfirmed` 只调用 Workspace `ApplyProject`；
- Apply 必须传 `ExplicitConfirmationGranted=true`；
- Dismiss 使用新增 internal `TryDiscardActiveProjectPreview(ProjectPreviewId)`；
- Invalidate 继续一次清空 document/project active preview；
- 成功 Apply 后项目结果保留一次 compound Undo/Redo；
- 任一失败保持两个文档原状，不静默重试、不自动回退为两个单文档 Apply。

## 10. 精确 UI 契约

### 10.1 不改变的 Shell 结构

- 不修改 `ShellWindow.xaml`；
- 不新增 Dock、ToolWindow、主编辑 pane 或布局 persistence；
- 继续复用 `Document.AuthoringDiff` 临时 LayoutDocument；
- 继续保留 Chat/Work 选择器、AI 面板尺寸和所有现有 AutomationId。

### 10.2 项目提案卡

Document proposal 的现有文案和视觉保持不变。Project proposal 显示：

```text
建议修改当前项目                         [可应用/需要复核/已阻止]
Bind <owner> to art and asset requirements
2 个 INI 文件 · N 项结构化更改
素材待办：<body>.shp、<cameo>.shp（不影响本次 INI 修改）
                         [查看更改] [应用到项目] [忽略]
```

约束：

- 操作明细继续使用现有虚拟化 ListBox；项目态按文件名分组，最多显示现有 240 DIP 高度；
- Manifest 只显示 requirement 文件名，不显示本地路径、不调用 Provider；
- 文案必须明确“素材待办”而不是“已生成素材”；
- Apply 按钮文字为 `应用到项目`，成功消息为“已应用到 2 个内存文档，尚未保存；可使用 Ctrl+Z
  整体撤销”；
- 不增加 Expander、DataGrid、表单式字段或独立 Manifest 面板。

新增 AutomationId：

```text
AiAssistant.EditProposalCard.ProjectSummary
AiAssistant.EditProposalCard.AssetManifestSummary
```

### 10.3 主工作区 Project Diff

标题：

```text
项目修改预览：rulesmd.ini + artmd.ini
```

状态行：

```text
AI 项目修改预览 · 2 个文件 · 4 项更改 · +N / -N · M 个差异块 · 错误 E · 警告 W
```

文件顺序严格按 Project Plan：rules 在前、art 在后。每个文件以已有 `FileHeader` row 显示：

```text
rulesmd.ini — rulesmd.ini
... unified diff ...
artmd.ini — artmd.ini
... unified diff ...
```

FileHeader 视觉：`MinHeight=28 DIP`、整行 `UiAccentSoftBrush` 或等价现有 token、文件名
SemiBold、相对路径 secondary text、无 old/new 行号与 change marker；不得新增图标资产、渐变或阴影。

新增 AutomationId：

```text
Shell.AuthoringDiff.FileHeader
```

现有 `Shell.AuthoringDiff.*` 全部保留。

### 10.4 生命周期与导航

- Project proposal 成功后自动打开同一个主工作区 Diff 文档；
- 关闭 Diff 只释放 View，卡片“查看更改”可恢复；
- 新 proposal supersede 旧 proposal；
- Apply/Dismiss/stale 后 Diff 只读失效并按现有生命周期关闭或保留状态；
- “返回源文件”回到发送请求时的活动源文件；
- Project Apply 成功后保持当前活动源文件，不强制跳到 art；
- 当前文件属于变更集时可定位其第一处 change；否则保持原 caret；
- 不实现点击 FileHeader 切换文件，避免在本阶段引入额外导航状态。

### 10.5 响应式行为

沿用 CONTENT-UI-1 的 900/640 DIP 规则。Project 统计在窄宽度时按以下顺序收缩：

1. 隐藏详细诊断文字，保留错误/警告数；
2. 隐藏相对路径，仅保留文件名；
3. 不隐藏 `应用全部`、`放弃修改` 或 `返回源文件`；
4. 不缩小正文/代码字号。

## 11. 素材独立性不变量

```text
INI Project Plan -> Preview -> explicit Apply (independent)
Asset Manifest   -> optional future asset workflow
```

- body/Cameo 文件不存在时，Project Preview、Apply、Undo、Save Current 均可正常进行；
- 本阶段不调用 `IRa2AutomationAssetProvider`；
- Manifest 不影响 diagnostics error count、ApplyPolicy 或 Save Preflight；
- 只有用户未来明确要求导入/生成/放置素材时，才进入独立 Asset Host 契约；
- 不能把“素材待办”写成必须满足的依赖或保存阻塞。

## 12. 允许与禁止文件

### 12.1 实施允许候选

```text
RA2IniEditor.IDE/AI/Ra2AiIntentAnalysisStage.cs
RA2IniEditor.IDE/AI/Ra2AiInteractionRoute.cs
RA2IniEditor.IDE/AI/Ra2AiAuthoringToolCatalog.cs
RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs
RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs
RA2IniEditor.IDE/AI/Ra2AiEditProposalContracts.cs
RA2IniEditor.IDE/AI/Ra2AiAuthoringToolAdapter.cs
RA2IniEditor.IDE/AI/Ra2AiAuthoringCoordinator.cs
RA2IniEditor.IDE/AI/Ra2AiProposalPreparationRunner.cs
RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs（仅 project Dismiss seam）
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringDiffViewModel.cs
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringDiffView.xaml(.cs)
RA2IniEditor.IDE/ViewModels/AI/Ra2AiEditProposalViewModel.cs
RA2IniEditor.IDE/Views/AI/Ra2AiEditProposalView.xaml(.cs)
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs（仅 AI/project capture/proposal wiring）
对应 RA2IniEditor.Tests/IDE 测试
Docs 当前契约、Stage Ledger、状态/能力/用户文档
```

若现有实现证明某候选无需改变，应保持不变，不制造机械 diff。

### 12.2 禁止

```text
ShellWindow.xaml
AvalonDock 默认布局、菜单、工具栏、Project Explorer、底部工具区、状态栏结构
Application Project Template / Project Preview 算法与 public DTO
IRa2AutomationCapabilityGateway public surface
Ra2ProjectDocumentSessionStore ownership/replace-many 算法
Ra2ProjectEditorTransactionCoordinator commit/rollback/Undo 算法
parser、classifier、Field Registry JSON/priority、Diagnostics、Completion、Hover
Save/Backup/Rollback/Save Preflight
Asset Provider/Host、素材文件、网络/模型配置、SSE/timeout/retry
项目文件、NuGet、legacy
```

如实施必须触碰禁止项，立即停止并重新审计，不以“接线需要”为由扩权。

## 13. Public API、持久化与兼容性

- Application exported allowlist 保持 `77`；
- Gateway capability catalog 保持 `9`；
- `IRa2AutomationCapabilityGateway` methods 保持 `11`；
- 不新增/修改 public DTO、enum、interface、method 或 serialized shape；
- 不修改 layout/settings/project/save 格式；
- 单文档 Work 的 tool schema、proposal、Preview、Apply、Undo 与所有 AutomationId 保持兼容；
- 新增 tool/capability/proposal scope 全部是 IDE internal。

若实现发现必须改变 public surface，本契约失效，需单独 R2/R4 API 审批。

## 14. 分阶段连续实施包

| Stage | 内容 | 必选门禁 |
|---|---|---|
| CPUI-1A | 代码事实、最终契约、自审 | 本文档、scope/diff audit、用户批准 |
| CPUI-1B | Project availability + intent capability + project tool schema/prompt | intent consistency、Chat zero tools、Work exactly two calls、schema exactness |
| CPUI-1C | request context + adapter + unified proposal/coordinator | strict union、scope mismatch、no-partial、stale/cancel、single-doc regression |
| CPUI-1D | Shell Project Snapshot capture + Workspace project Dismiss + Apply wiring | pair states、current recapture、ApplyProject only、write count 0、compound Undo/Redo |
| CPUI-1E | Proposal card + Project Diff visual projection | file order/header、stats、Manifest non-blocking、AutomationId、900/640 DIP |
| CPUI-1F | 全量回归、文档收口、clean package | focused + Application full + IDE non-UI + Debug build + package hygiene |

用户批准本契约后，CPUI-1B -> 1F 可连续执行，不需要阶段间重复批准；任一必选门禁失败即停止，
不得削弱测试、放宽权限或绕过 currency 继续。

## 15. 自动化验收矩阵

### 15.1 Intent / model protocol

1. Chat 对同一 prompt 零工具、零项目 capability。
2. Work 普通单文档请求仍走原 capability。
3. Work 明确 rules/art binding 请求得到 `techno-rules-art-binding`。
4. 完整 Techno/素材生成请求不被降级为仅 binding。
5. Project availability 不可用时只调用 intent analysis，不调用 execution。
6. 可用时恰好两次模型调用，第二次只暴露项目模板工具。
7. malformed/duplicate/unknown/mixed JSON fail closed；clarification 不产生 proposal。

### 15.2 Snapshot / adapter / preview

8. 唯一 md pair 与唯一 classic pair 成功。
9. missing pair、both pairs、duplicate names、readonly、oversize 精确失败。
10. response 前后任一 rules/art 文本、revision、registry 或 project session 变化返回 stale。
11. Adapter 只调用 `ExpandProjectTemplate`，不复制模板操作。
12. 成功 payload 含两个 document plans、Manifest、三个 field operations 和必要 section creation。
13. 任一叶 expansion/preview 失败时 Proposal/Manifest/UI payload 全空。
14. cancellation 在 capture、expansion、preview 各阶段可终止且无 partial state。

### 15.3 Coordinator / authority

15. 单文档与项目提案共享一个 active slot；新提案正确 supersede。
16. project risk policy 聚合所有叶文档。
17. Dismiss 只消费匹配 ProjectPreviewId。
18. Apply 按 scope 只调用一次 `ApplyProject`，绝不调用两次 `Apply`。
19. 第二文档 prepare/commit 或 active editor projection 失败时两文档保持 before state。
20. 成功 Apply 更新两个内存 session，不调用 Save/Backup/File.Write。
21. Ctrl+Z/Ctrl+Y 对两个文档整体 Undo/Redo；任一成员 stale 时零变化。
22. 现有单文档 AI Apply/Undo 行为和测试全部通过。

### 15.4 UI / Diff

23. Project card 标题、文件/操作统计、Apply 文案与素材待办准确。
24. Manifest 缺少真实文件不禁用 Apply，不增加 error/warning。
25. Diff 自动打开、关闭可恢复、Dismiss 不可恢复、supersede/stale 禁用 Apply。
26. Diff 恰有两个稳定 FileHeader，rules 在前、art 在后。
27. Project Builder 全局 row/hunk limit 失败时不显示 partial rows。
28. Document proposal 标题、统计、操作列表与视觉完全回归。
29. 全部旧 AutomationId 保留，三个新增 ID 存在。
30. Shell XAML、Dock profiles、layout persistence schema 零变化。

### 15.5 命令门禁

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Focused filters 至少覆盖：IntentAnalysis、ToolContract、PromptBuilder、Pipeline、ToolAdapter、Coordinator、
PreparationRunner、ProjectDocumentSessionStore、ProjectTransactionCoordinator、Workspace、DiffProjection、
DiffUiContract、ProposalViewModel、AiAuthoringShellBoundary。

真实 DeepSeek、电脑操控和屏幕截图不作为自动实现门禁；自动门禁通过后请求用户进行一次真实 Work
请求、Project Diff、Apply、Ctrl+Z、Ctrl+Y、Save Current 人工验收。

## 16. 手工验收脚本

前置：项目含唯一 `rulesmd.ini + artmd.ini`，rules 中存在 `[HTNK]`。

1. 选择 Work。
2. 输入：
   `给现有 HTNK 绑定 HTNKART，body 素材 ID 使用 HTNKBODY，cameo ID 使用 HTNKICON；只生成并预览 rules/art INI 绑定，不生成素材文件。`
3. 确认 AI 卡片显示“建议修改当前项目”“2 个 INI 文件”和非阻塞素材待办。
4. 确认主工作区 Diff 先显示 `rulesmd.ini`，再显示 `artmd.ini`。
5. 确认 rules 包含 `[HTNK] Image=HTNKART`；art 包含 `[HTNKART] Image=HTNKBODY` 与
   `Cameo=HTNKICON`。
6. 不创建任何 SHP 文件的情况下点击“应用到项目”，确认成功且未自动保存。
7. 切换 rules/art，确认两个内存文档都已更新并为 dirty。
8. Ctrl+Z 一次，确认两个文档整体恢复；Ctrl+Y 一次，确认整体重做。
9. 分别按现有 Save Current 流程保存；确认没有隐式 Save All。

## 17. 停止条件

- 需要修改 Application public API 或 Project transaction owner；
- 需要改变 parser、Field Registry、diagnostics、save、backup 或 persistence；
- 只能通过两个顺序单文档 Apply 才能完成；
- Project Snapshot 无法在模型调用前后稳定复核；
- 需要自动调用 Asset Provider 或要求素材存在；
- UI 需要新增 Dock/布局 schema 或超出本契约的自由重设计；
- focused/build/full/package 任一必选门禁失败且修复超出允许文件。

## 18. 自我审查

### 18.1 已封闭的返工风险

- 没有重复项目事务或 Diff 算法；
- request/current Project Snapshot 双捕获解决模型等待期间 stale；
- document/project 严格二选一 Proposal 避免 nullable 混态；
- 一个 active slot 保持既有 supersede/模式禁用行为；
- Apply/Dismiss 均经现有 Workspace，不把权威下放 UI；
- Intent 仍两调用，不用关键词路由作为最终语义；
- Manifest 明确非阻塞，避免 INI 与素材生命周期耦合；
- Public API、持久化和 Shell 布局保持冻结。

### 18.2 接受的剩余边界

- v1 只绑定现有 Techno，不创建完整对象；
- 项目工具仍依赖 DeepSeek 正确提供五个参数，但本地 compiler 是最终事实门；
- UI 只展示 Manifest 摘要，不提供素材操作入口；
- FileHeader 不支持点击导航；
- 真实 provider 遵循度、1920x1080 与窄 AI 面板体验仍需用户人工验收。

### 18.3 审查结论

契约自审通过，用户已明确确认后进入实施；CPUI-1B 至 CPUI-1F 均按本契约完成，未扩大到
Asset Provider、自动 Save、Shell 布局或完整对象创建。

### 18.4 完成说明

用户已确认本契约，CPUI-1B 至 CPUI-1F 已连续实施。最终事实、测试计数和剩余人工验收项见
`AUTOMATION-CONTENT-PROJECT-UI-1_StageLedger.md`。
