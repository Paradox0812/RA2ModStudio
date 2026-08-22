# AUTOMATION-HLI-1A1 Document Query Slice Final Contract

状态：Final / Self-reviewed / Awaiting implementation confirmation  
日期：2026-08-22  
父契约：`Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md`  
前置证据：`Docs/AUTOMATION-HLI-1A0_DependencyConeCharacterizationContract.md`  
实施风险：R3（程序集与跨层边界）+ R2（Experimental public API）

## 1. 目标与完成定义

HLI-1A1 的实施目标是新增真正可由 `net8.0` 调用方引用的 Document Query 纵向切片，
同时让现有 IDE 继续消费同一份 Section/Reference 语义算法。

本阶段只交付：

```text
ini.document.section.get
ini.document.references.find
```

完成必须同时满足：

1. `RA2IniEditor.Application` 为 `net8.0`，只引用 Core；
2. 22 个既有 Query foundation 文件从 IDE 移入 Application，IDE 不保留副本；
3. raw SemanticModel/classifier/symbol/caret/reference implementation 保持 internal；
4. 新增最小 `Automation.Experimental` 高层 public contract；
5. 新 `RA2IniEditor.Application.Tests` 在 `net8.0` 下独立通过；
6. 现有 IDE 调用方、测试和可观察行为保持等价；
7. Diagnostics、Preview、Apply、Save 和 Gateway 不进入本阶段。

## 2. 当前契约阶段裁决

本最终契约已经完成代码事实回归和自审，可以作为下一次生产实施的唯一 Task Card。
本轮只生成契约，不修改 `.cs/.csproj/.sln`。开始生产迁移仍需用户明确确认本契约。

### 风险门禁

- R3 已显式识别：新增程序集、改变 22 个 internal 类型的程序集/namespace 所有权；
- R2 已显式识别：新增 15 个 Experimental exported types；
- 不涉及 R4：没有 wire protocol、持久化、Save/rollback、Registry ownership 或授权边界变化；
- 如果实施中需要改变 parser/diagnostics/preview/save 语义，立即停止并修订契约。

## 3. 允许与禁止范围

### 3.1 允许

```text
RA2IniEditor.Application/**
RA2IniEditor.Application.Tests/**
RA2IniEditor.IDE/Classification/（仅删除/移动已列出的 4 个文件）
RA2IniEditor.IDE/Language/（仅删除/移动已列出的 18 个文件）
RA2IniEditor.IDE/RA2IniEditor.IDE.csproj
RA2IniEditor.Tests/RA2IniEditor.Tests.csproj
RA2IniEditor.Tests/IDE/Ra2AutomationDependencyConeCharacterizationTests.cs
RA2IniEditor.IDE/Highlighting/ReadonlyIniHighlightTokenizer.cs（仅 using）
RA2IniEditor.IDE/Services/ReadonlyProjectExplorerGroupingService.cs（仅 using）
RA2IniEditor.Tests/IDE/Ra2SectionClassifierTests.cs（仅 using）
RA2IniEditor.IDE.sln
HLI/API/Decision/CurrentStatus/Roadmap 文档
```

### 3.2 禁止

```text
ShellWindow.xaml / ShellWindow.xaml.cs
任意 XAML、Dock、AutomationId 或 UI 行为
TextModel 6-file full parser
Diagnostics implementation / ViewModels
Editing Preview / A3 Apply / Undo / Save
Field Registry runtime、priority、data pack 或 reload 语义
Completion / Hover / Quick Peek
AI / Search
Infrastructure project
Core public API 或 parser semantics
legacy solution/editor
JSON / IPC / MCP / CLI / Gateway
```

允许列表中的现有生产文件除 namespace/using/project wiring 外不得进行顺手重构。

## 4. 目标项目图

```text
RA2IniEditor.Core (net8.0)
          ↑
RA2IniEditor.Application (net8.0)
          ↑
RA2IniEditor.IDE (net8.0-windows, WPF)

RA2IniEditor.Application.Tests (net8.0)
        -> Application + Core

RA2IniEditor.Tests (net8.0-windows)
        -> IDE + Application + Infrastructure + Core
```

强制规则：

- Application `TargetFramework=net8.0`、`Nullable=enable`、`ImplicitUsings=enable`；
- Application 不设置 `UseWPF`、不引用 Infrastructure/IDE/AvalonEdit/AvalonDock；
- Core/Infrastructure 不反向引用 Application；
- Application.Tests 使用与现有测试工程相同版本的 xUnit/Test SDK 包；
- solution 同时包含 Application 和 Application.Tests，并具备 Debug/Release 配置。

## 5. 22 文件精确迁移清单

### 5.1 `Application/Classification`（4）

```text
IRa2SectionClassifier.cs
Ra2SectionClassificationResult.cs
Ra2SectionClassificationWarning.cs
Ra2SectionClassifier.cs
```

namespace 从：

```text
RA2IniEditor.IDE.Classification
```

改为：

```text
RA2IniEditor.Application.Classification
```

### 5.2 `Application/Language`（18）

```text
IRa2CaretContextService.cs
IRa2DocumentSemanticModelBuilder.cs
IRa2ReferenceFinder.cs
Ra2CaretContext.cs
Ra2CaretContextService.cs
Ra2CaretRegion.cs
Ra2DocumentSemanticModel.cs
Ra2DocumentSemanticModelBuilder.cs
Ra2DocumentSnapshot.cs
Ra2IniLineParser.cs
Ra2KeyValueSymbol.cs
Ra2ReferenceFinder.cs
Ra2ReferenceItem.cs
Ra2ReferenceResult.cs
Ra2SectionSymbol.cs
Ra2TextSpan.cs
Ra2ValueReferenceKind.cs
Ra2ValueReferenceSymbol.cs
```

namespace 从：

```text
RA2IniEditor.IDE.Language
```

改为：

```text
RA2IniEditor.Application.Language
```

### 5.3 搬迁不变量

- 22 个类型全部保持 internal；
- 除 namespace/using 外不改方法签名、算法、顺序或异常语义；
- 不合并 `Ra2SectionClassifier` 与 `Ra2DocumentSemanticModelBuilder` 的双阶段 parse；
- 不修改 inline comment、Section suffix、newline、classification 或 reference 规则；
- IDE 原目录中不保留 link/compile include/source copy/兼容副本。

## 6. IDE/Test 兼容策略

Application 项目只向以下程序集开放 internal：

```text
RA2IniEditor.IDE
RA2IniEditor.Tests
RA2IniEditor.Application.Tests
```

不使用 wildcard、动态 friend assembly 或 public 化 raw types。

IDE 和现有 Tests 的项目级 global using 分别加入：

```text
RA2IniEditor.Application.Language
RA2IniEditor.Application.Classification
```

Classification 原 namespace 在搬迁后为空，因此以下 3 个残留显式 using 必须精确改为
Application namespace：

```text
RA2IniEditor.IDE/Highlighting/ReadonlyIniHighlightTokenizer.cs
RA2IniEditor.IDE/Services/ReadonlyProjectExplorerGroupingService.cs
RA2IniEditor.Tests/IDE/Ra2SectionClassifierTests.cs
```

不得通过空 namespace marker、type forwarding、shim 或重复 wrapper 掩盖编译问题。

## 7. Public namespace 与稳定性

全部新 API 放在：

```csharp
RA2IniEditor.Application.Automation.Experimental
```

稳定性统一为 `Experimental`。它们是 solution-level 进程内 CLR API，不是 JSON、
IPC、MCP、CLI 或稳定第三方 SDK。HLI-2A Gateway 只能适配这些 typed contracts，
不能反向公开 internal SemanticModel。

## 8. 公共快照与 Span

### 8.1 `Ra2AutomationTextSpan`

Public readonly value type：

```csharp
int Start
int Length
int End
```

规则：

- 使用 UTF-16 `string` 的零基字符偏移；区间为 `[Start, End)`；
- `Start`、`Length` 非负，`Start + Length` 不得溢出；
- 是否落在具体文档内由 service 对 snapshot 校验。

### 8.2 `Ra2AutomationFieldRegistrySnapshot`

Public sealed immutable envelope：

```csharp
IRa2FieldDefinitionProvider Provider
long Revision
```

- Provider 非空，Revision 必须大于 0；
- Provider 只通过 Core readonly interface 使用；
- Host 必须传入当前一次捕获的稳定 provider generation；Application 不回读 runtime singleton；
- 该 envelope 不可序列化，不授予 Registry reload/apply 权限。

### 8.3 `Ra2AutomationDocumentSnapshot`

Public sealed immutable envelope：

```csharp
Guid DocumentId
int Version
string FilePath
string Text
bool IsEditable
Ra2AutomationFieldRegistrySnapshot FieldRegistry
```

- DocumentId 非空，Version 非负，FilePath 非空白，Text 非 null；
- FilePath 只作为身份/显示事实，不授予 I/O；
- Text 是本次调用唯一文档事实源，service 不读取磁盘或 editor；
- `IsEditable` 在本阶段不影响两个只读 Query，保留给 HLI-1B 共用 snapshot；
- 构造器结构错误抛 `ArgumentException`；运行时上限/取消/分析失败返回 typed result。

## 9. Service 精确接口

```csharp
public interface IRa2AutomationDocumentQueryService
{
    Ra2AutomationSectionQueryResult GetSection(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationSectionQuery request,
        CancellationToken cancellationToken = default);

    Ra2AutomationReferenceQueryResult FindReferences(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceQuery request,
        CancellationToken cancellationToken = default);
}

public sealed class Ra2AutomationDocumentQueryService
    : IRa2AutomationDocumentQueryService
{
    public const int MaximumDocumentCharacters = 8 * 1024 * 1024;
    public const int MaximumResultItems = 10_000;

    public Ra2AutomationSectionQueryResult GetSection(...);
    public Ra2AutomationReferenceQueryResult FindReferences(...);
}
```

实现必须无状态、线程安全、无需 Dispose。每次调用从 snapshot 构建 invocation-local
SemanticModel，不缓存 snapshot、Text、Provider 或 result。

## 10. Section Query 契约

### 10.1 Request

`Ra2AutomationSectionQuery`：

```csharp
string SectionName
int? Occurrence
```

- SectionName trim 后非空；匹配使用 `OrdinalIgnoreCase`；
- Occurrence 为 null：要求 SectionName 在文档中唯一；
- Occurrence 为非负整数：选择源码顺序中的零基 occurrence；
- 负 occurrence 属于构造错误并抛 `ArgumentOutOfRangeException`。

该 nullable 设计解决 HLI-0B 中“必须支持 occurrence”与“必须保留
AmbiguousSection”之间的冲突。

### 10.2 Result/Failure

`Ra2AutomationSectionQueryFailureKind`：

```text
None = 0
DocumentTooLarge = 1
NotFound = 2
AmbiguousSection = 3
ResultLimitExceeded = 4
Canceled = 5
AnalysisFailed = 6
```

`Ra2AutomationSectionQueryResult` 始终包含：

```text
Succeeded
FailureKind
Message
DocumentId / Version / FilePath / FieldRegistryRevision
Ra2AutomationSectionFact? Section
```

- 成功：FailureKind=None、Section 非空；
- 成功和失败的 Message 都必须是安全非空文本；
- 失败：Section 必须为 null；
- 不将 raw exception、provider body 或额外本机路径拼入 Message。

### 10.3 Section/Field facts

`Ra2AutomationSectionFact`：

```text
Name
Ra2SectionKind Kind
int Occurrence
int HeaderLineNumber                 // one-based
HeaderSpan / BodySpan / FullSpan
IReadOnlyList<Ra2AutomationFieldFact> Fields
```

`Ra2AutomationFieldFact`：

```text
Key
EffectiveValue
int LineNumber                       // one-based
LineSpan / KeySpan / nullable ValueSpan
```

语义：

- Sections 和 Fields 均保持源码顺序，不排序、不合并；
- 重复 key 保留为多个 field facts；
- 重复 Section 的字段必须按所选 Section 的 `BodySpan` 包含关系筛选，禁止只按名称筛选；
- Header/Body 直接映射现有 symbol；FullSpan 从 HeaderSpan.Start 到 BodySpan.End；
- 若所选 Section 字段数超过 10,000，返回 ResultLimitExceeded，不截断、不返回部分事实。

## 11. Reference Query 契约

### 11.1 Request

`Ra2AutomationReferenceQuery`：

```csharp
int SourceOffset
Ra2AutomationTextSpan? SelectionSpan
```

- SourceOffset 非负；service 校验 `SourceOffset <= Text.Length`；
- SelectionSpan 可空；非空时 Length 必须大于 0 且 End 不超过 Text.Length；
- SelectionSpan 存在时先按现有 finder 解析 selection，失败后按 SourceOffset caret context 回退；
- 不接受 WPF Caret、Selection、TextBox 或 navigation command。

### 11.2 Result/Failure

`Ra2AutomationReferenceQueryFailureKind`：

```text
None = 0
DocumentTooLarge = 1
InvalidLocation = 2
TargetNotResolved = 3
ResultLimitExceeded = 4
Canceled = 5
AnalysisFailed = 6
```

`Ra2AutomationReferenceQueryResult` 始终包含：

```text
Succeeded
FailureKind
Message
DocumentId / Version / FilePath / FieldRegistryRevision
Ra2AutomationReferenceTargetFact? Target
IReadOnlyList<Ra2AutomationReferenceFact> References
bool HasReferences
```

成功和失败的 Message 都必须是安全非空文本；失败时 Target 必须为 null 且
References 必须为空。

`Ra2AutomationReferenceTargetFact`：

```text
Name
Ra2SectionKind Kind
```

`Ra2AutomationReferenceFact`：

```text
SourceSectionName
SourceKey
int LineNumber                       // one-based
LineSpan
ValueSpan
```

语义：

- 目标已解析但引用数为 0：Succeeded=true、FailureKind=None、Target 非空、
  References 为空、HasReferences=false；
- offset/selection 无法解析目标：TargetNotResolved、Target=null；
- 目标名称不要求在当前文档存在定义；只要现有 finder 能从 header/value 解析目标即成功；
- 匹配保持 `OrdinalIgnoreCase`，结果保持现有 `model.References` 源码顺序；
- 只查当前 snapshot，不读项目文件、不调用 Project Search；
- 超过 10,000 条返回 ResultLimitExceeded，不截断、不返回部分事实。

## 12. Limits、取消与异常

### 12.1 Limits

- `Text.Length <= 8,388,608` UTF-16 chars；超过返回 DocumentTooLarge；
- 该限制独立于磁盘 adapter 的 8 MiB byte 限制，二者不得混用；
- 每个成功 payload 最多 10,000 个 field/reference facts；
- 不静默截断，不返回“成功但不完整”。

### 12.2 Cancellation checkpoints

Service 至少在以下位置检查 token：

1. 参数和 limits 校验后、构建 SemanticModel 前；
2. SemanticModel 构建后；
3. 投影结果时每 256 项；
4. 创建成功 result 前。

HLI-1A1 不修改 internal builder 签名来插入 parser 内部取消点。构建期间发生取消时，
构建结束后的检查必须返回 Canceled 且无 payload。后续若真实性能证据要求 parser 内
cooperative cancellation，必须独立契约，不得在搬迁阶段顺手改算法。

### 12.3 Exception policy

- null/结构无效的 snapshot/request 采用标准参数异常；
- `OperationCanceledException` 且 token 已取消时映射为 Canceled；
- 其他非 fatal 异常映射为 AnalysisFailed 和安全消息；
- OOM、AccessViolation、AppDomainUnloaded、BadImageFormat 等 fatal 异常不吞掉；
- 所有失败 result 不携带 Section/Target/References/Fields 等部分 payload。

## 13. 唯一权威实现路径

```text
Host-captured Ra2AutomationDocumentSnapshot
    -> Ra2DocumentSnapshot (internal, same Text reference)
    -> Ra2DocumentSemanticModelBuilder
       -> existing Ra2SectionClassifier
       -> existing Ra2IniLineParser / symbol/reference construction
    -> Section span projection OR existing CaretContext + ReferenceFinder
    -> immutable Experimental result DTO
```

禁止：

- 在 public service 中再次手写 Section parser/reference detector；
- 调用 Core `IniDocument.FindSection` 形成第二套 span/reference 语义；
- 为减少 IVT 而 public 化 raw model；
- 读取 FieldRegistryRuntimeService、磁盘、editor、Dispatcher 或 Environment；
- 让 Gateway/AI/provider output 直接构造成功 result。

## 14. 新增 production 文件清单

建议按以下文件组织，不得预建 Diagnostics/Preview 空壳：

```text
RA2IniEditor.Application/RA2IniEditor.Application.csproj
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationCommonContracts.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationSectionContracts.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationReferenceContracts.cs
RA2IniEditor.Application/Automation/Experimental/IRa2AutomationDocumentQueryService.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationDocumentQueryService.cs
```

CommonContracts 包含 Span 和两个 snapshot；Section/Reference 文件各自包含 request、
result、failure 和 facts。允许为了项目既有风格拆成更多文件，但 exported type 集合和
职责不得改变。

## 15. 测试契约

### 15.1 Application.Tests（真正 Headless）

必须覆盖：

- 项目边界：net8.0、Application 只引用 Core、无 WPF/IDE/Infrastructure/Avalon；
- exported type reflection allowlist 精确等于本契约 15 个 public types；
- snapshot/span/request 参数和结果状态不变量；
- Section unique、nullable occurrence ambiguity、显式 occurrence、out-of-range；
- 重复 Section 字段隔离、重复 key、源码顺序、大小写和 span；
- Reference header/value/selection/fallback、resolved-empty、unresolved、invalid location；
- Reference 目标无定义仍可解析、结果顺序和大小写；
- 预取消与构建后取消均返回无 payload Canceled；
- 8,388,609 chars 返回 DocumentTooLarge；
- 10,001 fields/references 返回 ResultLimitExceeded；
- 1/4/7 MiB deterministic query characterization。

### 15.2 现有 Tests

- 更新 HLI-1A0 source path/namespace 断言到 Application；
- 现有 classifier/builder/caret/reference tests 不迁走、不弱化；
- 54 项 Query dependency regression 必须保持通过；
- 全量 `RA2IniEditor.Tests` 必须通过。

### 15.3 静态边界门禁

Application production source 禁止出现：

```text
System.Windows
ICSharpCode.AvalonEdit
RA2IniEditor.IDE
RA2IniEditor.Infrastructure
FieldRegistryRuntimeService
File.Read / File.Write / Directory. / Environment. / Process.
Dispatcher / Clipboard
```

测试源码中的路径断言不计入 production forbidden-token 扫描。

## 16. 分卡实施顺序与停止规则

| Card | 目标 | 必须通过后才能继续 |
|---|---|---|
| 1A1-0 | 记录基线、确认 22 文件和 63/41 直接消费者计数 | 当前 54 项回归 + Debug build |
| 1A1-1 | 新建空 Application/Application.Tests 并加入 solution/references | restore + solution build |
| 1A1-2 | 原子移动 22 文件、namespace/IVT/global using/3 个显式 using | solution build + 54 项回归 |
| 1A1-3 | 实现 15-type Experimental contract 和唯一 query service | Application.Tests 全部通过 |
| 1A1-4 | 现有 IDE integration/full regression | existing full tests 全部通过 |
| 1A1-5 | package、diff/API/static audit、治理收口 | clean package + 全部门禁 |

任一卡失败：只修复当前允许范围；如果需要触碰禁止项、扩大 exported types 或改变
既有语义，停止整个阶段并写 partial Stage Ledger，不继续后续卡。

## 17. 回滚策略

HLI-1A1 不引入运行时 feature flag 或双实现。若迁移无法满足门禁：

1. 将 22 个原文件恢复至 IDE 原路径/namespace；
2. 删除 Application global using、IVT 和 IDE/Tests project reference；
3. 从 solution 删除 Application/Application.Tests；
4. 删除新增项目和 Experimental contract；
5. 恢复 HLI-1A0 characterization source path；
6. 重新运行原 54 项回归与 Debug build。

禁止保留一份 IDE 实现和一份 Application 实现作为临时回滚方案。

## 18. 验证命令

实施阶段必须执行：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

迁移中间卡使用的 54 项回归 filter：

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-restore `
  --filter "FullyQualifiedName~Ra2AutomationDependencyConeCharacterizationTests|FullyQualifiedName~Ra2SectionClassifierTests|FullyQualifiedName~Ra2DocumentSemanticModelBuilderTests|FullyQualifiedName~Ra2CaretContextServiceTests|FullyQualifiedName~Ra2ReferenceFinderTests"
```

## 19. Public API 精确 allowlist

Application assembly 在 HLI-1A1 完成时只允许新增以下 exported types：

```text
IRa2AutomationDocumentQueryService
Ra2AutomationDocumentQueryService
Ra2AutomationDocumentSnapshot
Ra2AutomationFieldRegistrySnapshot
Ra2AutomationTextSpan
Ra2AutomationSectionQuery
Ra2AutomationSectionQueryResult
Ra2AutomationSectionQueryFailureKind
Ra2AutomationSectionFact
Ra2AutomationFieldFact
Ra2AutomationReferenceQuery
Ra2AutomationReferenceQueryResult
Ra2AutomationReferenceQueryFailureKind
Ra2AutomationReferenceTargetFact
Ra2AutomationReferenceFact
```

不得在 HLI-1A1 同时新增 capability descriptor、generic result、Diagnostics、Preview、
serialization、Gateway、factory、cache/session 或 file-system adapter public API。

## 20. 自审结论

- Scope：通过；只覆盖 Document Section/Reference query。
- Reuse：通过；22 文件是唯一算法权威，没有第二套 parser/reference path。
- Data ownership：通过；Host capture、Application invocation-local read、无状态 service。
- API：通过；15-type allowlist 有明确 HLI-2A consumer，raw model 不公开。
- Duplicate semantics：通过；nullable occurrence 与 body-span field isolation 已明确。
- Failure semantics：通过；empty success、unresolved、limits、cancel 和 analysis 可区分。
- Compatibility：通过；IVT/global using/3 个显式 using 和 full regression 均已入清单。
- Cancellation：通过；不改变 parser 签名，无 partial payload。
- Rollback：通过；无双实现，无持久化迁移，可完整恢复到 HLI-1A0。
- Remaining risk：实际迁移仍为 R3/R2；只有用户确认本契约后才可开始 1A1-0。

## 21. 确认后的执行终点

用户确认本最终契约后，可连续执行 1A1-0 至 1A1-5；每卡自审、门禁通过后继续，
无需逐卡等待批准。整个 HLI-1A1 完成后必须停止，不自动进入 HLI-1A2 Diagnostics。
