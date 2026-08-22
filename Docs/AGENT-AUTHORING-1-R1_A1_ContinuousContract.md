# AGENT-AUTHORING-1-R1 A1 连续阶段最终契约

状态：最终、自审通过、用户已授权连续执行  
日期：2026-07-23  
包级风险：R3  
治理模式：Continuous StagePackage / Deferred Governance  
前置阶段：A0 Semantic Characterization 已完成  

## 1. 阶段目标

A1 建立后续单文档 Plan/Preview 所需的稳定只读基础：

```text
FieldRegistryRuntimeService
  -> captured Provider + Revision
  -> neutral Ra2LanguageAnalysisRequest
  -> existing TextModel + SemanticModel + Diagnostics
  -> read-only Ra2IniLanguageAnalysisResult
```

内置 AI、未来 Authoring Workspace 和当前 IDE 诊断必须复用同一现有语言与诊断实现，
不得各自维护解析、字段或诊断算法。

## 2. 子阶段

```text
A1-B1 FieldRegistryProviderSnapshotAndRevision
A1-A1 LanguageRequestAndDiagnosticFact
A1-A2 LanguageFailureAndResult
A1-A3 LanguageFacadeComposition
A1-C  SnapshotConsistencyAndEquivalenceGate
```

完成全部五张任务卡并通过 package-level 验证后，A1 才能标记为 Completed。

## 3. 非目标

- 不增加编辑 SessionId/Revision；属于 A2-A。
- 不生成 Edit Operation、Plan、Preview、PreviewId 或 Apply。
- 不修改 AvalonEdit、Undo/Redo、Completion 或 Add Property。
- 不修改保存、备份、回滚或文件写入。
- 不修改 AI 请求、SSE、Prompt、模型配置或 UI。
- 不统一 Core 与 IDE TextModel 解析差异。
- 不建立外部 IPC/MCP。
- 不改变 Field Registry Provider 优先级、查找、fallback、enrichment、provenance 或 pack 数据。

## 4. A1-B1 Field Registry Snapshot / Revision

### 4.1 新类型

```csharp
namespace RA2IniEditor.IDE.Services;

internal sealed class Ra2FieldRegistryProviderSnapshot
{
    internal Ra2FieldRegistryProviderSnapshot(
        IRa2FieldDefinitionProvider provider,
        long revision);

    public IRa2FieldDefinitionProvider Provider { get; }
    public long Revision { get; }
}
```

不变量：

- Provider 非 null。
- Revision 必须大于 0。
- Snapshot 创建后不可变。
- 不包含 CurrentState、Provenance、目录或可写 Runtime Service 引用。

### 4.2 Runtime Service 变更

保留现有 public API：

```csharp
public IRa2FieldDefinitionProvider CurrentProvider { get; }
public IRa2FieldDefinitionProvider Reload(string? projectRootPath);
```

新增 internal：

```csharp
internal Ra2FieldRegistryProviderSnapshot CaptureProviderSnapshot();
```

生命周期：

1. 构造函数完成 BuiltIn Provider 后发布 Revision 1。
2. Reload 在局部变量中完整构建 Provider、Provenance 和 State。
3. 构建失败时不发布任何新 Snapshot。
4. 成功发布时 Revision 精确递增一次。
5. 旧 Snapshot 永远保留旧 Provider 与旧 Revision。
6. `CurrentProvider` 返回当前已发布 Snapshot 的 Provider。
7. Reload 仍返回本轮成功发布的 Provider。

发布使用一个私有 gate；Loader 工作不在 gate 内执行，只有最终状态发布进入 gate。

## 5. A1-A1 中立请求与诊断事实

### 5.1 `Ra2LanguageAnalysisRequest`

```csharp
namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2LanguageAnalysisRequest
{
    internal Ra2LanguageAnalysisRequest(
        string projectRootPath,
        string filePath,
        string fileName,
        string text,
        int analysisVersion,
        Ra2FieldRegistryProviderSnapshot fieldRegistry);

    public string ProjectRootPath { get; }
    public string FilePath { get; }
    public string FileName { get; }
    public string Text { get; }
    public int AnalysisVersion { get; }
    public Ra2FieldRegistryProviderSnapshot FieldRegistry { get; }
}
```

边界：

- 不携带 `SourceEditorState`。
- `AnalysisVersion` 仅为诊断关联标签，不是编辑并发 Revision。
- FieldRegistry 必须是一次捕获所得 Snapshot；分析期间不再读取 Runtime Service。
- 字符串不得为 null；路径允许为空以兼容未命名/测试输入。
- 不序列化，不作为外部 wire DTO。

### 5.2 `Ra2DiagnosticFact`

```csharp
internal sealed class Ra2DiagnosticFact
{
    internal Ra2DiagnosticFact(
        string code,
        string sourceKind,
        IniIssueSeverity severity,
        string message,
        string filePath,
        int? lineNumber,
        int? columnNumber,
        string? sectionId,
        string? key,
        int analysisVersion);

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

不包含 ViewModel 显示属性；Message 不是稳定诊断身份，也不得未经 A4 上下文选择直接发给模型。

## 6. A1-A2 失败与结果

```csharp
internal enum Ra2LanguageAnalysisFailureKind
{
    None = 0,
    UnexpectedFailure
}
```

```csharp
internal sealed class Ra2IniLanguageAnalysisResult
{
    internal Ra2IniLanguageAnalysisResult(
        Ra2LanguageAnalysisRequest request,
        Ra2LanguageAnalysisFailureKind failureKind,
        string? failureMessage,
        Ra2IniTextDocument? textDocument,
        Ra2DocumentSemanticModel? semanticModel,
        IReadOnlyList<Ra2DiagnosticFact> diagnostics);

    public Ra2LanguageAnalysisRequest Request { get; }
    public bool Succeeded { get; }
    public Ra2LanguageAnalysisFailureKind FailureKind { get; }
    public string? FailureMessage { get; }
    public Ra2IniTextDocument? TextDocument { get; }
    public Ra2DocumentSemanticModel? SemanticModel { get; }
    public IReadOnlyList<Ra2DiagnosticFact> Diagnostics { get; }
    public long FieldRegistryRevision { get; }
}
```

不变量：

- 成功：FailureKind None、FailureMessage null、两个模型非 null。
- 失败：两个模型 null、Diagnostics 为空、FailureMessage 非空。
- Diagnostics 必须防御性复制为只读集合。
- DiagnosticFact 不可变。
- TextDocument/SemanticModel 定义为服务创建后不再修改的只读派生对象，不声称通用深度不可变。
- FailureMessage 使用稳定安全摘要，不复制原始异常文本。
- 不捕获/转换进程级致命异常。

## 7. A1-A3 门面

```csharp
internal interface IRa2IniLanguageAnalysisService
{
    Ra2IniLanguageAnalysisResult Analyze(
        Ra2LanguageAnalysisRequest request);
}
```

```csharp
internal sealed class Ra2IniLanguageAnalysisService
    : IRa2IniLanguageAnalysisService
{
    public Ra2IniLanguageAnalysisService();

    internal Ra2IniLanguageAnalysisService(
        IRa2IniTextDocumentParser textDocumentParser,
        IRa2DocumentSemanticModelBuilder semanticModelBuilder,
        CurrentFileReadonlyDiagnosticService diagnosticService);

    public Ra2IniLanguageAnalysisResult Analyze(
        Ra2LanguageAnalysisRequest request);
}
```

调用顺序：

```text
Validate request
  -> Parse TextModel from request.Text
  -> Build semantic model with request.FieldRegistry.Provider
  -> adapt request to Loaded CurrentSourceSnapshot
  -> call existing CurrentFileReadonlyDiagnosticService
  -> copy ViewModel fields into Ra2DiagnosticFact
  -> return read-only result carrying captured RegistryRevision
```

要求：

- 同一次分析只使用 `request.FieldRegistry.Provider`。
- 不读取 `FieldRegistryRuntimeService.CurrentProvider`。
- 不访问磁盘或 WPF。
- 不缓存 request、provider 或 result。
- 同步执行于调用线程；A1 不承诺任意第三方 Provider 可跨线程。
- 后台分析只允许使用 Runtime Service 发布的稳定 Snapshot。
- 不复制字段、引用或链路诊断算法。

## 8. A1-C 一致性门禁

必须通过以下事实：

1. Result RegistryRevision 等于 Request 捕获的 Revision。
2. Runtime Service 后续 Reload 不改变旧 Request/Result 的 Provider 和 Revision。
3. 同一 Request 的 DiagnosticFact 与现有诊断服务逐项、逐顺序等价。
4. SemanticModel 使用捕获 Provider，而不是 Reload 后 Provider。
5. A0 八项 Core/TextModel 特征测试继续通过。
6. internal 契约源文件不引用 WPF、AvalonEdit、Shell、Writer 或 FieldRegistryRuntimeService。

## 9. 任务卡与文件预算

### Card 1 — A1-B1

```text
RA2IniEditor.IDE/Services/Ra2FieldRegistryProviderSnapshot.cs
RA2IniEditor.IDE/Services/FieldRegistryRuntimeService.cs
RA2IniEditor.Tests/IDE/FieldRegistryRuntimeServiceTests.cs
```

### Card 2 — A1-A1

```text
RA2IniEditor.IDE/Language/Ra2LanguageAnalysisRequest.cs
RA2IniEditor.IDE/Language/Ra2DiagnosticFact.cs
RA2IniEditor.Tests/IDE/Ra2LanguageAnalysisContractTests.cs
```

### Card 3 — A1-A2

```text
RA2IniEditor.IDE/Language/Ra2LanguageAnalysisFailureKind.cs
RA2IniEditor.IDE/Language/Ra2IniLanguageAnalysisResult.cs
RA2IniEditor.Tests/IDE/Ra2LanguageAnalysisContractTests.cs
```

### Card 4 — A1-A3

```text
RA2IniEditor.IDE/Language/IRa2IniLanguageAnalysisService.cs
RA2IniEditor.IDE/Language/Ra2IniLanguageAnalysisService.cs
RA2IniEditor.Tests/IDE/Ra2IniLanguageAnalysisServiceTests.cs
```

### Card 5 — A1-C

```text
RA2IniEditor.Tests/IDE/Ra2IniLanguageAnalysisServiceTests.cs
RA2IniEditor.Tests/IDE/Ra2IniLanguageAnalysisBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2IniParserConsistencyCharacterizationTests.cs
```

每张卡最多 5 个文件、最多 2 个新增生产类型；不移动文件，不新增目录结构或依赖。

## 10. 全阶段禁止修改

```text
RA2IniEditor.Core/**
RA2IniEditor.Infrastructure/**
CurrentFileReadonlyDiagnosticService.cs
Ra2FieldDiagnosticService.cs
Ra2ReferenceDiagnosticService.cs
Ra2ChainDiagnosticService.cs
ManualFullDiagnosticsService.cs
Ra2SavePreflightDiagnosticService.cs
ShellWindow.xaml
ShellWindow.xaml.cs
Views/**
Themes/**
AI/**
Editing/**
*.csproj
BuiltIn field packs
legacy files/projects
```

## 11. 测试范围

每卡先运行对应新测试。包末定向测试至少覆盖：

```text
FieldRegistryRuntimeServiceTests
Ra2LanguageAnalysisContractTests
Ra2IniLanguageAnalysisServiceTests
Ra2IniLanguageAnalysisBoundaryTests
Ra2IniParserConsistencyCharacterizationTests
CurrentFileReadonlyDiagnosticServiceTests
Ra2DocumentSemanticModelBuilderTests
Ra2IniTextDocumentParserTests
```

包末必须运行：

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 12. Public/Contract Ledger

无 public API 变更。以下 internal 契约均为 Experimental：

| API | 下一阶段用途 |
|---|---|
| `Ra2FieldRegistryProviderSnapshot` | A2 Preview 捕获字段知识版本 |
| `Ra2LanguageAnalysisRequest` | A2 当前/候选文档分析输入 |
| `Ra2DiagnosticFact` | A2 诊断差异输入 |
| `Ra2LanguageAnalysisFailureKind` | A2/A4 显式失败 |
| `Ra2IniLanguageAnalysisResult` | A2 Planner 只读结果 |
| `IRa2IniLanguageAnalysisService` | A2/A4 共用分析入口 |

这些类型不得直接成为 A5 wire DTO。

## 13. 受控技术债

```text
ID: AGENT-AUTHORING-A1-TD-001
位置: Ra2IniLanguageAnalysisService / CurrentFileReadonlyDiagnosticService
债务: 为复用现有诊断编排，同一请求可能构建两次 SemanticModel。
接受原因: 避免在 A1 重写或拆分现有诊断体系。
影响: 额外 CPU/分配，不改变结果或接口。
偿还触发器: A2 Preview 基准证明该重复构建成为主要延迟，或诊断层获得非 ViewModel 领域入口。
后续: 单独 PreparedDiagnosticsContract，不得在 A1 内复制算法。
```

## 14. 架构决策

```text
Decision: Provider + Revision 由 FieldRegistryRuntimeService 原子发布
Status: Accepted
Reason: 后台分析必须绑定稳定字段知识版本。
Rejected: 门面每次读取 CurrentProvider；会产生 Reload 竞态。
Rejected: 暴露整个 Runtime Service；会泄漏可变所有权。
```

```text
Decision: 使用中立 Ra2LanguageAnalysisRequest
Status: Accepted
Reason: 避免 CurrentSourceSnapshot/SourceEditorState 成为未来 Authoring Workspace 的长期输入。
Rejected: 直接复用 CurrentSourceSnapshot；会把加载状态和加载 Version 带入 A2。
```

## 15. 自审结论

- Architecture：通过。Provider 所有权、分析所有权和表现层边界清晰。
- Reuse：通过。复用现有 Parser、Semantic Builder 和完整诊断服务。
- Data model：通过。Request 捕获 Provider Snapshot，Result 不读取可变 Runtime Service。
- Public API：通过。现有 public 签名不变；新增类型均 internal。
- Lifecycle：通过。Revision 在成功发布时递增，旧 Snapshot 稳定。
- Failure：通过。失败显式且不泄漏原始异常。
- Testability：通过。依赖构造函数与 InternalsVisibleTo 已存在。
- Evolvability：通过。A2 可直接消费 Request/Result，不需要伪造 Loaded CurrentSourceSnapshot。
- R4：未触发。无持久化、网络、保存、回滚或 wire contract。

## 16. 停止条件

连续执行仅在以下条件全部满足时继续：

- 当前任务卡定向测试通过；
- 实际 diff 未越过允许文件；
- 无 public API 或依赖变化；
- Provider 优先级和现有诊断结果保持；
- 未触发 R4。

任一条件失败时停止、记录部分状态并刷新治理队列。

## 17. 下一阶段

A1 全包通过后的下一安全入口：

```text
AGENT-AUTHORING-1-R1 A2-A
EditableSessionIdentityAndRevisionContract
```

## 18. 执行结果 — 2026-07-23

状态：Completed

五张任务卡均按本契约完成。实现结果与证据见：

```text
Docs/AGENT-AUTHORING-1-R1_A1_StageLedger.md
Docs/ContextCapsule_AGENT_AUTHORING_1_A1.md
```

验证基线：

```text
A1/A0/diagnostics 合并定向：45/45 passed
IDE-only solution build：0 warnings / 0 errors
完整非 UI 测试：2355/2355 passed
IdeOnly clean source package：passed，989 files
```

契约偏差：无功能偏差。失败枚举使用独立源文件，门面、契约和边界测试按职责分文件。
未改变 public API、Provider 优先级、解析/诊断/补全/保存语义、Shell、UI 或 legacy。
