# AUTOMATION-HLI-2A Capability Gateway Final Contract

契约日期：2026-08-22
状态：Final / Awaiting user implementation approval
前置基线：AUTOMATION-HLI-1C Completed / Verified
事实依据：`Docs/AUTOMATION-HLI-2A_CapabilityGatewayCodeFactAudit.md`

## 1. 目标

在 `RA2IniEditor.Application` 中建立首个最小、进程内、UI-neutral Capability Gateway：

```text
future caller / IDE consumer / CLI host
  -> IRa2AutomationCapabilityGateway
     -> immutable four-capability catalog
     -> typed method routing
     -> existing DocumentQuery/EditPreview services
     -> existing typed results
```

Gateway 让高层 Agent 有一个稳定发现和调用入口，但不新增算法、不拥有 Host 状态，也不
声称是 wire SDK。完成 HLI-2A 后停止；HLI-2B 才允许让内置 AI 成为 consumer。

## 2. 风险与治理门

```text
Current contract/docs change: R0 runtime / R3 architecture decision
Future implementation: R2 public API + R3 routing boundary
Persistence/wire risk: None in HLI-2A
UI/Shell risk: None
Governance: Immediate contract/public API/decision ledger update
```

如果实现需要更改 Apply/Save 权威、程序集依赖方向、序列化格式、持久化、provider schema
或动态调用模型，风险升级为 R4，立即停止并重新契约。

## 3. 非目标

HLI-2A 不实现：

- Apply、Undo、Save、Backup、Rollback、文件 I/O 或 active editor/session；
- Preview store、proposal handle、capability token 或事务；
- generic `Invoke`、`object`/`dynamic` request、dictionary payload、reflection dispatch；
- JSON/wire schema、IPC、MCP、HTTP、CLI 或 provider adapter；
- mutable capability registry、plugin registration、DI container 或 service locator；
- Job、Event、Artifact、workflow、logging、tracing、resume 或 permission engine；
- 项目级查询/替换、多文件编辑、Section 模板或素材能力；
- HLI-2B AI consumer、A4 policy/tool schema/prompt/chat 修改；
- Parser、Diagnostics、Field Registry、Completion、Search、Save、Shell、XAML 或 Dock 修改。

## 4. 架构契约

### 4.1 所有权

- Gateway 位于 `RA2IniEditor.Application.Automation.Experimental`。
- Application 继续只依赖 Core；不得引用 IDE、Infrastructure、WPF 或 provider。
- Gateway 是无状态 typed façade；不缓存 snapshot/result，不持有 host/session。
- concrete Gateway 通过 public parameterless constructor 创建，并在内部持有现有两个
  canonical stateless service。
- 不增加 public service injection 构造器；测试通过行为/反射验证，不引入替身扩展面。

### 4.2 唯一执行路径

- `GetSection`、`FindReferences`、`Validate` 必须委托
  `Ra2AutomationDocumentQueryService`。
- `Preview` 必须委托 `Ra2AutomationEditPreviewService.Preview`，不得调用 internal
  `PreviewForHost` 或复制 planner。
- Gateway 不捕获、翻译或包装现有 typed result；正常结果、failure、identity、限制和取消
  必须与直接 service 调用等价。
- 参数构造不变量继续由现有 DTO 抛出 programmer-error exceptions；Gateway 不创建通用
  `GatewayFailureKind` 掩盖调用方错误。

### 4.3 生命周期与并发

- capability catalog 在进程内构造一次并保持只读；调用方不能增删或替换 descriptor。
- Gateway 不保存 invocation state，允许同一实例并发调用。
- snapshot、request、plan、result 与 cancellation token 均由调用方拥有。
- 不建立 global singleton 要求；调用方可创建实例，但每个实例行为必须一致。

## 5. Capability ID 与版本契约

新增 public static `Ra2AutomationCapabilityIds`，只包含以下常量：

```csharp
public const int CurrentVersion = 1;
public const string DocumentSectionGet = "ini.document.section.get";
public const string DocumentReferencesFind = "ini.document.references.find";
public const string DocumentDiagnosticsValidate = "ini.document.diagnostics.validate";
public const string DocumentEditPreview = "ini.document.edit.preview";
```

- ID 使用 ordinal、大小写敏感的稳定字符串，不添加 alias。
- 四项 descriptor 的 Version 均为 `CurrentVersion`。
- typed method 不增加 version 参数。当前接口即 v1 进程内签名；未来不兼容版本通过新
  interface/method 或另行冻结的 wire adapter 表达，不在本阶段制造 generic version error。

## 6. Descriptor 契约

### 6.1 枚举

```csharp
public enum Ra2AutomationCapabilityRisk
{
    Query = 0,
    Edit = 1,
}

public enum Ra2AutomationCapabilityStability
{
    Experimental = 0,
}
```

风险是供未来 policy/consumer 判断的声明事实，不是授权票据。Preview 的 `Edit` 表示它生成
候选文本；它仍不能 Apply 或 Save。

### 6.2 类型表面

`Ra2AutomationCapabilityDescriptor` 必须是 public sealed、immutable，public surface 精确为：

```csharp
string Id { get; }
int Version { get; }
Ra2AutomationCapabilityRisk Risk { get; }
Ra2AutomationCapabilityStability Stability { get; }
int MaximumDocumentCharacters { get; }
int? MaximumResultItems { get; }
int? MaximumOperations { get; }
```

构造器保持 internal，防止调用方伪造 catalog entry；不增加 setters、metadata dictionary、
display name、description、category、input/output `Type` 或 schema ID。

### 6.3 精确目录

`GetCapabilities()` 返回固定顺序的四项只读目录：

| 顺序 | ID | Version | Risk | Stability | Max chars | Max items | Max operations |
|---:|---|---:|---|---|---:|---:|---:|
| 0 | `ini.document.section.get` | 1 | Query | Experimental | 8,388,608 | 10,000 | null |
| 1 | `ini.document.references.find` | 1 | Query | Experimental | 8,388,608 | 10,000 | null |
| 2 | `ini.document.diagnostics.validate` | 1 | Query | Experimental | 8,388,608 | 10,000 | null |
| 3 | `ini.document.edit.preview` | 1 | Edit | Experimental | 8,388,608 | 10,000 | 128 |

Preview 的 `MaximumResultItems` 表示现有 diagnostic item ceiling。descriptor 只投影既有
常量，service 仍是限制执行权威。

## 7. Gateway Public API 契约

新增 public interface：

```csharp
public interface IRa2AutomationCapabilityGateway
{
    IReadOnlyList<Ra2AutomationCapabilityDescriptor> GetCapabilities();

    Ra2AutomationSectionQueryResult GetSection(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationSectionQuery request,
        CancellationToken cancellationToken = default);

    Ra2AutomationReferenceQueryResult FindReferences(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceQuery request,
        CancellationToken cancellationToken = default);

    Ra2AutomationDocumentDiagnosticsResult Validate(
        Ra2AutomationDocumentSnapshot snapshot,
        CancellationToken cancellationToken = default);

    Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default);
}
```

新增 public sealed `Ra2AutomationCapabilityGateway` 实现该接口，public 构造器精确为一个
parameterless constructor。除上述五个接口方法和构造器外，不增加 public method/property。

没有按字符串查找/调用的 API。发现由 descriptor list 完成，执行由编译期 typed method
完成；未来 wire adapter 才负责把 ID/version 映射到这些方法。

## 8. Public API 精确增量

允许新增且只允许新增以下 6 个 public 类型：

1. `IRa2AutomationCapabilityGateway`
2. `Ra2AutomationCapabilityGateway`
3. `Ra2AutomationCapabilityDescriptor`
4. `Ra2AutomationCapabilityIds`
5. `Ra2AutomationCapabilityRisk`
6. `Ra2AutomationCapabilityStability`

Application Experimental exported allowlist 必须从 29 精确变为 35。现有 29 个类型、方法、
属性、constructor 可见性和 enum 数值不得修改。

## 9. Host 与 HLI-2B 边界

- HLI-2A Gateway 只返回 public Application Preview result，不接纳 active slot。
- HLI-2B 的 IDE adapter 必须实现现有 internal `IRa2IniEditPreviewService`，再通过
  `Ra2IniEditPreview.FromAutomation` 与 Workspace admission 进入 A3/A4。
- Gateway 不得引用 `Ra2IniEditPreview`、Workspace、transaction port 或 Shell。
- 当前 IDE Host path 使用 `PreviewForHost` 保留既有 budget；Gateway public path 使用
  8M/10k。是否让内置 AI 改用 public budget 必须在 HLI-2B 单独明确，HLI-2A 不改变产品行为。

## 10. 允许文件

用户确认实施后，production/test 改动只允许：

```text
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationCapabilityContracts.cs (new)
RA2IniEditor.Application/Automation/Experimental/IRa2AutomationCapabilityGateway.cs (new)
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationCapabilityGateway.cs (new)
RA2IniEditor.Application.Tests/Ra2AutomationCapabilityGatewayTests.cs (new)
RA2IniEditor.Application.Tests/Ra2AutomationBoundaryTests.cs
```

阶段完成时允许更新：

```text
Docs/AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md
Docs/AUTOMATION-HLI-2A_StageLedger.md (new)
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/DevelopmentRoadmap.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/README.md
```

不得为此修改 csproj；三个新 production 文件应由现有 SDK-style glob 自动纳入。

## 11. 禁止文件与语义

不得修改：

```text
RA2IniEditor.Application internal parser/semantic/diagnostic/edit algorithms
RA2IniEditor.Core/**
RA2IniEditor.Infrastructure/**
RA2IniEditor.IDE/**
RA2IniEditor.Tests/**
RA2IniEditor.UiAutomationTests/**
all XAML / Shell / Dock / AutomationIds
*.csproj / *.sln / package tooling
Field Registry data
legacy
```

不得改变 Section/Reference/Diagnostics/Preview 的成功、失败、取消、顺序、限制、identity、
span、candidate 或 diagnostic delta 语义。

## 12. 测试契约

新增 `Ra2AutomationCapabilityGatewayTests` 至少覆盖：

1. catalog 精确四项、顺序、ID、version、risk、stability 与 limits；
2. list 和 descriptor 不可由调用方修改；
3. Gateway/interface/concrete public surface 精确，concrete 只有一个 public parameterless ctor；
4. Application exported allowlist 精确 35；
5. `GetSection` 与 direct service success/failure/cancellation parity；
6. `FindReferences` 与 direct service success/failure/cancellation parity；
7. `Validate` 与 direct service success/failure/cancellation parity；
8. `Preview` 与 direct service success/failure/cancellation parity；
9. document/result/diagnostic/operation limits 保持现有 service 语义；
10. 同一 Gateway 实例并发调用确定且不串扰；
11. Application 继续只引用 Core，不出现 IDE/WPF/Infrastructure/provider；
12. public surface 不出现 Apply/Save/store/session/transaction/job/event/artifact/file/process、
    generic invoke、`object`/`dynamic` payload、reflection 或 serialization contract。

新增事实不得少于 10；Application.Tests 当前 82/82，实施后应至少 92 项且全部通过。
测试应比较结构化属性，不依赖偶然的中文/英文 safe message 全文。

## 13. 连续任务卡

### HLI-2A-0 Code-fact Audit and Final Contract

- 完成当前代码事实审计、复用判断、public API 候选和最终契约。
- 更新 CurrentPhase/Roadmap/PublicApiLedger/DecisionLog/Compact Context/README。
- 只修改文档；等待用户确认后才进入生产实现。

### HLI-2A-1 Descriptor and Catalog Contracts

- 新增 ID/version、risk/stability 和 immutable descriptor。
- 建立固定四项只读 catalog。
- 先完成 reflection/immutability/limits 测试。

### HLI-2A-2 Typed Gateway Delegation

- 新增 Gateway interface/concrete class。
- 精确委托两个现有 service，不捕获或翻译 result。
- 完成四项 parity、limits、cancellation 与 concurrency 测试。

### HLI-2A-3 Boundary and Regression Gates

- 更新 exported allowlist 29 -> 35。
- 验证 Application Core-only 和禁止 surface。
- 运行 build、Application targeted/full、IDE non-UI full regression。

### HLI-2A-4 Governance, Package and Stop

- 生成 Stage Ledger 与 Verification Matrix。
- 将 Public API 候选改为 Implemented / Experimental。
- Proposed Decision 经实现证据后改为 Accepted。
- 更新状态文档，生成 IdeOnly clean package并停止，不进入 HLI-2B。

每张任务卡完成后自审；任一必选门禁失败即停止，不带失败进入下一卡。

## 14. 验证命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

HLI-2A 不修改 Host 行为，因此不要求电脑控制或 UI 烟测。

## 15. 静态门禁

- production diff 精确为 3 个新 Application Experimental 文件；不改现有算法文件。
- test diff 精确为 1 个新 Gateway test 文件和 allowlist test 更新。
- Application exported allowlist 精确 35。
- Application project reference 继续只有 Core。
- catalog 精确四项、固定顺序、version=1、limits 与现有常量一致。
- Gateway public surface 只有 typed discovery/query/preview，无 generic dispatcher。
- IDE/Core/Infrastructure/Shell/XAML/project/legacy diff 为 0。
- clean package 无 `.vs/bin/obj/artifacts/TestResults/old zip`。

## 16. 停止规则

- 需要修改现有 service/DTO/failure/limit：停止并生成 HLI-2A-R1。
- 需要 generic invocation 或 wire schema 才能继续：停止，移入独立 R4 transport 契约。
- 需要 Apply/Save/store/session/transaction：停止，违反 Host authority。
- 需要 IDE/provider/A4 修改：停止；该工作属于 HLI-2B。
- 需要 Job/Event/Artifact 或持久化：停止；该工作属于 AUTOMATION-1。
- allowlist 不是精确 35、parity 不成立或完整回归失败：不得宣称完成。

## 17. 自审与返工防线

| 审查项 | 结论 | 处理 |
|---|---|---|
| 是否复制算法 | No | typed Gateway 只委托现有两个 service |
| 是否需要 generic `Invoke` | No | 当前只有四项固定 CLR 能力；typed method 更可审计 |
| 是否需要 mutable registry | No | 当前没有 plugin consumer；固定只读 catalog 足够 |
| 是否需要 schema ID | No | 当前是进程内 CLR API；wire schema 另行 R4 契约 |
| 是否需要统一 Gateway failure | No | capability-specific typed failure 已完整，统一包装会丢语义 |
| 是否需要版本参数 | No | v1 由 descriptor + typed interface 冻结；不虚构协商行为 |
| descriptor 是否过度设计 | No | 只保留 discovery/policy 当前需要的 ID/version/risk/stability/limits |
| 是否泄漏 Host authority | No | 无 Apply/Save/store；HLI-1C seam 不变 |
| 是否改变用户行为 | No | HLI-2A 无 IDE consumer；public budget 差异留给 HLI-2B |
| 是否为未来 CLI/Agent 留入口 | Yes | Core-only typed Gateway 可由后续 host 直接引用 |
| 是否阻塞 Job/Artifact 扩展 | No | 后续可新增独立能力/运行时，不污染当前 snapshot/result |
| 是否可能删除本阶段实现 | Low | 目录与 typed façade 都是后续 HLI-2B/2C 的直接依赖 |

自审结论：该契约在当前代码事实下足够可靠。它冻结了 HLI-2B 真正需要的最小入口，
同时把最易引发返工的 wire、dynamic dispatch、Job/Artifact 与 Host write authority 排除在外。

## 18. 当前停止点

HLI-2A-0 审计与最终契约已完成；Gateway 生产实现尚未开始。下一安全入口是在用户确认本
最终契约后执行 HLI-2A-1，不得自动进入 HLI-2B。
