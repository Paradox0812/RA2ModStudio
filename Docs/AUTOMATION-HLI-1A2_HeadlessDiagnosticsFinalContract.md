# AUTOMATION-HLI-1A2 Headless Diagnostics Final Contract

契约日期：2026-08-22  
状态：Completed / Verified
前置基线：AUTOMATION-HLI-1A1 Completed / Verified  
事实依据：`Docs/AUTOMATION-HLI-1A2_DiagnosticsCodeFactAudit.md`

## 1. 目标

建立首个可由 `net8.0` Agent/Gateway 调用的当前文档诊断能力：

```text
ini.document.diagnostics.validate
```

该能力从显式 `Ra2AutomationDocumentSnapshot` 运行现有结构、字段、引用与链路
规则，返回 UI-neutral immutable facts。IDE 问题面板、项目级 diagnostics、A1/Preview
和 Save Preflight 必须继续消费同一份规则实现。

## 2. 风险与授权门

```text
Implementation risk: R3 assembly/authority migration
Public contract risk: R2 Experimental API expansion
Persistence/snapshot/wire risk: None
UI risk: None; no XAML or AutomationId change
Governance mode: Deferred during continuous cards; flush at HLI-1A2 stop
```

本文档已获用户明确实施授权，并已按 1A2-0 至 1A2-5 连续完成。最终证据见
`Docs/AUTOMATION-HLI-1A2_StageLedger.md`。

## 3. 非目标

HLI-1A2 明确不实现：

- project-wide public diagnostics API；
- 文件枚举、读盘、后台索引或项目一致性 snapshot；
- 新 Diagnostics 规则、修复建议、code action 或 auto-fix；
- 修改 severity、message、code、location、order 或过滤规则；
- 改变 Field Registry `Project > Global > BuiltIn` 优先级或数据；
- 公开 SemanticModel、FieldTrust、reference catalog 或 internal fact；
- 修改 Issues UI、Shell、XAML、AutomationId、AI 面板或 Dock；
- 改变 Save/Apply/Undo/Backup/Rollback 权威；
- 合并 Parser/TextModel/SemanticModel 的双解析路径；
- 实现 Gateway、CLI、MCP、JSON/IPC 或网络契约。

## 4. 架构决策

### 4.1 唯一权威

```text
Application internal diagnostic core
  -> neutral Ra2DiagnosticFact
       -> Experimental public projection
       -> IDE compatibility ViewModel projection
```

Application 中只有一套 structure/field/reference/chain 规则。IDE 旧文件不保留
规则副本；compatibility wrapper 只转换 snapshot、failure 和 presentation DTO。

### 4.2 复用现有 service

在已有 `IRa2AutomationDocumentQueryService` 增加 `Validate`。不新建
`IRa2AutomationDiagnosticsQueryService`，不新增空请求 DTO。这与 HLI-0B 的两个高层
service 边界一致，也避免 Gateway 注册膨胀。

### 4.3 数据所有权

| 概念 | 所有者 | 生命期/可变性 | 序列化 |
|---|---|---|---|
| Document/Registry snapshot | Host capture -> Application invocation | 调用期间只读 | 否，进程内 CLR contract |
| SemanticModel/catalog | Application internal | invocation-local derived data | 否 |
| `Ra2DiagnosticFact` | Application internal | immutable invocation result | 否 |
| `Ra2AutomationDiagnosticFact` | Experimental boundary | immutable defensive copy | 否，不是 wire DTO |
| `IdeDiagnosticIssueViewModel` | IDE presentation | immutable display projection | 否 |
| Problems filters/navigation | IDE ViewModel | UI session | 否 |

不增加 cache、session、singleton、registry 引用查找或持久化字段。

## 5. 精确 public API

命名空间继续是：

```csharp
RA2IniEditor.Application.Automation.Experimental
```

### 5.1 现有接口的唯一签名扩展

```csharp
public interface IRa2AutomationDocumentQueryService
{
    // HLI-1A1 existing methods remain byte-for-byte compatible.

    Ra2AutomationDocumentDiagnosticsResult Validate(
        Ra2AutomationDocumentSnapshot snapshot,
        CancellationToken cancellationToken = default);
}
```

`Ra2AutomationDocumentQueryService` 是唯一生产实现。Section/Reference 签名、限制和
行为不得修改。接口加方法对第三方自定义 implementer 具有 source/binary 风险；
当前仓库无第二实现，API 仍为 Experimental，因此本阶段接受该风险。

### 5.2 新增精确 3-type allowlist

```csharp
public enum Ra2AutomationDocumentDiagnosticsFailureKind
{
    None = 0,
    DocumentTooLarge = 1,
    ResultLimitExceeded = 2,
    Canceled = 3,
    AnalysisFailed = 4
}

public sealed class Ra2AutomationDocumentDiagnosticsResult
{
    public bool Succeeded { get; }
    public Ra2AutomationDocumentDiagnosticsFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid DocumentId { get; }
    public int Version { get; }
    public string FilePath { get; }
    public long FieldRegistryRevision { get; }
    public IReadOnlyList<Ra2AutomationDiagnosticFact> Diagnostics { get; }
}

public sealed class Ra2AutomationDiagnosticFact
{
    public string Code { get; }
    public string SourceKind { get; }
    public IniIssueSeverity Severity { get; }
    public string Message { get; }
    public string FilePath { get; }
    public int? LineNumber { get; }
    public int? ColumnNumber { get; }
    public string? SectionId { get; }
    public string? Key { get; }
    public int AnalysisVersion { get; }
}
```

构造器保持 `internal`，所有 property 只读，Diagnostics 在 result 构造时做 defensive
copy。Application exported type allowlist 从 15 精确增加到 18；不允许额外 public type。

`LineNumber` 和 `ColumnNumber` 保持现有 1-based 语义，无法导航时为 null；
`AnalysisVersion` 必须等于输入 snapshot Version。首版不在 diagnostic fact 额外
公开 span、navigation command、fix 或 confidence。

### 5.3 Result invariant

- `Succeeded == (FailureKind == None)`。
- 成功可返回空 Diagnostics；“无问题”是成功，不是 failure。
- 失败时 Diagnostics 必须为空，不返回 partial payload。
- identity/version/revision 在成功和失败中均来自输入 snapshot。
- `ArgumentNullException`/constructor validation 仅用于编程错误；运行时限制、取消和分析失败使用 typed result。
- Message 是安全显示文本，不包含 raw exception/provider body。

## 6. 精确诊断行为

### 6.1 输入事实

- `snapshot.Text` 是唯一文本事实源；FilePath 只是 identity/display，不授予 I/O。
- `snapshot.FieldRegistry.Provider` 和 Revision 是一次捕获，调用期间不重读 runtime singleton。
- `IsEditable` 不影响只读 diagnostics。
- public Validate 只建立 current-document reference catalog，不查看项目其他文件。

### 6.2 顺序和等价性

输出必须按以下顺序追加，不做全局重排：

1. Core `INI_STRUCTURE`。
2. Field diagnostics。
3. Generic reference diagnostics，排除与 chain 同 line/column/Section/key 的 missing target。
4. Chain diagnostics。

必须逐属性保持 code、source kind、severity、message、file path、line/column、
Section/key 和 analysis version。已记录的 alias、numeric key、unknown section、inline
comment、allowed value、neutral token、trust-level 和 chain precedence 语义全部不变。

### 6.3 限制和取消

复用 `Ra2AutomationDocumentQueryService` 已有边界：

```text
MaximumDocumentCharacters = 8,388,608 UTF-16 chars
MaximumResultItems = 10,000 diagnostic facts
```

检查点至少包括：

- 构建任何 parser/semantic model 前；
- Core parse/validate 后；
- SemanticModel build 前后；
- field/reference/chain 投影每 256 项；
- 每个规则组之间；
- 构造成功 result 前。

一旦结果超过 10,000，立即返回 `ResultLimitExceeded`，不携带前 10,000 项。
取消只在传入 token 已请求取消时映射为 `Canceled`。

IDE compatibility wrapper 调用同一 internal core，但不将新 public item limit 静默施加到
既有 UI 行为；它使用 host 已有文件边界和 `CancellationToken.None`。

### 6.4 异常契约

- public Validate 的非致命异常 -> `AnalysisFailed` + safe message + empty facts。
- 仅 token 已请求时 `OperationCanceledException` -> `Canceled`。
- `OutOfMemoryException`、`AccessViolationException`、`AppDomainUnloadedException`、
  `BadImageFormatException` 不降级，继续抛出。
- public Validate 不把 `DIAGNOSTIC_EXCEPTION` 当成成功 fact；该 code 只为现有 IDE
  compatibility wrapper 的异常投影。
- IDE wrapper 在本阶段保持现有 `DIAGNOSTIC_EXCEPTION` code/source/severity/location/version
  行为，不借迁移改变 Problems/Save Preflight 可观察语义。

取消是协作式的：本阶段不改写 Core `IniParser` 为可中断 parser，因此 token
不能在单次 Core parse 内部抢占终止；必须在 parse 前后和后续循环检查。

## 7. 内部迁移契约

### 7.1 原子迁移的 9 个现有文件

```text
RA2IniEditor.IDE/Diagnostics/Ra2FieldDiagnosticService.cs
RA2IniEditor.IDE/Diagnostics/Ra2ReferenceDiagnosticCatalog.cs
RA2IniEditor.IDE/Diagnostics/Ra2ReferenceDiagnosticCatalogBuilder.cs
RA2IniEditor.IDE/Diagnostics/Ra2ReferenceDiagnosticService.cs
RA2IniEditor.IDE/Diagnostics/Ra2ChainDiagnosticService.cs
RA2IniEditor.IDE/FieldTrust/Ra2FieldTrustClassifier.cs
RA2IniEditor.IDE/FieldTrust/Ra2FieldTrustInfo.cs
RA2IniEditor.IDE/FieldTrust/Ra2FieldTrustLevel.cs
RA2IniEditor.IDE/Language/Ra2DiagnosticFact.cs
```

目标分别为：

```text
RA2IniEditor.Application/Diagnostics/**
RA2IniEditor.Application/FieldTrust/**
```

移动后保持 internal，旧 IDE 文件必须删除。规则文件只允许进行 namespace、
input/fact type、cancellation/limit collector 的必要机械改造；不修改判定分支或 message。

### 7.2 新 internal 组合核心

新增一个 `Ra2DocumentDiagnosticService` internal 组合层，负责：

- Core parse/validate；
- 需要 field provider 时的 SemanticModel；
- current/project-supplied catalog 选择；
- structure -> field -> generic reference -> chain 顺序；
- chain/generic duplicate suppression；
- cancellation/result limit collector。

它不引用 IDE、Infrastructure、WPF、ViewModel、IO 或 runtime singleton，也不捕获与翻译
Host/public failure；失败翻译由边界 adapter 负责。

### 7.3 IDE 兼容适配

`CurrentFileReadonlyDiagnosticService` 的 public 类名、无参构造、`Analyze` 参数和返回
形状保持。其内部只：

1. 将 loaded `CurrentSourceSnapshot` 投影为 Application internal document snapshot；
2. 调用 `Ra2DocumentDiagnosticService`；
3. 按原顺序把 `Ra2DiagnosticFact` 逐属性映射为 `IdeDiagnosticIssueViewModel`；
4. 保持 null/unloaded/exception 兼容行为。

`ManualFullDiagnosticsService` 继续建立 project catalog 并通过 internal Application 类型传入同一
核心；它的 I/O、skip、current editor override、analyzed/skipped count 和 status text 不变。

`Ra2IniLanguageAnalysisService` 及 Preview 继续消费迁入 Application 的 internal
`Ra2DiagnosticFact`。为降低 HLI-1A2 语义风险，A1 的完整 TextModel orchestration 和
双解析性能债务不在本阶段重写；HLI-1B 再进行完整 Preview 闭包收口。

## 8. 允许文件

实施阶段只允许修改：

```text
RA2IniEditor.Application/Automation/Experimental/IRa2AutomationDocumentQueryService.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationDocumentQueryService.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationDocumentDiagnosticContracts.cs (new)
RA2IniEditor.Application/Diagnostics/** (moved/new internal core)
RA2IniEditor.Application/FieldTrust/** (moved internal)
RA2IniEditor.IDE/Diagnostics/CurrentFileReadonlyDiagnosticService.cs
RA2IniEditor.IDE/Diagnostics/ManualFullDiagnosticsService.cs (only required namespace/adapter wiring)
RA2IniEditor.IDE/GlobalUsings.cs
RA2IniEditor.Tests/GlobalUsings.cs
RA2IniEditor.Application.Tests/** diagnostics/boundary tests
RA2IniEditor.Tests/IDE/** directly affected diagnostics/A1/boundary tests
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/DevelopmentRoadmap.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/README.md
Docs/AUTOMATION-HLI-1A2_StageLedger.md (new at completion)
```

`RA2IniEditor.IDE/Language/Ra2DiagnosticFact.cs` 及上述 8 个旧算法/FieldTrust 路径只允许
作为迁移删除。

SDK-style project 会自动纳入新 `.cs` 文件；因此 solution/csproj 默认不得修改。
`BuiltInFieldRegistryPackLoaderTests.cs` 应通过 Tests global using 兼容，不得为迁移顺手修改其
现有 CS8602 或 Field Registry 断言。

## 9. 禁止文件和语义

不得修改：

```text
ShellWindow.xaml / ShellWindow.xaml.cs
all other XAML and AutomationIds
RA2IniEditor.Core parser/validator/schema behavior
RA2IniEditor.Infrastructure Field Registry runtime/data/provider priority
IssuesViewModel filtering/sorting/navigation behavior
Ra2SavePreflightDiagnosticService behavior
Completion/Hover/Quick Peek visible behavior
AI provider/model/streaming/edit policy
A2/A3 Apply/Undo/Save/Backup/Rollback behavior
legacy solution/project/editor
package exclusions or build configuration unrelated to this slice
```

如果编译需要改变上述行为，必须停止并修订契约，不得为了绿测试
削弱旧断言。

## 10. 实施任务卡

### HLI-1A2-0 Baseline Guard

- 确认 HLI-1A1 工作区和 public 15-type allowlist。
- 运行 Diagnostics/A1/FieldTrust 基线集，要求 149/149。
- 生成精确 moved-file/consumer 清单。
- Gate 失败则停止。

### HLI-1A2-1 Neutral Core Migration

- 原子迁移 9 个 internal 文件。
- 将规则输出从 ViewModel 替换为 `Ra2DiagnosticFact`。
- 新增 internal `Ra2DocumentDiagnosticService` 和 cancellation/limit collection。
- 删除旧路径，静态证明无算法副本。
- 运行 direct field/reference/chain/FieldTrust tests。

### HLI-1A2-2 IDE Compatibility Wiring

- 把 `CurrentFileReadonlyDiagnosticService` 收窄为 adapter。
- 保持 ManualFullDiagnostics project catalog/IO host 语义。
- 保持 A1/Preview/Save/Issues/AI 消费者编译与行为。
- 运行 149 项基线，必须全绿才能继续。

### HLI-1A2-3 Experimental Validate API

- 仅增加 `Validate` 和 3 个 public types。
- 实现 identity、revision、immutable list、empty success 和 failure invariants。
- 实现 char/item/cancellation/fatal/nonfatal 契约。
- 将 reflection exported allowlist 精确更新为 18。
- 运行独立 Application diagnostics contract tests。

### HLI-1A2-4 Integration and Regression

- 运行 Application.Tests、149 项依赖集、完整现有 tests。
- 审计 Application 仅引用 Core，禁止 WPF/IDE/Infrastructure/IO。
- 检查旧路径 0、旧 fully-qualified names 0、算法副本 0。
- 运行 `git diff --check`。

### HLI-1A2-5 Governance and Package Stop

- 更新 PublicApiLedger，把 3 个候选标记 Implemented / Experimental。
- 接受/更新 DecisionLog，生成 Stage Ledger 和 Verification Matrix。
- 更新 CurrentPhase/Roadmap/Compact Context。
- 生成 IdeOnly clean source package 并检查排除项。
- 停止于 HLI-1A2，不自动进入 HLI-1B。

## 11. 测试契约

### 11.1 Application headless tests

至少覆盖：

- 空文档成功空结果；
- Core unknown-line/duplicate Section/duplicate Key 等价事实；
- field unknown/alias/boolean/enum/enum-list/integer/float/inline comment；
- guardrail/obsolete/non-existent/pseudo/inferred trust 语义；
- missing generic reference、neutral token、allowed value、case-insensitive catalog；
- Weapon/Projectile/Warhead chain 和 generic suppression；
- current-document scope，不隐式使用项目其他文档；
- 完整 fact 属性、顺序、identity/version/revision；
- result defensive copy 和 failure-no-partial invariant；
- 8,388,608 char 边界；
- 10,000 item 边界；
- pre-canceled 和 mid-projection cancellation；
- nonfatal -> safe AnalysisFailed；fatal exception rethrow；
- thread-safe/stateless repeated invocation；
- reflection allowlist 精确 18。

### 11.2 IDE compatibility tests

必须保持：

- 审计基线过滤集 149/149；
- `CurrentFileReadonlyDiagnosticServiceTests` 的 null/unloaded/exception/text-only 契约；
- field/reference/chain 每个既有行为断言；
- ManualFullDiagnostics 的当前内存文本覆盖、读盘、skip 和 cross-file catalog；
- Save Preflight 问题来源/严重度摘要；
- A1 逐属性、不重排映射与 safe failure；
- Preview diagnostic delta 指纹；
- Completion/Hover/FieldDetails/AI 的 FieldTrust 回归。

### 11.3 实施收口命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build `
  --filter "FullyQualifiedName~Diagnostic|FullyQualifiedName~Ra2IniLanguageAnalysis|FullyQualifiedName~Ra2FieldTrustClassifier"
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

全套测试是 R3 迁移的必选门禁，不能用新增测试代替。

## 12. 静态门禁

- Application target 仍为 `net8.0`，仅 ProjectReference Core。
- Application production source 无 `System.Windows`、AvalonEdit、AvalonDock、IDE、Infrastructure、
  `File/Directory/Process/Dispatcher/Clipboard`。
- exported types 与精确 18-type allowlist 一致。
- moved old paths 0，FieldTrust/Diagnostics duplicate implementation 0。
- Section/Reference public API 签名和既有测试不变。
- Shell/XAML/Core/Infrastructure runtime/Field Registry data/legacy diff 为 0。
- clean package 无 `.vs/bin/obj/artifacts/TestResults/old zip`。

## 13. Diff Intent

| 路径 | 类型 | 理由 | 范围内 |
|---|---|---|---|
| `Application/Diagnostics/**` | Move/Add | 建立 neutral 唯一诊断核心 | Yes |
| `Application/FieldTrust/**` | Move | 诊断与 HLI-1B 共享唯一可信度分类 | Yes |
| `Application/Automation/Experimental/**` | Additive | Validate + 3 public types | Yes |
| `IDE/Diagnostics/CurrentFileReadonlyDiagnosticService.cs` | Refactor | 单向兼容 adapter | Yes |
| `IDE/Diagnostics/ManualFullDiagnosticsService.cs` | Wiring only | 项目 host 复用 Application catalog/core | Yes |
| project global usings/tests | Compatibility/Test | 控制 namespace churn 和等价证据 | Yes |
| governance docs | Docs | API/decision/status/verification 收口 | Yes |

## 14. 回滚策略

- 本阶段没有持久化或用户数据迁移。
- 每卡保持可构建；不使用 IDE/Application 双份规则作为“临时回滚”。
- 若 1A2-1 迁移不能通过 direct tests，停在该卡并回报实际 diff，不继续 public API。
- 若 public API 发现必须公开 raw model/catalog 才能实现，停止并重议 API，不扩大 allowlist。

## 15. 自审结果

| 项目 | 结果 | 证据/处理 |
|---|---|---|
| 是否复制诊断算法 | Passed | 9 文件原子迁移，旧路径必须为 0 |
| 是否过早公开 internal model | Passed | 只增 3 个 high-level types |
| 是否复用现有 snapshot/service | Passed | 复用 HLI-1A1 snapshot 和 query service |
| 是否漏掉 FieldTrust 共享依赖 | Passed | 3 文件与 diagnostics 同卡原子迁移 |
| 是否保持 project diagnostics | Passed by contract | ManualFull 留 IDE，目录算法复用 Application |
| 是否保持 Save/Problems/UI | Passed by contract | 仅兼容 adapter，禁止 UI/Save 行为改动 |
| 失败是否可区分 | Passed | large/limit/cancel/analysis 独立 failure |
| 是否泄露 exception | Passed | public safe failure；IDE legacy projection 保持且不外溢 |
| 是否有 partial result 误用 | Passed | 任何 failure 返回空 facts |
| 是否考虑限制/取消 | Passed | 8M/10k + 256 checkpoints |
| 是否覆盖现有消费者 | Passed | 149 基线 + Application + full regression |
| 是否影响持久化/网络 | None | 进程内 CLR Experimental API |
| 是否需要 AgentPilot | No | 机械迁移也应先以主代理执行，避免再次高 Token 循环 |

审查结论：契约边界完整，实现和回归门禁均已通过，未发现会强制 HLI-1B
返工的 API 或所有权问题。

## 16. Stop Rule

当前停在 HLI-1A2 完成点，不自动进入 HLI-1B Headless Edit Preview。
