# AUTOMATION-HLI-2C First High-Level Agent Loop Code-Fact Audit

审计日期：2026-08-23
状态：Completed / Read-only code-fact audit
前置证据：`Docs/AUTOMATION-HLI-2B_StageLedger.md`

## 1. 审计目标

确认 HLI-2B 之后，距离“首个高层 Agent 闭环”还缺哪些真实能力，并裁决 HLI-2C 是否需要：

1. 新增 public Agent façade 或 workflow DTO；
2. 改造 Gateway、Provider、Prompt 或 A3 Apply；
3. 补齐 Query -> Preview -> explicit Apply -> Diagnostics 的端到端证据；
4. 修复 Apply 后 Problems 仍显示旧文档诊断的宿主刷新缺口；
5. 把本阶段与外部 Agent/CLI、Job Runtime、素材流水线明确分界。

本轮只读源码、测试与权威文档；未修改生产代码、测试、XAML 或项目文件。

## 2. 当前真实能力链

### 2.1 Agent-facing headless 能力

`IRa2AutomationCapabilityGateway` 已公开一个目录方法和四项 typed capability：

```text
GetCapabilities
GetSection
FindReferences
Validate
Preview
```

Gateway 使用显式 immutable document/registry snapshot，不读取 WPF、活动编辑器、磁盘或
Provider 配置。Application exported allowlist 精确为 35，Apply/Save 不在 public surface。

### 2.2 内置 AI 编辑链

当前生产链已经是：

```text
natural-language explicit edit
  -> conservative local route + snapshot/budget preflight
  -> official DeepSeek required structured tool
  -> Ra2AiAuthoringToolAdapter -> bounded Ra2IniEditPlan
  -> Ra2AiAuthoringCoordinator
  -> Ra2IniAuthoringWorkspace
  -> Ra2IniEditPreviewService
  -> typed Gateway Preview
  -> local proposal card
  -> explicit user Apply
  -> Shell transaction + one semantic Undo unit
  -> dirty in-memory document; no automatic Save
```

`Ra2AiAuthoringCoordinator` 已是高层 proposal/confirmation lifecycle owner；另建
`AgentCoordinator`、`AgentSession` 或 `WorkflowFacade` 会形成重叠编排。

## 3. 已确认缺口

### 3.1 缺少一个完整、可复现的闭环测试

现有证据是分段的：

- Gateway tests 验证 Query/Validate/Preview；
- HLI-2B tests 验证 Gateway Preview 进入 Workspace 并 single-use Apply；
- loopback test 验证 Provider tool call 可以形成 locally validated Preview；
- Shell transaction tests 验证 Apply/Undo/Dirty/no-Save。

但没有一项测试把以下事实串成同一 scenario：

```text
Gateway query/diagnostics
  -> structured provider plan
  -> Gateway Preview
  -> Workspace admission
  -> explicit Apply
  -> updated snapshot
  -> post-apply Validate
  -> no automatic Save
```

因此当前可以分别证明组件正确，尚不能用单一证据宣称“首个高层 Agent 闭环已完成”。

### 3.2 Apply 后 Problems 不会立即刷新

`ApplyAuthoringPreviewTransaction` 已正确提交 editor/session/Dirty/Undo；随后
`AiEditProposalView_OnApplyRequested` 只更新 proposal card 和 AI context summary。

Shell 已有 `ShellViewModel.RefreshCurrentFileDiagnostics(currentEditorText, provider)`，并且它
通过现有 neutral diagnostic core 更新 Problems。成功 Apply 分支当前没有调用它，所以用户
可能看到已修改文本与旧 Problems 状态并存，直到手动刷新或其他刷新路径发生。

这是 HLI-2C 唯一确认需要的生产行为补口。

### 3.3 Gateway Query 尚无 IDE production caller

IDE production 当前使用 Gateway 的 descriptor 和 Preview；`GetSection`、`FindReferences`、
`Validate` 仍主要面向未来 Agent/CLI，并由 Application tests 直接验证。HLI-2C 不应为了制造
调用次数而替换现有 PromptBuilder、Problems adapter 或 Language UI 路径。

正确做法是用端到端 contract scenario 证明 Agent 可组合这些 typed capabilities，同时保持
当前 IDE presentation adapters 复用同一 canonical algorithms。

## 4. 复用决策

HLI-2C 必须复用：

- `IRa2AutomationCapabilityGateway` 和固定四能力 catalog；
- `Ra2AiAssistantPipeline` 与 required tool policy；
- `Ra2AiAuthoringToolAdapter` 的 untrusted JSON -> bounded plan；
- `Ra2AiAuthoringCoordinator` 的 proposal generation、policy、single active proposal；
- `Ra2IniAuthoringWorkspace` 的 generation/active slot/single-use Apply；
- Shell transaction 的 live currency、editor sync、Dirty 和 semantic Undo；
- `ShellViewModel.RefreshCurrentFileDiagnostics` 的 Problems 投影；
- Application `Validate` 作为 headless post-apply verification authority。

无需新增 parser、planner、diagnostic mapper、Agent state、session store、preview registry、event bus、
Job、Artifact、generic dispatcher 或 public Apply。

## 5. Public API 与数据模型裁决

HLI-2C public API diff 应为 0：

- Gateway interface、descriptor、IDs、risk、snapshot、plan、result 和 failure 不变；
- Application exported allowlist 保持 35；
- 不新增 Agent/Workflow/Session/Trace public 类型；
- 不新增 serialized shape、configuration key、wire DTO 或 capability ID；
- 不公开 `Ra2AiAuthoringCoordinator`、Workspace、ApplyResult 或 transaction port。

端到端 trace 只存在于测试断言和 Stage Ledger，不建立 production state model。

## 6. 最小生产改动裁决

唯一 production 改动应位于 `ShellWindow.xaml.cs` 的成功 Apply 分支：

1. 读取成功 `Ra2AiEditProposalApplyResult.AuthoringResult.TextToSyncToEditor`；
2. 调用现有 `ShellViewModel.RefreshCurrentFileDiagnostics`；
3. 使用当前 Field Registry provider；
4. 刷新失败不得反转已经成功的编辑事务；
5. stale/blocked/failed/dismissed 不触发 post-apply refresh。

不修改 transaction 本身，不在 Coordinator/Application 中引用 ViewModel，不调用 Save。

## 7. 基线验证

本轮实际运行：

```text
Ra2AutomationCapabilityGatewayTests: Passed 12/12
HLI-2B + DeepSeek loopback + Coordinator + Shell transaction: Passed 30/30
```

最新可信完整基线仍为 HLI-2B：Application 94/94，focused 78/78，full non-UI
2547/2547，IdeOnly clean package 1119 files。

## 8. 被否决的方向

### 新增 public `IAgent` / `AgentWorkflow`

否决。当前没有独立 host、wire identity、permission token、audit/session 或 cost policy；此时公开
Agent façade 会把 IDE provider lifecycle 错写成长期协议，并与未来 A5/R4 host bridge 冲突。

### 把 Apply/Save 加入 Gateway

否决。Apply 需要 live editor、用户确认、currency 和 Undo；Save 需要 backup/rollback。它们
继续属于 Host/User authority。

### 为了 HLI-2C 强制所有 IDE Query 改走 Gateway

否决。现有 UI adapters 已复用同一 Application canonical algorithms；机械切换只增加映射和
回归面，不增加 Agent 能力。

### 新增 Job/Event/Artifact Runtime

否决。首个单次当前文档闭环无需长任务状态；这些属于后续 AUTOMATION-1。

### 使用真实 DeepSeek 作为必选验收

否决。Provider 行为不确定且有外部成本；loopback SSE/tool-call 足以验证本地闭环。真实模型
只作为用户可选手工验收，不作为确定性门禁。

## 9. 大阶段距离结论

若“大阶段”指 `Minimum High-Level INI Capability / HLI-v1`：

- 已完成 9 个路线包：HLI-0A、0B、1A0、1A1、1A2、1B、1C、2A、2B；
- 剩余 1 个路线包：HLI-2C；
- 按路线包计为 9/10，约 90%，但这不是等工作量百分比；
- HLI-2C 实现并通过后，可宣称“当前文件 Query/Diagnostics/Preview + IDE explicit Apply 的
  最小高层 INI Agent 接口闭环完成”。

这不等于完整 Agentic Mod Production Pipeline。独立 Agent/CLI、内容模板、多文件事务、
Job/Event/Artifact、素材流水线和 Runtime Test Host 仍属于后续大阶段。

## 10. 审计结论

HLI-2C 可以在 public API 0 change、单一 production hunk、两类端到端测试的范围内可靠完成。
最小正确方案是补齐 full-loop evidence 和 Apply 后 diagnostics refresh，而不是新增高层 facade。
详细实施边界由 `Docs/AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md` 冻结。
