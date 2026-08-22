# AGENT-AUTHORING-1-R1 A1-A 只读语言分析门面契约

状态：最终候选，等待用户确认后实施  
日期：2026-07-23  
风险等级：R2（internal 事实模型、结果类型和接口）  
治理模式：ContractOnly / StopForApproval  
前置阶段：`AGENT-AUTHORING-1-R1 A0 Semantic Characterization` 已完成  

## 1. 功能目标

建立一条 internal、只读、与 WPF 表现类型隔离的当前文档语言分析入口，组合：

- IDE Span 感知 `IRa2IniTextDocumentParser`；
- `IRa2DocumentSemanticModelBuilder`；
- 现有 `CurrentFileReadonlyDiagnosticService`；
- 调用时捕获的 `IRa2FieldDefinitionProvider`。

该门面供后续 A2 Preview Planner 和 A4 内置 AI 适配器读取同一份分析结果。它不修改编辑器、字段库或文件。

## 2. 非目标

A1-A 不实施：

- `SessionId`、编辑 `Revision` 或内容摘要；
- Field Registry Revision 或 Provider 发布时序；
- Edit Operation、Plan、Preview、PreviewId 或 Apply；
- AvalonEdit 事务与 Undo/Redo；
- 保存、备份、回滚或自动保存；
- AI JSON、tool call、流式协议或 UI；
- 项目级多文件引用目录；
- Core 与 IDE TextModel 的语义统一；
- 通用 Parser 一致性检测器；
- 外部 IPC/MCP 或 public API。

## 3. 当前代码事实

### 3.1 可复用入口

```csharp
internal interface IRa2IniTextDocumentParser
{
    Ra2IniTextDocument Parse(string text);
}

internal interface IRa2DocumentSemanticModelBuilder
{
    Ra2DocumentSemanticModel Build(
        Ra2DocumentSnapshot snapshot,
        IRa2FieldDefinitionProvider fieldProvider);
}

public sealed class CurrentFileReadonlyDiagnosticService
{
    public IReadOnlyList<IdeDiagnosticIssueViewModel> Analyze(
        CurrentSourceSnapshot? snapshot,
        IRa2FieldDefinitionProvider? fieldProvider = null);
}
```

`CurrentFileReadonlyDiagnosticService` 是当前唯一完整组合 Core Parse/Validate、字段、
引用与链路诊断的入口。A1-A 必须复用它，不得复制五套诊断算法。

### 3.2 当前耦合

现有诊断入口返回 `IdeDiagnosticIssueViewModel`。A1-A 只能在实现类边界将其复制为
不可变事实；新接口、结果和事实类型不得引用：

```text
RA2IniEditor.IDE.ViewModels
System.Windows
ICSharpCode.AvalonEdit
ShellWindow
FieldRegistryRuntimeService
```

### 3.3 A0 约束

Core 与 IDE TextModel 的既有差异已由
`Ra2IniParserConsistencyCharacterizationTests` 锁定。A1-A 不新增第三条解析路径，也不
通过规范化隐藏差异。

## 4. 精确 internal 契约

以下类型均为 `internal`，稳定性为 `Experimental`。

### 4.1 `Ra2LanguageAnalysisFailureKind`

```csharp
internal enum Ra2LanguageAnalysisFailureKind
{
    None = 0,
    SnapshotNotAnalyzable,
    UnexpectedFailure
}
```

说明：

- `SnapshotNotAnalyzable`：`CurrentSourceSnapshot.CanRunDiagnostics == false`。
- `UnexpectedFailure`：TextModel Parse、SemanticModel Build 或结果投影发生未预期异常。
- `null` snapshot/provider 属于程序错误，使用 `ArgumentNullException`，不转为运行时失败结果。
- 诊断服务自身捕获的异常继续以 `DIAGNOSTIC_EXCEPTION` 事实存在；如果 TextModel 和
  SemanticModel 构建成功，整体分析仍为成功。

### 4.2 `Ra2DiagnosticFact`

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

边界：

- 只包含诊断事实，不包含 `SeverityText`、图标、Marker、LocationText 或本地化后的
  SourceText。
- `Message` 是显示内容，不是稳定诊断身份。
- `AnalysisVersion` 只用于关联输入分析快照，不是 A2 的编辑并发 Revision。
- A2 需要的诊断 fingerprint/delta 属于后续契约，不在本类型提前实现。

### 4.3 `Ra2IniLanguageAnalysisResult`

```csharp
internal sealed class Ra2IniLanguageAnalysisResult
{
    internal Ra2IniLanguageAnalysisResult(
        CurrentSourceSnapshot sourceSnapshot,
        Ra2LanguageAnalysisFailureKind failureKind,
        string? failureMessage,
        Ra2IniTextDocument? textDocument,
        Ra2DocumentSemanticModel? semanticModel,
        IReadOnlyList<Ra2DiagnosticFact> diagnostics);

    public CurrentSourceSnapshot SourceSnapshot { get; }
    public bool Succeeded { get; }
    public Ra2LanguageAnalysisFailureKind FailureKind { get; }
    public string? FailureMessage { get; }
    public Ra2IniTextDocument? TextDocument { get; }
    public Ra2DocumentSemanticModel? SemanticModel { get; }
    public IReadOnlyList<Ra2DiagnosticFact> Diagnostics { get; }
}
```

构造不变量：

- `SourceSnapshot` 永不为 null。
- 成功：`FailureKind=None`、`FailureMessage=null`、两个模型非 null。
- 失败：两个模型为 null、Diagnostics 为空、FailureMessage 非空。
- Diagnostics 在构造时复制为只读集合，调用者不能通过原数组改变结果。
- 不返回可变 `IniDocument`。

### 4.4 `IRa2IniLanguageAnalysisService`

```csharp
internal interface IRa2IniLanguageAnalysisService
{
    Ra2IniLanguageAnalysisResult Analyze(
        CurrentSourceSnapshot snapshot,
        IRa2FieldDefinitionProvider fieldProvider);
}
```

### 4.5 `Ra2IniLanguageAnalysisService`

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
        CurrentSourceSnapshot snapshot,
        IRa2FieldDefinitionProvider fieldProvider);
}
```

默认构造函数只组合现有实现。internal 构造函数用于测试真实边界，不允许增加服务定位器或
静态全局状态。

## 5. 调用顺序

```text
Validate arguments
  -> check snapshot.CanRunDiagnostics
  -> Ra2IniTextDocumentParser.Parse(snapshot.Text)
  -> Ra2DocumentSemanticModelBuilder.Build(
         new Ra2DocumentSnapshot(snapshot.FilePath, snapshot.Text, snapshot.Version),
         captured fieldProvider)
  -> CurrentFileReadonlyDiagnosticService.Analyze(snapshot, captured fieldProvider)
  -> copy IdeDiagnosticIssueViewModel fields to Ra2DiagnosticFact
  -> construct immutable read result
```

不得在调用过程中：

- 从 `FieldRegistryRuntimeService.CurrentProvider` 再次取 Provider；
- 访问磁盘验证 FilePath；
- 读取或写入 WPF 控件；
- 缓存 snapshot、provider 或 result；
- 修改输入文本；
- 调用保存链路。

## 6. 数据所有权与生命周期

| 数据 | 主要所有者 | 生命周期 | 可变性/序列化 |
|---|---|---|---|
| `CurrentSourceSnapshot` | 当前源文档/后续 Authoring Adapter | 单次分析输入 | 现有只读对象；不新增序列化 |
| `IRa2FieldDefinitionProvider` | 调用者捕获 | 单次 Analyze 调用 | 门面不缓存、不替换 |
| TextDocument | 分析结果 | 与 Result 相同 | 只读派生模型，不序列化 |
| SemanticModel | 分析结果 | 与 Result 相同 | 只读派生模型，不序列化 |
| DiagnosticFact | 分析结果 | 与 Result 相同 | 不可变复制，不序列化 |

拒绝的所有者：

- ViewModel：不能拥有可供 Planner 使用的诊断事实。
- Shell：不能成为语言分析编排器。
- Field Registry Runtime Service：不能被门面持有或暴露。
- 外部 Agent：不能持有任何当前 internal 模型。

## 7. 实施任务卡

每张卡遵守最多 5 个修改文件、最多 2 个新增类型的默认预算。

### A1-A1：Diagnostic Fact Contract

允许：

```text
RA2IniEditor.IDE/Language/Ra2DiagnosticFact.cs
RA2IniEditor.IDE/Language/Ra2LanguageAnalysisFailureKind.cs
RA2IniEditor.Tests/IDE/Ra2DiagnosticFactTests.cs
```

验收：

- 字段逐项保存，无表现层计算属性。
- 类型均为 internal。
- 事实类型不引用 ViewModels/WPF/AvalonEdit。

### A1-A2：Result And Interface Contract

允许：

```text
RA2IniEditor.IDE/Language/Ra2IniLanguageAnalysisResult.cs
RA2IniEditor.IDE/Language/IRa2IniLanguageAnalysisService.cs
RA2IniEditor.Tests/IDE/Ra2IniLanguageAnalysisResultTests.cs
```

验收：

- 成功/失败不变量完整。
- Diagnostics 为防御性只读副本。
- 无 public API。

### A1-A3：Facade Composition And Equivalence

允许：

```text
RA2IniEditor.IDE/Language/Ra2IniLanguageAnalysisService.cs
RA2IniEditor.Tests/IDE/Ra2IniLanguageAnalysisServiceTests.cs
RA2IniEditor.Tests/IDE/Ra2IniParserConsistencyCharacterizationTests.cs
```

验收：

- 诊断事实与现有 `CurrentFileReadonlyDiagnosticService.Analyze` 逐字段等价且顺序一致。
- SemanticModel 使用传入的同一个 Provider。
- 非 Loaded 状态返回 `SnapshotNotAnalyzable`。
- 依赖抛出时返回 `UnexpectedFailure`，没有半成品模型。
- A0 八个特征测试继续通过。

### Governance Flush

三张任务卡全部通过后更新：

```text
Docs/AGENT-AUTHORING-1-R1_A1A_ReadonlyLanguageAnalysisFacadeContract.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

## 8. 全阶段禁止修改

```text
RA2IniEditor.Core/**
RA2IniEditor.Infrastructure/**
CurrentFileReadonlyDiagnosticService.cs
Ra2FieldDiagnosticService.cs
Ra2ReferenceDiagnosticService.cs
Ra2ChainDiagnosticService.cs
ManualFullDiagnosticsService.cs
Ra2SavePreflightDiagnosticService.cs
FieldRegistryRuntimeService.cs
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

如果实现要求修改任一禁止文件，A1-A 必须停止并重新审查，不得扩大范围。

## 9. AutomationId 与 UI

A1-A 不修改 UI：

- 无新增 AutomationId。
- 无需要保留之外的新 UI 元素。
- 不运行电脑操控或截图烟测。

## 10. 验证矩阵

### 定向测试

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~Ra2DiagnosticFactTests|FullyQualifiedName~Ra2IniLanguageAnalysisResultTests|FullyQualifiedName~Ra2IniLanguageAnalysisServiceTests|FullyQualifiedName~Ra2IniParserConsistencyCharacterizationTests|FullyQualifiedName~CurrentFileReadonlyDiagnosticServiceTests|FullyQualifiedName~Ra2DocumentSemanticModelBuilderTests|FullyQualifiedName~Ra2IniTextDocumentParserTests"
```

### 构建

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
```

### 全量测试触发条件

以下任一发生时必须运行完整非 UI 测试：

- 实际 diff 修改现有诊断/语义/TextModel 生产文件；
- 诊断等价测试失败；
- public API 意外变化；
- 定向测试出现跨模块失败。

正常按本契约实施时，全量测试为可选的 package-level 置信检查，不替代定向等价测试。

## 11. Public API / Contract Ledger

无 public API 变更。以下 internal 契约均标记为 `Experimental`：

| API | 种类 | 下一阶段用途 | 稳定性 | 兼容风险 |
|---|---|---|---|---|
| `Ra2DiagnosticFact` | Fact | A2 预览诊断比较输入 | Experimental | 不可作为外部 wire DTO |
| `Ra2LanguageAnalysisFailureKind` | FailureKind | A2/A4 显式失败处理 | Experimental | 可在 A2 前增补 internal 枚举值 |
| `Ra2IniLanguageAnalysisResult` | Result | A2 Planner 只读输入 | Experimental | 不序列化 |
| `IRa2IniLanguageAnalysisService` | Interface | A2/A4 共用分析入口 | Experimental | internal only |

仓库当前没有 PublicApiLedger 文档；本阶段先在本契约维护 internal pending ledger，不创建新的
项目级 public ledger。

## 12. 已知限制与偿还触发器

现有诊断服务内部会自行 Parse/Build SemanticModel，而门面还需要生成供调用者读取的
TextDocument/SemanticModel，因此 A1-A 可能重复一次语义构建。

该限制不会改变新接口，也不会复制诊断算法。满足以下任一条件时，才单独设计 prepared
diagnostics 优化契约：

- A2 Preview 性能测试证明重复分析成为主要延迟；
- 受支持文档尺寸下分析超过后续明确预算；
- 诊断服务获得正式的非 ViewModel 领域结果入口。

A1-A 不为消除此成本修改当前诊断体系。

## 13. 反向审查

### 通过项

- 没有暴露 WPF、ViewModel、AvalonEdit 或可变 Field Registry 服务。
- 没有复制字段、引用和链路诊断算法。
- 没有把加载 Snapshot Version 误定义成编辑并发令牌。
- 没有暴露 Core 可变 `IniDocument`。
- 没有提前设计 Plan、Preview、Apply 或 wire DTO。
- 下一阶段可以在不删除本接口的前提下加入 Authoring Snapshot 适配。

### 拒绝的替代方案

- 直接返回 `IdeDiagnosticIssueViewModel`：表现层泄漏。
- 重写所有诊断为新 Fact：范围和回归风险过大。
- 让门面读取 `FieldRegistryRuntimeService.CurrentProvider`：破坏 Provider 快照边界。
- 返回 Core `IniDocument`：可变对象泄漏且 A2 当前不需要。
- 在 A1-A 实现 Parser 差异比较器：A0 已锁定事实，当前无运行时消费者。
- 现在修改 `CurrentFileReadonlyDiagnosticService`：不满足最小风险范围。

## 14. 停点

本契约确认前不得创建上述 C# 类型。

实施过程中发生以下任一情况必须停止：

- 需要修改禁止文件；
- 需要新增 public API；
- 需要缓存 Field Provider 或访问 Runtime Service；
- 等价测试只能通过改变现有诊断结果；
- 需要引入第三方依赖；
- 实际风险升至 R3/R4。

用户确认本最终契约后，下一入口为：

```text
AGENT-AUTHORING-1-R1 A1-A1 DiagnosticFactContract
```
