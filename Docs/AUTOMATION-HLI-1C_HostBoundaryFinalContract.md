# AUTOMATION-HLI-1C Host Boundary Confirmation Final Contract

契约日期：2026-08-22
状态：Completed / Verified
前置基线：AUTOMATION-HLI-1B Completed / Verified
事实依据：`Docs/AUTOMATION-HLI-1C_HostBoundaryCodeFactAudit.md`

## 1. 目标

以自动化测试正式确认 HLI-1B Application Preview 与现有 A3 Host Apply/Undo 之间的唯一
安全连接方式，为 HLI-2A/2B 提供稳定接入边界：

```text
Host snapshot + plan
  -> Workspace-owned Preview invocation/generation
  -> injected IRa2IniEditPreviewService
  -> Application today / IDE Gateway adapter later
  -> Host projection
  -> one active slot
  -> PreviewId + explicit confirmation
  -> Shell-owned live currency + transaction
  -> one in-memory Session revision + one semantic Undo unit
```

本阶段确认并加固边界，不新增用户功能。完成后 HLI-2A 可以设计 Gateway，但仍不能 Apply。

## 2. 风险与治理门

```text
Architecture/authority guard: R3, explicitly approval-gated
Authorized implementation diff: two internal Host guards + R1 tests + R0 docs
Public API risk: None; exported allowlist remains exactly 29
Persistence/wire risk: None
UI risk: None
Shell risk: Read-only static verification; no Shell diff
Governance mode: Deferred during cards; flush at HLI-1C stop
```

生产修改严格限于本契约列出的两处 internal admission guard。若还需要其他生产改动，
本契约立即停止并按 R3 重新审查，不能借“边界确认”静默扩围。

## 3. 非目标

HLI-1C 不实现：

- Capability Gateway、descriptor、registry、dispatcher 或版本协商；
- built-in AI Gateway consumer；
- public/internal `RegisterPreview`、`AdoptPreview`、Preview dictionary 或 global store；
- public Apply/Undo/Save/transaction/session/file API；
- wire/JSON/IPC/MCP/CLI DTO 或 Preview 持久化；
- 自动确认、自动 Apply、自动 Save 或多文件事务；
- 多级程序化 Undo；
- A4 policy、tool schema、provider、聊天或提案卡修改；
- Parser、Diagnostics、Field Registry、Completion、Search、Save 行为修改；
- Shell/XAML/Dock/AutomationId/UI 修改。

## 4. 架构契约

### 4.1 唯一 admission seam

`IRa2IniAuthoringWorkspace.Preview(snapshot, plan, token)` 是 Host 创建可应用 Preview 的唯一
入口。Workspace 必须在调用 injected `IRa2IniEditPreviewService` 之前：

1. 递增 generation；
2. 清空旧 active slot；
3. 捕获本次 generation。

service 返回后，只有捕获 generation 仍为当前值且 Preview 成功时才能写入 active slot。
未来 Gateway adapter 必须实现现有 internal `IRa2IniEditPreviewService`，不得新增返回后注册
旁路。

### 4.2 PreviewId 不是独立权威

- Application 可以生成 PreviewId，但不保存它。
- 未经 Workspace 当前 generation 接纳的 ID 只用于相关性，不可 Apply。
- Workspace 只匹配唯一 active PreviewId，不维护字典或历史。
- Apply 调用方只能提供 ID 与显式确认，不能提供 Preview、candidate text、Session、
  Registry revision 或 editor text。

### 4.3 Host projection

未来 IDE Gateway adapter 的逻辑边界固定为：

```text
IRa2IniEditPreviewService.Preview(snapshot, plan, token)
  -> invoke typed Gateway Preview capability
  -> obtain Ra2AutomationEditPreviewResult
  -> Ra2IniEditPreview.FromAutomation(snapshot, plan, result)
```

`FromAutomation` 必须继续检查 DocumentId、Version、FilePath、RegistryRevision 和 PlanId。
并新增以下完整性检查：

- success result 的 operation evidence 与传入 plan 按 index/kind/section/key/value 精确一致；
- 每个 operation evidence 的 original span 必须落在 snapshot text 的半开区间边界内；
- 复用 `Ra2TextChangeSet.Apply(snapshot.Text)`，结果必须与 CandidateText ordinal 相等；
- span 越界、operation 错配或 candidate/change 不一致统一拒绝为 Host projection error，
  不产生可注册 Preview。

Workspace 在写入 active slot 前还必须以 `ReferenceEquals` 确认返回 wrapper 持有的
Host snapshot/plan 就是本次 invocation 的两个 immutable 实例。错配统一转换为绑定本次
input 的 `UnexpectedFailure`，使用固定安全消息、空 PreviewId，不注册，也不泄漏底层
异常文本。结构等价但实例不同同样拒绝，避免 adapter 悄悄替换 Host authority envelope。

失败/取消/foreign result 不得成为 active Preview。HLI-1C 不创建 Gateway adapter；具体
Gateway 调用签名在 HLI-2A 冻结，IDE consumer 在 HLI-2B 实现。

### 4.4 Apply 与事务端口

- 未确认 Apply 返回 `ConfirmationRequired` 且不消费 active Preview。
- 匹配且已确认的 Apply 必须在锁内 claim/clear，然后才调用 transaction port。
- 一旦 claim，成功、stale、rejected、非致命异常均不可重放。
- 并发同一 ID 最多一次进入 transaction port。
- `IRa2EditorTransactionPort` 继续只接受 Host `Ra2IniEditPreview`，保持 internal。

### 4.5 live currency 与提交

Shell private transaction port 在提交瞬间捕获 live Session、editor text、Registry revision 和
Caret，并完整检查：

- Preview 成功；
- 可编辑 Session 存在且非只读；
- DocumentId 一致；
- EditRevision 一致；
- Session text 与 snapshot text 一致；
- Editor text 与 snapshot text 一致；
- Field Registry revision 一致。

成功仍为一次 Session revision、一次 editor sync、一个 semantic Undo unit；不调用 Save。

## 5. 数据所有权与生命周期

| 数据 | Owner | 创建 | 失效/消费 | 持久化 |
|---|---|---|---|---|
| Automation snapshot/plan | caller | Host capture / plan creation | invocation 结束后可丢弃 | No |
| Automation result | Application/Gateway caller | Preview completion | caller 丢弃 | No |
| Host Preview wrapper | IDE adapter | result projection | 未接纳、失效或消费 | No |
| generation | Workspace | 每次 Preview/Invalidate/Apply | 单调推进，允许溢出比较相等 | No |
| active Preview | Workspace | 当前代次成功结果 | replace/invalidate/claim | No |
| A4 proposal | AI coordinator | active Host Preview 后 | dismiss/invalidate/apply | No |
| live editor state | Shell | commit-time capture | 当前 UI/session 生命周期 | 既有会话规则 |
| semantic Undo state | Shell | successful Apply | 后续语义操作/文本漂移 | No |

不新增 DTO、handle type、store、cache、serializer 或 snapshot format。新增 guard 只读取
现有 immutable snapshot/plan/result，不保存新状态。

## 6. Public API 契约

HLI-1C public API diff 必须为 0：

- Application exported allowlist 继续精确为 29；
- 不增加 `Apply`、`Save`、`Store`、`Session`、`Transaction` 或 `ProposalHandle`；
- 不修改 HLI-1A1/1A2/1B 方法、DTO、enum 数值或 failure semantics；
- A3 Host 类型继续全部 internal；
- `Ra2AutomationEditPreviewResult` 继续是进程内 Experimental result，不声明 wire 稳定。

## 7. 资源策略兼容

当前 IDE concrete adapter 使用 `PreviewForHost` 保留既有 Host budget；public Preview 使用
8,388,608 chars / 10,000 diagnostics。HLI-1C 不改变两者。

HLI-2B 将内置 AI 切换到 Gateway 时，必须在其契约中明确选择 public budget 或经批准的
Host policy；不得因为更换 adapter 静默缩小或放大用户行为。

## 8. 允许文件

经用户确认后，HLI-1C 实施只允许修改：

```text
RA2IniEditor.IDE/Editing/Ra2IniEditPreview.cs
RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs
RA2IniEditor.Tests/IDE/Ra2Hli1CHostBoundaryContractTests.cs (new)
Docs/AUTOMATION-HLI-1C_HostBoundaryFinalContract.md
Docs/AUTOMATION-HLI-1C_StageLedger.md (new at completion)
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/DevelopmentRoadmap.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/README.md
```

两处生产文件只允许增加 projection/invocation binding guard，不得改变正常 Preview、
active-slot、Apply 或 presentation 分支。若单一新测试文件无法表达契约，应先报告原因；
不得修改测试基础设施。

## 9. 禁止文件

不得修改：

```text
RA2IniEditor.Application/**
RA2IniEditor.Core/**
RA2IniEditor.Infrastructure/**
RA2IniEditor.IDE/** all other production code
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
all XAML / Dock / AutomationIds
*.csproj / *.sln / package tooling
Field Registry data
legacy
```

若测试失败要求修改这些文件，停止并生成 HLI-1C-R1 修订事实报告。

## 10. 测试契约

新增 `Ra2Hli1CHostBoundaryContractTests` 至少覆盖：

1. Host workspace/preview/apply/transaction 类型全部 internal，Application allowlist 29；
2. Workspace/Preview seam/Apply request/Transaction Port 精确 surface，无 register/store/save；
3. Gateway-like test adapter 通过现有 Preview seam 进入 active slot，并只能 Apply 一次；
4. Workspace 外单独生成的 Automation PreviewId 无法 Apply；
5. foreign Document/Version/Registry/Plan/FilePath result 无法接纳或激活；
6. same PlanId 但 operation 内容错配时拒绝；
7. Changes 应用于 snapshot text 后与 CandidateText 不一致时拒绝；
8. Workspace adapter 返回 foreign wrapper 时转为失败且不激活；
9. failed/canceled result 不进入 active slot；
10. transaction stale/rejected/非致命异常后 Preview 不可重放；
11. 既有 older-generation completion 测试继续证明旧结果不能覆盖新槽位；
12. Shell transaction 顺序、currency、无 Save/Writer 静态边界继续通过；
13. 现有 A4 Normal/Caution/Blocked、dismiss/invalidate/replay 行为不变。

测试不得通过新 production hook、放宽 internal 可见性或修改 Shell 来构造。
新增测试用例不得少于 8 个；当前 Host lifecycle filter 基线为 32/32，实施后的同一
filter（含新类）通过数应不少于 40，精确数量在 Stage Ledger 记录。

## 11. 连续任务卡

### HLI-1C-0 Baseline and Exact Surface Freeze

- 确认 clean worktree 和 HLI-1B revision。
- 运行 Application 82/82、Host lifecycle 32/32 基线。
- 生成 exact type/method/caller manifest。
- 不需要 PreChange 源码包：批准 diff 仅 tests/docs，Git revision 是回滚锚点。

### HLI-1C-1 Projection and Invocation Binding Guards

- `FromAutomation` 增加 operation 与 candidate/change 一致性验证，复用现有 ChangeSet。
- Workspace 在 active-slot admission 前验证 wrapper 绑定，错配转安全失败。
- 新增对应 foreign/same-ID-wrong-content/inconsistent-candidate tests。

### HLI-1C-2 Authority, Visibility and Gateway-like Admission Tests

- 新增 internal visibility、29-type allowlist 和 exact surface tests。
- 证明 public/Application 无 Apply/Save/store/session/transaction surface。
- 证明 unadmitted PreviewId 不产生 Host 权威。
- 用 test-only adapter 实现现有 `IRa2IniEditPreviewService`。
- adapter 调用 public Application Preview 并使用既有 `FromAutomation` 投影。
- 验证成功接纳、identity rejection、failed result 和 generation race。
- 不添加生产 Gateway、adapter 或 handle。

### HLI-1C-3 Claim, Failure and Regression

- 验证 confirmation、claim-before-port、stale/rejected/throw 后 single-use。
- 运行现有 Workspace/Currency/Shell/A4 tests。
- 运行 Application、IDE full non-UI suite 和静态禁止项审计。

### HLI-1C-4 Governance, Package and Stop

- 生成 Stage Ledger 和 Verification Matrix。
- PublicApiLedger 记录“0 change / allowlist 29 preserved”。
- 将 Proposed Host seam 决策更新为 Accepted。
- 更新 Roadmap/CurrentPhase/Compact Context/README。
- 生成 IdeOnly clean package并停止，不进入 HLI-2A。

## 12. 验证命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build `
  --filter "FullyQualifiedName~Ra2Hli1CHostBoundaryContractTests|FullyQualifiedName~Ra2IniAuthoringWorkspaceTests|FullyQualifiedName~Ra2IniEditApplyContractTests|FullyQualifiedName~Ra2IniEditPreviewCurrencyEvaluatorTests|FullyQualifiedName~Ra2AuthoringShellTransactionBoundaryTests|FullyQualifiedName~Ra2AiAuthoringCoordinatorTests|FullyQualifiedName~Ra2AiProposalPreparationRunnerTests|FullyQualifiedName~Ra2EditorSessionControllerProgrammaticTextTests"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 13. 静态门禁

- production diff 必须精确限制为两个 internal Host guard 文件；其余唯一 `.cs` diff 是
  新 HLI-1C test 文件。
- Shell/XAML/Core/Infrastructure/Application/project/legacy diff 必须为 0。
- Application exported allowlist 精确 29。
- Host boundary types 继续 internal。
- `IRa2IniAuthoringWorkspace` 无 raw result registration/store/save/session 参数。
- `Ra2IniEditApplyRequest` 属性仍精确为 PreviewId + ExplicitConfirmationGranted。
- `IRa2EditorTransactionPort` 仍只有 `Apply(Ra2IniEditPreview)`。
- public Experimental namespace 无 Apply/Save/store/session/transaction API。
- clean package 无 `.vs/bin/obj/artifacts/TestResults/old zip`。

## 14. 验收矩阵

| Gate | 必须结果 |
|---|---|
| Pre-change Application | 82/82 |
| Pre-change Host lifecycle | 32/32 |
| HLI-1C new contract tests | 至少 8 项，全部通过 |
| Post-change Host lifecycle | 至少 40 项，全部通过 |
| Full non-UI | 不低于当前 2526 基线，0 failed |
| Build | 0 errors；新增 warning 为 0 |
| Public reflection | allowlist 精确 29 |
| Production diff | 只允许两个 internal Editing guard 文件 |
| Shell/XAML/Application/Core/Infrastructure/project/legacy diff | 0 |
| IdeOnly package | Passed，禁止条目 0 |
| UI/computer control | NotRun；无 UI 行为变更 |

## 15. 停止与回滚规则

- 两处已授权 guard 之外的新生产缺口：保留失败证据，停止并生成 HLI-1C-R1。
- 需要 `RegisterPreview/AdoptPreview`：停止；先证明为何现有 invocation seam 不足。
- 需要 public Apply/Save 或 global Preview store：停止并拒绝在 HLI-1C 扩权。
- 需要修改 Shell、currency、Undo、Save 或 A4 policy：停止并单独架构审查。
- 只允许回滚新增测试/文档；不得恢复旧 Preview 算法或 legacy。

## 16. 自审结果

| 项目 | 结果 | 处理 |
|---|---|---|
| 是否需要新增 production adapter | No | Gateway 尚无契约；现有 internal seam 已足够 |
| Host 是否完整绑定返回数据 | Yes after approved guards | 校验 operations、changes/candidate 和 invocation wrapper |
| 是否需要 proposal handle type | No | 复用 PreviewId；权威来自 active slot，不来自 ID |
| 是否存在外部结果注册竞态 | Avoided | 禁止 post-hoc register；Workspace 包围 invocation |
| 是否会形成第二套 Preview | No | test adapter 调用唯一 Application service |
| 是否泄漏 Apply/Save | No | public diff 0；transaction port 保持 internal |
| 是否保留 generation/single-use | Yes | 新增直接 contract tests |
| 是否保留 live currency | Yes | 复用现有 evaluator/Shell static gate |
| 是否修改 Shell/UI | No | 两处 internal Editing guard；Shell/UI diff 0 |
| 是否处理 Gateway budget 差异 | Explicitly deferred | HLI-2B 必须单独决策，不在 1C 静默改变 |
| 是否引入持久化/wire 债务 | No | 无 DTO/store/serializer |
| 是否可能下一阶段删除本阶段代码 | No | 只增加永久边界测试，无临时 runtime abstraction |

审查结论：修正版契约足够可靠。HLI-1C 只增加两处可长期保留的 Host 完整性 guard，
同时避免最可能导致返工的 `RegisterPreview` 和过早 Gateway adapter。剩余未知仅是新增
测试是否会揭示两处 guard 之外的真实缺口；该情况已有强制停止规则。

## 17. 完成状态

用户已确认并授权连续执行。HLI-1C-0..1C-4 已完成，生产改动精确限于两处 internal
Host guard；11 个新契约测试、Application 82/82、Host 定向 53/53 和完整非 UI
2537/2537 均通过。完成证据见 `Docs/AUTOMATION-HLI-1C_StageLedger.md`。

本阶段停止，不进入 HLI-2A 实现。下一安全入口是 HLI-2A Capability Gateway 的
代码事实审计与最终契约。
