# AUTOMATION-HLI-2A Capability Gateway Stage Ledger

日期：2026-08-22
状态：Completed / Verified
基线 revision：`7d5c2aae6b00a1e5b5c0ac781bfa0de2f5502c2f`

## 1. 阶段结果

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| HLI-2A-0 | 代码事实审计与最终契约 | audit/contract/governance docs | Application baseline 82/82；static audit | Completed | 2A-1 |
| HLI-2A-1 | Descriptor 与固定目录 | 2 new Application contract/catalog files + focused tests | targeted build 0/0；catalog/immutability facts | Completed | 2A-2 |
| HLI-2A-2 | typed Gateway 直接委托 | new interface/Gateway + focused tests | Gateway focused 12/12 | Completed | 2A-3 |
| HLI-2A-3 | public boundary 与完整回归 | allowlist test + HLI-1C stale assertion repair | Application 94/94；Host 11/11；non-UI 2537/2537 | Completed | 2A-4 |
| HLI-2A-4 | 治理、package、停止 | contract/ledger/status docs | governance flush；IdeOnly clean package 1115 files | Completed | stop before HLI-2B |

## 2. 关键实现事实

- Application Experimental public allowlist 从 29 精确增加为 35，只新增最终契约批准的
  6 个类型。
- 固定 catalog 精确包含 Section、Reference、Diagnostics、Edit Preview 四项能力，顺序、
  version=1、Query/Edit risk、Experimental stability 和现有限制由测试锁定。
- catalog 使用一次性 `Array.AsReadOnly`，Descriptor 只有 get-only 属性和 internal constructor。
- `Ra2AutomationCapabilityGateway` 是 public sealed、无状态、只有一个 public parameterless
  constructor；五个 public method 精确匹配 interface。
- 四项执行直接委托现有 DocumentQuery/EditPreview service；不捕获、包装或翻译结果。
- 没有 generic Invoke、mutable registry、wire/provider schema、统一 Gateway failure、
  Apply/Save/store/session/transaction、Job/Event/Artifact 或文件/进程能力。

## 3. HLI-2A-R1 窄边界修复

首次完整非 UI 回归为 2536/2537。唯一失败是 HLI-1C 历史测试将 Application exported type
数量硬编码为 29；HLI-2A 批准的 additive API 使正确值变为 35。

修复精确限于：

```text
RA2IniEditor.Tests/IDE/Ra2Hli1CHostBoundaryContractTests.cs
29 -> 35
```

这是 R1 test compatibility repair，不改变 HLI-1C Host authority 断言、生产代码或用户行为。
原最终契约遗漏了该下游断言，允许文件/静态门禁已在完成收口中同步修正。修复后 HLI-1C
定向 11/11、完整非 UI 2537/2537。

## 4. AgentPilot/Luna 执行记录

- Request：`H:\AgentPilotLite\Requests\ra2-hli-2a-capability-gateway-luna-v3.json`
- Provider/model/effort：Luna / `gpt-5.6-luna` / high。
- 限制：1,200,000 input、25,000 output、15,000 reasoning（结束后审计）；50 tool calls、
  1500s、2 MiB output（运行时硬门禁）；context/patch 各 512 KiB。零重试。
- TaskResult：`failed`；scope/diff gates passed，首个 build 因 `Array.AsReadOnly` 无法推断
  collection expression 元素类型（CS0411）失败，Worker tests 未运行。
- Usage：input 882,072（cached 791,808）、output 15,518、reasoning 4,920、total 897,590；
  usageBudgetStatus=passed，toolCallCount=11，codexEventCount=45。
- 保留 worktree：`H:\AgentPilotLite.Workspaces\ra2-hli-2a-capability-gateway-672d5f9b`。
- 主线程只读审计候选后没有 merge/commit/copy；以显式泛型和更稳健的反射测试在主仓库
  按同一契约重新实施并验证。未重试、未切 provider、未 cleanup。

Worker guard 和环境过滤只降低同用户进程的误操作风险，不隔离恶意同用户进程。当前 CLI
没有可信 model request count、peak request input、cache write 或精确费用数据。

## 5. Verification Matrix

| Step | Status | Evidence |
|---|---|---|
| Source baseline | Passed | branch `codex/hli-1a2`，HEAD `7d5c2aa`，初始 worktree clean |
| Pre-change Application | Passed | 82/82 |
| AgentPilot candidate | Failed / not adopted | allowed 5 paths；CS0411 at first build；tests NotRun |
| Targeted Application build | Passed | 0 warnings，0 errors after controlled implementation |
| Gateway focused | Passed | 12/12 |
| Restore | Passed | IDE-only solution projects up to date |
| IDE-only Debug build | Passed with existing warning | 0 errors；1 CS8602 in untouched `BuiltInFieldRegistryPackLoaderTests.cs` |
| Application contract/full | Passed | 94/94；exported allowlist 35 |
| HLI-1C boundary targeted | Passed | 11/11 after narrow assertion repair |
| Full non-UI | Passed | 2537/2537 |
| Diff/static boundaries | Passed | production 3 new Application files；no algorithm/IDE/Shell/XAML/project/legacy diff |
| UI/computer control | NotRun | HLI-2A 无 UI 或用户行为变化 |
| IdeOnly clean package | Passed | `artifacts/RA2IniEditor.IDE.SourceClean.zip`；1115 files；禁止项已排除 |

## 6. Diff Intent Table

| File | Change Type | Reason | In Allowed Scope |
|---|---|---|---|
| `RA2IniEditor.Application/Automation/Experimental/Ra2AutomationCapabilityContracts.cs` | public contracts | IDs、version、risk/stability、immutable descriptor | Yes |
| `RA2IniEditor.Application/Automation/Experimental/IRa2AutomationCapabilityGateway.cs` | public interface | typed discovery/query/preview boundary | Yes |
| `RA2IniEditor.Application/Automation/Experimental/Ra2AutomationCapabilityGateway.cs` | stateless façade | fixed catalog + canonical service delegation | Yes |
| `RA2IniEditor.Application.Tests/Ra2AutomationCapabilityGatewayTests.cs` | tests | 12 focused catalog/surface/parity/limit/concurrency facts | Yes |
| `RA2IniEditor.Application.Tests/Ra2AutomationBoundaryTests.cs` | contract test | exact allowlist 29 -> 35 | Yes |
| `RA2IniEditor.Tests/IDE/Ra2Hli1CHostBoundaryContractTests.cs` | narrow R1 test repair | downstream historical count 29 -> 35 | Yes after HLI-2A-R1 |
| HLI-2A contract/ledger and current governance docs | docs | completion evidence and next entry | Yes |

## 7. Deferred Governance Queue Flush

| Queue | Flushed result |
|---|---|
| PublicApiLedger | 6 candidates -> Implemented / Experimental；allowlist 35 |
| TechnicalDebt | 无新增产品债务；AgentPilot failure 和既有 warning 只记录为证据 |
| DecisionLog | 固定目录 + typed Gateway 决策 Proposed -> Accepted |
| CurrentStatus | HLI-2A Completed；下一入口 HLI-2B audit/final contract |
| Superseded docs | None |

## 8. 明确未做与停止点

- 未实现 HLI-2B IDE/AI consumer、CLI/MCP/IPC、wire schema 或 provider adapter。
- 未新增 Apply/Save/store/session/transaction、Job/Event/Artifact、permission 或 persistence。
- 未修改 parser、diagnostics、Preview 算法、Field Registry、Completion、Search、Shell/XAML、
  project files 或 legacy。
- 未自动清理 AgentPilot worktree；cleanup 需要单独明确授权。
- HLI-2A 在 final package 后停止；下一阶段必须先做 HLI-2B 代码事实审计与最终契约。
