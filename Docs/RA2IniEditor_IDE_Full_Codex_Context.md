# RA2IniEditor.IDE — Compact Codex Context

更新时间：2026-08-22  
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
| 下一最终契约 | `Docs/AUTOMATION-HLI-1A1_DocumentQuerySliceFinalContract.md` |
| Public API 候选与状态 | `Docs/PublicApiLedger.md` |

## 3. Solution 与所有权

```text
RA2IniEditor.Core              net8.0，INI model/parser/schema/validation primitives
RA2IniEditor.Infrastructure    net8.0，Field Registry、BuiltIn 数据、IO helpers
RA2IniEditor.IDE               net8.0-windows，WPF Shell、language/editing/AI/search
RA2IniEditor.Tests             non-UI tests
RA2IniEditor.UiAutomationTests opt-in UIA smoke
```

未来候选 `RA2IniEditor.Application` 已通过 HLI-0B/HLI-1A0 冻结方向和首个 Query
闭包，但当前 solution 中仍不存在。

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

精确边界与证据见 `Docs/CurrentCapabilities.md`。

## 5. 当前不存在的能力

- 独立 Headless/Application 程序集、Capability Gateway、CLI 或外部 Agent host。
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

HLI-0B 已确认，HLI-1A0 已完成。首个 Query 闭包是 22 个 internal
Classification/Language 文件；完整 TextModel、Diagnostics 和 Preview 不属于首切片。
HLI-1A1 最终契约已生成并自审，当前下一入口是用户确认该契约。

契约已冻结：Application/Core 单向引用、新 Application.Tests `net8.0`、精确 IVT、
project-level global using、15-type Experimental allowlist、nullable occurrence、重复
Section body-span 隔离、Reference 空成功/无法解析失败、8M chars/10k items 限制，
以及只搬迁不改写既有双解析语义。

停止条件：若需要改变 parser、diagnostics、Field Registry priority、Save、
Apply ownership、public API、程序集方向或持久化格式，必须先形成对应风险契约。

## 14. 历史说明

旧累积 Context 和 CurrentPhase 已移入 `Docs/Archive/`。它们保留完整历史，但不
再参与当前状态判定。需要某阶段细节时读取其 Contract、Stage Ledger 或 Context
Capsule，不把历史 “next phase” 当成当前指令。
