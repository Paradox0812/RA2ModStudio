# AUTOMATION-HLI-2B IDE/AI Gateway Consumer Final Contract

契约日期：2026-08-22  
状态：Final / Awaiting implementation approval  
前置基线：AUTOMATION-HLI-2A Completed / Verified  
事实依据：`Docs/AUTOMATION-HLI-2B_GatewayConsumerCodeFactAudit.md`

## 1. 目标

让内置 AI 的明确当前文件编辑请求通过 HLI-2A typed Gateway 生成 semantic Preview，同时
完整保留 A4-R1 provider/policy 与 HLI-1C Host authority：

```text
explicit edit request
  -> local availability + Gateway descriptor budget preflight
  -> official provider required structured tool
  -> bounded Ra2IniEditPlan
  -> Workspace-owned invocation/generation
  -> existing Ra2IniEditPreviewService (only Host adapter)
  -> IRa2AutomationCapabilityGateway.Preview
  -> Ra2IniEditPreview.FromAutomation
  -> one active slot
  -> explicit user Apply
  -> Shell live currency + one semantic Undo unit
  -> no automatic Save
```

完成后内置 AI 与未来进程内 Agent 使用同一 public Preview 资源契约；HLI-2B 不把 provider
DTO、Apply 或 Save 下移到 Application。

## 2. 风险与治理门

```text
Contract/docs change: R0
Implementation: R3 (framework adapter + Shell request admission boundary)
Public API: 0 change; exported allowlist stays 35
Persistence/wire: None
UI/XAML: None
Shell: one explicitly bounded pre-provider availability/preflight change only
Governance: Deferred during 2B-1..2B-3; flush at 2B-4 stop
```

用户确认本最终契约后，才授权 HLI-2B-1..2B-4 连续实施。若需要改变 public API、Preview
算法、Apply/Save authority、provider schema 或 Shell transaction，立即停止并生成修订契约。

## 3. 非目标

HLI-2B 不实现：

- 新 capability、generic `Invoke`、wire/JSON/IPC/MCP/CLI adapter；
- public Apply/Save/Undo/transaction/session/store/handle；
- Job/Event/Artifact/workflow/permissions/tracing；
- 项目级 query/edit、多文件事务、新 Section/template 或素材能力；
- 自动确认、自动 Apply、自动 Save、自动重试或 provider fallback；
- A4 prompt、tool schema、model catalog、endpoint policy、proposal card 或 apply policy 改造；
- parser、diagnostics、Field Registry、Completion、Search、Save、backup/rollback 行为；
- XAML、Dock、布局、字体、主题或 AutomationId 修改。

## 4. 唯一 adapter 契约

`Ra2IniEditPreviewService` 继续是唯一 production `IRa2IniEditPreviewService` 实现。不得新增
平行 Gateway adapter。

其依赖和执行路径改为：

```text
private readonly IRa2AutomationCapabilityGateway _gateway
Preview(snapshot, plan, token)
  -> _gateway.Preview(snapshot.ToAutomationSnapshot(), plan, token)
  -> Ra2IniEditPreview.FromAutomation(snapshot, plan, result)
```

约束：

- Shell composition root 创建一个 `IRa2AutomationCapabilityGateway`，并注入 adapter；
- 保留一个 internal interface-injection constructor 供 production composition 与 contract test，
  不公开新 surface；
- 现有 language-analysis/insert-planner compatibility constructor 可在本阶段保留，避免无关
  test 构造 churn，但其内部只能委托新 Gateway 路径，不得再创建或持有
  `Ra2AutomationEditPreviewService`；
- adapter 不捕获或重写 typed result；Host projection 继续负责本地消息和完整性 guard；
- adapter 不缓存 descriptor、snapshot、result、PreviewId 或 active state。

## 5. 资源预算最终决策

内置 AI 结构化编辑明确采用 Gateway v1 public budget：

```text
MaximumDocumentCharacters = 8,388,608 UTF-16 chars
MaximumDiagnosticItems = 10,000
MaximumOperations = 128
```

- `Ra2AutomationCapabilityGateway.Preview` 是执行权威；IDE 不复制限制常量。
- 8 MiB 内行为保持现有语义；现有 7 MiB performance case 必须继续通过。
- 超过文档限制返回/预防 `DocumentTooLarge`，不生成 active Preview。
- diagnostics 超限返回 `ResultLimitExceeded`，不生成 active Preview。
- 不允许回退 unlimited Host path。
- `Ra2AutomationEditPreviewService.PreviewForHost` 在 consumer 切换后删除；production 源码中
  不得再出现该 symbol。

这是已声明的产品兼容性收窄，不是偶然实现副作用。普通 advisory 请求继续使用既有 65,536
字符总 prompt 与局部上下文截断策略，不因当前文件超过 8 MiB 自动禁用。

## 6. 发送前门禁契约

### 6.1 限制来源

IDE 必须从 Shell composition root 持有并注入 adapter 的同一 Gateway instance 的
`GetCapabilities()` 中按 ordinal ID 查找
`Ra2AutomationCapabilityIds.DocumentEditPreview`，并验证：

- 精确一项；
- Version 等于 `CurrentVersion`；
- Risk 为 `Edit`；
- `MaximumDocumentCharacters > 0`；
- `MaximumOperations == Ra2IniEditPlan.MaximumOperationCount`。

不得硬编码第二个文档字符限制。catalog 缺失、重复、版本/risk/limit 不一致时 fail closed，
编辑能力标记为 unavailable；普通 advisory 仍可发送。

### 6.2 时序

明确编辑请求的资源检查必须发生在：

```text
CaptureCurrentAuthoringSnapshot
  -> compare snapshot.Text.Length with descriptor
  -> resolve interaction route
  -> only then CreateAiAssistantPipeline / TryStart / provider Send
```

超限时：

- 不创建 provider request session；
- 不调用 DeepSeek client；
- 输入保留；
- 显示“当前文档超过 AI 结构化编辑 8 MiB 资源上限，尚未发送”的本地提示；
- 不创建/替换 active proposal 或 active Preview。

### 6.3 内部可用性

允许在 `Ra2AiEditAvailabilityKind` 末尾追加 `ResourceLimitExceeded`；不重排已有 enum 值。
Router 对它与其他 unavailable 状态一样返回 `EditUnavailable`。这是 IDE internal 状态，不是
Application failure kind 或 public API。

## 7. A4 policy 保持契约

以下必须零语义变化：

- 只有 official endpoint + ready configuration 可获得 edit tool；
- custom endpoint 继续 advisory-only；
- 明确编辑、歧义编辑和 advisory 的路由规则；
- provider required-tool 及未调用工具失败；
- 一次只接受一个 tool call；
- plan 只支持 Upsert/Replace，1..128 operations；
- Normal/Caution/Blocked apply policy；
- dismiss、invalidate、generation、replay 和 cancellation；
- provider prose/raw JSON 不能创建提案。

新增资源状态只在明确编辑请求发送前拒绝；不得改变 prompt、tool schema 或 response parsing。

## 8. Host authority 契约

- Workspace 继续在 invocation 前建立 generation 并清空旧槽位。
- Gateway result 必须通过 `FromAutomation` identity/content guard。
- success wrapper 必须与 invocation snapshot/plan exact instance binding。
- PreviewId 未被当前 Workspace 接纳前没有 Apply 权威。
- Apply 仍只接受 PreviewId + explicit confirmation。
- claim 后 success/stale/rejected/非致命异常均 single-use。
- Shell transaction/live currency/Undo 实现零 diff。
- Apply 后仍不 Save；Save/Backup/Rollback 无改动。

## 9. Public API 与数据模型

HLI-2B public API diff 必须为 0：

- Application exported allowlist 精确 35；
- Gateway interface/concrete/descriptor/ID/enum surface 不变；
- snapshot/plan/result/failure enum/limits 不变；
- 不新增 DTO、fact、failure kind、serialized shape 或 configuration key；
- `Ra2AiEditAvailabilityKind.ResourceLimitExceeded` 是 IDE internal，追加值不影响 public ledger。

## 10. 允许文件

用户确认实施后，production/test 改动只允许：

```text
RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationEditPreviewService.cs
RA2IniEditor.IDE/AI/Ra2AiInteractionRoute.cs
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2Hli2BGatewayConsumerContractTests.cs (new)
RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs
```

Shell diff 精确限于一个 Gateway field、constructor 中的 Gateway -> adapter 注入、
`GenerateAiAssistantResponse` 的 snapshot budget availability 判定和
`FormatAiEditUnavailableMessage` 的一个资源提示分支；不得修改 transaction、Apply、Save、
Dock 或视觉代码。

阶段完成时允许更新：

```text
Docs/AUTOMATION-HLI-2B_GatewayConsumerFinalContract.md
Docs/AUTOMATION-HLI-2B_StageLedger.md (new)
Docs/CurrentCapabilities.md
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/DevelopmentRoadmap.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/README.md
```

## 11. 禁止文件

不得修改：

```text
RA2IniEditor.Application internal parser/semantic/diagnostic/edit engine files
RA2IniEditor.Core/**
RA2IniEditor.Infrastructure/**
RA2IniEditor.IDE/AI/** except Ra2AiInteractionRoute.cs
RA2IniEditor.IDE/Editing/** except Ra2IniEditPreviewService.cs
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs outside exact approved methods
all other XAML / Dock / AutomationIds
*.csproj / *.sln / package tooling
Field Registry data
legacy
```

## 12. 测试契约

新增 HLI-2B 聚合契约测试至少覆盖：

1. production adapter 持有 `IRa2AutomationCapabilityGateway` 并调用 typed `Preview`；
2. production IDE/Application 源码无 `PreviewForHost`；
3. Gateway success 经现有 Workspace admission 并只 Apply 一次；
4. canceled/failed Gateway result 不进入 active slot；
5. 7 MiB 文档仍可成功 Preview；
6. 8,388,609 chars 返回 `DocumentTooLarge`、无 candidate/Preview authority；
7. Shell 与 adapter 共享一个 Gateway instance，Preview descriptor 是 preflight 的唯一限制
   来源，无第二个硬编码 8 MiB；
8. `ResourceLimitExceeded` 明确编辑路由为 `EditUnavailable`，advisory 仍为 `Advisory`；
9. Shell preflight 在 pipeline/session/provider send 之前；超限本地提示明确“尚未发送”；
10. Application exported allowlist 精确 35，Gateway public surface不变；
11. A4 endpoint/tool/policy、HLI-1C authority 与 Shell transaction static gates继续通过；
12. adapter concurrency/generation 不覆盖较新 active Preview。

新增事实不得少于 8 项。测试比较结构化状态和时序 token，不绑定非契约消息全文，唯一本地
“尚未发送”提示允许做关键片段断言。

## 13. 连续任务卡

### HLI-2B-0 Code-Fact Audit and Final Contract

- 完成当前 consumer、budget、provider 时序和 authority 审计。
- 生成本最终契约并更新当前治理文档。
- DocsOnly 停止，等待用户确认后才实施。

### HLI-2B-1 Gateway Adapter Switch

- 原位把唯一 Host adapter 改为 `IRa2AutomationCapabilityGateway` consumer。
- 删除 `PreviewForHost` internal bypass。
- 完成 success/failure/cancellation/projection tests。
- 自审 production diff 不出现第二 adapter 或算法复制。

### HLI-2B-2 Budget Preflight and Cost Gate

- 通过 Gateway descriptor 冻结 edit budget snapshot。
- 增加 internal resource-unavailable 路由与 Shell 发送前检查。
- 验证超限明确编辑零 provider 调用，advisory 不受影响。
- 自审无重复常量、无 provider/prompt/tool schema变化。

### HLI-2B-3 Authority and Regression Gates

- 运行 HLI-2B focused、Application、HLI-1C/A4/Shell targeted 和 full non-UI。
- 审计 allowlist 35、public API 0 change、无 `PreviewForHost`、无 Shell transaction diff。
- 任一必选门禁失败即停止，不进入治理收口。

### HLI-2B-4 Governance, Package and Stop

- 生成 Stage Ledger 与 Verification Matrix。
- 更新 CurrentCapabilities、PublicApiLedger 0-change、DecisionLog 和状态文档。
- 生成 IdeOnly clean package并停止，不自动进入 HLI-2C。

## 14. 验证命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build `
  --filter "FullyQualifiedName~Ra2Hli2BGatewayConsumerContractTests|FullyQualifiedName~Ra2Hli1CHostBoundaryContractTests|FullyQualifiedName~Ra2IniEditPreviewServiceTests|FullyQualifiedName~Ra2IniEditPreviewBoundaryAndPerformanceTests|FullyQualifiedName~Ra2IniAuthoringWorkspaceTests|FullyQualifiedName~Ra2AiAuthoringCoordinatorTests|FullyQualifiedName~Ra2AiProposalPreparationRunnerTests|FullyQualifiedName~Ra2AiAssistantPipelineTests|FullyQualifiedName~Ra2AuthoringShellTransactionBoundaryTests"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

不需要电脑控制或 UI 烟测：无 XAML/视觉变化；Shell code-behind 通过静态时序测试和完整
non-UI regression 验证。

## 15. 静态门禁

- production diff 精确限于 4 个批准文件；其中 Shell 只有 Gateway field/constructor 注入和
  两个批准方法的局部 hunk。
- test diff 精确限于 1 个新契约测试和 router test 增量。
- production 源码无 `PreviewForHost`。
- IDE adapter 无 `new Ra2AutomationEditPreviewService` 或 direct service field。
- public allowlist 精确 35；Application project reference 继续只有 Core。
- Gateway catalog 精确四项、顺序/version/risk/limits 不变。
- A4 prompt/tool/provider/coordinator/workspace/projection/transaction/Save/XAML diff 为 0。
- clean package 无 `.vs/bin/obj/artifacts/TestResults/old zip`。

## 16. 验收矩阵

| Gate | 必须结果 |
|---|---|
| Pre-change Application | 94/94 |
| Pre-change related Host/AI | 48/48 |
| HLI-2B new contract facts | 至少 8，全部通过 |
| Focused HLI-2B/HLI-1C/A4 | 全部通过 |
| Full non-UI | 不低于 2537 基线，0 failed |
| Build | 0 errors；新增 warning 0 |
| Public reflection | allowlist 精确 35，public API 0 change |
| Budget | 7 MiB success；8,388,609 chars typed reject；超限明确编辑零 provider 调用 |
| Production path | Gateway only；无 `PreviewForHost` |
| Shell/XAML | XAML 0 diff；transaction/Apply/Save 0 diff |
| IdeOnly package | Passed，禁止条目 0 |
| UI/computer control | NotRun；无视觉行为变化 |

## 17. 停止与回滚规则

- 需要新 public API/descriptor/failure kind：停止并生成 HLI-2B-R1。
- 需要第二 adapter、generic dispatch 或 host-budget fallback：停止，违反唯一路径。
- 无法在 provider send 前完成资源门禁：停止；不得接受事后失败作为等价实现。
- 需要改 prompt/tool/provider/coordinator/workspace/projection/transaction/Save：停止并单独审查。
- 8 MiB 内行为、A4 policy、HLI-1C authority 或 full suite 回归：停止并保留失败证据。
- 回滚只撤销 HLI-2B diff；不得恢复旧算法、legacy 或扩大 Host authority。

## 18. 自审结果

| 审查项 | 结论 | 处理 |
|---|---|---|
| 是否新增第二 adapter | No | 原位改造现有唯一 seam |
| 是否复制 Gateway/Preview 算法 | No | typed Gateway + existing projection |
| 是否明确预算变化 | Yes | 内置 AI 采用 public 8 MiB/10k/128 |
| 是否避免超限付费请求 | Yes by contract | descriptor-driven preflight before provider |
| 是否存在双预算回退 | No | 删除 `PreviewForHost` |
| 是否修改 public API | No | allowlist 保持 35 |
| 是否泄漏 provider DTO | No | A4 tool/adapter 留在 IDE |
| 是否泄漏 Apply/Save | No | Workspace/Shell authority不变 |
| 是否影响 advisory | No | 只拒绝超限明确编辑；advisory仍截断发送 |
| 是否修改 UI | No | 无 XAML/AutomationId/视觉变化 |
| 是否需要 Shell 变更 | Yes, bounded | 只做发送前 availability 和本地提示 |
| 是否可能下一阶段删除本阶段代码 | Low | adapter 与资源门禁是 HLI-2C 直接前置 |

自审结论：契约足够可靠。它同时解决 consumer 切换、预算一致性和 provider 成本时序，且把
改动限制在一个既有 adapter、一个 internal route 状态、一个 Shell preflight 与测试。最容易
导致返工的第二 adapter、Host fallback、事后超限失败和 A4 下移均被明确禁止。

## 19. 当前停止点

HLI-2B-0 代码事实审计、基线验证与最终契约已完成；生产实现尚未开始。下一安全入口是用户
确认本最终契约后连续执行 HLI-2B-1..2B-4。未确认前不得修改生产代码或测试。
