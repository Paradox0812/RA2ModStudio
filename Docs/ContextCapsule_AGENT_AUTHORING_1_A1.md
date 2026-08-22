# Context Capsule — AGENT-AUTHORING-1-R1 A1

## 1. Scope

- Project: RA2IniEditor.IDE-only
- Module / Stage: AGENT-AUTHORING-1-R1 A1 Language Services Facade
- Updated: 2026-07-23
- Prepared for: A2-A contract and later Plan/Preview work

## 2. Current Goal

A1 已完成。项目现在有一个 internal、UI 无关的单文档只读语言分析门面，
并能把一次分析绑定到稳定的 Field Registry Provider Revision。下一目标是先为
可编辑会话身份、Edit Revision 和 stale-preview 规则建立契约，不直接进入 Apply。

## 3. Current Architecture Invariants

- Built-in AI 与未来外部 Agent 必须共享同一 Authoring Workspace 路径。
- Agent 不接触 WPF 控件、Shell、文件 writer 或保存链路。
- 一次分析只使用 Request 捕获的 Provider Snapshot。
- Core parser 与 IDE TextModel 的 A0 八项差异不得在 A2 顺手合并。
- Provider priority 仍为 Project > Global > BuiltIn。
- 预览不等于写入；保存仍由用户控制并经过现有 preflight/backup/writer。

## 4. Recently Completed

| Task | Status | Key Change | Verification |
|---|---|---|---|
| A0 | Completed | 锁定 Core/TextModel 八项可观察差异 | 8/8 + related 26/26 |
| A1-B1 | Completed | Provider Snapshot + Revision | runtime tests 18/18 |
| A1-A1/A2 | Completed | neutral request/fact/result | contract tests 3/3 |
| A1-A3/C | Completed | facade、诊断等价、Snapshot 门禁 | combined targeted 45/45 |
| A1 package | Completed | build/full tests/clean package | 0/0 build；2355/2355；989 files |

## 5. Current Code / Data Shape

- `FieldRegistryRuntimeService` 拥有当前 Provider Snapshot；初始 Revision=1。
- 每次成功 Reload 发布一个新 Snapshot 并只递增一次 Revision。
- 旧 Snapshot/Request/Result 不会读取新的 Provider。
- `Ra2LanguageAnalysisRequest` 携带文本、分析版本和捕获的 Registry Snapshot。
- `Ra2IniLanguageAnalysisResult` 显式区分成功/失败，携带 TextModel、
  SemanticModel 和中立 DiagnosticFact。
- 门面复用完整现有诊断服务；诊断顺序和字段与现有输出一致。

## 6. Key Files for Next Work

- `Docs/AGENT-AUTHORING-1-R1_A1_ContinuousContract.md`: A1 权威契约。
- `Docs/AGENT-AUTHORING-1-R1_A1_StageLedger.md`: 实施、验证、决策和债务证据。
- `RA2IniEditor.IDE/Language/Ra2LanguageAnalysisRequest.cs`: A2 当前/候选分析输入。
- `RA2IniEditor.IDE/Language/Ra2IniLanguageAnalysisResult.cs`: A2 before/after 结果。
- `RA2IniEditor.IDE/Language/IRa2IniLanguageAnalysisService.cs`: A2 只读分析端口。
- `RA2IniEditor.IDE/Services/FieldRegistryRuntimeService.cs`: Registry Revision 所有者。
- `RA2IniEditor.Tests/IDE/Ra2IniLanguageAnalysisServiceTests.cs`: Snapshot/等价基线。

## 7. Open Risks and Technical Debt

- `AGENT-AUTHORING-A1-TD-001`: SemanticModel 在门面和现有诊断服务中各构建一次。
  仅在 A2 性能证据显示它是主要延迟时偿还。
- A1 没有定义 Edit Revision、文档身份、多变更事务或 Apply 端口；不得假设已存在。
- A1 没有承诺第三方 Provider 任意跨线程安全；后台分析只消费 Runtime Service 发布的 Snapshot。

## 8. Public API / Contract Notes

- 无 public API 变更。
- A1 新增契约全部为 internal。
- `CurrentProvider` 与 `Reload` 的既有 public 可见行为和签名保持。

## 9. Decisions and Rejected Alternatives

- Accepted: Runtime Service 发布 Provider+Revision Snapshot。
- Accepted: 使用中立 Request，不复用 `CurrentSourceSnapshot` 作为长期契约。
- Accepted: 适配现有诊断服务，不复制诊断算法。
- Rejected: 在 A1 合并 Core/TextModel 或创建“统一编译器”大重构。

## 10. Next Recommended Task

`AGENT-AUTHORING-1-R1 A2-A EditableSessionIdentityAndRevisionContract`

Allowed scope:

- 只设计文档身份、Edit Revision、Registry Revision 和 stale-preview 契约；
- 回归现有 editable session 与单 `Ra2TextChange` 接口事实；
- 设计 current/candidate Preview 所有权。

Forbidden scope:

- Agent Apply、自动保存、writer、Shell/WPF 接线；
- parser/diagnostics/Completion/Field Registry priority 重构；
- public API、依赖或项目结构变化。

Stop condition:

- 版本所有权不唯一；
- Preview 无法同时绑定文档身份、Edit Revision 与 Registry Revision；
- 需要越过现有 editable-session/save 边界。

## 11. Verification Baseline

- Last credible build/test: 2026-07-23，IDE-only build 0 warnings/0 errors；
  full non-UI tests 2355/2355。
- Clean package: passed，989 files。
- Known NotRun: UI Automation（A1 无 UI 变化）。
- Required next profile: A2-A 为 CONTRACT_ONLY 时只读回归；实现阶段另行分类。

## 12. Handoff Notes

下一上下文先读 `AGENTS.md`、A1 Continuous Contract、A1 Stage Ledger 和本 Capsule。
不要把 A1 的 `AnalysisVersion` 当成可编辑会话并发 Revision；这正是 A2-A 要解决的问题。
