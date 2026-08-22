# AUTOMATION-HLI-1A0 Dependency Cone Characterization Contract

状态：Completed  
日期：2026-08-22  
父契约：`Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md`  
风险：R1（tests + docs only）；未来 HLI-1A1 为 R3/R2

## 1. 目标

在创建 `RA2IniEditor.Application` 前冻结真实依赖闭包、现有输出语义、迁移接缝、
共享方式和验证门禁，避免边搬边猜、复制算法或将大量 internal 模型误变为 public API。

本阶段允许：

- 只读审计 Core/Infrastructure/IDE/Tests 源码和项目引用；
- 增加不改变产品行为的依赖锥/输出/性能特征测试；
- 更新 HLI 契约、Stage Ledger、API/Decision/CurrentStatus 文档。

本阶段禁止：

- 创建 Application 项目或修改 `.sln/.csproj`；
- 移动、复制、重命名 production 源码；
- 新增或修改 public API/DTO；
- 改变 parser、classification、diagnostics、preview、Apply、Save 或 UI 行为。

## 2. HLI-0B 可靠性裁决

结论：**通过，但必须采用本阶段冻结的共享 internal foundation 和切片修正。**

HLI-0B 的稳定部分：

- 需要 Headless assembly 才能服务独立 Agent/CLI/Job host；
- Application 只做 query/diagnostics/preview，不拥有活动编辑器、Apply 或 Save；
- 使用 immutable host-captured snapshot；
- 复用现有 A1/A2/Reference/Diagnostics 权威实现；
- 先做纵向切片，再做 Gateway；wire/IPC 后置。

原契约若不补充会产生返工的部分：

- Semantic foundation 被大量 IDE 功能直接消费，不能简单“移动几个服务文件”；
- Section/Reference 首切片不需要完整 TextModel parser；
- 当前 Windows test project 不能单独证明 net8.0 Headless 可消费；
- Duplicate Section occurrence 和 resolved-empty Reference 语义需要在 public DTO 前冻结；
- Diagnostics 与 ViewModel、Host Snapshot 与 IDE Services 的耦合必须分阶段处理。

本文件已把这些问题变为明确迁移策略和测试门禁，因此无需推翻 HLI-0B。

## 3. 当前程序集事实

```text
RA2IniEditor.Core              net8.0, no project dependency
RA2IniEditor.Infrastructure    net8.0 -> Core + Encoding.CodePages
RA2IniEditor.IDE               net8.0-windows + WPF -> Core + Infrastructure
RA2IniEditor.Tests             net8.0-windows -> Core + Infrastructure + IDE
```

当前不存在 Application、Application.Tests、Capability Registry/Gateway 或 CLI。

## 4. HLI-1A1 Query foundation 精确闭包

下列 22 个 production 文件构成 Section/Reference 首切片的最小算法闭包：

### Classification（4）

```text
Classification/IRa2SectionClassifier.cs
Classification/Ra2SectionClassificationResult.cs
Classification/Ra2SectionClassificationWarning.cs
Classification/Ra2SectionClassifier.cs
```

### Language/Semantic/Reference（18）

```text
Language/IRa2CaretContextService.cs
Language/IRa2DocumentSemanticModelBuilder.cs
Language/IRa2ReferenceFinder.cs
Language/Ra2CaretContext.cs
Language/Ra2CaretContextService.cs
Language/Ra2CaretRegion.cs
Language/Ra2DocumentSemanticModel.cs
Language/Ra2DocumentSemanticModelBuilder.cs
Language/Ra2DocumentSnapshot.cs
Language/Ra2IniLineParser.cs
Language/Ra2KeyValueSymbol.cs
Language/Ra2ReferenceFinder.cs
Language/Ra2ReferenceItem.cs
Language/Ra2ReferenceResult.cs
Language/Ra2SectionSymbol.cs
Language/Ra2TextSpan.cs
Language/Ra2ValueReferenceKind.cs
Language/Ra2ValueReferenceSymbol.cs
```

闭包只依赖 Core Schema 和 BCL。新增边界测试确认它不引用：WPF、AvalonEdit、
ViewModels、Diagnostics、Editing、IDE Services、Infrastructure、Registry singleton、
文件/目录 I/O、Environment、Process、Clipboard 或 Dispatcher。

### 明确不进入 HLI-1A1

```text
TextModel/IRa2IniTextDocumentParser.cs
TextModel/Ra2IniDocumentLine.cs
TextModel/Ra2IniDocumentLineKind.cs
TextModel/Ra2IniNewLineKind.cs
TextModel/Ra2IniTextDocument.cs
TextModel/Ra2IniTextDocumentParser.cs
```

完整 TextModel 是 A1 Diagnostics/A2 Preview 所需，不是 Section/Reference Query 首切片
的依赖。HLI-1A1 不得为了目录整洁提前搬迁。

## 5. 下游影响范围与共享策略

对 22 个类型的静态引用扫描得到：

```text
Production files containing references: 63（包含闭包自身）
Test files containing references: 41（包含 HLI-1A0 test）
```

这些消费者覆盖 AI Context、Completion、Hover、Navigation、Diagnostics、Editing、
Highlighting、Quick Peek、Search、Project Explorer、Find References 和 Shell wiring。

### 冻结策略

HLI-1A1 必须：

1. 把 22 文件移动到 Application 的 internal semantic foundation，而非复制；
2. 使用 `RA2IniEditor.Application.Language/Classification` 命名空间；
3. 通过 Application 的显式 `InternalsVisibleTo` 只授权 IDE、Application.Tests 和
   现有 Tests 访问 internal foundation；
4. IDE/Tests 使用各自一个 project-level global using 过渡到新命名空间；
5. 不把 `Ra2DocumentSemanticModel`、symbols、caret context 或 classifier 设为 public；
6. Agent/CLI 只能消费新的高层 public query contract；
7. 同一 commit 内让 IDE 原消费者编译到 Application 权威实现，原 IDE 文件不得保留副本。

该策略避免修改约 100 个消费者文件中的 using，同时避免 public API 膨胀和命名空间
长期错误。IVT 是 solution-internal host seam，不是 Agent 扩展点。

## 6. Query 输出语义冻结

### Section

- `SemanticModel.Sections` 按源码顺序保留重复 Section。
- 新 `GetSection` 必须使用显式零基 occurrence。
- occurrence 超界映射 `NotFound`；不得静默回退到首项。
- 现有 `FindSectionByName` 返回首项，只能保留为 internal compatibility helper，
  不能作为 public occurrence 语义。
- Section fields 保持源码顺序；不合并重复 key。

### Reference

- source offset/selection 先通过现有 CaretContext/Finder 解析目标。
- 已解析目标但 `Items.Count == 0` 是成功空结果。
- Finder 返回空 target name 表示目标未解析，应映射 `TargetNotResolved`。
- 匹配保持 OrdinalIgnoreCase，结果顺序保持 `model.References` 源码顺序。
- 不调用项目文本 Search，不读取磁盘，不扩大为跨文件 reference query。

新增特征测试锁定 duplicate Section source order 与 resolved-empty/unresolved 区别。

## 7. 既有双解析事实

当前 SemanticModel 构建内部有两个既有阶段：

1. `Ra2SectionClassifier` 使用自己的 `StringReader` parse；
2. `Ra2DocumentSemanticModelBuilder` 使用 `Ra2IniLineParser` 再构建 span-rich symbols。

A0 已证明不同 parser surface 有可观察差异。HLI-1A1 只搬迁原实现，不合并解析器、
不改变 inline comment、Section suffix、newline 或 classification 语义。若性能证据表明
双解析成为瓶颈，另开契约偿还；不得在 assembly move 中顺手优化。

## 8. Diagnostics 闭包与 HLI-1A2 接缝

当前 A1/Diagnostics 路径：

```text
Ra2IniLanguageAnalysisService
  -> full TextModel parser
  -> SemanticModelBuilder
  -> CurrentFileReadonlyDiagnosticService
       -> Core IniParser/Validator
       -> FieldDiagnosticService + FieldTrust
       -> Reference catalog/service
       -> ChainDiagnosticService
       -> IdeDiagnosticIssueViewModel
  -> Ra2DiagnosticFact adapter
```

已确认耦合：

- `CurrentFileReadonlyDiagnosticService` public 返回 `IdeDiagnosticIssueViewModel`；
- field/reference/chain services 也直接构造该 ViewModel；
- A1 为保持现有规则会再次构建 SemanticModel（既有受控债）。

HLI-1A2 必须移动规则实现并让 Application 直接产生 neutral diagnostic facts；IDE
保留 public compatibility wrapper，把 facts 映射回原 ViewModel。不得在 IDE 和
Application 各保留一套诊断算法，不得改变 code/severity/order/location/message。

## 9. Preview 闭包与 HLI-1B 接缝

未来 Preview slice 至少包含：

```text
TextModel 6 files
Language analysis request/result/failure/service/interface
FieldTrust 3 files
Ra2AddPropertyInsertPlan / Planner
Ra2IniEditOperation / Plan / Preview / PreviewService / interface
Ra2TextChange / Ra2TextChangeSet
```

边界：

- 当前 `Ra2AuthoringSnapshot` 依赖 IDE Services、EditableSession 和 editor state，
  继续作为 Host capture；不直接搬迁或改 public。
- Host adapter 把它映射为新的 immutable Application snapshot。
- `Ra2IniEditPreviewCurrency`、A3 Workspace、TransactionPort、TextChangeApplier、
  Apply result 和 Undo 继续留在 IDE。
- Preview 继续只产生 candidate/change set/evidence/diagnostic delta，不修改状态。

## 10. 测试程序集策略

HLI-1A1 候选应新增：

```text
RA2IniEditor.Application.Tests     net8.0 -> Application + Core
```

职责：

- 证明公共 contract 和 internal semantic foundation 不依赖 Windows/WPF；
- Section/Reference contract、ordering、failure、cancellation 和 1/4/7 MiB 特征；
- Application dependency boundary source test。

现有 `RA2IniEditor.Tests` 继续：

- 证明 IDE Completion/Hover/Diagnostics/Search/A2/A3/A4 仍消费同一实现；
- 完整非 UI 回归和 Shell boundary tests。

不 multi-target 现有 Tests，不让新 Headless test project 引用 IDE/Infrastructure。

## 11. 性能特征基线

HLI-1A0 在当前 Debug test host 中对同一文本连续构建两次 SemanticModel：

| Approx size | Two builds | Chars | Sections / Keys / Refs |
|---:|---:|---:|---:|
| 1 MiB | 20 ms | 1,048,606 | 3 / 3 / 2 |
| 4 MiB | 102 ms | 4,194,346 | 3 / 3 / 2 |
| 7 MiB | 174 ms | 7,340,034 | 3 / 3 / 2 |

该数据仅是当前机器的 characterization，不是硬 SLA。HLI-1A1 必须保持确定性和不
修改输入，并记录迁移后的同类数据；若出现数量或数量级回退，停止诊断。不得用
脆弱的毫秒上限造成测试抖动。

## 12. HLI-1A1 预期 Diff Manifest

允许候选范围：

```text
RA2IniEditor.Application/                    new net8.0 project
RA2IniEditor.Application.Tests/              new net8.0 test project
RA2IniEditor.IDE/Classification/*            4 source moves
RA2IniEditor.IDE/Language/<22-file subset>   18 source moves
RA2IniEditor.IDE/<one global using file>     internal seam
RA2IniEditor.Tests/<one global using file>   test seam
RA2IniEditor.IDE.csproj                      Application reference
RA2IniEditor.Tests.csproj                    Application reference
RA2IniEditor.IDE.sln                         two project entries
HLI-1A1 tests and governance docs
```

HLI-1A1 明确禁止触碰：

```text
ShellWindow.xaml / ShellWindow.xaml.cs
TextModel 6-file full parser
Diagnostics implementation
Editing Preview/A3/Save
Field Registry runtime/data
Completion/Hover behavior
AI/Search behavior
XAML/Dock/AutomationIds
legacy
```

如果编译要求修改这些禁止项中的 production 行为，HLI-1A1 必须停止并修订契约，
不得边迁移边扩大范围。

## 13. Public API 预登记

本阶段实际 public API：None。

HLI-1A1 仅允许最小 Experimental surface：

- `IRa2AutomationDocumentQueryService`；
- immutable document/registry snapshot；
- Section/Reference request/result/failure DTO；
- 不公开 raw SemanticModel、symbols、classifier 或 caret context。

Diagnostics 和 Preview public DTO 分别后置到 HLI-1A2/HLI-1B，不在 HLI-1A1
预先占位。完整候选见 `Docs/PublicApiLedger.md`。

## 14. 验证契约

HLI-1A0 已执行：

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-restore `
  --filter "FullyQualifiedName~Ra2AutomationDependencyConeCharacterizationTests" `
  --logger "console;verbosity=detailed"
```

结果：7/7 passed；存在一个既有 `BuiltInFieldRegistryPackLoaderTests.cs` CS8602 warning。

收口回归另执行 Query 依赖集（characterization/classifier/semantic model/caret/reference）：
54/54 passed。`RA2IniEditor.IDE.sln` Debug build：0 warnings / 0 errors。

HLI-1A1 必须执行：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

附加静态门禁：Application `net8.0`、无 UseWPF、仅引用 Core；production source 无
WPF/AvalonEdit/IDE/Infrastructure/IO/global singleton；IDE 不再包含 22 文件副本；
public surface 与 PublicApiLedger 精确一致。

## 15. 自审

- Scope：通过；测试与文档之外无修改。
- Reuse：通过；22 文件闭包是现有唯一权威，完整 TextModel 后置。
- Ownership：通过；Host snapshot/Apply/Save 不下移。
- API control：通过；raw model 保持 internal，高层 Query public surface 后置。
- Consumer migration：通过；IVT + global using 避免约 100 文件机械 churn。
- Determinism：通过；duplicate occurrence、resolved-empty、1/4/7 MiB 特征已锁定。
- Anti-rework：通过；Diagnostics 与 Preview 接缝单独列出，不在 1A1 混迁。
- Remaining risk：HLI-1A1 仍是 R3/R2，必须有独立最终契约和用户确认。

## 16. 下一入口与停止规则

下一安全入口：

```text
AUTOMATION-HLI-1A1 Document Query Slice Final Contract
```

HLI-1A0 完成后停止。未确认 HLI-1A1 前，不创建 Application/Application.Tests，
不修改 solution/project references，不移动 production 文件，不新增 public API。
