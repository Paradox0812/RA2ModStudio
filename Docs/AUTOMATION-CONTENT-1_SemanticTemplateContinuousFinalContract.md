# AUTOMATION-CONTENT-1 Semantic Template Continuous Final Contract

契约日期：2026-08-23
状态：Final contract candidate / Awaiting user confirmation / Not implemented
前置审计：`Docs/AUTOMATION-POST-HLI-0_SemanticHostPriorityCodeFactAudit.md`

## 1. 阶段目标

`CONTENT-1` 要把当前“已有 Section 中字段 Upsert/Replace”扩展为字段库驱动的当前文档内容创作能力：

```text
Field Registry snapshot
  -> GetFieldSchema
  -> ResolveReference
  -> structured CreateSection plan
  -> semantic Template validation / expansion
  -> canonical Preview + diagnostic delta
  -> existing IDE explicit Apply + one Undo unit
```

本连续包完成后，可以宣称：Agent 能在显式当前文档和字段库快照上查询字段 schema、解析引用、
生成新 Section/模板计划、获得确定性 Preview，并经既有 IDE 权限边界显式应用。仍不能宣称支持
跨文件原子提交、自动 Save、独立 Agent Host、素材生成或任意对象模板库。

本契约把前置审计中的 1A..1D 细分为 1A..1F，以隔离 public query、Preview engine、模板模型
和 IDE Host 风险；路线方向不变。

## 2. 总体风险与授权

| 子阶段 | 风险 | 原因 | 实施授权 |
|---|---|---|---|
| CONTENT-1A | R2 | additive Experimental Field Schema public DTO/method/capability | 确认本契约后允许 |
| CONTENT-1B | R2 | additive Experimental Reference Resolve public DTO/method/capability | 确认本契约后允许 |
| CONTENT-1C | R3 | 扩展 canonical EditPlan/Preview engine 支持 Section creation | 确认本契约后允许，必须阶段审查 |
| CONTENT-1D | R3 | 新 internal template domain/compiler 与字段信任策略 | 确认本契约后允许，public API 0 change |
| CONTENT-1E | R2/R3 | Template discovery/expansion public API + 首个真实模板 gate | 确认本契约后允许；模板来源不可靠时停止 |
| CONTENT-1F | R3 | 既有 AI/Workspace/Host consumer 接入；不改变 Apply authority | 确认本契约后允许，UI 视觉改动另行审批 |

Governance mode：连续实施时使用 Deferred；每个子阶段记录 checkpoint，1C、1E、1F 完成后和
整个包停止时刷新 Stage Ledger、PublicApiLedger、DecisionLog、CurrentPhase 与 Compact Context。

下列事项属于 R4，明确不由本契约授权：wire/JSON/IPC、模板持久化格式、字段库 ownership/priority、
项目快照、多文档原子提交、Save/Backup/Rollback、外部工具权限和素材 Artifact commit。

## 3. 不可破坏的不变量

### 3.1 事实源

- 当前 INI 文本是文档事实源；模板、计划、索引和 Preview 都是派生物。
- 字段事实来自捕获后的 effective provider，优先级继续是 `Project > Global > BuiltIn`。
- 每项查询/计划必须保留 `DocumentId + Version + FieldRegistryRevision`。
- 字段库刷新后，旧 plan/Preview 必须 stale；不得重新绑定到新 provider。
- 不创建第二份字段 schema、reference catalog、parser、diagnostics 或 change planner。

### 3.2 权限

- Application 只 Query、Validate、Expand 和 Preview，不持有活动编辑器或磁盘。
- IDE Workspace 继续持有 active preview、single-use admission 和 explicit confirmation。
- TransactionPort 继续是 Apply/Undo 唯一写入端口。
- Save/Backup/Rollback 继续由用户和既有保存链拥有；任何 CONTENT Apply 都不自动 Save。

### 3.3 失败原子性

- 所有失败结果无 partial fact、partial plan、candidate text 或可应用 change。
- 多 Section/多字段模板要么形成一个完整 plan/Preview，要么完整失败。
- Apply 成功后形成一个语义 Undo 单元；任何 stale、blocked、dismissed 或 failed proposal 不改文本。

### 3.4 数据所有权

| 数据 | Primary owner | Lifetime | Serialization |
|---|---|---|---|
| Effective Field Registry | IDE captured provider snapshot | revision-bound readonly snapshot | 非 wire，不序列化 provider |
| Schema/Resolve facts | Application query result | 单次调用 immutable derived fact | CONTENT-1 不承诺 wire shape |
| Template definition/catalog | Application authoring definition | process/catalog lifetime，immutable | v1 internal definition 不持久化 |
| Template arguments/instance | caller request | 单次 expansion | 仅 CLR contract，不承诺 JSON |
| EditPlan/Preview | Application semantic authoring | document/version/revision-bound | 进程内 Experimental contract |
| Active preview/Apply | IDE Workspace/TransactionPort | single-use active editor lifetime | 不序列化、不跨进程 |

### 3.5 资源上限

- document 继续使用 8 MiB characters；diagnostic/result 基线继续使用 10,000；plan total work 128；
- 一个 Field Schema 最多 1,024 个 AllowedValues、256 个 aliases，所有 schema display/description/
  alias/value 文本合计最多 64 KiB；任何单个 key/alias 仍不超过 256，单个 value 不超过 8,192；
- ResolveReference target token 不超过 256，ReferenceList 最多检查 10,000 个 token；
- Template catalog 最多 256 项，每项最多 64 个参数；一次 expansion 最多 64 个 arguments，单值
  不超过 8,192，最终 section+field work 仍不得超过 128；
- 超限必须返回 typed failure 且无 partial payload；不得静默截断 schema、reference 或 template plan。

## 4. 代码事实与复用裁决

| 能力 | Canonical implementation | CONTENT 决策 |
|---|---|---|
| Effective field lookup | Core `IRa2FieldDefinitionProvider` | 直接查询 captured provider；不访问 runtime singleton |
| Field trust | Application `Ra2FieldTrustClassifier` | 唯一 trust mapping；复用现有 public automation trust enum |
| Section parse/classify | Application parser + semantic model + classifier | 复用；模板不得强制改写 parser truth |
| Known references | `Ra2DocumentSemanticModelBuilder` + `Ra2ReferenceFinder` | 复用 Weapon/Projectile/Warhead 等现有语义识别 |
| Generic reference declaration | Field Registry `Reference/ReferenceList` | 只作为 schema evidence；目标 kind 不得猜测 |
| Field edit Preview | `Ra2AutomationEditPreviewEngine` | 扩展为唯一 Section-create Preview engine，不建第二套 engine |
| Text insertion/newline | `Ra2LineInsertionPrimitive` / current text model | 扩展或复用 internal primitive；不拼装无坐标 raw patch |
| Diagnostics delta | canonical document diagnostics + delta calculator | 全候选文本重新分析；规则不复制、不降级 |
| Apply | Workspace + TransactionPort | 无第二 preview store、apply service 或 save path |

当前字段库能表达 `ValueKind=Reference/ReferenceList`，但不能表达引用目标的
`Ra2SectionKind`。现有 semantic model 对已知武器链使用代码规则识别目标。因此 CONTENT-1B 必须
返回 resolution basis；字段库声明的通用引用允许 `TargetSectionKind=Unknown`，不得由字段名猜测。

## 5. 字段可信度与模板写入策略

`GetFieldSchema` 总是如实返回有效字段定义和 trust，不因 authoring policy 隐藏结果。模板扩展采用
比手工字段 Upsert 更严格的 disposition：

| Trust level | Schema query | Template disposition | 规则 |
|---|---|---|---|
| Verified | 返回 | Normal | 可正常进入 plan |
| ManualCurated | 返回 | Normal | 可正常进入 plan |
| Inferred | 返回 | Caution | 允许 Preview，必须产生 warning |
| AutoExtracted | 返回 | Caution | 允许 Preview，必须产生 warning |
| Unknown | 返回 | Caution | 允许 Preview，必须产生 warning |
| VerifiedGuardrail | 返回 | Blocked | 表示上下文保护，不自动写入目标 kind |
| Obsolete | 返回 | Blocked | 不自动新增废弃字段 |
| NonExistent | 返回 | Blocked | 不进入 plan |
| PseudoField | 返回 | Blocked | 不作为普通字段写入 |

字段库不声明“必填字段”、完整对象结构、Section 创建顺序或模板默认值；这些只能由显式 Template
Definition 决定。不得根据 `GetFields` 自动推断完整模板。

## 6. CONTENT-1A — Field Schema Query

### 6.1 目标

增加 current-snapshot `GetFieldSchema(sectionKind, key)`，把 effective `Ra2FieldDefinition` 投影为
immutable、UI-neutral、Agent-safe fact。查询不修改 provider、文档或 diagnostics。

### 6.2 Public API 候选（全部 Experimental）

```text
Ra2AutomationFieldSchemaQuery
Ra2AutomationFieldSchemaQueryFailureKind
Ra2AutomationFieldSchemaFact
Ra2AutomationFieldSchemaQueryResult
Ra2AutomationFieldAuthoringDisposition
```

扩展：

```text
IRa2AutomationDocumentQueryService.GetFieldSchema(snapshot, request, cancellationToken)
IRa2AutomationCapabilityGateway.GetFieldSchema(snapshot, request, cancellationToken)
Ra2AutomationCapabilityIds.DocumentFieldSchemaGet
  = "ini.document.field-schema.get"
```

catalog 顺序在既有四项之后追加；`CurrentVersion` 保持 1，risk=`Query`，stability=`Experimental`，
document limit 继续 8 MiB，maximumResultItems=1。

### 6.3 Query 与 Fact

Query：

- `Ra2SectionKind SectionKind`；
- bounded non-empty `string Key`，沿用 256 字符和禁止换行/NUL/`=` 的标识约束。

Fact 最小冻结字段：

- `Key`、`SectionKind`、`AppliesTo`；
- `FieldEditorKind EditorKind`、`Ra2FieldValueKind ValueKind`；
- `Ra2FieldBooleanValueStyle BooleanStyle`；
- `AllowedValues`（raw value，保持 provider 顺序并 defensive copy）；
- `EnumName`、`Separator`；
- `DisplayName`、`Description`、`Aliases`；
- `Ra2FieldSourceKind SourceKind`；
- 现有 `Ra2AutomationFieldTrustLevel TrustLevel`；
- `Ra2AutomationFieldAuthoringDisposition AuthoringDisposition`。

不返回 provider 实例、pack 路径、runtime singleton 或“精确 Project/Global/BuiltIn provenance scope”；
当前 snapshot 只证明优先级已经生效，并不携带 provenance query contract。

FailureKind 精确为：

```text
None
DocumentTooLarge
NotFound
ResultLimitExceeded
Canceled
AnalysisFailed
```

### 6.4 验收门禁

- 覆盖九种 trust -> disposition；
- Project/Global/BuiltIn 使用 composite provider fixture 证明 effective winner，不修改优先级；
- allowed values/aliases/AppliesTo defensive copy 和稳定顺序；
- NotFound 失败无 Fact；
- identity/version/revision 回传；
- cancellation、8 MiB boundary、determinism、parallel query；
- nested schema count/text budget 超限时 ResultLimitExceeded 且无 Fact；
- Gateway/direct service parity；
- Application exported allowlist 35 -> 40；Gateway catalog 4 -> 5、interface methods 5 -> 6；
- 完整既有 Application 和 non-UI 回归通过。

## 7. CONTENT-1B — Reference Resolution Query

### 7.1 目标

增加按 `Section + occurrence + Key + field occurrence + reference index` 的 current-document
ResolveReference。它解析“这个字段当前指向什么”，不同于 FindReferences 的“谁指向这个目标”。

### 7.2 Public API 候选（全部 Experimental）

```text
Ra2AutomationReferenceResolveQuery
Ra2AutomationReferenceResolveFailureKind
Ra2AutomationReferenceResolutionBasis
Ra2AutomationReferenceResolutionFact
Ra2AutomationReferenceResolveResult
```

扩展：

```text
IRa2AutomationDocumentQueryService.ResolveReference(...)
IRa2AutomationCapabilityGateway.ResolveReference(...)
Ra2AutomationCapabilityIds.DocumentReferenceResolve
  = "ini.document.reference.resolve"
```

catalog 追加为第六项；risk=`Query`，document limit 8 MiB，maximumResultItems=1。

### 7.3 定位与解析规则

Query：

- `SectionName`；
- nullable zero-based `SectionOccurrence`，null 要求唯一；
- `Key`；
- nullable zero-based `FieldOccurrence`，null 要求唯一；
- zero-based `ReferenceIndex`，默认 0。

解析顺序：

1. 用 canonical semantic model 定位 Section/Field；
2. 若已有 `Ra2ValueReferenceSymbol`，basis=`SemanticKnown`，保留其 target kind；
3. 否则查询 captured Field Registry；只有 ValueKind 为 Reference/ReferenceList 才继续；
4. 按 schema separator 和 canonical effective-value/comment rules 提取指定 token；
5. basis=`FieldSchemaDeclared`；若当前文档存在同名唯一 Section，可返回其实际 kind；否则 kind=Unknown；
6. 不把 schema-assisted 结果写回 global semantic model，不改变 FindReferences/Diagnostics 行为。

ResolutionFact：

- source section/key/occurrences/line/span；
- raw effective token、reference index；
- target section name/kind；
- basis；
- `IsTargetDefined`、`TargetDefinitionCount`；
- `IsSchemaDeclaredReference`。

目标缺失仍是成功解析：返回 target name 且 `IsTargetDefined=false`。调用方可据此提出创建目标，
不得把“目标不存在”与“无法理解来源字段”混为同一失败。

FailureKind 精确为：

```text
None
DocumentTooLarge
SectionNotFound
AmbiguousSection
FieldNotFound
AmbiguousField
UnsupportedReference
EmptyReference
ReferenceIndexOutOfRange
ResultLimitExceeded
Canceled
AnalysisFailed
```

### 7.4 验收门禁

- semantic-known Weapon/Projectile/Warhead parity；
- schema-declared generic Reference 返回 Unknown target kind，不猜测；
- ReferenceList index、separator、inline comment、空 token和越界；
- target/token/list budget 超限且无 partial fact；
- duplicate Section/Key occurrence；
- missing target success、duplicate target count；
- 不改变现有 `FindReferences`、reference/chain diagnostics 数量与结果；
- Gateway/direct service parity、limits/cancel/determinism/thread safety；
- exported allowlist 40 -> 45；catalog 5 -> 6、Gateway methods 6 -> 7；
- 完整回归通过。

## 8. CONTENT-1C — Section Creation Preview Primitive

### 8.1 目标与模型

扩展现有 `Ra2AutomationEditPlan`，使同一个 plan 可以声明要创建的新 Section，并让既有
`Ra2AutomationEditOperation` 对这些 Section 执行 `UpsertField`。不新增第二种 Preview/ChangeSet。

新增 public Experimental 类型：

```text
Ra2AutomationSectionCreateOperation
Ra2AutomationSectionCreatePreview
```

`Ra2AutomationEditPlan`：

- 保留现有构造器，委托到 `SectionCreations=[]`，既有 caller 零行为变化；
- 增加 additive overload 和 immutable `SectionCreations`；
- total work budget = section creations + field operations，仍不得超过 128；
- plan 的 identity/version/registry revision 规则不变。

`Ra2AutomationEditPreviewResult` 增加 immutable `SectionCreationPreviews`；失败时仍必须为空。

### 8.2 SectionCreateOperation

- `SectionName`：同现有 Section 标识约束，case-insensitive identity；
- `ExpectedSectionKind`：用于字段 schema 校验和风险证据，不覆盖 parser/classifier truth；
- 手工结构化 Section creation 可使用 Unknown，但必须进入 Caution；Template Definition 不得以
  Unknown 逃避字段适用性校验；
- v1 固定 placement 为 EndOfDocument，不公开 speculative placement enum；
- fields 继续由普通 Upsert operations 表达，Section creation 不接收 raw body text。

### 8.3 确定性插入规则

- 目标名称已存在（忽略大小写）则完整失败；
- 同一 plan 重复声明 Section 则完整失败；
- 所有 Section creation 按 plan 顺序追加；每个 Section 内字段按 operation 顺序；
- 非空文档与首个新 Section 之间至少一空行；已有更多尾部空行时不得删除；
- 多个本次新建 Section 之间恰好一空行；
- 保留原文全部字符，不清理已有尾部空白或换行；只为新增内容补足最少 separator；
- newline 使用 canonical current-document newline policy；空文档也产生完整 header/field 行；
- 每个新 Section 结束于一个 newline；不改变 encoding/BOM/Save 行为。

### 8.4 分类与信任

- fields 使用 `ExpectedSectionKind` 查询 effective schema 和 trust；
- candidate 完成后仍运行 canonical classifier/diagnostics；
- actual kind 已知且与 expected kind 冲突：失败 `SectionClassificationMismatch`；
- actual kind 为 Unknown：Preview 可成功，但 section preview 标记 classification unresolved，
  IDE apply policy 至少为 Caution；不得伪造已分类事实；
- Blocked disposition 字段使计划失败；Caution disposition 形成 evidence/warning。
- 上述 Blocked/Caution 强策略只作用于本 plan 新建 Section 和模板扩展；既有 Section 上的普通
  Upsert/Replace 保持当前 A4 Preview/Apply policy，不产生兼容性收紧。

在现有 Preview failure enum 末尾 additive 增加：

```text
SectionAlreadyExists
ConflictingSectionCreations
SectionClassificationMismatch
BlockedFieldTrust
```

不得重排现有数值。

### 8.5 验收门禁

- existing field-only plan byte-for-byte/parity 不变；
- empty/non-empty/CRLF/LF/no-final-newline/multiple-section golden tests；
- case-insensitive conflicts、duplicate creates、undeclared target Section；
- existing/new Section mixed operations；
- blocked/caution trust；
- candidate text = ordered change set apply result；
- diagnostic delta、cancel、limits、stale/no-partial；
- IDE `FromAutomation` 验证 section preview/span/candidate closure；
- exported allowlist 45 -> 47；Gateway capability 数不变；
- Application、Host boundary、Workspace 和完整 non-UI 回归通过。

## 9. CONTENT-1D — Internal Template Domain and Compiler

### 9.1 所有权

模板定义属于 Application authoring definition，不属于 Field Registry、Provider DTO、UI 或磁盘。
首版全部 internal，不计入 exported allowlist，不承诺 JSON/YAML 文件格式。

internal model：

```text
Ra2ContentTemplateDefinition       Id + Version + DisplayName + Parameters + Sections
Ra2ContentTemplateParameter       Name + Kind + Required + bounded Default
Ra2ContentTemplateSectionSpec     SectionNameSource + ExpectedKind + Fields
Ra2ContentTemplateFieldSpec       Key + ValueSource
Ra2ContentTemplateValueSource     Literal | Parameter
Ra2ContentTemplateCompiler        definition + arguments + snapshot -> edit plan/result
```

参数 Kind 首版：Identifier、String、Integer、Float、Boolean、Reference。只允许 Literal/Parameter
绑定，不引入表达式语言、脚本、字符串插值、文件读取或网络访问。需要派生名称时由调用者提供显式参数。

### 9.2 编译规则

- template ID 使用 ordinal stable ID，version 为正整数；同 catalog ID/version 唯一；
- arguments 名称 ordinal、禁止未知/重复参数；required/default 规则确定；
- identifier 使用 Section/Key 边界；数值/布尔先做语法验证，再由字段 schema 验证；
- 每个字段必须通过 expected kind 的 GetFieldSchema 等价 lookup；NotFound 不自动降级为 Unknown field；
- Blocked disposition 使 expansion 失败；Caution 形成 typed warning；
- compiler 只生成绑定 snapshot 的 `Ra2AutomationEditPlan`，不直接 Preview/Apply；
- 相同 definition/arguments/snapshot 得到语义等价且顺序一致的 plan；除 PlanId 外不得有随机差异；
- 模板 definition 不成为 INI 事实源，不能覆盖 parser/classifier/diagnostic 结果。

### 9.3 首个模板选择 gate

1D 使用 test-owned deterministic definitions 验证 compiler，但不得把测试 fixture 宣称为产品模板。
1E 开始前必须对首个真实 BuiltIn template 做来源与用途审计，确认：

- 是真实 RA2/YR/Ares/Phobos authoring workflow，不是 Mock；
- 所有固定字段与默认值有现有字段库/source-backed 证据；
- 不要求跨文件事务、素材生成、类型列表自动编号或未知 target-kind 推断；
- 若没有满足条件的模板，CONTENT 包在 1E-0 停止并提交审计结果，不修改 engine contract。

候选 `Weapon/Projectile/Warhead` 链仅是审计候选，不在本契约中预先认定字段/default 正确。

### 9.4 验收门禁

- model invariants/immutability/identity/version；
- missing/unknown/duplicate arguments；
- type/bounds/defaults；
- deterministic ordering；
- field NotFound、Blocked、Caution；
- multiple Section plan、conflict预检和128 total budget；
- public API/serialized shape/project files 零 diff。

## 10. CONTENT-1E — Template Discovery and Expansion Gateway

### 10.1 进入条件

- 1A..1D 全部门禁通过；
- 1E-0 首个真实模板来源审计通过；
- 模板不需要 R4 能力；否则停止，不用 generic/raw template 代替。

### 10.2 Public API 候选（全部 Experimental）

```text
IRa2AutomationTemplateService
Ra2AutomationTemplateService
Ra2AutomationTemplateDescriptor
Ra2AutomationTemplateParameterDescriptor
Ra2AutomationTemplateParameterKind
Ra2AutomationTemplateArgument
Ra2AutomationTemplateExpansionRequest
Ra2AutomationTemplateExpansionResult
Ra2AutomationTemplateExpansionFailureKind
Ra2AutomationTemplateWarningKind
Ra2AutomationTemplateWarningFact
```

Service/Gateway：

```text
GetTemplates()
ExpandTemplate(snapshot, request, cancellationToken)
```

Capability ID：

```text
ini.content.template.expand
```

`GetTemplates()` 与 `GetCapabilities()` 一样属于 discovery metadata，不登记为可执行 capability，
因此不需要伪造 document limit。Expand risk=`Edit`，但 Edit 只表示生成绑定 plan，不授予 Apply。
catalog 保持 immutable 固定顺序；existing six capabilities 不重排，只追加 Expand 一项。

Descriptor 只公开 ID/version/display/summary/parameter descriptors，不公开 internal definition。
Expansion success 返回绑定 snapshot 的 `Ra2AutomationEditPlan` 和 typed warnings；failure 无 plan。

FailureKind：

```text
None
TemplateNotFound
TemplateVersionMismatch
InvalidArguments
MissingRequiredArgument
UnknownArgument
DuplicateArgument
FieldSchemaNotFound
BlockedFieldTrust
OperationLimitExceeded
DocumentTooLarge
Canceled
ExpansionFailed
```

### 10.3 验收门禁

- catalog immutability/order/unique identity；
- public descriptor 与 internal definition 隔离；
- request defensive copy、unknown/duplicate/missing/version；
- direct service/Gateway parity、cancel/limits/thread safety/determinism；
- expansion plan 再经现有 Gateway Preview 成功形成同一 candidate；
- warnings 不被 message-string 解析；
- allowlist 47 -> 58；Gateway catalog 6 -> 7、methods 7 -> 9；
- 不出现 wire/serialization attribute、filesystem/template persistence 或 provider DTO。

## 11. CONTENT-1F — IDE Agent Integration

### 11.1 目标

让内置 AI 在明确的模板/新 Section 请求中使用 Template discovery/expansion，再把返回的既有
EditPlan 交给同一 Gateway Preview、proposal card、Workspace 和 explicit Apply。

```text
explicit content request
  -> current snapshot + budget preflight
  -> required structured template tool
  -> local TemplateService.Expand
  -> Gateway.Preview
  -> existing proposal lifecycle
  -> explicit Apply
  -> one Undo + Problems refresh
  -> no Save
```

### 11.2 边界

- 复用现有 official/custom endpoint policy；custom endpoint 仍不能取得编辑工具权限；
- provider 只能返回 template ID/version/arguments，不能提交 raw Section body 或 candidate text；
- advisory、普通 field-edit 和 template-edit 路由必须互斥；一次 response 仍只接受一个 authoring tool；
- template expansion warning 进入现有 Caution policy；Blocked 不创建 proposal；
- Workspace/TransactionPort/Save 零 public change；
- Shell XAML、Dock、工具栏和布局零 diff；
- 若现有 proposal list 无法表达 Section creation，只允许 ViewModel projection 增加“创建 Section”行；
  任何 XAML/视觉重构必须另行生成 UI contract 并等待审批。

### 11.3 验收门禁

- deterministic local provider loopback：template tool -> expand -> preview；
- stale document/registry、template version mismatch、blocked trust、caution、cancel；
- Apply single-use、one Undo、Dirty、Problems refresh、no automatic Save；
- ordinary Upsert/Replace flow parity；
- custom endpoint/advisory 不获得 template tool；
- Application public allowlist 保持 58；
- focused integration、完整 non-UI、clean package；
- 不使用真实付费模型作为必选门禁，真实 DeepSeek 仅用户可选手工验收。

## 12. 预计文件边界

### 12.1 允许的 production 区域（实施时按子阶段精确收窄）

```text
RA2IniEditor.Application/Automation/Experimental/*
RA2IniEditor.Application/Editing/*
RA2IniEditor.Application/Content/Templates/*          (new internal folder)
RA2IniEditor.IDE/AI/*                                 (1F only)
RA2IniEditor.IDE/Editing/Ra2IniEditPreview*.cs        (projection only)
RA2IniEditor.IDE/ViewModels/AI/*                      (1F projection only)
RA2IniEditor.Application.Tests/*
RA2IniEditor.Tests/IDE/*                              (focused boundary tests)
Docs/*                                                (contract/ledger/status)
```

### 12.2 禁止区域

```text
ShellWindow.xaml
ShellWindow.xaml.cs，除非后续 1F 审计证明无法避免且另行明确批准
Field Registry JSON/packs/priority/load/apply/rollback
Core parser/serializer/save semantics
Diagnostics rule behavior
Completion/Hover/Quick Peek
Search
project files / dependencies
legacy solution/editor
wire/CLI/IPC/MCP
assets/job/runtime/test-host
```

若实施需要越过禁止区域，当前子阶段停止并提交 Design Review，不以测试失败为由扩大范围。

## 13. 子阶段执行与审查顺序

每个阶段采用：

```text
code-fact recheck
  -> exact Task Card / API diff
  -> implementation
  -> targeted tests
  -> public/reflection/diff audit
  -> Stage Checkpoint
  -> next stage
```

连续顺序：

```text
1A Field Schema Query
-> 1B Reference Resolve
-> 1C Section Creation Preview
-> 1D Internal Template Domain/Compiler
-> 1E Template Gateway + first real template gate
-> 1F IDE Agent Integration
```

在用户确认本最终契约后，1A..1F 可连续推进，不需要每阶段再次等待批准；但以下 stop condition
仍强制停止：

- public type/method/count 与本契约不一致；
- 需要修改 parser、diagnostics、Field Registry priority/data、Save 或 Shell UI；
- 1E 无法选出 source-backed 真实模板；
- 任何 required build/test/reflection/parity gate 失败且修复超出本阶段；
- 实现需要 wire、persistence、multi-file、raw patch 或外部付费调用；
- 实际风险升为 R4。

## 14. 验证矩阵

各阶段最小命令：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter <stage focused filter>
```

1F/package stop point额外：

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

| Gate | 1A | 1B | 1C | 1D | 1E | 1F |
|---|---:|---:|---:|---:|---:|---:|
| Build | Required | Required | Required | Required | Required | Required |
| Application targeted/full | Required | Required | Required | Required | Required | Required |
| Gateway parity/reflection | Required | Required | Required | No public diff | Required | Required |
| Existing semantic parity | Required | Required | Required | Required | Required | Required |
| Host/Workspace focused | NotRun | NotRun | Required | NotRun | Preview only | Required |
| Full non-UI | Package checkpoint | Package checkpoint | Required | Package checkpoint | Required | Required |
| Clean package | NotRun | NotRun | Checkpoint | NotRun | Checkpoint | Required |
| Computer/UI smoke | NotRun | NotRun | NotRun | NotRun | NotRun | User optional only |

不通过的 required gate 不能以较弱测试替代。测试数量只记录实际结果，不在契约中预写虚假计数。

## 15. Public API Ledger 预登记

| Stage | API group | Reason | Next use | Stability | Compatibility |
|---|---|---|---|---|---|
| 1A | FieldSchema query/fact/result/disposition | Agent/template 读取 effective schema | 1C/1D/Host | Experimental | additive；allowlist 40 |
| 1B | ReferenceResolve query/fact/result/basis | Agent理解字段当前目标 | 1D/Asset binding | Experimental | additive；allowlist 45 |
| 1C | SectionCreate operation/preview + plan/result additions | Template 编译到唯一 Preview | 1D/1E | Experimental | additive overload/property/enum tail；allowlist 47 |
| 1D | None | internal-first model/compiler | 1E | Internal | public diff 0 |
| 1E | Template service/descriptor/request/result/warnings | Gateway discovery/expansion | 1F/Host | Experimental | additive；allowlist 58 |
| 1F | None | IDE consumer 接入既有 APIs | product loop | Internal | public diff 0 |

数字以精确类型清单为约束。若实现发现某个新增 public type 没有明确 1C/1D/1F/Host 消费者，
应保持 internal 并相应下调 allowlist；不得为满足预写数字制造 public API。任何调整必须先更新
Task Card 和 ledger proposal，再实施。

## 16. 明确后置到 CONTENT-2 / HOST / ASSET

- Reference target-kind schema enrichment；当前字段库没有该维度；
- project snapshot、cross-file Find/Resolve/Rename；
- type-list registration、数字索引分配和多文件对象创建；
- RenameSymbol、SetReference 的跨文件原子变更；
- 用户模板持久化、导入/导出、签名和迁移；
- wire/CLI/IPC/MCP/session/permission；
- Artifact plan、Icon/VOX/SHP provider 和 INI asset binding；
- Job/Event Runtime 和 AssemblyGraph。

素材阶段开始前必须至少完成与首个素材类型有关的 target-kind/binding semantic contract；不能让
Asset Provider 直接写 `Image=`、Art Section 或文件路径来绕过 CONTENT Preview。

## 17. 完成定义

只有全部满足时，`CONTENT-1` 才能标记 Completed：

1. 1A..1F 所有 required gates 通过；
2. Field Registry priority/data/load semantics 零变化；
3. schema/resolve/create/template 均通过 explicit snapshot/revision；
4. current-document template request 能形成真实 plan、Preview、diagnostic delta 和 proposal；
5. explicit Apply 为一个 Undo 单元，成功后 refresh，不自动 Save；
6. ordinary Upsert/Replace、FindReferences、Diagnostics 行为保持兼容；
7. Application public surface 与 ledger 最终清单一致；
8. legacy、Shell layout、XAML、依赖和项目文件未恢复/未扩张；
9. Stage Ledger、DecisionLog、PublicApiLedger、CurrentPhase、Compact Context 与 clean package 收口；
10. 不能把 test template、Unknown target kind 或 slice output 夸大为完整对象/素材生产。

## 18. 契约自审结论

- Reuse：通过。字段库、semantic model、reference finder、Preview、diagnostics、Workspace 均为唯一复用路径。
- Data ownership：通过。provider snapshot、template definition、instance、plan、Preview 和 Apply authority 分离。
- Public API：受控。1A/1B/1C/1E additive Experimental；1D/1F internal-first/zero diff。
- Compatibility：通过。既有构造器和 field-only plan 保留；enum 只尾部追加。
- Failure atomicity：通过。任何失败无 partial plan/candidate/apply payload。
- Field Registry honesty：通过。不把字段库当对象模板，也不猜测 reference target kind/provenance。
- UI/Shell：通过。CONTENT 核心不依赖 WPF；1F 不批准视觉重构。
- Anti-rework：通过。先查询事实，再扩展 canonical plan，最后模板/Host consumer；不提前冻结 wire/persistence。
- Remaining controlled risk：首个真实模板的具体内容必须在 1E-0 来源审计中选择；这是数据包选择，
  不改变已冻结的 engine/public contract。

本契约确认前不得实施。确认后从 `CONTENT-1A Field Schema Query` 开始连续推进。
