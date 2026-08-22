# AUTOMATION-HLI-0B Minimum Capability Contract

状态：Confirmed / Completed (contract stage)  
日期：2026-08-22  
前置审计：`Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`  
当前差异风险：R0（DocsOnly）  
后续实现风险：R3（程序集、所有权与跨层边界）；未来进程外协议为 R4

## 1. 目标

冻结首批 UI-neutral / headless INI 能力的最小边界，使当前 WPF IDE、内置 AI，以及未来独立 Agent/CLI 能复用同一套语义算法，同时保持：

- A1 语言分析、现有引用解析、诊断规则和 A2 Preview 为唯一算法来源；
- A3 Apply/Undo、活动编辑器和 A4 proposal 生命周期继续由 IDE 主机拥有；
- Save/Backup/Writer/Rollback 继续由用户与 IDE 保存链路拥有；
- 进程内 CLR contract 与未来 IPC/MCP/wire DTO 分离；
- 不通过公开 WPF、AvalonEdit、ViewModel、Shell 或运行时全局服务来实现复用。

本文件只批准设计边界，不直接授权创建项目、移动源码或新增 public API。用户于
2026-08-22 确认“若契约可靠则执行”；代码事实复审通过后，本契约按自身门禁进入
HLI-1A0 特征化。生产迁移仍必须由独立 HLI-1A1 契约授权。

## 2. Headless 拆分必要性裁决

### 2.1 结论

**需要建立 headless 程序集边界，但不需要、也不允许一次性搬迁全部算法。**

| 产品目标 | 是否需要物理拆分 | 原因 |
|---|---:|---|
| 仅让当前 WPF 内置 AI 使用 A1-A4 | No | 当前 internal IDE 服务已经可在进程内完成 query、Preview 和 Apply |
| 让独立 `net8.0` Agent/CLI/Job 宿主复用算法 | **Yes** | 独立宿主不能引用 `net8.0-windows` WPF IDE assembly |
| 只增加一个 IPC/MCP wrapper，但算法仍留在 IDE | Not sufficient | wrapper 仍把 WPF IDE 进程变成唯一执行宿主，无法形成独立 headless 能力层 |
| 将整个 Language/Diagnostics/Editing 目录一次搬走 | No | 依赖锥横跨 TextModel、Classification、FieldTrust、ViewModel 和 host lifecycle，风险过大 |

本项目已经把独立 Agent/Automation 作为后续路线，因此“解除 WPF 程序集依赖”是必要架构工作；“整包重构”不是必要工作。正确路径是先冻结中立 contract，再以能力纵向切片迁移最小依赖锥。

### 2.2 风险等级

| 方案 | 当前成本 | 长期风险 | 综合判断 |
|---|---:|---:|---|
| 不拆分，未来各宿主复制算法 | Low | Critical：parser/diagnostics/edit planner 漂移、双重事实源 | Reject |
| Gateway 直接引用 IDE assembly | Low | High：锁定 Windows/WPF，泄漏 Shell/ViewModel 生命周期 | Reject as long-term design |
| 一次性搬迁全部 UI-neutral 候选 | High | High：依赖锥与回归面过大 | Reject |
| 新建最小 Application contract，逐能力迁移并让 IDE 反向消费 | Medium | Low/Controlled | **Recommended** |

风险分类：推荐路径仍是 R3，因为它改变程序集和跨层所有权；但它不改变 parser、diagnostics、Preview 或保存语义。按阶段执行并保留等价测试后，风险可控制在中等。

## 3. 当前事实与拆分难点

当前项目图：

```text
RA2IniEditor.Core                 net8.0
        ^
        |
RA2IniEditor.Infrastructure       net8.0
        ^
        |
RA2IniEditor.IDE                  net8.0-windows + WPF
```

已确认的依赖锥：

- `Ra2IniTextDocumentParser`、SemanticModel 和 ReferenceFinder 算法无控件依赖，但位于 IDE；
- `Ra2DocumentSemanticModel` 还引用 IDE Classification 类型；
- `Ra2IniLanguageAnalysisService` 调用现有 diagnostics，并出现 ViewModel 适配依赖；
- `CurrentFileReadonlyDiagnosticService` 直接返回 `IdeDiagnosticIssueViewModel`；
- `Ra2IniEditPreview` 引用 IDE FieldTrust、Language 和 TextModel；
- `Ra2AuthoringSnapshot` 引用 IDE Services 中的 Registry Snapshot；
- A3 transaction 与 Shell/AvalonEdit/UI thread/Undo 有意耦合，不属于 headless 算法；
- Save 链路包含磁盘、encoding、backup 和 rollback，不属于 Agent capability。

因此，物理拆分不能简单等同于“移动几个 Service 文件”。HLI-1 必须沿能力调用闭包移动或中立化依赖，并让 IDE 继续调用新权威实现。

## 4. 目标程序集与依赖方向

推荐新增候选程序集：

```text
RA2IniEditor.Application          net8.0
```

HLI-1 首版依赖方向必须为：

```text
RA2IniEditor.Application -> RA2IniEditor.Core
RA2IniEditor.IDE         -> RA2IniEditor.Application
RA2IniEditor.IDE         -> RA2IniEditor.Infrastructure
RA2IniEditor.IDE         -> RA2IniEditor.Core
```

约束：

1. `RA2IniEditor.Application` 首版只能引用 Core 和 .NET BCL；不得引用 IDE、WPF、AvalonEdit 或 Infrastructure。
2. Infrastructure 在 HLI-1A 中保持现有依赖，不为方便而让 Application 反向引用 Infrastructure。
3. 磁盘读取、项目枚举、活动编辑器 capture、Registry runtime capture 都由 IDE/Infrastructure adapter 完成，Application 只接收显式不可变输入。
4. 若以后需要 Infrastructure 实现 Application port，必须使用 `Infrastructure -> Application` 的单向引用并单独审查；不得形成循环。
5. 不引入 NuGet 依赖，不 multi-target IDE，不通过 source link/重复 Compile 同一 `.cs` 文件规避迁移。

此程序集方案是 HLI-0B 的推荐最终契约候选；只有用户确认后，HLI-1A 才能修改 `.sln/.csproj`。

## 5. Headless 合格标准

进入 Application 的类型和算法必须同时满足：

- 不引用 `System.Windows.*`、WPF Dispatcher、AvalonEdit、Shell、Dock 或 ViewModel；
- 不访问文件系统、环境变量、网络、注册表、Clipboard 或 UI Automation；
- 不读取 `FieldRegistryRuntimeService.Current*` 等全局可变状态；
- 只使用调用方提供的 immutable document/registry snapshot；
- 对同一输入产生确定性、有序结果；
- 支持 `CancellationToken`，取消不返回可应用的部分结果；
- 预期失败使用 typed result，不以异常或英文消息解析表示；
- 不保留当前文档、当前项目、当前 Registry 或 active Preview 的静态/单例状态；
- 不执行 Apply、Undo、Save、Backup 或磁盘写入。

不满足任一项的逻辑必须留在 host/adapter 层，而不是为了“无头”名义强行搬迁。

## 6. 首批能力集合

HLI-0B 冻结四个进程内能力 ID。HLI-0A 中的 `project.*` 名称只是候选；本契约改用 `ini.document.*` 明确首版单文档范围，避免把 current-document 实现误称为 project-wide。

| Capability ID | 首版能力 | 复用权威 | 明确排除 |
|---|---|---|---|
| `ini.document.section.get` | 从显式文本快照按 Section 名称和 occurrence 查询 Section/field/span facts | TextModel + SemanticModel builder | 磁盘读取、项目扫描、UI navigation |
| `ini.document.references.find` | 在同一文档 SemanticModel 中解析目标并返回该文档内引用 | `Ra2ReferenceFinder` | 项目级 cross-file references；该能力保留给未来 `ini.project.references.find` |
| `ini.document.diagnostics.validate` | 对一个文档快照运行现有只读诊断并返回 neutral facts | A1 analysis + current diagnostic algorithms | ViewModel、问题面板、保存阻断、项目聚合 |
| `ini.document.edit.preview` | 对当前文档 Snapshot 和受限 Plan 生成 A2 等价 Preview | A2 Snapshot/Plan/Preview/ChangeSet | Apply、generic patch、multi-file、Save |

首版不增加单独的 Field Schema capability。Field schema 通过 captured Registry Snapshot 作为上述能力的分析上下文；对 Agent 暴露字段查询应在 Gateway 阶段单独决定，避免同时形成两套 Registry surface。

## 7. 最小进程内 contract

以下名称和形状是 HLI-1 实现输入；稳定性统一为 `Experimental`。它们是 solution-level CLR API，不是 JSON、IPC 或 MCP payload。

### 7.1 Common snapshot

`Ra2AutomationDocumentSnapshot`：

- `Guid DocumentId`
- `int Version`：映射现有 `EditRevision`；必须非负
- `string FilePath`：只作诊断身份/显示，不授予文件访问权
- `string Text`：本次调用唯一文本事实源
- `bool IsEditable`：只影响 Preview eligibility，不影响只读查询
- `Ra2AutomationFieldRegistrySnapshot FieldRegistry`

`Ra2AutomationFieldRegistrySnapshot`：

- `IRa2FieldDefinitionProvider Provider`
- `long Revision`：必须为正数

规则：

- Host capture 负责验证 editor text 与 session text 一致；Application 不接触编辑器。
- Provider/Revision 必须一次捕获并贯穿调用；调用期间 Registry reload 不改变旧 snapshot。
- `ProjectRootPath` 不进入首版算法权威。需要展示时可作为 host metadata；不能用来读盘。
- 磁盘来源 adapter 继续遵守当前 8 MiB 文件边界；Application 的文本长度预算必须在 HLI-1A 性能契约中以字符单位冻结，不能把 byte limit 混作 `string.Length`。

### 7.2 Services

候选接口：

```csharp
public interface IRa2AutomationDocumentQueryService
{
    Ra2SectionQueryResult GetSection(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2SectionQuery request,
        CancellationToken cancellationToken = default);

    Ra2ReferenceQueryResult FindReferences(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2ReferenceQuery request,
        CancellationToken cancellationToken = default);

    Ra2DocumentDiagnosticsResult Validate(
        Ra2AutomationDocumentSnapshot snapshot,
        CancellationToken cancellationToken = default);
}

public interface IRa2AutomationEditPreviewService
{
    Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default);
}
```

接口拆为 Query 与 Preview 两项，避免一个通用 Gateway 接口过早承担 Apply/Save，也避免四个微型 Service 造成注册膨胀。未来 Gateway 只做 capability ID 到 typed service 的适配。

### 7.3 Result/failure rules

不使用一个粗粒度 failure enum 覆盖所有领域失败。每个 result 必须包含：

- `Succeeded`
- capability-specific `FailureKind`
- safe localized/display message（不得含 raw exception、绝对敏感路径或 provider body）
- 成功时的 immutable payload；失败时不得携带可误用的部分 payload
- `Canceled` 必须是显式 failure kind

领域映射：

- Section query 保留 `NotFound` 与 `AmbiguousSection` 区别；
- Reference query 区分 `TargetNotResolved`、`NoReferences` 和分析失败；`NoReferences` 是成功空结果，不是异常；
- Diagnostics 保留 A1 analysis failure，并返回按现有顺序映射的 neutral `Ra2DiagnosticFact` 等价事实；
- Preview 必须保留现有 `Ra2IniEditPreviewFailureKind` 的可区分语义，不合并 stale、ambiguous、conflict、overlap、no-op 和 analysis failure。

构造器参数错误可以抛出 `ArgumentException`；运行时输入、取消、分析和业务失败必须返回 typed result。不得通过 `bool/null` 或消息文本推断状态。

## 8. Section / reference / diagnostic payload 边界

### Section

请求至少包含 Section name 和零基 occurrence。结果包含：

- canonical input document identity/version/registry revision；
- Section name/kind；
- header/body/full span；
- 有序 field facts：key、effective value、line/span；
- 不返回 TextBox、Caret、selection、navigation command 或 ViewModel。

### Reference

首版请求通过显式 source offset 或 selection span 解析目标，保持与现有 finder 等价；不把 WPF caret 对象放入 contract。结果包含目标名称/SectionKind 和有序的 current-document reference facts。

`ini.document.references.find` 不得读取项目文件，也不得内部调用 Project Search。项目级语义引用必须另有 project snapshot、文档加载失败、上限、取消和一致性契约。

### Diagnostics

结果必须是 UI-neutral facts，保留 code、source kind、severity、message、file identity、line/column、Section/key 和 analysis version。IDE 问题面板负责把 facts 映射为现有 ViewModel；Application 不返回 `IdeDiagnosticIssueViewModel`。

诊断算法和顺序以现有路径为权威。抽离不得修改规则、降低 severity、重新排序或为通过测试而过滤结果。

## 9. Preview 与 Host Apply 分界

`ini.document.edit.preview` 首版只支持现有：

- `UpsertField`
- `ReplaceFieldValue`
- 1..128 operations
- PlanId、ExpectedDocumentId、ExpectedVersion、ExpectedFieldRegistryRevision
- deterministic non-overlapping TextChangeSet
- candidate text、operation evidence、current/candidate diagnostics delta

Application Preview：

- 可以生成 PreviewId，但不保存 active Preview；
- 不检查 live editor，因为它只拥有 immutable snapshot；
- 不进入 WPF thread；
- 不修改文本、Session、Undo 或磁盘。

IDE Host：

- 现有 A3 workspace 继续拥有一个 active Preview、generation 和 single-use claim；
- Apply 前继续读取 live editor/session/Registry 并执行完整 currency check；
- 成功 Apply 继续产生一次 Session revision、一次 editor sync 和一个 semantic Undo unit；
- Apply 后仍不自动 Save。

Gateway/Agent 不得获得 `IRa2EditorTransactionPort`、Save service 或 file writer。

## 10. Lifecycle 与并发契约

1. Host capture：一次捕获 DocumentId、Version、exact Text、editable state 和 Registry Provider/Revision。
2. Invocation：每次 capability 调用只读取该 snapshot；不得回读 host singleton。
3. Cancellation：在分析/枚举/Preview 的有界检查点响应；取消结果不携带 candidate/applicable payload。
4. Completion：结果不可变；service 不把它注册为 active state。
5. Host proposal：只有 IDE A3/A4 adapter 可以把成功 Preview 注册为 active proposal。
6. Currency：用户编辑、切换文档或 Registry reload 后，旧结果仍可展示，但不能绕过 A3 live recheck 应用。
7. Disposal：首版 DTO 无 unmanaged ownership；调用方拥有 CancellationTokenSource 和任何 host resource。

## 11. 明确禁止的能力

HLI-1/HLI-2 首个闭环不得注册或隐式提供：

- `semantic.proposal.apply`
- `file.save` / `file.write` / 任意路径读写
- `project.multi_file.edit`
- generic text patch / unified diff apply
- Shell command / process launch
- WPF、AvalonEdit、Dispatcher、ViewModel handle
- Field Registry reload/apply/import/learning/rollback
- runtime、asset、test execution
- API key/config/env access
- 自动确认、自动 Apply、自动 Save 或自动重试

## 12. 渐进迁移计划

### HLI-1A0 Dependency Cone Characterization（R2/R3）

- 新建 Application 项目之前列出 section/reference 所需 TextModel、Classification、Language 文件闭包；
- 增加 assembly-boundary tests，明确禁止 WPF/AvalonEdit/IDE ViewModel 引用；
- 锁定现有 section/reference 输出顺序和空结果语义；
- 输出 exact file move/adapter manifest 后停下审查。

### HLI-1A1 Document Query Slice（R3）

- 新建 `RA2IniEditor.Application` 和最小公共 Experimental contracts；
- 只迁移 section/reference 所需的最小算法闭包；
- IDE 原调用方改为消费 Application 权威实现；
- 删除或保留为单向 compatibility adapter，禁止两份算法长期并存。

### HLI-1A2 Neutral Diagnostics Slice（R3）

- 把 neutral diagnostic facts/analysis orchestration 移入 Application；
- 诊断规则继续只有一个实现；
- IDE 在边界映射为原 ViewModel；
- 对比当前与迁移后 facts 的 code/severity/order/location 完全等价。

### HLI-1B Semantic Preview Slice（R3）

- 迁移 A2 Snapshot/Plan/Preview/ChangeSet 的 UI-neutral 闭包；
- 保留全部 failure/currency/diagnostic delta 语义；
- A3 workspace/transaction 留在 IDE，通过 adapter 消费新 Preview；
- 验证一次 Apply/Undo 与迁移前等价且仍不保存。

### HLI-1C Host Boundary Confirmation（R3）

- 只确认 IDE adapter 与 A3/A4 生命周期连接；
- 不向 Application 或 Gateway 下移 active editor、Apply、Undo、Save 所有权。

### HLI-2A Gateway（后置）

- 只有上述 typed services 稳定后才实现 capability descriptor/registry；
- Gateway 不重新实现算法；
- wire/IPC/MCP 继续后置到单独 R4 契约。

## 13. 风险登记与缓解

| 风险 | 等级 | 触发方式 | 缓解与停止条件 |
|---|---:|---|---|
| 依赖锥被低估 | High | SemanticModel 间接依赖 IDE Classification/FieldTrust/ViewModel | HLI-1A0 先生成 exact manifest；超过批准范围即停，不边搬边猜 |
| 形成第二套 parser/diagnostics/planner | Critical | 新项目复制现有逻辑但 IDE 继续走旧路径 | 每个切片必须让 IDE 调用新权威；等价后移除算法副本或仅留 adapter |
| public Experimental API 过度膨胀 | Medium | 为每个 internal model 创建镜像 DTO | 只公开两个 service 与最小 input/result；raw SemanticModel 保持 internal |
| 诊断语义/顺序漂移 | High | ViewModel 中立化时顺手重写规则 | golden equivalence tests；任何 code/severity/order/location 差异即失败停止 |
| Registry snapshot 失去代次稳定性 | High | Application 回读 runtime singleton | Provider + Revision 一次捕获；旧 snapshot 在 reload 后保持旧 provider |
| Preview stale/Apply 边界弱化 | Critical | 把 live check 或 active Preview ownership 下移 | A3 为唯一 Apply；Document/Version/Registry/text 全量复检不变 |
| 性能/内存回退 | Medium | 为 query 重复 parse 或复制大文本 | 记录 parse 次数和 1/4/7 MiB 基线；不在 HLI-0B 猜 byte/char limit |
| 取消返回部分可应用结果 | High | 中途返回 candidate/change set | Canceled 结果无 applicable payload；A3 不接收失败结果 |
| wire DTO 与 CLR contract 锁死 | High | 直接 JSON 序列化 public CLR 类型 | HLI-0B 明确禁止；R4 独立设计版本化协议 |
| 一次性大迁移导致回归定位困难 | High | 合并 Language/Diagnostics/Preview 大批移动 | 按 1A0/1A1/1A2/1B/1C 分阶段，每阶段独立 build/test/review |

## 14. Public API Ledger 预登记

本阶段实际 public API 变化：**None**。

HLI-1 获批后候选项：

| Candidate | Kind | Reason | Expected next use | Stability | Required tests |
|---|---|---|---|---|---|
| `IRa2AutomationDocumentQueryService` | public interface | 跨程序集复用 section/reference/diagnostics | HLI-1A / HLI-2A | Experimental | contract + equivalence + dependency boundary |
| `IRa2AutomationEditPreviewService` | public interface | 跨程序集复用 A2 Preview | HLI-1B / HLI-2A | Experimental | A2 parity + cancellation + limits |
| common snapshot types | public immutable DTO | 显式文本/Registry 事实源 | all HLI-1 capabilities | Experimental | invariants + old-snapshot retention |
| query/diagnostic/preview results | public immutable DTO/failure kinds | 结构化成功/失败 | HLI-1/HLI-2 | Experimental | state consistency + failure mapping |

兼容策略：不修改 Core 现有 public field schema；IDE internal A1/A2 类型在迁移阶段通过单向 adapter 或受控 move 过渡。不得为减少 using 修改而把旧类型直接改 public。

## 15. 验证契约

HLI-0B 文档阶段：

- DocsOnly 路径、互链和精确写入范围；
- contract 必须同时包含必要性裁决、四个 capability、程序集方向、host-only 禁止项、风险表和下一门禁；
- build/test/package 为 NotRun，因为无源码或项目变化。

未来 HLI-1A0/1A1：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
```

额外必须验证：

- Application target framework 为 `net8.0`，不含 `UseWPF`；
- Application 项目引用仅包含 Core；
- Application production source 无 `System.Windows`、AvalonEdit、IDE ViewModels/Shell/Services runtime singleton 引用；
- section/reference/diagnostic/A2 Preview 等价测试；
- existing IDE callers 使用新权威实现；
- full non-UI suite；
- package 只在阶段完成且实现已通过时执行。

## 16. 自审结论

- Necessity：通过。独立宿主目标使程序集拆分成为必要；仅 WPF 内置 AI 不构成拆分理由。
- Scope：通过。只拆 query/diagnostic/preview 算法，Apply/Save 明确保留在 host。
- Reuse：通过。现有 A1/A2/Reference/Diagnostics 是唯一权威，不新写平行算法。
- Dependency direction：通过。Application 首版只依赖 Core，避免 Infrastructure/WPF 反向污染。
- Data ownership：通过。Host capture，Application pure invocation，A3 owns active Preview/Apply。
- API control：通过。只预登记两个 public Experimental service 和最小 DTO；wire protocol 后置。
- Anti-rework：通过。先 1A0 exact dependency manifest，再纵向切片迁移，不整包搬迁。
- Remaining gate：R3 implementation 未授权。用户确认本契约后，下一阶段仍应先执行 HLI-1A0，而不是直接搬迁全部算法。

## 17. 停止条件与下一入口

HLI-0B 在本契约生成、自审和状态更新后停止。下一安全入口：

```text
AUTOMATION-HLI-1A0 Dependency Cone Characterization Contract
```

HLI-0B 已获得用户确认。HLI-1A0 完成前，以及 HLI-1A1 未获得独立确认前，不得：

- 新建 `RA2IniEditor.Application`；
- 修改 solution/project references；
- 移动或公开 A1/A2 类型；
- 修改任何运行时、测试、Shell、Field Registry 或保存行为。

## 18. HLI-1A0 复审修正（2026-08-22）

HLI-1A0 的真实依赖锥审计确认总体架构可靠，同时冻结以下防返工修正：

1. Section/Reference 首切片不需要完整 `TextModel/Ra2IniTextDocumentParser`；它只需
   `Ra2IniLineParser`、Classification、SemanticModel、CaretContext 和 ReferenceFinder
   的 22 文件闭包。完整 TextModel parser 留到 Diagnostics/Preview 切片。
2. 这 22 个类型当前被 63 个 production 文件和 41 个 test 文件引用（包含闭包自身）。
   HLI-1A1 不得通过逐文件复制或把全部 internal 类型改 public 解决引用问题。
3. 推荐的最小共享方式是：将闭包作为 Application 的 internal semantic foundation，
   使用明确的 `InternalsVisibleTo` 只开放给 IDE 与测试程序集，并在 IDE/Tests 使用一个
   project-level global using。外部 Agent 只能看到高层 Experimental contract，不能看到
   raw SemanticModel。
4. 新增独立 `RA2IniEditor.Application.Tests` (`net8.0`) 候选，以证明 Headless contract
   不依赖 Windows/WPF；现有 `RA2IniEditor.Tests` 继续负责 IDE 集成回归。
5. 重复 Section 的 source order 和 occurrence 必须保留；不能继续使用只返回首项的
   `FindSectionByName` 作为新的 occurrence API。
6. Reference 必须把“目标已解析但无引用”和“光标/选择无法解析目标”映射为不同结果。
7. `CurrentFileReadonlyDiagnosticService` 的 ViewModel 耦合留到 HLI-1A2；
   `Ra2AuthoringSnapshot` 的 IDE Services/Session 耦合保留为 Host capture，不迁入 Application。
8. 当前 Classifier parse 与 Semantic builder parse 是既有双阶段行为；HLI-1A1 只搬迁，
   不顺手合并或改写解析语义。任何优化必须等待独立性能证据。

这些修正缩小首切片、避免 public API 膨胀，并为 IDE 与未来 Agent 保留同一算法权威；
它们不改变 HLI-0B 的能力 ID、所有权、保存边界或外部产品语义。
