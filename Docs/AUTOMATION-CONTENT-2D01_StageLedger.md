# CONTENT-2D-0/1 — Object Closure and Registration Stage Ledger

更新时间：2026-08-24  
契约：`Docs/AUTOMATION-CONTENT-2D01_ObjectClosureRegistrationFinalContract.md`

## 1. 阶段结果

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| CONTENT-2D-0 | 冻结对象闭包与注册策略边界 | contract、internal registration model | build + compiler contract tests | Completed | 是 |
| CONTENT-2D-1 | 当前文档确定性数字注册分配 | compiler、registration allocator、tests | 37/37 focused；162/162 Application；106/106 IDE focused；2610/2610 IDE non-UI | Completed | 是；可进入 2D-2 契约 |

## 2. 实现事实

- `Ra2ContentTemplateDefinition` additive 持有 internal `Registrations`；既有调用零改动。
- `ExplicitNumberedList` 从当前 Snapshot 的唯一注册 Section 读取事实。
- 新索引为 `max + 1`；不填洞、不重排，已有唯一对象注册幂等。
- 非数字/负数/重复索引、重复对象、非法 ID、Section 缺失/类型不符及索引溢出均整体失败。
- 注册写入继续生成既有 `UpsertField` 并进入 canonical Preview/Apply/Undo。
- 现有六个生产 Profile 未声明 Registration，用户可见行为和工具 schema 不变。
- Field Registry、parser、classifier、diagnostics、Save、Shell 和 XAML 未修改。

## 3. 验证证据

```text
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
Passed: 0 warnings, 0 errors

Ra2ContentTemplateCompilerTests + Ra2AutomationTemplateServiceTests
Passed: 37/37

RA2IniEditor.Application.Tests full
Passed: 162/162

IDE ContentTemplate/AiAuthoring/AiAssistantPipeline/DeepSeek loopback focused
Passed: 106/106

RA2IniEditor.Tests non-UI full
Passed: 2610/2610
```

```text
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
Passed: 1183 files; build/cache/archive exclusions confirmed
```

## 4. Public API

零变化。新增 registration model、catalog、allocator 和 failure kinds 全部为 Application internal；
Application Experimental allowlist 继续精确为 59，Gateway catalog/method surface 不变。

## 5. Deferred Governance Queue

### PublicApiLedger Pending Entries

已在包停止点合并为 public API 零变化记录。

### TechnicalDebt Pending Entries

| Stage | Debt | Reason | Impact | Suggested Resolution | Status |
|---|---|---|---|---|---|
| 2D-1 | 注册 Section→Kind 目录与 classifier 的 private 目录具有同源数据 | 本阶段禁止改 classifier | 新增注册家族时需同步两处 | 在独立 classifier/catalog 复用契约中抽取唯一 internal catalog | Open / controlled |

### DecisionLog Candidate Entries

已记录“注册是 typed template operation，不是 Field Registry row”。

### CurrentStatus Pending Updates

已刷新 CurrentPhase、Compact Context 与 Roadmap。

## 6. 明确未做

- 没有新增 Techno、SuperWeapon、Faction 或 AI Profile；
- 没有多文档事务或 `artmd.ini` 写入；
- 没有自动 Apply/Save；
- 没有真实 DeepSeek 或 WPF 电脑操控测试；本阶段无新 UI/Provider 行为。

## 7. 下一安全入口

`CONTENT-2D-2 Project Multi-Document Transaction Code-Fact Audit and Final Contract`。
该阶段必须先解决跨文档 Snapshot currency、统一 Diff、原子 Apply/rollback 和 compound Undo，
之后才能实现 rules/art 绑定。
