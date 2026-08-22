# AUTOMATION-HLI-1C Host Boundary Confirmation Stage Ledger

日期：2026-08-22
状态：Completed / Verified
基线 revision：`163eff6f74eeb00050022201b341a0456e4f28ad`

## 1. 阶段结果

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| HLI-1C-0 | 冻结基线、surface 和范围 | docs/request only | clean baseline；既有 Application 82/82、Host 32/32 | Completed | 1C-1 |
| HLI-1C-1 | 加固 projection 与 invocation binding | 2 internal Editing files | build passed；错配/越界/foreign tests | Completed | 1C-2 |
| HLI-1C-2 | 证明唯一 admission seam 与 authority | 1 new test file | 11 new facts；Host filter 53/53 | Completed | 1C-3 |
| HLI-1C-3 | claim/failure/full regression | tests only | Application 82/82；non-UI 2537/2537 | Completed | 1C-4 |
| HLI-1C-4 | 治理、package、停止 | phase/governance docs | restore/build/package passed | Completed | HLI-2A audit/contract |

## 2. 关键实现事实

- `Ra2IniEditPreview.FromAutomation` 在成功投影前逐 index 精确验证 operation kind、
  Section、Key、Value 和 original span，并复用 `Ra2TextChangeSet.Apply` 验证 CandidateText。
- `Ra2IniAuthoringWorkspace.Preview` 在写入 active slot 前用 `ReferenceEquals` 绑定本次
  snapshot/plan；null 或 foreign wrapper 转为固定安全 `UnexpectedFailure`。
- generation、单 active slot、显式确认、claim-before-port、single-use、live currency、
  一个 session revision、一个 Undo 单元和 no-save 语义保持不变。
- public API diff 为 0；Application exported allowlist 精确保持 29。

## 3. AgentPilot/Luna 执行记录

- Request：`H:\AgentPilotLite\Requests\ra2-hli-1c-host-boundary-luna-v3.json`
- Provider/model/effort：Luna / `gpt-5.6-luna` / medium。
- 限制：400k input、10k output、6k reasoning（结束后审计）；20 tool calls、900s、
  1 MiB output（运行时硬门禁）。零重试。
- TaskResult：`needs_review`；第 21 次 tool call 触发硬停止；上游同时出现并发限流和
  stream timeout。usage unavailable，verification NotRun。
- 保留 worktree：`H:\AgentPilotLite.Workspaces\ra2-hli-1c-host-boundary-dd9ed329`。
- 独立审计：worktree HEAD 仍为基线，status/diff 为空；没有候选补丁可采纳。
- 主线程按同一批准契约完成实现。未重试、未扩预算、未切 provider、未 merge/push/cleanup。

Worker guard 和环境过滤只降低同用户进程的误操作风险，不是针对恶意同用户进程的隔离。

## 4. Verification Matrix

| Step | Status | Evidence |
|---|---|---|
| Source baseline | Passed | branch `codex/hli-1a2`，HEAD `163eff6`，初始 worktree clean |
| AgentPilot candidate | NeedsReview / no patch | toolCallCount 21，codexEventCount 50，usage unavailable，verification NotRun |
| Restore | Passed | `dotnet restore .\RA2IniEditor.IDE.sln`；所有项目已是最新 |
| Debug build | Passed | 最终 `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`；0 warnings，0 errors |
| HLI-1C/Host targeted | Passed | 53/53；新契约类 11 facts |
| Application contract | Passed | 82/82；allowlist 29 reflection gate included |
| Full non-UI | Passed | 2537/2537 |
| Diff/static boundaries | Passed | production 仅 2 internal Editing files；Shell/XAML/Application/Core/Infrastructure/project/legacy 0 diff |
| UI/computer control | NotRun | 本阶段无 UI、XAML、Shell 行为变更 |
| IdeOnly clean package | Passed | `artifacts/RA2IniEditor.IDE.SourceClean.zip`；1108 files；禁止目录/模式排除 |

第一次 targeted run 为 52/53：测试伪造计划错误地对不存在字段执行 replace，导致
Application 正确返回 failure；修正测试输入为同字段不同值后，同一门禁 53/53。生产
guard 未因该测试缺陷改变。

## 5. Diff Intent Table

| File | Change Type | Reason | In Allowed Scope |
|---|---|---|---|
| `RA2IniEditor.IDE/Editing/Ra2IniEditPreview.cs` | internal guard | operation/span/candidate-change integrity | Yes |
| `RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs` | internal guard | invocation wrapper instance binding | Yes |
| `RA2IniEditor.Tests/IDE/Ra2Hli1CHostBoundaryContractTests.cs` | tests | authority、surface、foreign/failure/replay contract | Yes |
| HLI-1C contract/ledger and current governance docs | docs | completion evidence and next entry | Yes |

## 6. Deferred Governance Queue Flush

| Queue | Flushed result |
|---|---|
| PublicApiLedger | HLI-1C = 0 change；allowlist 29 preserved |
| TechnicalDebt | 无新增产品债务；AgentPilot failure 只记录为执行证据 |
| DecisionLog | Workspace 包围式 Preview seam 更新为 Accepted |
| CurrentStatus | HLI-1C Completed；下一入口 HLI-2A audit/contract |
| Superseded docs | None |

## 7. 明确未做与停止点

- 未实现 Capability Gateway、descriptor、registry、dispatcher 或版本协商。
- 未修改 Shell/XAML/AutomationId、A4 policy、Apply/Save/Undo、parser、diagnostics、
  Field Registry、Completion、project files 或 legacy。
- 未自动清理 AgentPilot worktree；清理需要单独明确授权。
- HLI-1C 在 package/governance 完成后停止。下一阶段只能先做 HLI-2A 事实审计与契约。
