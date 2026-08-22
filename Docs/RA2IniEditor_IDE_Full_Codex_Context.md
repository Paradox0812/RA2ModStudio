# RA2IniEditor.IDE — Compact Codex Context

更新时间：2026-08-23
用途：为新任务恢复足够但不重复历史的工程上下文。历史阶段细节应读取对应
Contract/Stage Ledger，不再追加到本文件。

## 1. 产品身份

RA2IniEditor.IDE 是面向 RA2 / YR / Ares / Phobos 的 source-first INI IDE。
当前技术栈为 .NET 8、WPF、AvalonEdit 和 AvalonDock。IDE-only solution 是唯一
构建入口；旧表格编辑器和 legacy root solution 不属于产品。

最终目标是自然语言驱动的 Mod 内容生产 Agent：统一编排 INI、Cameo/Icon、
VOX/VXL 和 SHP 产物。当前项目只完成了真实 INI IDE 与受限当前文件 AI 编辑闭环，
素材自动生成和独立 Agent 平台尚未实现。

## 2. 权威文档

| 主题 | 文档 |
|---|---|
| 稳定工程规则 | `AGENTS.md` |
| 文档入口与权威顺序 | `Docs/README.md` |
| 最终需求 | `Docs/ProductVisionAndRequirements.md` |
| 当前实现能力 | `Docs/CurrentCapabilities.md` |
| 当前阶段 | `Docs/Codex_CurrentPhase.md` |
| 路线图 | `Docs/DevelopmentRoadmap.md` |
| 架构决策 | `Docs/DecisionLog.md` |
| 高层接口代码事实 | `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md` |
| Headless 最小能力契约 | `Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md` |
| 最新依赖锥证据 | `Docs/AUTOMATION-HLI-1A0_DependencyConeCharacterizationContract.md` |
| 最新 Headless 实现证据 | `Docs/AUTOMATION-HLI-1A1_StageLedger.md` |
| HLI-1A2 事实证据 | `Docs/AUTOMATION-HLI-1A2_DiagnosticsCodeFactAudit.md` |
| HLI-1A2 最终契约 | `Docs/AUTOMATION-HLI-1A2_HeadlessDiagnosticsFinalContract.md` |
| HLI-1B 事实证据 | `Docs/AUTOMATION-HLI-1B_EditPreviewCodeFactAudit.md` |
| HLI-1B 最终契约 | `Docs/AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md` |
| HLI-1B 完成证据 | `Docs/AUTOMATION-HLI-1B_StageLedger.md` |
| HLI-1C 事实证据 | `Docs/AUTOMATION-HLI-1C_HostBoundaryCodeFactAudit.md` |
| HLI-1C 最终契约 | `Docs/AUTOMATION-HLI-1C_HostBoundaryFinalContract.md` |
| HLI-1C 完成证据 | `Docs/AUTOMATION-HLI-1C_StageLedger.md` |
| HLI-2A 事实证据 | `Docs/AUTOMATION-HLI-2A_CapabilityGatewayCodeFactAudit.md` |
| HLI-2A 最终契约 | `Docs/AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md` |
| HLI-2A 完成证据 | `Docs/AUTOMATION-HLI-2A_StageLedger.md` |
| HLI-2B 事实证据 | `Docs/AUTOMATION-HLI-2B_GatewayConsumerCodeFactAudit.md` |
| HLI-2B 最终契约 | `Docs/AUTOMATION-HLI-2B_GatewayConsumerFinalContract.md` |
| HLI-2B 完成证据 | `Docs/AUTOMATION-HLI-2B_StageLedger.md` |
| HLI-2C 事实证据 | `Docs/AUTOMATION-HLI-2C_FirstAgentLoopCodeFactAudit.md` |
| HLI-2C 最终契约 | `Docs/AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md` |
| HLI-2C 完成证据 | `Docs/AUTOMATION-HLI-2C_StageLedger.md` |
| 下一安全入口 | 独立 Agent Host 与 CONTENT-1 的下一纵向切片优先级/代码事实审计 |
| Public API 候选与状态 | `Docs/PublicApiLedger.md` |

## 3. Solution 与所有权

```text
RA2IniEditor.Core              net8.0，INI model/parser/schema/validation primitives
RA2IniEditor.Infrastructure    net8.0，Field Registry、BuiltIn 数据、IO helpers
RA2IniEditor.Application       net8.0，Core-only Query/Diagnostics/Edit Preview 与 Experimental API
RA2IniEditor.IDE               net8.0-windows，WPF Shell、editing/AI/search 和 Application consumer
RA2IniEditor.Application.Tests net8.0 headless contract tests
RA2IniEditor.Tests             IDE/non-UI integration tests
RA2IniEditor.UiAutomationTests opt-in UIA smoke
```

`RA2IniEditor.Application` 已通过 HLI-1A1/1A2/1B 迁入 Query、Diagnostics 与 Preview
唯一闭包。HLI-2A 已增加固定四能力目录和 typed Gateway；HLI-2B 已让 IDE 内置 AI 的唯一
Host adapter 消费同一 Gateway，并在 provider 前执行 descriptor-driven 资源门禁。

## 4. 当前已完成能力

- 源码编辑、项目浏览、导航、Dirty、Undo/Redo、Save Preflight 和 backup/rollback。
- Completion、轻量 Hover、Quick Peek、Find References 和 current/project diagnostics。
- Field Registry Project > Global > BuiltIn、Manager、学习/导入预览和 FR-DQ-4 数据清理。
- AvalonDock 工作区、浮动 Search、返回 Home、默认布局重置和 v2 持久化。
- 项目文本 Search；当前文件 Preview-first Replace All，不自动保存。
- DeepSeek V4 Flash/Pro、Flash 默认、生产 Mock 移除、流式增量、取消/超时/
  Failure Taxonomy、上下文/隐私/资源边界。
- A1 UI-neutral 只读分析模型、A2 deterministic Preview、A3 host transaction、
  A4-R1 official endpoint structured-edit proposal 和显式 Apply。
- HLI-1A1 Core-only Application：Section Get、current-document References Find、
  15-type Experimental API、typed failure/limits/cancellation。
- HLI-1A2/1B：同一 Core-only Application 提供 Diagnostics 与受限 semantic Edit
  Preview；public allowlist 精确为 29，Preview 不修改 Host 或磁盘。
- HLI-2A：同一 Application 提供固定四能力 immutable catalog 与 typed Gateway；public
  allowlist 精确为 35，执行只委托现有 Query/Preview service。
- HLI-2B：内置 AI 通过唯一 Host adapter 消费 typed Gateway；8 MiB/10k/128 预算统一，超限
  明确编辑在 provider 前本地拒绝，public API 保持 35。
- HLI-2C：Gateway Query/Validate、provider structured plan、Preview、explicit Apply、Problems
  refresh 和 updated-snapshot Validate 已形成确定性闭环；public API 保持 35，不自动 Save。

精确边界与证据见 `Docs/CurrentCapabilities.md`。

## 5. 当前不存在的能力

- CLI 或外部 Agent host。
- 通用模板、新对象/Section 完整创建、多文件语义事务、自动 Apply/Save。
- Job/Event/Artifact Runtime。
- Cameo/Icon、VOX/SliceStack/VXL、SHP 生成与自动绑定。
- RA2TestHost / IRuntimeAdapter / deterministic runtime regression。

## 6. 编辑和信任边界

```text
Provider/model output = untrusted proposal input
Application semantic Preview = deterministic candidate authority
IDE host = active document, currency, Apply and Undo authority
Save pipeline = disk/encoding/backup/rollback authority
User/policy = external cost, overwrite and final commit authority
```

- INI/MAP/真实素材文件是事实源；索引和 Manifest 是投影/产物记录。
- Model/Agent 不直接写文件、不持有 UI 控件、不解析全局 mutable singleton。
- 当前 A4 编辑只支持明确的当前文件字段 Upsert/Replace。
- 自动重试、模型 fallback 和 custom endpoint tool 均未授权。

## 7. Field Registry 当前基线

来源：`Docs/ContextCapsule_FR_DQ_4.md`

```text
Runtime BuiltIn rows: 2604
Uniform inferred templates: 0
Auto-extracted rows: 0
Empty/unrecognized quality: 0
Exact identity duplicates: 0
PendingManualReview: 0
```

Diagnostic-only rows保留给 lookup/Hover/Quick Peek/Diagnostics，但不进入 key Completion。
AA/AG Projectile canonical 行保留；错误 Techno/Weapon 上下文只作为 guardrail。

## 8. AI 与 Authoring 当前基线

- 生产模型目录仅 Flash/Pro，Flash 默认。
- SSE streaming、增量 Shell rendering、失败上下文隔离和恢复提示词已完成。
- 官方 endpoint 的明确编辑请求使用 required structured tool。
- Provider prose/raw JSON 不能创建提案；只有本地 A3 Preview 可以。
- Apply 只改内存、一次消费、一个 Undo 单元，永不自动 Save。
- A4-R1 最终证据：build 0/0，non-UI tests 2519/2519，IdeOnly package 1049 files。

## 9. Search 当前基线

- 搜索 Project Explorer 中的规范顶层 `.ini` 文件；当前文件使用内存文本覆盖。
- 支持大小写、全字、500 ms 单文件正则超时、10,000 结果上限。
- 大于 8 MiB 的延迟文件和读取失败会跳过并报告。
- Replace All 只限当前文件，必须 Preview，拒绝 stale，一次 Undo，不自动保存。
- 不存在项目级 Replace 或后台索引。

## 10. UI 当前基线

- 1920x1080 是默认几何基准，主编辑区优先。
- 现代化浅色资源、模板、字体和多个二级界面已实施；具体 Stage Ledger 中标注
  visual acceptance pending 的状态仍需人工验收。
- Search 作为独立浮动 Dock；布局可以 Return Home / Reset / persist。
- 深色主题后置。

## 11. 素材路线事实

当前没有生产素材生成代码。用户确认的 VXL 近期路径是：

```text
VOX -> 1 pixel = 1 voxel 的无损二维切片 -> SliceStack Manifest
-> VXLSE III 导入 -> 最终 VXL/HVA 修整与保存
```

不得把切片包写成“最终 VXL 已生成”。Cameo/Icon 与 SHP 同样需要先冻结中立
artifact contract、palette、manifest、provider adapter 和验证规则。

## 12. 构建与验证

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

不得创建或使用 legacy `RA2IniEditor.sln` / `RA2IniEditor.csproj`。

## 13. 当前下一入口

HLI-1A1 已完成：22 个 internal Classification/Language 文件迁入 Application，IDE
复用同一实现；15-type Experimental allowlist、nullable occurrence、重复 Section
body-span 隔离、Reference 空成功/无法解析失败、8M chars/10k items 和取消门禁均已
通过 31 项 headless、54 项迁移和 2526 项完整测试。

HLI-1A2 已完成：diagnostic/FieldTrust 唯一闭包位于 Application，IDE 保留单向
ViewModel adapter，public allowlist 精确为 18。

HLI-1B 已完成：6 个 TextModel + 2 个 TextChange 已原子迁入 Application internal，
唯一 semantic preview engine 与 11 个新增 Experimental public types 已实现，IDE 保留
thin Host adapter，allowlist 精确为 29。82/82、88/88、390/390 和 2526/2526 均通过。

HLI-1C 已完成：现有 `IRa2IniEditPreviewService` 继续是唯一 Host admission seam；
`FromAutomation` 现验证 operation evidence、span 与 candidate/change 闭合，Workspace 在
active-slot admission 前验证返回 wrapper 与 invocation snapshot/plan 的实例绑定。无需新增
`RegisterPreview`、proposal handle store、public Apply 或 Gateway adapter。Application
allowlist 保持 29；82/82、Host 53/53 和完整非 UI 2537/2537 通过。

HLI-2A 已完成：固定四项 immutable catalog、version=1、Query/Edit risk、现有限制和 typed
façade 已实现；新增精确 6 个 Experimental public 类型，allowlist 29 -> 35。Gateway 只委托
两个 canonical service，无 generic dispatcher、wire schema、Apply/Save 或 Job/Event/Artifact。
12/12、Application 94/94、HLI-1C 11/11 和完整非 UI 2537/2537 均通过。

HLI-2B 已完成：现有唯一 `Ra2IniEditPreviewService` 已改为 typed Gateway consumer，unlimited
`PreviewForHost` 已删除；Shell 使用同一 Gateway descriptor 在 provider 请求前拒绝超 8 MiB
的明确编辑，advisory 继续使用截断上下文。Application 94/94、聚焦 78/78、完整 non-UI
2547/2547 与 clean package 通过；public allowlist 保持 35。

HLI-2C 已完成：确定性 Gateway scenario 与 DeepSeek-compatible loopback 均覆盖
Query/Validate -> structured plan -> Preview -> explicit single-use Apply -> updated-snapshot Validate；
Shell 只在成功事务后用 committed text 刷新当前文件 Problems。Application 94/94、聚焦 37/37、
完整 non-UI 2549/2549、IdeOnly package 1123，public allowlist 保持 35。Minimum HLI-v1 完成。

下一安全入口是对独立 Agent Host 与 CONTENT-1 语义模板层做下一纵向切片优先级和代码事实
审计；不得直接公开 Apply/Save，或跳到 wire、Job/Event/Artifact 和素材流水线实现。

停止条件：若需要改变 parser、diagnostics、Field Registry priority、Save、
Apply ownership、public API、程序集方向或持久化格式，必须先形成对应风险契约。

## 14. 历史说明

旧累积 Context 和 CurrentPhase 已移入 `Docs/Archive/`。它们保留完整历史，但不
再参与当前状态判定。需要某阶段细节时读取其 Contract、Stage Ledger 或 Context
Capsule，不把历史 “next phase” 当成当前指令。
