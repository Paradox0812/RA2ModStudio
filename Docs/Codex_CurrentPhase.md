# RA2IniEditor.IDE — Current Phase

更新时间：2026-08-23
状态类型：CurrentStatus / concise index

## 1. 当前产品目标

已确认的最终目标：用户以自然语言描述 Mod 制作需求，Agent 自动编排 INI、
Cameo/Icon、VOX/VXL、SHP 的创建、预览、绑定和验证。当前安全默认仍是本地
Preview + 显式 Apply/Save；更高自治级别需要后续单独契约。

权威需求：`Docs/ProductVisionAndRequirements.md`  
当前能力：`Docs/CurrentCapabilities.md`  
路线图：`Docs/DevelopmentRoadmap.md`

## 2. 最新可信状态

### Completed / Verified

- IDE-only source-first WPF/AvalonDock IDE。
- Field Registry FR-DQ-4 清退和可信度收口。
- AI-STREAM-0..3、AI-REL-1..3。
- UI-DOCK 布局、恢复与持久化主线。
- SEARCH-1-R1 项目查找 + 当前文件 Preview-first Replace All。
- AGENT-AUTHORING A0、A1、A2、A3、A4 和 A4-R1。
- AUTOMATION-HLI-0A 现有能力审计与矩阵。
- AUTOMATION-HLI-0B 最小能力契约已确认并完成契约阶段。
- AUTOMATION-HLI-1A0 Query 依赖锥特征化与门禁测试（7/7）。
- AUTOMATION-HLI-1A1 Headless Document Query（Application、15-type Experimental API、31/31 + 2526/2526）。
- AUTOMATION-HLI-1A2 Headless Diagnostics（唯一 neutral core、IDE adapter、18-type Experimental API、47/47 + 149/149 + 2526/2526）。
- AUTOMATION-HLI-1B Headless Edit Preview（唯一 semantic engine、IDE thin adapter、29-type Experimental API、82/82 + 88/88 + 390/390 + 2526/2526）。
- AUTOMATION-HLI-1C Host Boundary Confirmation（两处 internal admission/projection guard、
  11 个新契约测试、public API 0 change、82/82 + 53/53 + 2537/2537）。
- AUTOMATION-HLI-2A-0 Capability Gateway 代码事实审计与最终契约：确认当前无生产
  Gateway/descriptor/registry，冻结固定四能力目录与强类型门面。
- AUTOMATION-HLI-2A-1..2A-4 最小 Capability Gateway：固定四能力目录、6 个新增
  Experimental public 类型、allowlist 35、12/12 + 94/94 + 2537/2537。
- AUTOMATION-HLI-2B-0 IDE/AI Gateway Consumer 代码事实审计与最终契约：已冻结唯一
  adapter、public budget、发送前成本门禁和 Host authority。
- AUTOMATION-HLI-2B-1..2B-4 IDE/AI Gateway Consumer：唯一 adapter 已切换 typed Gateway，
  删除 unlimited bypass，descriptor-driven preflight 已在 provider 前生效；public API 0 change，
  94/94 + 78/78 + 2547/2547 + clean package。
- AUTOMATION-HLI-2C-0 First High-Level Agent Loop：代码事实审计与最终契约已完成；确认
  当前缺口是端到端闭环证据和 Apply 后 Problems 刷新，不需要新 public Agent façade。
- AUTOMATION-HLI-2C-1..2C-4 First High-Level Agent Loop：确定性 Gateway 与 provider loopback
  闭环、显式单次 Apply、更新后 Validate 和 Problems 刷新已完成；public API 0 change，
  94/94 + 37/37 + 2549/2549 + clean package 1123。
- AUTOMATION-POST-HLI-0 Semantic / Host Priority Audit：只读代码事实确认 CONTENT-1 应先于
  独立 Agent Host，素材侧继续后置；public API 和生产代码 0 change。

### Implemented / Acceptance Pending

- 部分 UI-MODERN/M4-R2/Visual Fix 自动化门禁已完成，但对应真实 WPF 截图或
  特定硬件视觉验收不能由文档整理任务补记为通过。

### Contracted / Not Implemented

- AUTOMATION-CONTENT-1 连续最终契约候选已生成，等待用户确认；生产实现尚未开始。
- 独立 Agent/CLI、Job/Event/Artifact、素材/图标/SHP/VXL 流水线和 Runtime Test Host
  均未实现。

## 3. 最新完整实现证据

来源：`Docs/AUTOMATION-HLI-2C_StageLedger.md`

```text
Restore: Passed
Debug build: Passed, 0 warnings, 0 errors
Application.Tests: Passed 94/94
HLI-2C/HLI-2B/A4/Workspace/Shell focused: Passed 37/37
Non-UI tests: Passed 2549/2549
IdeOnly clean package: Passed, 1123 files
Computer control: NotRun; no XAML or visual layout changed
```

HLI-2C 静态证据：production diff 精确限于 `AiEditProposalView_OnApplyRequested` 成功分支；
XAML/project/legacy 和 transaction/Save diff 为 0，Application exported allowlist 精确保持 35。

## 4. 当前关键边界

- A4-R1 可对明确的当前文件字段编辑请求形成真实本地提案并经用户 Apply。
- Apply 只改当前内存会话并形成一个 Undo 单元；成功后刷新当前文件 Problems；不会自动保存。
- Custom endpoint 只能 advisory；官方 endpoint 才可进入 required authoring tool。
- 当前能力不等于任意自然语言编辑、Section 模板、多文件写入或素材生成。
- 自动重试、模型 fallback、深色主题、项目级替换继续后置。
- Legacy 不得恢复。

## 5. 当前风险与债务

| ID/Area | 状态 | 影响 |
|---|---|---|
| HLI-TD-001 | Repaid through HLI-2B | Section/Reference/Diagnostics/Preview 唯一权威与 typed Gateway 均在 Application；IDE consumer 已接入 |
| HLI-TD-002 | Repaid | diagnostic core 已 neutral；IDE 只保留单向 ViewModel adapter |
| HLI-2B budget transition | Closed / verified | 已统一为 public 8 MiB/10k/128，并在 provider 调用前 fail closed |
| AGENT-AUTHORING-A1-TD-001 | Open / controlled | SemanticModel 可能重复构建，只影响潜在性能 |
| SEARCH-UIA-001 | Open | AvalonDock 浮动 child-HWND 阻止外部 UIA 穿透 Search 内容 |
| Mixed-DPI visual coverage | Manual | 特定多屏硬件状态未由自动化覆盖 |
| Real project Field Registry delta | Unknown | 仓库无真实 `.ini` corpus，无法统计实际 Unknown Key 增量 |

## 6. 下一安全入口

下一安全操作是：

```text
确认 AUTOMATION-CONTENT-1 Semantic Template Continuous Final Contract
```

最终契约候选见 `Docs/AUTOMATION-CONTENT-1_SemanticTemplateContinuousFinalContract.md`。确认后
可从 CONTENT-1A 开始按 1A..1F 连续实施并逐阶段审查；确认前不得修改生产代码。wire、模板
持久化、multi-file、Apply/Save、Job/Event/Artifact 和素材仍明确后置。

## 7. 最小继续阅读集

1. `AGENTS.md`
2. `Docs/README.md`
3. `Docs/ProductVisionAndRequirements.md`
4. `Docs/CurrentCapabilities.md`
5. 本文件
6. `Docs/AUTOMATION-HLI-0A_ExistingCapabilityMatrix.md`
7. `Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md`
8. `Docs/AUTOMATION-HLI-1A0_DependencyConeCharacterizationContract.md`
9. `Docs/PublicApiLedger.md`
10. `Docs/AUTOMATION-HLI-1A1_DocumentQuerySliceFinalContract.md`
11. `Docs/AUTOMATION-HLI-1A1_StageLedger.md`
12. `Docs/AUTOMATION-HLI-1A2_DiagnosticsCodeFactAudit.md`
13. `Docs/AUTOMATION-HLI-1A2_HeadlessDiagnosticsFinalContract.md`
14. `Docs/AUTOMATION-HLI-1A2_StageLedger.md`
15. `Docs/AUTOMATION-HLI-1B_EditPreviewCodeFactAudit.md`
16. `Docs/AUTOMATION-HLI-1B_HeadlessEditPreviewFinalContract.md`
17. `Docs/AUTOMATION-HLI-1B_StageLedger.md`
18. `Docs/AUTOMATION-HLI-1C_HostBoundaryCodeFactAudit.md`
19. `Docs/AUTOMATION-HLI-1C_HostBoundaryFinalContract.md`
20. `Docs/AUTOMATION-HLI-1C_StageLedger.md`
21. `Docs/AUTOMATION-HLI-2A_CapabilityGatewayCodeFactAudit.md`
22. `Docs/AUTOMATION-HLI-2A_CapabilityGatewayFinalContract.md`
23. `Docs/AUTOMATION-HLI-2A_StageLedger.md`
24. `Docs/AUTOMATION-HLI-2B_GatewayConsumerCodeFactAudit.md`
25. `Docs/AUTOMATION-HLI-2B_GatewayConsumerFinalContract.md`
26. `Docs/AUTOMATION-HLI-2B_StageLedger.md`
27. `Docs/AUTOMATION-HLI-2C_FirstAgentLoopCodeFactAudit.md`
28. `Docs/AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md`
29. `Docs/AUTOMATION-HLI-2C_StageLedger.md`
30. `Docs/AUTOMATION-POST-HLI-0_SemanticHostPriorityCodeFactAudit.md`
31. `Docs/AUTOMATION-CONTENT-1_SemanticTemplateContinuousFinalContract.md`

旧累积状态已保存在：

- `Docs/Archive/Codex_CurrentPhase_Accumulated_Through_2026-08-22.md`
- `Docs/Archive/RA2IniEditor_IDE_Full_Codex_Context_Accumulated_Through_2026-08-22.md`
