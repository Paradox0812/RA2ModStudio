# AUTOMATION-HLI-0A Stage Ledger

阶段：AGENT-AUTHORING-1-R1 / AUTOMATION-HLI-0A Existing Capability Audit  
日期：2026-08-20  
最终状态：Completed（DocsOnly）  
权威契约：`Docs/AUTOMATION-HLI-0A_ExistingCapabilityAuditContract.md`  
能力矩阵：`Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`

## 1. Stage Result Ledger

| Stage | Goal | Files touched | Verification | State after stage | Next entry satisfied |
|---|---|---|---|---|---|
| 0A-1 Inventory | 核对 Core/Infrastructure/IDE 项目引用、真实服务、输入结果和测试 | None | 静态源码与 csproj 阅读 | Completed | Yes |
| 0A-2 Boundary classification | 区分算法无头、宿主可消费、UI/磁盘/运行时耦合 | Contract draft | 逐能力事实映射 | Completed | Yes |
| 0A-3 Capability matrix | 形成 ReuseAsIs/Extract/Move/Adapter/NotPresent/Deferred 决策 | Matrix | 路径存在性与能力缺口检索 | Completed | Yes |
| 0A-4 Architecture comparison | 比较 Application/Core/IDE 三种程序集落点 | Matrix | 当前依赖方向静态核对 | Completed | Yes |
| 0A-5 Governance flush | 更新 contract、ledger、CurrentPhase 和 Full Context | Five approved docs | DocsOnly scope audit | Completed | Yes, HLI-0B contract only |

## 2. 关键事实记录

1. `RA2IniEditor.Core` 与 `RA2IniEditor.Infrastructure` 为 `net8.0`；`RA2IniEditor.IDE` 为 `net8.0-windows` + WPF。
2. A1 Language、当前文档 references/definitions、Search、diagnostics 和 A2 Preview 大多算法上不需要 UI 控件，但均位于 IDE assembly，当前不能作为独立无头宿主引用。
3. Core `IRa2FieldDefinitionProvider` / `Ra2FieldDefinition` 是当前最接近可直接复用的中立领域 contract。
4. Current diagnostics 返回 `IdeDiagnosticIssueViewModel`，需先抽 neutral result，不能直接成为 Gateway contract。
5. Project Search 是文本能力；项目级 semantic reference capability 仍不完整。
6. A3 Apply、Undo 和当前活动 Preview 生命周期必须继续由 IDE host 持有；Save/Writer 同样不得暴露给 Agent。
7. Template、Capability Registry/Gateway、Job State、Event Bus 和 Artifact identity 在当前源码中不存在。

## 3. Diff Intent Table

| File | Change type | Reason | In allowed scope |
|---|---|---|---:|
| `Docs/AUTOMATION-HLI-0A_ExistingCapabilityAuditContract.md` | Add | 固化 HLI-0A 范围、方法、门禁与验收 | Yes |
| `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md` | Add | 记录真实能力、耦合、复用和迁移决策 | Yes |
| `Docs/AUTOMATION-HLI-0A_StageLedger.md` | Add | 记录阶段证据、验证和治理队列 | Yes |
| `Docs/Codex_CurrentPhase.md` | Update | 将 HLI-0A 标为最新可信阶段 | Yes |
| `Docs/RA2IniEditor_IDE_Full_Codex_Context.md` | Update | 维护跨上下文架构事实和下一入口 | Yes |

## 4. Verification Matrix

选定 profile：`DocsOnly`

| Step | Status | Evidence |
|---|---|---|
| Contract/matrix presence | Passed | 三份 HLI-0A 文档存在且互相引用 |
| Source path inventory | Passed | 矩阵中“已存在”的主要实现路径均由当前工作区源码核对 |
| Assembly dependency direction | Passed | 三个 csproj 的 target framework 与 ProjectReference 已核对 |
| Missing-infrastructure search | Passed | Core/Infrastructure/IDE/Tests 中未找到正式 Capability/Job/Event/Artifact/Template contract |
| Exact write scope | Passed | 仅五份获批文档写入；无源码、项目或测试文件改动 |
| Contract term consistency | Passed after bounded correction | 首次检查发现矩阵只写缩写；补齐 `Algorithmically headless` / `Headless-host consumable` 完整术语后以同一检查重跑通过 |
| Build / Compile | NotRun | DocsOnly；无 source/project/config/public API change |
| Targeted Tests | NotRun | DocsOnly；本阶段只记录已有测试证据，不改变行为 |
| Full Suite | NotRun | DocsOnly；全量测试不能提高本次文档事实核对的可信度 |
| Package | NotRun | 无产品/发布行为变化，不生成新包 |
| UI / computer control | NotRun | 无 UI 变更，且本阶段禁止电脑操控 |

验证充分性：对 HLI-0A 文档审计充分；不构成对未来 HLI-0B 接口或 HLI-1 实现的编译/运行验证。

## 5. Deferred Governance Queue

### PublicApiLedger Pending Entries

| Stage | API | Kind | Reason | Expected next use | Stability | Tests |
|---|---|---|---|---|---|---|
| HLI-0B | query/diagnostic/preview request-result-failure contracts | application contract candidates | 解除 IDE assembly placement | HLI-1A/1B | Experimental | 必须新增 contract/characterization tests |
| HLI-2A | capability id/descriptor/invocation result | gateway contract candidate | 统一 Agent/CLI 调用入口 | HLI-2B/2C | Experimental | registry/routing tests |
| HLI-3 | wire DTO / IPC protocol | serialized public protocol | 外部进程桥 | later | Unspecified | protocol/security/integration tests |

### TechnicalDebt Pending Entries

| Stage | Debt | Reason | Impact | Suggested resolution | Status |
|---|---|---|---|---|---|
| HLI-0A | `HLI-TD-001` UI-neutral code in WPF assembly | 历史上与 IDE 一起演进 | 独立宿主不可消费 | HLI-1A/1B 渐进迁移 | Open / Controlled |
| HLI-0A | `HLI-TD-002` diagnostics return ViewModel | presentation 与 analysis 混合 | 不适合作为 contract | neutral diagnostic facts | Open / Controlled |
| HLI-0A | `HLI-GAP-001` no general project semantic reference query | 当前 finder 单文档 | capability scope 易被夸大 | HLI-0B 明确 scope | Open |
| HLI-0A | `AGENT-AUTHORING-A1-TD-001` duplicate semantic model build | 既有诊断复用方式 | 性能而非正确性 | 后续独立性能阶段 | Existing / Controlled |

### DecisionLog Candidate Entries

| Stage | Decision | Status | Reason | Needs human review |
|---|---|---|---|---:|
| HLI-0A | 以新 `net8.0` Application layer 作为推荐候选 | Proposed, not approved | 避免 Core 膨胀并解除 WPF assembly 依赖 | Yes, HLI-0B |
| HLI-0A | Agent 只获得 query + preview，Apply/Save 为 host-only | Reaffirmed candidate | 延续 A2/A3/A4 单一事务与用户确认 | Yes, HLI-0B |
| HLI-0A | 进程内 contract 与未来 wire DTO 分离 | Proposed | 避免过早序列化 internal model | Yes, HLI-0B |
| HLI-0A | Job/Event/Artifact runtime 后置 | Proposed | 当前无基础设施且首闭环不需要 | Yes, HLI-2/HLI-3 |

### CurrentStatus Pending Updates

| Area | New status | Latest trusted doc | Next entry |
|---|---|---|---|
| High-level Automation | HLI-0A audit completed | `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md` | HLI-0B contract only |
| Agent Authoring | A4-R1 remains runtime baseline | `Docs/AGENT-AUTHORING-1-R1_A4_R1_StageLedger.md` | Consume future Gateway only after HLI-2B |

## 6. Scope self-review

- Public API：None。
- Persistence/serialization：None。
- Unity：Not applicable / None。
- Shell/XAML/Dock/AutomationId：None。
- Parser/diagnostics/Completion/Field Registry/Search/Save/Undo behavior：None。
- Dependencies/project structure：None。
- Legacy：未恢复、未读取为活动入口、未修改。
- Generated/build/cache files：None。

## 7. Stop and next safe entry

HLI-0A 到此停止。下一阶段只能是 `AUTOMATION-HLI-0B Minimum Capability Contract`：先决定程序集、数据所有权、failure/cancellation/version 语义和首批能力 scope，再等待用户确认。不得直接创建 `RA2IniEditor.Application`、Gateway、public API 或外部桥。
