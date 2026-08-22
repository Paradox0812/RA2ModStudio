# RA2IniEditor.IDE Public API Ledger

更新时间：2026-08-22  
当前阶段：AUTOMATION-HLI-1A0

本台账只记录跨程序集或未来 Gateway 可见的契约候选。当前 HLI-1A0 仅完成
特征化，没有新增、修改或发布任何 public API。

## 1. 当前变更

| 阶段 | Public API 变更 | 兼容性影响 |
|---|---|---|
| HLI-1A0 | None | None；tests + docs only |

## 2. HLI-1A1 候选查询契约

以下是 `AUTOMATION-HLI-1A1` 最终契约冻结的 Proposed/Experimental allowlist，
不是当前已实现 API：

| Task/Stage | API | Kind | Reason | Expected Next Use | Stability | Tests | Notes |
|---|---|---|---|---|---|---|---|
| HLI-1A1 | `IRa2AutomationDocumentQueryService` | Interface | 跨程序集调用 Section/Reference | HLI-2A Gateway | Experimental | interface/reflection/behavior | 不含 Diagnostics/Preview |
| HLI-1A1 | `Ra2AutomationDocumentQueryService` | Stateless implementation | 给 IDE/Agent host 可构造的唯一实现 | HLI-2A Gateway | Experimental | thread/boundary/parity | 无 cache/IO/singleton |
| HLI-1A1 | `Ra2AutomationDocumentSnapshot` | Immutable envelope | 绑定文档身份、版本、文本和 Registry generation | all HLI-1 slices | Experimental | constructor/identity/retention | FilePath 不授予 I/O |
| HLI-1A1 | `Ra2AutomationFieldRegistrySnapshot` | Immutable envelope | 绑定 readonly Provider + Revision | all HLI-1 slices | Experimental | constructor/revision/retention | 非 wire DTO |
| HLI-1A1 | `Ra2AutomationTextSpan` | Value DTO | 对外表达零基半开字符区间 | HLI-1A1/1B/2A | Experimental | range/overflow/bounds | 不公开 internal span |
| HLI-1A1 | `Ra2AutomationSectionQuery` | Request DTO | 名称 + nullable zero-based occurrence | HLI-2A | Experimental | unique/ambiguous/occurrence | null 要求唯一 |
| HLI-1A1 | `Ra2AutomationSectionQueryResult` | Result DTO | 结构化成功/失败与文档身份 | HLI-2A | Experimental | state invariants | 失败无 Section payload |
| HLI-1A1 | `Ra2AutomationSectionQueryFailureKind` | Failure enum | 区分 not found/ambiguous/limit/cancel/analysis | HLI-2A | Experimental | every enum path | 不以 message 推断 |
| HLI-1A1 | `Ra2AutomationSectionFact` | Fact DTO | Section kind/occurrence/spans/fields | HLI-2A/2C | Experimental | ordering/spans/duplicate isolation | 不公开 SemanticModel |
| HLI-1A1 | `Ra2AutomationFieldFact` | Fact DTO | 所选 Section 内字段事实 | HLI-2A/2C | Experimental | duplicate key/order/spans | 按 body span 归属 |
| HLI-1A1 | `Ra2AutomationReferenceQuery` | Request DTO | offset + optional selection | HLI-2A | Experimental | offset/selection/fallback | 不含 WPF caret |
| HLI-1A1 | `Ra2AutomationReferenceQueryResult` | Result DTO | 目标和有序 current-document 引用 | HLI-2A | Experimental | empty/unresolved/order | empty 可成功 |
| HLI-1A1 | `Ra2AutomationReferenceQueryFailureKind` | Failure enum | 区分 location/target/limit/cancel/analysis | HLI-2A | Experimental | every enum path | 无 NoReferences failure |
| HLI-1A1 | `Ra2AutomationReferenceTargetFact` | Fact DTO | 已解析目标名称和类型 | HLI-2A/2C | Experimental | header/value/missing definition | 不保证定义存在 |
| HLI-1A1 | `Ra2AutomationReferenceFact` | Fact DTO | current-document 引用来源与 spans | HLI-2A/2C | Experimental | source order/case/spans | 不含 navigation command |

命名空间统一为 `RA2IniEditor.Application.Automation.Experimental`。实现完成前表中
状态不得改为 Implemented；实现时 exported types 必须与这 15 项精确相等。

## 3. 延后候选

| 候选 | 状态 | 原因 |
|---|---|---|
| Diagnostics query/result DTO | Deferred to HLI-1A2 | 当前算法返回 IDE ViewModel，需先抽离唯一权威算法再由 IDE 映射 |
| Edit Preview request/result/change DTO | Deferred to HLI-1B | 依赖完整 TextModel、FieldTrust 与 A2 planner；不属于 Query 首切片 |
| Apply/Undo API | Host-only by design | 继续由 IDE active session/currency/transaction 拥有 |
| Save/Backup/Rollback API | Host/user-owned by design | 不成为 Headless Application 的写盘接口 |

## 4. 明确不公开的实现基础

下列现有类型在 HLI-1A1 中即使移动，也应保持 internal，并通过精确
`InternalsVisibleTo` 仅供 IDE、现有 Tests 和新 Application.Tests 使用：

- `Ra2IniLineParser`
- `Ra2SectionClassifier` 及 classification result/warning
- `Ra2DocumentSemanticModelBuilder`、`Ra2DocumentSemanticModel`
- `Ra2CaretContextService`、`Ra2ReferenceFinder`
- 相关 symbol/span/reference internal records

禁止用公开这些内部模型的方式减少迁移改动；未来 Gateway 只能依赖上面的高层
Experimental DTO/service。

## 5. 下一次台账更新门禁

HLI-1A1 最终契约已经冻结名称、namespace、失败类型、limits、cancellation 和兼容
策略，但尚未获得生产实施确认。只有实施完成且 reflection/contract/full regression
通过后，才能把候选改为 Implemented。若签名或形状变化，必须先修订最终契约。
