# AUTOMATION-HLI-2A Capability Gateway Code-Fact Audit

审计日期：2026-08-22
状态：Completed / Read-only code-fact audit
前置证据：`Docs/AUTOMATION-HLI-1C_StageLedger.md`

## 1. 审计目标

核实 HLI-1A1、HLI-1A2、HLI-1B 与 HLI-1C 完成后，最小 Capability Gateway 应当复用
哪些现有入口、还缺少哪些生产类型，以及外部自动化架构路线中哪些职责可以进入
HLI-2A、哪些必须继续延后。

本轮只读取源码、测试和权威文档，未修改生产代码、测试、项目文件或 UI。

## 2. 已核实的程序集与依赖方向

```text
RA2IniEditor.Application (net8.0)
  -> RA2IniEditor.Core
  -> no WPF / IDE / Infrastructure / provider dependency

future caller / IDE adapter / CLI host
  -> Capability Gateway in Application
  -> existing typed Application services
  -> existing single semantic authority
```

`RA2IniEditor.Application/Automation/Experimental` 当前精确导出 29 个 public 类型，且只提供
两个高层服务接口：

- `IRa2AutomationDocumentQueryService`：`GetSection`、`FindReferences`、`Validate`；
- `IRa2AutomationEditPreviewService`：`Preview`。

对应 concrete service 均为 public sealed、无持久状态，并从调用方提供的 immutable snapshot
工作。Application 没有 mutable registry、session、active editor、文件系统或 provider 依赖。

## 3. 当前已存在的四项能力

| Capability ID（HLI-0B 文档基线） | 现有权威入口 | 输入 | 输出 |
|---|---|---|---|
| `ini.document.section.get` | `IRa2AutomationDocumentQueryService.GetSection` | document snapshot + section query | typed section result |
| `ini.document.references.find` | `IRa2AutomationDocumentQueryService.FindReferences` | document snapshot + reference query | typed reference result |
| `ini.document.diagnostics.validate` | `IRa2AutomationDocumentQueryService.Validate` | document snapshot | typed diagnostics result |
| `ini.document.edit.preview` | `IRa2AutomationEditPreviewService.Preview` | document snapshot + edit plan | typed preview result |

这些 ID 目前只存在于文档，还没有生产常量、descriptor 或 Gateway。现有 service/result
已经提供 capability-specific typed failure、取消结果、不可变 payload 与安全消息；失败不返回
partial payload。

## 4. 当前资源限制事实

| 能力组 | 最大文档字符数 | 最大结果/诊断数 | 最大操作数 |
|---|---:|---:|---:|
| Section / Reference / Diagnostics | 8,388,608 | 10,000 | N/A |
| Edit Preview public path | 8,388,608 | 10,000 diagnostics | 128 |

这些数值已经由现有 service/plan 常量与测试锁定。Gateway 不应复制一套可漂移的执行限制；
descriptor 只投影这些现有事实，实际强制仍由 canonical service 承担。

## 5. 当前明确不存在的 Gateway 能力

源码静态搜索确认 Application/Application.Tests 中不存在：

- `CapabilityGateway`、`CapabilityDescriptor`、Capability registry 或 dispatcher；
- capability ID/version 的生产常量；
- generic `Invoke`、`object`/`dynamic` request、reflection routing 或 JSON schema router；
- Job、Event、Artifact、workflow、handle、session、cache 或 persistence；
- Apply、Undo、Save、Backup、Rollback、文件写入或 Shell/command execution；
- CLI、IPC、MCP、HTTP 或 provider transport adapter。

因此 HLI-2A 不是“补齐现有 Gateway”，而是新增一个很窄的 Application public routing
boundary。不能把测试替身或 IDE provider 工具目录误判为生产 Gateway。

## 6. 不能复用为 Gateway 的相似实现

### 6.1 IDE `Ra2AiCapabilityMode`

`AdvisoryOnly` / `CurrentDocumentEditPreview` 只决定内置 DeepSeek 请求是否附带 provider
tool。它是 IDE internal policy/presentation 状态，不是 capability identity、风险分类或
Headless catalog，不能下移到 Application。

### 6.2 `Ra2AiAuthoringToolCatalog`

其中 `preview_ini_edit_plan` 的 JSON schema 是 DeepSeek/OpenAI-style provider tool schema。
它描述模型输出，不描述本地 CLR Gateway，也不具备 Section/Reference/Diagnostics 能力。
将其提升为 Gateway schema 会把 provider 格式固化进 Application，予以否决。

### 6.3 HLI-1C `GatewayLikePreviewService`

它是 test-only adapter，用于证明 public Application Preview 可以经现有 Workspace seam
进入 Host admission。它不提供 discovery/version/risk，也不是生产依赖。其价值是证明
HLI-2B 可以实现薄 IDE consumer，而非证明 HLI-2A 已存在。

## 7. Host 与写入权威事实

HLI-1C 已确认：

```text
Gateway/Application Preview result
  -> IDE IRa2IniEditPreviewService adapter
  -> Workspace-owned generation + active slot
  -> explicit confirmation
  -> live currency
  -> Shell-owned transaction / one Undo unit
  -> no automatic Save
```

public `PreviewId` 在被 Workspace 当前 generation 接纳前没有 Apply 权威。HLI-2A 若新增
Apply、Save、proposal store 或结果注册接口，将直接破坏已接受边界，必须拒绝。

## 8. 外部架构文档的重合与分期

`C:/Users/PC/Desktop/RA2IniEditor_Automation_Architecture_v1.md` 提出的长期 Gateway 职责
包括 discovery、schema、risk、logging、job、tracing 与 permissions。与当前实现重合的是：

- capability 使用显式 snapshot；
- Agent 只调用高层能力，不直接成为写入权威；
- Preview 与 Apply/Save 分层；
- 需要可发现的版本、风险和资源边界。

HLI-2A 只承接其中已经有真实消费者和数据语义的部分：固定 discovery、version、risk、
limits 与强类型调用。以下部分缺少已冻结的数据模型或当前消费者，继续延后：

- wire schema / schema ID / serialization compatibility；
- permission engine 与授权票据；
- Job/Event/Artifact、恢复、审计日志与 tracing；
- 动态 plugin registration；
- CLI/MCP/IPC/HTTP transport。

这是分阶段实现，不是对长期路线的否定。过早合并这些职责会把 HLI-2A 从 R2/R3 扩大为
R4，并在素材/长任务数据模型确定后产生返工。

## 9. 复用与新增边界

必须直接复用：

- 两个现有 public service interface/concrete service；
- 全部 29 个现有 Experimental snapshot/request/result/fact 类型；
- service 内现有限制、取消和 failure semantics；
- HLI-1C 已冻结的 IDE Host admission seam。

确有新增必要的最小抽象：

- 一个固定 capability ID/version 常量表；
- 一个 immutable descriptor；
- risk/stability 两个窄 enum；
- 一个 typed Gateway interface 和一个无状态实现。

无需新增 registry、factory、resolver、generic result、schema abstraction、DTO mapper、cache、
session 或 dependency injection container。

## 10. Public API 影响候选

HLI-2A 实现候选精确新增 6 个 public 类型，使 Application Experimental allowlist 从
29 变为 35：

1. `IRa2AutomationCapabilityGateway`
2. `Ra2AutomationCapabilityGateway`
3. `Ra2AutomationCapabilityDescriptor`
4. `Ra2AutomationCapabilityIds`
5. `Ra2AutomationCapabilityRisk`
6. `Ra2AutomationCapabilityStability`

本审计没有实际增加 public API。候选签名、属性、枚举值和兼容策略由
`Docs/AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md` 冻结，实施仍需用户确认。

## 11. 已运行证据

```text
Application.Tests baseline: Passed 82/82
Static exported/service scan: 29 existing Experimental types; 2 service interfaces
Static Gateway scan: no production Gateway/descriptor/registry/job/artifact type found
Latest trusted Host/full evidence: 53/53 and 2537/2537 (HLI-1C Stage Ledger)
```

本轮不运行 build/full suite/package：当前变更目标是代码事实审计与文档契约，生产与测试
代码均不修改。

## 12. 审计结论

HLI-2A 可以在不迁移算法、不修改 IDE Host、不引入持久状态的前提下实现。最可靠方案是
固定四项能力的 immutable catalog 与 typed façade，Gateway 只委托 HLI-1 已完成的两个
canonical service。

generic dynamic dispatcher、provider schema、Apply/Save、Job/Event/Artifact 和 wire contract
均没有进入本阶段的代码事实基础。若实施发现必须新增这些能力，应停止并生成 HLI-2A-R1，
不能在当前契约下扩围。
