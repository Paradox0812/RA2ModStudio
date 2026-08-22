# AUTOMATION-HLI-2C First High-Level Agent Loop Final Contract

契约日期：2026-08-23
状态：Final / Awaiting implementation approval
前置基线：AUTOMATION-HLI-2B Completed / Verified
事实依据：`Docs/AUTOMATION-HLI-2C_FirstAgentLoopCodeFactAudit.md`

## 1. 阶段目标

以一个确定性、可复现的当前文件场景完成首个高层 Agent 闭环证据，并修复 Apply 后 Problems
不会立即刷新这一宿主缺口：

```text
bounded current-document snapshot
  -> Gateway GetSection + Validate
  -> official-provider-compatible structured tool call (loopback in tests)
  -> bounded Ra2IniEditPlan
  -> existing Coordinator / Workspace
  -> Gateway Preview
  -> local proposal
  -> explicit user Apply
  -> Shell editor/session/Dirty/one Undo unit
  -> post-apply current-file diagnostics refresh
  -> updated snapshot Gateway Validate
  -> no automatic Save
```

这是一项“现有能力闭环收口”，不是新 Agent 平台、外部 bridge 或自动写盘阶段。

## 2. 风险与治理

```text
HLI-2C-0 docs/audit: R0
HLI-2C implementation: R3 (Shell post-commit presentation timing + end-to-end harness)
Public API: 0 change; Application exported allowlist stays 35
Persistence/wire: None
UI/XAML: None
Provider cost: None; deterministic loopback only
Governance: Deferred during 2C-1..2C-3; flush at 2C-4
```

用户确认本最终契约后，才授权 HLI-2C-1..2C-4 连续实施。若实现需要新增 public API、
Agent/Job/Session model、Apply/Save capability、Prompt/Tool Schema 或新的 Shell UI，立即停止并
生成 HLI-2C-R1。

## 3. 架构裁决

### 3.1 不新增 Agent façade

当前长期边界已经存在：

- public `IRa2AutomationCapabilityGateway`：Agent-facing typed Query/Validate/Preview；
- internal `Ra2AiAuthoringCoordinator`：provider proposal、policy 与 explicit Apply lifecycle；
- internal `Ra2IniAuthoringWorkspace`：active Preview/generation/single-use authority；
- Shell transaction：live editor/Undo/Dirty；
- Save pipeline：disk/backup/rollback。

HLI-2C 不增加 `IAgent`、`AgentWorkflow`、`AgentSession`、`AgentResult` 或通用 orchestrator。
未来独立进程 host 需要 R4 wire/permission/session 契约，不能由当前 IDE 类型冒充。

### 3.2 两条路径、一个语义权威

HLI-2C 验证两条路径组合，而不强迫 UI adapter 全部改走 Gateway：

```text
Agent/headless lane:
  Gateway GetSection / FindReferences / Validate / Preview

IDE authority lane:
  provider tool -> Coordinator -> Workspace -> same Gateway Preview
  -> explicit Apply -> existing diagnostics presentation adapter
```

两条路径共享 Application semantic/diagnostic/preview core；Problems 继续由现有 ViewModel adapter
投影，不新建 Gateway-result-to-ViewModel mapper。

## 4. 数据所有权与生命周期

| 数据 | Owner | 生命周期 | HLI-2C 变化 |
|---|---|---|---|
| Automation snapshot | Caller/Host | 单次调用不可变 | None |
| Registry snapshot | Host capture | 与调用 snapshot 同代 | None |
| Edit plan/result | Agent output / Gateway result | 单次 Preview | None |
| Active Preview | Workspace | generation + single-use | None |
| Proposal | AI Coordinator | 一个活动提案 | None |
| Editor/session/Undo | Shell transaction | live document | None |
| Problems items | ShellViewModel | 当前显示状态 | 成功 Apply 后立即刷新 |
| Disk/save state | Save pipeline/User | 显式保存 | None |

不新增序列化、缓存、session store、trace store 或持久化 identity。

## 5. 端到端场景契约

新增 HLI-2C scenario 必须使用真实 production types 与 fake/loopback provider，不得建立测试专用
production 分支。最小场景：

1. 创建可编辑 `[E1]` 文档和稳定 Registry snapshot；
2. Gateway `GetSection("E1")` 成功并返回 `Strength=100`；
3. Gateway `Validate` 返回 typed pre-edit diagnostics；
4. loopback SSE 返回一次合法 `preview_ini_edit_plan` tool call；
5. Tool adapter 生成绑定 snapshot identity/version/registry 的 Replace plan；
6. Coordinator 通过现有 Gateway adapter/Workspace 生成本地 Preview；
7. proposal 需要显式 Apply，不能因测试而自动确认；
8. Apply 只成功一次，candidate 变为 `Strength=150`；
9. updated session revision 正好 +1、Dirty=true、磁盘/Save 未触发；
10. 从 updated session 捕获新 snapshot 后 Gateway `Validate` 成功；
11. stale/replay/cancel/blocked 路径不产生第二次事务或诊断刷新。

`FindReferences` 继续由现有 Gateway tests 覆盖；首个字段修改场景不为凑调用数强制执行无关
reference query。

## 6. Apply 后诊断刷新契约

唯一 production 行为变化位于 `AiEditProposalView_OnApplyRequested` 成功分支：

```text
Coordinator.ApplyConfirmed
  -> result.Succeeded
  -> proposal card MarkApplied
  -> use AuthoringResult.TextToSyncToEditor
  -> ShellViewModel.RefreshCurrentFileDiagnostics(text, current provider)
  -> detach proposal view
  -> refresh compact AI context summary
```

约束：

- 只在 Apply 已成功提交后刷新；
- 使用成功 result 的 committed text，不重新推断 candidate；
- 使用当前 Field Registry provider；
- refresh 是 presentation follow-up，不得改变成功 ApplyResult、回滚文本或生成第二 Undo；
- refresh 的非致命异常不得把已提交事务报告成失败；
- stale、blocked、failed、dismissed、preview-only 不刷新；
- 不运行 project-wide diagnostics，不自动打开 Problems，不自动 Save。

## 7. Public API 契约

HLI-2C public API diff 必须为 0：

- Application allowlist 精确 35；
- Gateway catalog 精确四项，ID/version/risk/limits/order 不变；
- Gateway interface 继续只有五个方法；
- snapshot/query/diagnostic/plan/preview/result/failure shape 不变；
- IDE availability/proposal/workspace/apply types继续 internal；
- 不新增 public Agent、host、policy、token、audit、job 或 artifact 类型。

PublicApiLedger 只记录零变更确认。

## 8. Provider、Prompt 与成本边界

- 不修改 DeepSeek endpoint/model/configuration、SSE parser、timeout/cancel/failure taxonomy；
- 不修改 PromptBuilder、context budget、tool schema、required-tool policy 或 response parsing；
- 不进行真实付费 Provider 调用；使用 loopback HTTP/SSE 与 fake client；
- 不自动重试、不 fallback、不把 provider prose/raw JSON 当计划；
- pre-provider 8 MiB descriptor gate 保持 HLI-2B 行为。

## 9. Host authority 保持

- Workspace 在 invocation 开始清空旧 active slot并推进 generation；
- success result 必须经 `FromAutomation` 与 invocation identity guard；
- proposal/PreviewId 未进入 active Workspace 前没有 Apply authority；
- Apply 仍需 proposal card 的显式用户动作；
- success/stale/rejected/exception 后 claim single-use；
- Apply 仍只改内存，Dirty=true，一个 semantic Undo unit；
- Save、Backup、Rollback 和磁盘文件零调用。

## 10. 实施允许文件

用户确认后，production/test 只允许：

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2Hli2CAgentLoopContractTests.cs (new)
RA2IniEditor.Tests/IDE/DeepSeekRa2AiLoopbackIntegrationTests.cs
RA2IniEditor.Tests/IDE/Ra2AuthoringShellTransactionBoundaryTests.cs
```

Shell diff 精确限于 `AiEditProposalView_OnApplyRequested` 的成功 Apply 后诊断刷新；不得修改
`ApplyAuthoringPreviewTransaction`、Generate/pipeline、Dock、layout 或视觉代码。

阶段完成允许更新：

```text
Docs/AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md
Docs/AUTOMATION-HLI-2C_StageLedger.md (new)
Docs/CurrentCapabilities.md
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/DevelopmentRoadmap.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/README.md
```

## 11. 禁止文件与行为

不得修改：

```text
RA2IniEditor.Application/**
RA2IniEditor.Core/**
RA2IniEditor.Infrastructure/**
RA2IniEditor.IDE/AI/**
RA2IniEditor.IDE/Editing/**
RA2IniEditor.IDE/ViewModels/**
all XAML / themes / Dock / AutomationIds
*.csproj / *.sln / package tooling
Field Registry data
legacy
```

不得新增 capability、wire/IPC/MCP/CLI、public Apply/Save、automatic confirmation、Job/Event/
Artifact、multi-file transaction、template/Section creation、asset/runtime ability。

## 12. 测试契约

新增聚合契约测试至少覆盖 10 个结构化事实：

1. Gateway Section query 和 pre-edit Validate 使用同一 snapshot/registry identity；
2. provider loopback required tool 生成 bounded plan，prose/raw JSON 不获得 authority；
3. plan identity/version/registry 与 request snapshot 完全绑定；
4. Preview 经 production `Ra2IniEditPreviewService` typed Gateway 路径进入 Workspace；
5. proposal policy 与 explicit Apply 保持；
6. Apply 成功一次，replay/stale 不产生第二事务；
7. committed text、session revision、Dirty 和 one-Undo evidence正确；
8. updated snapshot Gateway Validate 成功且使用新 version/text；
9. Save/file writer/backup 未调用；
10. Shell 只在 success branch 用 committed text 刷新当前文件 diagnostics；
11. refresh 位于 transaction success 后，且不进入 transaction method；
12. failed/blocked/stale/dismissed 不刷新；
13. Gateway allowlist 35/catalog/surface 不变；
14. HLI-2B budget、A4 policy、HLI-1C authority 和 Shell transaction gates继续通过。

断言以 typed state、identity、version、call count、ordering token 为主；不绑定普通展示文案全文。

## 13. 连续任务卡

### HLI-2C-0 Code-Fact Audit and Final Contract

- 完成本审计、基线聚焦测试与最终契约。
- DocsOnly 停止，等待用户确认。

### HLI-2C-1 Deterministic Gateway Loop Harness

- 新增 Query/Validate/Preview/Apply/re-capture/Validate 的完整 scenario。
- 使用 production Gateway、adapter、Workspace 和 transaction fake。
- 自审无测试专用 production 分支、无 public API 变化。

### HLI-2C-2 Provider-to-Host Loopback Closure

- 扩展现有 loopback tool-call 测试到 explicit Apply 与 post-apply Validate。
- 验证 required tool、single proposal、single transaction、no Save。
- 自审不依赖真实 Provider、定时碰运气或 UI 控件。

### HLI-2C-3 Post-Apply Diagnostics Refresh

- 在批准的 Shell success branch 调用现有 current-file diagnostics refresh。
- 增加静态时序与负路径门禁。
- 自审 ApplyResult/Undo/Save/Problems authority不混淆。

### HLI-2C-4 Regression, Governance and Stop

- 运行 Application、HLI-2A/2B、A4、HLI-1C、Shell transaction 和 full non-UI。
- 更新能力/API/决策/路线/状态，生成 Stage Ledger 与 IdeOnly clean package。
- 宣布 Minimum HLI-v1 完成并停止；不自动进入外部 Agent host 或 CONTENT-1。

## 14. 验证命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build `
  --filter "FullyQualifiedName~Ra2Hli2CAgentLoopContractTests|FullyQualifiedName~Ra2Hli2BGatewayConsumerContractTests|FullyQualifiedName~DeepSeekRa2AiLoopbackIntegrationTests|FullyQualifiedName~Ra2AiAuthoringCoordinatorTests|FullyQualifiedName~Ra2IniAuthoringWorkspaceTests|FullyQualifiedName~Ra2AuthoringShellTransactionBoundaryTests"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

电脑控制、真实 DeepSeek 和 UI smoke 为 NotRun：没有 XAML/视觉变化，确定性 loopback 与静态
Shell 时序测试是本阶段权威证据。用户可在完成后手工验收一次真实提案/Apply/Problems 更新。

## 15. 验收矩阵

| Gate | 必须结果 |
|---|---|
| Pre-change Gateway | 12/12 |
| Pre-change HLI-2B/A4/Coordinator/Shell | 30/30 |
| New HLI-2C facts | 至少 10，全部通过 |
| Application | 不低于 94，0 failed |
| Focused | 全部通过 |
| Full non-UI | 不低于 2547，0 failed |
| Build | 0 errors；新增 warning 0 |
| Public reflection | allowlist 35，Gateway catalog/surface不变 |
| Loop | Query/Validate -> tool plan -> Preview -> explicit Apply -> Validate |
| Transaction | one Apply、one revision、Dirty、one Undo、no Save |
| Problems | success 后刷新；失败/预览/关闭不刷新 |
| Shell/XAML | 仅批准 success hunk；XAML 0 diff |
| IdeOnly package | Passed；禁止条目 0 |

## 16. 停止规则

- 需要新增/修改 public API、capability 或 failure kind：停止并生成 HLI-2C-R1。
- 需要新 Agent façade、session/store/trace/job model：停止，超出最小闭环。
- 需要修改 Provider/Prompt/Tool Schema/Coordinator/Workspace/transaction/Save：停止。
- 诊断刷新会影响 Apply 成败或需要回滚：停止；presentation 不得反向控制 authority。
- 端到端场景只能通过真实付费 Provider 或 UI automation：停止并重新设计 deterministic seam。
- full suite、single-use、Undo、Dirty、no-Save 或 allowlist 回归：停止，不进入治理收口。

## 17. 自审结果

| 审查项 | 结论 | 处理 |
|---|---|---|
| 是否需要新高层 Agent 类 | No | Gateway + Coordinator 已分担正确职责 |
| 是否需要 public API | No | 现有 typed capabilities 足够首个闭环 |
| 是否实际覆盖 query/edit/diagnostics | Yes by contract | 同一 scenario 的 pre/post typed evidence |
| 是否保留 explicit Apply | Yes | proposal card/user action不变 |
| 是否自动保存 | No | Dirty only；Save path 0 call |
| 是否修复用户可见闭环缺口 | Yes by contract | Apply 后 current-file Problems刷新 |
| 是否复制 diagnostics mapper | No | 复用现有 ShellViewModel adapter |
| 是否引入真实 Provider 不确定性 | No | loopback SSE/tool-call |
| 是否提前建设 Job/Artifact | No | 后续 AUTOMATION-1 |
| 是否能被下一阶段保留 | Yes | 测试与刷新行为是长期回归门禁 |

自审结论：契约可靠。它把 HLI-2C 限制为一项可验证的闭环收口和一个真实宿主刷新缺口，
不新增抽象、不扩大授权，并为外部 Agent host、内容模板和素材阶段保留清晰边界。

## 18. 大阶段完成定义与后续距离

HLI-2C-4 通过后，以下大阶段可标记完成：

```text
Minimum High-Level INI Capability / HLI-v1
```

完成含义严格限定为：

- 普通 `net8.0` caller 可 Query/Validate/Preview；
- 内置 AI 可把明确当前文件自然语言请求转换为本地 semantic proposal；
- 用户可显式 Apply 为一次内存事务并立即看到 Problems 更新；
- 仍不自动 Save。

以下不属于 HLI-v1 完成定义，仍需后续独立阶段：

1. 独立 Agent/CLI 或 IPC/MCP host bridge（R4）；
2. Field Schema query、ResolveReference、Rename、Template、新 Section 与多文件事务；
3. permission token、audit、Job/Event/Artifact Runtime；
4. Cameo/Icon、VOX/SliceStack/VXL、SHP 与 Assembly Graph；
5. RA2TestHost、Runtime Adapter、deterministic Test Runner。

## 19. 当前停止点

HLI-2C-0 审计、基线验证与最终契约已完成；生产实现尚未开始。下一安全入口是用户确认本
最终契约后连续执行 HLI-2C-1..2C-4。未确认前不得修改生产代码或测试。
