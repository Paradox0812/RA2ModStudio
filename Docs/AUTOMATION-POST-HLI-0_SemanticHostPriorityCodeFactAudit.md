# AUTOMATION-POST-HLI-0 Semantic / Host Priority Code-Fact Audit

审计日期：2026-08-23
状态：Completed / Read-only code-fact and priority audit
前置证据：`Docs/AUTOMATION-HLI-2C_StageLedger.md`

## 1. 审计目标与边界

本审计在 Minimum HLI-v1 完成后，对以下两个候选方向做代码事实和返工风险比较：

1. 独立 Agent Host / CLI / IPC；
2. `CONTENT-1` 语义对象与模板层。

用户已确定总体顺序为“先完成语义侧，再完成素材侧”。本轮只裁决优先级、复用路径、
阶段边界和下一安全入口，不实现 Host、模板、素材、public API、wire protocol 或生产代码。

本轮风险为 `R0 / DocsOnly`。未来 `CONTENT-1` 实现预计为 `R2/R3`；独立 Host 涉及跨进程
身份、权限、序列化和提交授权，预计为 `R4`。

## 2. 已确认的当前基线

### 2.1 Minimum HLI-v1 已完成的能力

当前 `IRa2AutomationCapabilityGateway` 是 `RA2IniEditor.Application` (`net8.0`) 中的进程内
Experimental typed façade，精确提供：

```text
GetCapabilities
GetSection
FindReferences
Validate
Preview
```

对应 catalog 只有四项 capability：Section query、current-document references、document
diagnostics 和 current-document edit preview。HLI-2C 已证明内置 IDE 可以完成：

```text
Query / Validate
  -> provider structured field plan
  -> Gateway Preview
  -> explicit user Apply
  -> one Undo unit
  -> updated-document diagnostics
```

Apply 仍属于 IDE `Ra2IniAuthoringWorkspace` + `IRa2EditorTransactionPort`，Save/Backup/Rollback
仍属于既有保存链。Gateway 不拥有活动编辑器、磁盘或 WPF 生命周期。

### 2.2 当前公开契约不是 wire DTO

`Ra2AutomationDocumentSnapshot` 直接持有 `Ra2AutomationFieldRegistrySnapshot`，后者又直接
持有进程内 `IRa2FieldDefinitionProvider`。这对同进程、显式捕获的只读事实是正确的，但不能
直接 JSON 序列化或跨进程传输。

因此当前 public surface 是 headless in-process API，不是 CLI/IPC/MCP/JSON 协议。任何独立
Host 都必须先定义自己的 capture/session/permission/wire boundary，不能把现有 CLR 对象图
直接宣布为传输协议。

## 3. 独立 Agent Host 代码事实审计

### 3.1 可直接复用的基础

- `RA2IniEditor.Application` 已为 `net8.0`，可由非 WPF 宿主引用；
- Gateway 无状态、线程安全、只委托唯一 Query/Preview services；
- descriptor 已包含 capability ID、version、risk、stability 和资源上限；
- request/result 已绑定 DocumentId、Version 和 Field Registry Revision；
- typed failure、取消、大小/结果/操作数限制已经存在；
- provider JSON 到受限 plan 的防御性校验和 IDE explicit Apply 生命周期已有实现证据。

这些事实说明未来 Host 无需重写 parser、diagnostics、reference finder 或 edit planner。

### 3.2 当前不存在的 Host 能力

生产源码中没有 Named Pipe、JSON-RPC、stdio command protocol、MCP、HTTP/WebSocket server、
CLI executable 或独立 Agent executable。Solution 唯一产品 executable 仍是 WPF `WinExe`。

还不存在以下外部边界：

- Host session / caller identity / project binding；
- capability permission grant 与 explicit Apply confirmation handshake；
- snapshot capture、registry facts 传输和 stale renewal；
- wire DTO/version negotiation/error envelope；
- endpoint authentication、local-only policy、request size 和 concurrency policy；
- audit/cost/token/event/job/artifact protocol；
- IDE 与外部进程之间的 Apply/Undo/Save ownership bridge。

### 3.3 现在先做 Host 的返工风险

当前 Host 若立即冻结 wire，只能可靠暴露四项 HLI-v1 capability，并且编辑仅支持现有 Section
中的 `UpsertField` / `ReplaceFieldValue`。它不能表达新 Section、对象模板、引用设置、重命名、
多文档计划或素材绑定。

这会导致两类可预见返工：

1. `CONTENT-1` 增加语义能力后，Host catalog、wire request/result、权限分类和版本协商必须
   再扩展一次；
2. 若为规避扩展而提前设计 generic command/raw patch，反而会破坏 typed capability、Preview、
   diagnostic delta 和 Host-owned Apply 边界。

结论：独立 Host 具备底层可行性，但当前不是最高价值、最低返工的下一纵向切片。

## 4. CONTENT-1 代码事实审计

### 4.1 可复用的语义基础

`CONTENT-1` 不需要重建 INI 语义栈，可复用：

| 目标能力 | 现有唯一基础 | 当前覆盖 |
|---|---|---|
| Section 查询/分类 | Application semantic model + classifier + `GetSection` | 已有 Section 可查，重复 occurrence 有失败语义 |
| Field Schema 查询 | Core `IRa2FieldDefinitionProvider` / `Ra2FieldDefinition` | 算法与数据完整，但 Gateway 尚无中立 schema fact/result |
| 引用识别/查找 | semantic model references + `FindReferences` | 当前文档、以 offset/selection 解析目标并查引用 |
| 引用校验 | reference catalog + reference/chain diagnostics | 有内部 current/project catalog 基础，无 public resolve-by-section/key API |
| 字段编辑 | `UpsertField` / `ReplaceFieldValue` + unique Preview engine | 只支持已存在且唯一的 Section |
| 变更安全 | snapshot currency、diagnostic delta、trust evidence、limits | 已完成并由 Gateway 复用 |
| IDE 提交 | Workspace active preview + single-use Apply + transaction/Undo | 可继续作为唯一 Host authority，不应搬入模板层 |

### 4.2 当前确实不存在的语义模板能力

生产源码中不存在正式的 `ITemplateService`、语义 template/object specification、参数 schema、
template catalog/version/provenance 或 template expansion result。源码中的其他 `Template` 命中均为
WPF control template、提示词排版或 UI 名称，不能作为语义模板复用。

当前 edit operation enum 精确只有：

```text
UpsertField
ReplaceFieldValue
```

Preview engine 在目标 Section 不存在时返回 `SectionNotFound`。因此多项 Upsert 可以在一个已有
Section 中确定性插入字段，但不能创建新的 Section，也不能把对象模板实例化到空文档。

当前还缺：

- `GetFieldSchema` 的 Gateway-neutral fact/result；
- 按 section/key 解析引用的 `ResolveReference` query；
- `CreateSection` / `InsertSection` 语义 operation 和定位策略；
- template identity/version/parameters/defaults/provenance；
- template expansion -> bounded semantic operations 的确定性规则；
- Section 名冲突、重复 Section、缺字段/未知字段/低可信字段的 typed policy；
- 跨文件 project snapshot、multi-document preview 和原子提交；
- Artifact plan 与素材 binding（应后置，不塞入首个模板切片）。

### 4.3 与长期架构文档的重合和调整

外部架构文档 Phase 1 要求的 `GetSection`、`FindReferences`、`Validate/GetDiagnostics` 和字段
SetValue 基础已经由 HLI-v1 覆盖；`GetFieldSchema`、`ResolveReference`、`SetReference`、
`RenameSymbol`、`ApplyTemplate` 尚未覆盖。

需要调整原路线的地方：

- 不再新建一组平行的 Section/Reference/Diagnostics/Edit services；应扩展现有 Gateway 后面的
  canonical Application services；
- 不把 `ITemplateService` 一开始定义成文件写入或 IDE session service；它只负责纯函数式的
  template validation/expansion 和 Preview plan 生成；
- `SetReference` 首版应复用字段 schema + semantic edit operation，不建立 raw text 专用通道；
- `RenameSymbol` 和跨文件原子提交依赖 project snapshot，不能塞进 current-document 首切片；
- `Artifact plan` 属于语义与素材的连接层，应在模板 current-document vertical slice 稳定后再契约。

## 5. 优先级裁决

| 维度 | CONTENT-1 语义侧 | 独立 Agent Host |
|---|---|---|
| 对当前用户价值 | 直接扩大自然语言可完成的 INI 任务 | 主要改变调用位置，能力内容基本不变 |
| 现有实现复用率 | 高：schema/query/reference/diagnostics/preview/apply 均可复用 | 中：Gateway 可复用，但 transport/session/permission 全缺 |
| 下一阶段风险 | R2/R3 | R4 |
| 先做后的协议稳定性 | 为 Host 提供更完整、真实的 capability 面 | 会冻结不完整语义面并在 CONTENT-1 后扩展 |
| 对素材路线的前置价值 | 高：素材最终必须生成 Art/Rules 引用与 Section | 低到中：提供编排入口，但不能替代语义 binding |
| 返工概率 | 可通过小纵向切片控制 | 当前较高 |

最终顺序：

```text
CONTENT-1 语义能力
  -> 独立 Agent Host
  -> Asset/Artifact 基础
  -> Icon / VOX SliceStack / SHP
  -> 多产物 Assembly
```

这一定义只接受路线顺序，不提前批准任何 public/wire shape。

## 6. 推荐的连续阶段

### CONTENT-1A：Semantic Query Completion

目标：补齐 `GetFieldSchema` 与 `ResolveReference` 的 current-document typed query，继续使用显式
Document/Registry snapshot。此阶段只读，不新增模板写入、Apply/Save 或跨文件状态。

契约前必须裁决：schema fact 的最小字段、trust/provenance 投影、resolve success/target-missing/
ambiguous/unsupported failure、capability IDs/version 和 exported allowlist。

### CONTENT-1B：Template Domain Contract

目标：冻结中立 template identity/version/parameters/section specification/field specification、
provenance 和 typed validation failure。先采用代码内确定性 fixture/catalog 证明数据模型，不在首切片
引入用户模板持久化格式、网络 catalog 或 Provider DTO。

### CONTENT-1C：Current-Document Template Expansion Preview

目标：把一个 template instance 确定性展开为 bounded semantic plan，并支持创建一个新 Section
及其字段；复用唯一 Preview engine、diagnostic delta、trust evidence 和 limits。失败时无 partial payload。

只完成 Preview，不公开 Apply/Save，不做多文件事务。

### CONTENT-1D：IDE Host Apply Integration

目标：让既有 proposal/Workspace/transaction 路径消费 CONTENT-1C Preview；继续显式确认、单次 Apply、
一个 Undo 单元、Apply 后 diagnostics refresh、不自动 Save。

### HOST-1：Independent Agent Host

在 CONTENT-1A..1D 的 capability surface 与失败语义稳定后，单独冻结：

1. wire-neutral DTO 与版本协商；
2. caller/session/project binding；
3. read/query/preview 权限；
4. IDE-mediated explicit Apply handshake；
5. local transport、取消、并发、资源和审计边界。

首版 Host 仍不得拥有任意路径、raw patch、直接磁盘写入或 Save authority。

### ASSET-0 及后续

Host 和 current-document template slice 稳定后，再冻结 AssetRequest、ArtifactDescriptor、内容哈希、
Manifest、palette/profile 和受控 commit。随后按 `ASSET-ICON-1 -> ASSET-VOX-1 -> ASSET-SHP-1`
推进，所有 INI binding 必须回到 Semantic Edit Service/Preview，不由素材 provider 直接改文本。

跨文件 project snapshot、RenameSymbol、multi-document atomic commit、Job/Event Runtime 和 AssemblyGraph
应按真实消费者逐项引入，不与首个 CONTENT-1 模板切片捆绑。

## 7. Public API 候选裁决

本审计 public API 变更：**None**。

以下仅为后续契约候选，不代表名称、属性、序列化形状或兼容承诺已批准：

| 候选 | 当前状态 | 最早评审阶段 |
|---|---|---|
| Field schema request/result/fact/failure | Candidate / Not approved | CONTENT-1A |
| Resolve reference request/result/fact/failure | Candidate / Not approved | CONTENT-1A |
| Template definition/instance/parameter/failure | Candidate / Prefer internal-first | CONTENT-1B |
| CreateSection / ApplyTemplate preview operation/result | Candidate / Not approved | CONTENT-1C |
| wire DTO/session/permission/error envelope | Explicitly deferred | HOST-1 |
| Apply/Undo/Save public API | Rejected for Gateway | Host-owned |
| Artifact/Job/Event contract | Explicitly deferred | ASSET-0/AUTOMATION-1 |

## 8. 下一阶段允许与禁止

下一安全入口为 `CONTENT-1A Semantic Query Completion` 的代码事实回归和最终契约。

允许：

- 精确审计 field schema 与 reference resolve 的现有算法、调用方和测试；
- 设计最小 immutable typed facts、limits、failure semantics 和 capability descriptor；
- 评估 additive Experimental public API 与 allowlist；
- 制定 Application/Application.Tests 的最小实现与回归矩阵。

禁止：

- 在最终契约确认前修改 C#/项目文件；
- 直接实现 template persistence、CreateSection、独立 Host、wire、CLI 或素材代码；
- 公开 Core provider、SemanticModel、Workspace、transaction port 或 live editor；
- 把 Apply/Save 加入 Gateway；
- 复制 parser、reference、diagnostics 或 Preview 算法；
- 把项目文本搜索冒充语义引用查询。

## 9. 审计结论

先 CONTENT-1、后独立 Host、再素材，是当前代码事实下返工风险最低的路线。语义侧有高复用率，
且决定未来 Host 与素材 binding 的真实能力形状；独立 Host 当前缺少的主要是高风险 transport/
permission/session boundary，而不是 parser 或 planner。

本轮完成路线裁决和事实审计，没有改变 public API、生产行为、Shell、语义规则或持久化格式。
