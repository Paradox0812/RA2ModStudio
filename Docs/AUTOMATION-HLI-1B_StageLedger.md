# AUTOMATION-HLI-1B Stage Ledger

更新日期：2026-08-22
状态：Completed / Verified
契约：`Docs/AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md`

## 1. 完成结果

HLI-1B 已建立可由普通 `net8.0` 调用方消费的单文档结构化编辑预览能力。
Application 是 TextModel、change/insertion 与 semantic Preview 的唯一算法权威；IDE
只保留 Host snapshot、中文 presentation、A3 active preview/currency/apply/undo 的兼容层。

public service 只返回候选文本、精确变化、逐操作证据和诊断差异，不修改 Session、
Undo、Dirty 或磁盘。Apply/Save/Gateway/CLI/自动写入均未实现或扩权。

## 2. Stage Result Ledger

| Stage | 结果 | 证据 | 是否继续 |
|---|---|---|---|
| 1B-0 Baseline Guard and Rollback | Passed | Application 47/47；受影响 88/88；PreChange IdeOnly rollback package | Yes |
| 1B-1 Public Data Contract | Passed | 11 个 Experimental types；构造/immutability/failure invariants；allowlist 29 | Yes |
| 1B-2 Neutral Text and Insertion Foundation | Passed | 6 TextModel + 2 TextChange 原子迁移；共享 insertion primitive；390/390 | Yes |
| 1B-3 Semantic Preview Engine | Passed | 唯一 Application engine；limits/cancel/parity/delta/thread tests；Application 82/82 | Yes |
| 1B-4 IDE Host Adapter | Passed | IDE preview service 40 行 thin adapter；A2/A3/A4 88/88 | Yes |
| 1B-5 Integration and Regression | Passed | restore/build；82/82；88/88；390/390；full 2526/2526 | Yes |
| 1B-6 Governance, Package and Stop | Passed | API/decision/status/docs 收口；IdeOnly clean package；停止于 HLI-1B | Stop |

## 3. Public API 与行为

- 新增 `IRa2AutomationEditPreviewService` 与唯一 public 实现
  `Ra2AutomationEditPreviewService`。
- 新增 operation/plan/failure/outcome/trust/change/operation-preview/result 共 9 个类型；
  HLI-1B 总计新增 11 个 public types，Application allowlist 由 18 精确增至 29。
- public policy：8,388,608 UTF-16 chars、10,000 diagnostics、1..128 operations；
  Section/Key/Value 上限为 256/256/8192。
- 失败使用 typed kind 且不携带 candidate/change/operation/diagnostic partial payload。
- semantic payload 可重复；`PreviewId` 明确为每次调用新身份，不属于确定性比较。
- public service 无 Apply、Save、store、session 或文件 I/O。

## 4. 迁移与唯一权威

- 6 个 TextModel 和 2 个 TextChange 实现已迁入 Application internal；旧 IDE 实现路径为 0。
- IDE 旧 TextModel namespace 只保留无算法 compatibility marker。
- `Ra2LineInsertionPrimitive` 是 Preview 与 IDE AddProperty 共用的唯一行插入实现。
- A2 semantic planner 与 diagnostic delta 位于 Application；IDE 没有第二套算法。
- `Ra2IniEditPreviewService` 只做 Host snapshot 投影并调用 Application service。
- A3/A4 active proposal、live currency、显式确认、Apply/Undo 和 Save 边界保持不变。

## 5. Verification Matrix

| Gate | 结果 | 证据 |
|---|---|---|
| Pre-change Application baseline | Passed | 47/47 |
| Pre-change affected baseline | Passed | 88/88；契约预估 84，实际筛选集多 4 项 |
| Restore | Passed | IDE-only solution |
| Debug build | Passed | 0 warnings / 0 errors |
| Application.Tests | Passed | 82/82 |
| A2/A3/A4 regression | Passed | 88/88 |
| TextModel/AddProperty/Search/Completion/Save regression | Passed | 390/390 |
| Full non-UI tests | Passed | 2526/2526 |
| Application boundary | Passed | net8.0/Core-only；HLI-1B 变更文件无 WPF/IDE/Infrastructure/IO reference；全程序集无文件读写调用 |
| Migration hygiene | Passed | 8 个旧 Text/change 路径和 2 个旧 IDE plan 路径为 0；算法副本 0 |
| Reflection contract | Passed | exported allowlist 精确 29 |
| Diff check | Passed | `git diff --check` exit 0 |
| IdeOnly package | Passed | `artifacts/RA2IniEditor.IDE.SourceClean.zip`；排除规则由打包脚本验证 |
| Computer control/UI smoke | NotRun | 契约禁止 UI/Shell 变更，用户亦未要求 UI 验证 |

## 6. AgentPilot Lite / Luna 审计

- Request：`H:\AgentPilotLite\Requests\ra2-hli-1b-semantic-preview-luna-v3.json`。
- Provider/model/effort：Luna / `gpt-5.6-luna` / high。
- TaskResult：`needs_review`，不是 succeeded。post-run input 1,763,221 超过
  1,200,000 ceiling，因此 AgentPilot 未进入独立 verification。
- Usage：cached input 1,637,376；output 22,502；reasoning 8,626；total 1,785,723；
  tool calls 21/50；约 602 秒。
- Retained workspace：
  `H:\AgentPilotLite.Workspaces\ra2-hli-1b-semantic-preview-bd84de77`。
- 处理：未重试、未提高预算、未切换 provider、未自动合并/推送/清理。主流程只将
  候选作为参考，并独立修复缺少 namespace、错误 change 类型、插入重复合并、诊断
  delta 重复计算、delta traversal 取消检查和测试覆盖不足后运行全部门禁。

Token ceiling 是运行后审计，不是调用前硬中断；Worker 是同用户全权限进程，不构成
恶意代码隔离。保留工作区是操作性清理事项，不是产品运行时技术债。

## 7. Diff Intent 与边界

| 区域 | 意图 | 结果 |
|---|---|---|
| Application TextModel/change/insertion | 唯一 UI-neutral 基础 | Completed |
| Application semantic Preview | 唯一确定性候选权威 | Completed |
| Experimental API | additive 11 types / allowlist 29 | Completed |
| IDE Preview | Host compatibility thin adapter | Completed |
| Application/IDE tests | contract/parity/limits/regression | Completed |
| Shell/XAML/Core/Infrastructure/project files/legacy | 禁止变更 | No stage diff |

全 Application 静态检索仍能看到 HLI-1A 迁入的
`Classification/Ra2SectionClassifier.cs` 中一个未使用 `using System.IO;`；程序集内不存在
`File`、`Directory`、`Stream` 或 `Path` 调用。它不是 HLI-1B 引入的 I/O 或运行时依赖，
本阶段不为删除无关 import 扩大批准文件范围。

## 8. Deferred Governance Queue

### PublicApiLedger Pending Entries

已清空：11 个 HLI-1B API 已标记 `Implemented / Experimental`。

### TechnicalDebt Pending Entries

- HLI-1 Preview assembly/authority debt 已偿还；没有新增产品运行时债务。
- 既有 SemanticModel 重建性能债务保持 Open，本阶段未顺手引入 cache/session。
- AgentPilot retained workspace 由操作者按需清理，未自动删除。

### DecisionLog Candidate Entries

已清空：HLI-1B 唯一 Preview 权威与 Host Apply 所有权决策已更新为 Accepted / implemented。

### CurrentStatus Pending Updates

已清空：CurrentPhase、Roadmap、CurrentCapabilities、Compact Context、DeveloperNotes 和
README 已更新。

## 9. 回滚与包

- Pre-change rollback：
  `artifacts/RA2IniEditor.IDE.SourceClean.AUTOMATION-HLI-1B.PreChange.Rollback.zip`。
- Final clean package：`artifacts/RA2IniEditor.IDE.SourceClean.zip`。
- 无持久化格式或用户数据迁移；未恢复 legacy。

## 10. Stop Rule

HLI-1B 在此完成并停止。下一阶段只能先做 HLI-1C Host Boundary Confirmation 的
代码事实回归和最终契约；不得从本台账推断 Gateway、CLI、public Apply/Save、自动写盘
或素材流水线已经实现。
