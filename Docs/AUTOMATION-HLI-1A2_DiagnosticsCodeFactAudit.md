# AUTOMATION-HLI-1A2 Diagnostics Code Fact Audit

更新日期：2026-08-22  
状态：Completed / Read-only code fact audit  
实现状态：Implemented / Verified；完成证据见 `AUTOMATION-HLI-1A2_StageLedger.md`

## 1. 审计目标和边界

本文档只回归当前 Diagnostics 实现、消费者、行为基线和可迁移闭包，
为 `AUTOMATION-HLI-1A2` 最终契约提供事实依据。审计期间没有修改生产代码、
Diagnostics 规则、Field Registry、Parser、Save Preflight、Shell 或 XAML。

范围只是 `ini.document.diagnostics.validate`；不把项目级文件枚举、读盘、
Problems 面板、Save 决策或 AI 摘要误写为 headless capability。

## 2. 当前权威调用路径

### 2.1 当前文件

```text
CurrentSourceSnapshot (IDE host state)
  -> CurrentFileReadonlyDiagnosticService
       -> Core IniParser + IniValidator
       -> Ra2DocumentSemanticModelBuilder
       -> Ra2FieldDiagnosticService + Ra2FieldTrustClassifier
       -> Ra2ReferenceDiagnosticCatalogBuilder
       -> Ra2ReferenceDiagnosticService
       -> Ra2ChainDiagnosticService
       -> IdeDiagnosticIssueViewModel
```

该路径无盘读，但算法直接构造 presentation ViewModel，因此 `net8.0`
Application 或独立 Agent host 无法消费。

### 2.2 A1 / Preview

```text
Ra2IniLanguageAnalysisService
  -> TextModel parse
  -> SemanticModel build
  -> CurrentFileReadonlyDiagnosticService -> ViewModel list
  -> property-by-property map -> Ra2DiagnosticFact
```

`Ra2DiagnosticFact` 已经是 UI-neutral 形状，但仍位于 IDE assembly。A1 为保持现有
规则和结果顺序，存在一次 ViewModel 往返和可能的 SemanticModel 重复构建。
后者是已记录的性能债务，HLI-1A2 不借迁移改写 parser 或合并双解析。

### 2.3 项目级诊断

`ManualFullDiagnosticsService` 拥有文件枚举、`IIniFileStore` 读取、当前编辑器文本
覆盖、8 MiB byte 过滤、忽略目录和项目级 reference catalog 组装。它是 host
orchestration，应继续留在 IDE；其每份文档的规则计算则必须改为消费同一
Application 实现。

### 2.4 Save / Issues / AI

- `Ra2SavePreflightDiagnosticService` 重用 current-file diagnostics，但是否保存的决策还是 Host-only。
- `IssuesViewModel` 负责过滤、排序、去重、计数和导航，不是诊断算法。
- `Ra2CurrentFileAiDiagnosticSummaryProvider` 只对已生成问题做上下文优先级摘要，不是
  Application diagnostics 的一部分。

## 3. 算法闭包

### 3.1 必须迁入 Application 的现有唯一实现

| 现有文件 | 作用 | 直接外部依赖 | 结论 |
|---|---|---|---|
| `Diagnostics/Ra2FieldDiagnosticService.cs` | 字段存在性、上下文、可信度与值类型 | Core Schema + FieldTrust + Language + ViewModel | 迁移并改为 neutral fact |
| `Diagnostics/Ra2ReferenceDiagnosticCatalog.cs` | 大小写不敏感 Section 目录 | Core Schema | 原子迁移，保持 internal |
| `Diagnostics/Ra2ReferenceDiagnosticCatalogBuilder.cs` | 从一份/多份 SemanticModel 建目录 | Application Language | 原子迁移，保持 internal |
| `Diagnostics/Ra2ReferenceDiagnosticService.cs` | 通用缺失引用 | Core Schema + Language + ViewModel | 迁移并改为 neutral fact |
| `Diagnostics/Ra2ChainDiagnosticService.cs` | Weapon/Projectile/Warhead 链路 | Core Schema + Language + ViewModel | 迁移并改为 neutral fact |
| `FieldTrust/Ra2FieldTrustClassifier.cs` | 字段可信度唯一分类器 | Core Schema | 与 2 个模型原子迁移 |
| `FieldTrust/Ra2FieldTrustInfo.cs` | 分类结果 | None | 保持 internal |
| `FieldTrust/Ra2FieldTrustLevel.cs` | 可信度等级 | None | 保持 internal |
| `Language/Ra2DiagnosticFact.cs` | UI-neutral 诊断事实 | Core | 迁入 Application Diagnostics，保持 internal |

共 9 个现有 internal 文件。实施时必须删除 IDE 旧路径，不允许两份
FieldTrust 或 Diagnostics 规则并存。

### 3.2 应留在 IDE 的边界

| 文件/领域 | 原因 |
|---|---|
| `CurrentSourceSnapshot` / `SourceEditorState` | 当前编辑器和加载状态属于 Host |
| `CurrentFileReadonlyDiagnosticService` | 保留 public IDE 兼容入口，只做 snapshot/fact/ViewModel 适配 |
| `ManualFullDiagnosticsService` | 项目文件枚举、读盘、skip 策略与聚合属于 Host |
| `Ra2SavePreflightDiagnosticService` | Save 决策和摘要属于 Host |
| `IdeDiagnosticIssueViewModel` / `IssuesViewModel` / XAML | presentation/navigation |
| AI diagnostic summary | 对 UI 问题的上下文摘要，不是规则权威 |

## 4. 当前可观察诊断契约

### 4.1 结果顺序

1. `IniParser` / `IniValidator` 结构问题，结构组内按行号。
2. Field diagnostics，按 SemanticModel key-value source order。
3. Generic reference diagnostics，按 reference source order；与 chain issue 同位置时删除 generic 项。
4. Chain diagnostics，按 reference source order。

Application 迁移不得进行全局 severity/location/code 排序；Issues ViewModel 的显示排序
是另一层 presentation 行为。

### 4.2 代码与来源

| Source kind | Codes |
|---|---|
| `CoreParserValidator` | `INI_STRUCTURE` |
| `Field` | `FIELD_UNKNOWN_KEY`, `FIELD_WRONG_CONTEXT`, `FIELD_OBSOLETE_KEY`, `FIELD_NON_EXISTENT_KEY`, `FIELD_PSEUDO_FIELD`, `FIELD_BOOLEAN_INVALID`, `FIELD_ENUM_INVALID`, `FIELD_ENUMLIST_INVALID`, `FIELD_NUMBER_INVALID` |
| `Reference` | `REF_MISSING_TARGET` |
| `Chain` | `CHAIN_WEAPON_MISSING`, `CHAIN_PROJECTILE_MISSING`, `CHAIN_WARHEAD_MISSING` |
| `DiagnosticService` | `DIAGNOSTIC_EXCEPTION`，仅为既有 IDE 兼容错误投影 |

`FIELD_INFERRED_FALLBACK` 常量存在，但当前实现对 Inferred/AutoExtracted 不生成
Issues；HLI-1A2 不借迁移激活该代码。

### 4.3 关键行为

- null field provider 只返回 structure issues。
- Unknown key 只在该 Section 已出现至少一个已知字段时报告。
- Unknown/Global/numeric key 的特定跳过规则保持。
- alias、allowed values 和 Section lookup 按当前大小写不敏感语义。
- inline semicolon comment 不参与 boolean/enum/list/number/reference 值校验。
- neutral/complex reference token 保持跳过。
- Weapon/Projectile/Warhead 链路问题优先于同行 generic missing-reference。
- current-document catalog 不读取或猜测项目其他文件。
- 结果保留 code、source kind、severity、message、file path、line/column、Section/key 和 version。

## 5. 异常、限制和安全事实

- `CurrentFileReadonlyDiagnosticService` 当前把诊断过程异常转为
  `DIAGNOSTIC_EXCEPTION` ViewModel，消息包含原 exception message。这是 IDE 兼容行为，
  不能作为新 public API 的安全 failure contract。
- `Ra2IniLanguageAnalysisService` 对自身非致命失败返回安全 `UnexpectedFailure`，
  不暴露 exception text；致命异常不降级。
- current-file 规则层当前没有 CancellationToken 或 result-item 上限。新的 public
  Application 入口需要复用 HLI-1A1 的 8,388,608 char / 10,000 item 边界，
  但 IDE 兼容入口不得因此静默改变既有结果。

## 6. 依赖与 public 面事实

- `RA2IniEditor.Application` 当前仅引用 Core，公开 15 个 Experimental types。
- 已有 `Ra2AutomationDocumentSnapshot` 提供 DocumentId、Version、FilePath、Text 和
  captured FieldRegistry Provider/Revision，Diagnostics 不需要第二个 snapshot。
- HLI-0B 已把 `Validate(snapshot, token)` 候选放在现有
  `IRa2AutomationDocumentQueryService`；没有足够理由新建微型 Diagnostics service 或 request DTO。
- raw SemanticModel、catalog、FieldTrust 和 internal diagnostic fact 均不应公开。

## 7. 测试基线

审计时执行：

```powershell
dotnet test RA2IniEditor.Tests/RA2IniEditor.Tests.csproj -c Debug --no-build `
  --filter "FullyQualifiedName~Diagnostic|FullyQualifiedName~Ra2IniLanguageAnalysis|FullyQualifiedName~Ra2FieldTrustClassifier"
```

结果：149 passed / 0 failed / 0 skipped。

该集合覆盖 current-file、field、reference、chain、manual project、save preflight、
AI summary、FieldTrust、A1 language analysis 和边界 guardrails。实施后必须保持这
149 项通过，并增加独立 `net8.0` Application contract tests。

## 8. 审计结论

1. 抽离 headless diagnostics 有必要；现有规则本身已无盘读，真正阻塞是
   assembly placement 和 ViewModel construction。
2. 不需要重写诊断规则，也不需要新依赖；应迁移唯一闭包并用
   IDE 单向 adapter 保持现有产品行为。
3. 初步方案中“新建 `IRa2AutomationDiagnosticsQueryService`”不必要；应按 HLI-0B
   扩展现有 Document Query service，减少公开面和 Gateway 注册负担。
4. 项目级 diagnostics 、Save Preflight 和 Issues UI 不属于 HLI-1A2 public capability。
5. 实施风险为 R3 跨程序集权威迁移 + R2 Experimental API 扩展；必须先确认
   `AUTOMATION-HLI-1A2_HeadlessDiagnosticsFinalContract.md`。
