# AUTOMATION-HLI-2B IDE/AI Gateway Consumer Code-Fact Audit

审计日期：2026-08-22  
状态：Completed / Read-only code-fact audit  
前置证据：`Docs/AUTOMATION-HLI-2A_StageLedger.md`

## 1. 审计目标

核实内置 DeepSeek 当前文件编辑链路应如何成为 HLI-2A Gateway consumer，并冻结：

1. 最小生产改动点与唯一 adapter；
2. public 8 MiB / 10,000 diagnostics 与旧 Host unlimited budget 的兼容策略；
3. provider 调用前的资源门禁，避免注定失败的付费请求；
4. A4 proposal、A3 admission、Apply/Undo/Save 的所有权是否保持不变；
5. HLI-2B 是否需要新增 public API、修改 Application 算法或引入第二套 Preview 路径。

本轮只读取源码、测试和权威文档，未修改生产代码、测试、项目文件或 UI。

## 2. 当前真实调用链

```text
Shell explicit-edit routing
  -> capture Ra2AuthoringSnapshot
  -> official DeepSeek endpoint + required preview_ini_edit_plan tool
  -> Ra2AiAuthoringToolAdapter creates bounded Ra2IniEditPlan
  -> Ra2AiAuthoringCoordinator
  -> Ra2IniAuthoringWorkspace.Preview
  -> Ra2IniEditPreviewService
  -> Ra2AutomationEditPreviewService.PreviewForHost (int.MaxValue budgets)
  -> Ra2IniEditPreview.FromAutomation
  -> Workspace generation + one active slot
  -> explicit user Apply
  -> Shell live currency + one semantic Undo unit
  -> no automatic Save
```

HLI-2A Gateway 当前没有 IDE caller。`Ra2IniEditPreviewService` 是唯一 production Host adapter，
因此 HLI-2B 不需要新增第二个 adapter，也不需要修改 Coordinator 或 Workspace interface。

## 3. 已核实的复用路径

必须复用：

- `IRa2AutomationCapabilityGateway.Preview` 与 `Ra2AutomationCapabilityGateway`；
- 现有 internal `IRa2IniEditPreviewService` admission seam；
- `Ra2IniEditPreview.FromAutomation` 的 identity、operation、span、candidate/change 完整性 guard；
- Workspace generation、active slot、single-use claim；
- A4 provider tool、proposal policy、显式确认和 Shell transaction；
- 既有 typed `DocumentTooLarge`、`ResultLimitExceeded` 与 `Canceled` failure。

无需新增 factory、resolver、generic dispatcher、Gateway result mapper、Preview store、proposal
handle、session、cache 或 DI container。

## 4. 当前 adapter 事实

`RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs` 当前：

- 持有 concrete `Ra2AutomationEditPreviewService`；
- 调用 internal `PreviewForHost`；
- 将 public result 投影为 Host `Ra2IniEditPreview`；
- 构造器接收的 `IRa2IniLanguageAnalysisService` 与 `Ra2AddPropertyInsertPlanner` 已不参与算法，
  只是 HLI-1B 迁移后保留的 internal compatibility shape；
- 没有状态、Apply、Save 或 UI 依赖。

最小可靠改法是在此类原位把依赖替换为 `IRa2AutomationCapabilityGateway`。新建平行
`GatewayEditPreviewService` 会形成两个 admission adapter，予以否决。

## 5. 预算差异与产品影响

当前两条路径：

| 路径 | 最大文档字符 | 最大诊断项 | 当前 consumer |
|---|---:|---:|---|
| public Gateway Preview | 8,388,608 | 10,000 | 尚无 IDE consumer |
| internal `PreviewForHost` | `int.MaxValue` | `int.MaxValue` | 内置 AI Host adapter |

HLI-2B 推荐并冻结内置 AI 改用 public Gateway budget，理由：

- Gateway descriptor 与实际执行限制保持一致；
- 内置 AI 与未来 Agent/CLI 使用同一安全资源边界；
- 超限是既有 typed failure，不需要新 failure kind；
- 避免 concrete Gateway 内部出现“同一能力、两套未声明预算”的旁路；
- 8 MiB 已高于当前 7 MiB Preview 性能基线。

这是一个有意、可见的兼容性收窄：超过 8,388,608 UTF-16 chars 的当前文件不再允许 AI
结构化编辑 Preview。普通 advisory 问答仍使用既有截断上下文，不因文档过大被禁止。

## 6. 发送前资源门禁缺口

Shell 当前在捕获任意长度 snapshot 后立即把明确编辑请求标记为 `Available`，随后才发送
provider 请求。若只替换 adapter，超 8 MiB 文档会在 DeepSeek 已返回工具调用后才得到
`DocumentTooLarge`，造成一次可预先避免的真实模型调用。

因此 HLI-2B 不能只改 adapter。必须在 `GenerateAiAssistantResponse` 创建 pipeline / request
session 之前，使用 Gateway descriptor 的 `DocumentEditPreview` 限制做本地 preflight：

- 超限的明确编辑请求直接 `EditUnavailable`；
- 输入保留且不发送 provider；
- 显示安全的本地资源上限说明；
- advisory 请求不受此门禁影响；
- adapter 仍执行同一 public limit，防止非 Shell caller 绕过 preflight。

门禁不能硬编码第二个 `8_388_608`。Shell composition root 应创建一个 Gateway instance，
同时注入现有 Host adapter 并用其固定 Preview descriptor 做 preflight；descriptor 缺失、重复
或无效时 fail closed 为 `SnapshotUnavailable`，不得发送编辑请求。共享实例不是状态要求，
而是避免 composition 与 preflight 各自创建入口、让调用路径更易审计。

## 7. A4 与 Host authority 保持不变

HLI-2B 不修改：

- official/custom endpoint 判定和 required-tool policy；
- `Ra2AiAuthoringToolCatalog` JSON schema；
- `Ra2AiAuthoringToolAdapter` 的 1..128 operations、Section/Key/Value 限制；
- Coordinator proposal generation、Normal/Caution/Blocked policy；
- Workspace admission、active slot、single-use、discard/invalidate；
- Shell live currency、transaction ordering、Undo、Save/Backup/Rollback。

Gateway result 仍必须先经 `FromAutomation` 和 Workspace admission。Gateway `PreviewId` 在
Workspace 外继续没有 Apply 权威。

## 8. Public API 与程序集影响

- HLI-2B public API 变化为 0；Application exported allowlist 保持精确 35。
- 不增加 capability、descriptor property、failure kind、DTO 或 constructor。
- Application 继续只依赖 Core。
- IDE 继续依赖 Application；不存在反向引用。
- `PreviewForHost` 是 internal compatibility bypass。consumer 切换后应在同阶段删除，防止
  后续重新引入未声明的 unlimited path；这不改变 public API 或 Preview engine。

## 9. Shell 与 UI 影响

需要一个 Gateway field、constructor 中向 adapter 的注入，以及一处极窄 Shell code-behind
资源 preflight；不修改 XAML、布局、控件、AutomationId 或视觉行为。Shell transaction、
Apply 和 Save 方法保持零 diff。

若实际实现需要修改上述 composition 注入与编辑请求可用性判定以外的 Shell 区域，必须停止
并生成 HLI-2B-R1。

## 10. 当前测试事实

本轮实际运行：

```text
Application.Tests: Passed 94/94
HLI-2B related Host/AI targeted baseline: Passed 48/48
Latest trusted full non-UI baseline: HLI-2A Passed 2537/2537
```

现有 7 MiB performance case 会验证公共 8 MiB 内仍成功；当前缺少：

- production adapter 确实调用 Gateway 的静态与行为证据；
- >8 MiB 返回 `DocumentTooLarge` 且不激活 Preview；
- 明确编辑请求在 provider 调用前被本地拒绝；
- advisory 在同一超大文档下不被错误阻断；
- production 源码完全无 `PreviewForHost` caller/method；
- Application allowlist 35 与 A3/A4 authority 回归聚合门禁。

## 11. 被否决的方向

### 新增第二个 IDE Gateway adapter

否决。现有 `Ra2IniEditPreviewService` 已是 HLI-1C 冻结的唯一 seam，新增类型只会制造
并行路径和后续删除工作。

### Gateway 在超限时回退 `PreviewForHost`

否决。descriptor 与实际执行不一致，也使同一 AI 请求是否受限取决于隐式 caller 身份。

### 给 public Gateway 增加 Host budget 参数/overload

否决。会产生无当前外部消费者正当理由的 R2 API，并允许调用方自行放大资源上限。

### 只在 provider 返回后处理 `DocumentTooLarge`

否决。正确但会消耗可预先避免的真实模型请求，可靠性与成本边界不足。

### 把 A4 coordinator/tool schema 移入 Application

否决。provider proposal lifecycle 和 UI policy 必须留在 IDE；HLI-2B 是 adapter-only。

## 12. 审计结论

HLI-2B 可以在不新增 public API、不修改 semantic Preview engine、不改变 Apply/Undo/Save
authority 的前提下完成。可靠路径是：原位将唯一 Host adapter 改为 typed Gateway consumer，
统一采用 public budget，发送前通过 Gateway descriptor fail closed，随后删除 internal
`PreviewForHost` bypass。

该方案比“只换一行调用”多一个必要的成本门禁，但避免了超限付费请求、双预算漂移与第二
adapter 三类返工。精确允许文件和连续任务卡由最终契约冻结。
