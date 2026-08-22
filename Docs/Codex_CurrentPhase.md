# RA2IniEditor.IDE — Current Phase

更新时间：2026-08-22  
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
- AUTOMATION-HLI-1A2 Headless Diagnostics（唯一 neutral core、IDE adapter、18-type Experimental API、42/42 + 149/149 + 2526/2526）。

### Implemented / Acceptance Pending

- 部分 UI-MODERN/M4-R2/Visual Fix 自动化门禁已完成，但对应真实 WPF 截图或
  特定硬件视觉验收不能由文档整理任务补记为通过。

### Contracted / Not Implemented

- HLI-1B Headless Edit Preview 尚未完成代码事实回归或最终契约。
- Capability Gateway、独立 Agent/CLI、Job/Event/
  Artifact、素材/图标/SHP/VXL 流水线和 Runtime Test Host 均未实现。

## 3. 最新完整实现证据

来源：`Docs/AUTOMATION-HLI-1A2_StageLedger.md`

```text
Restore: Passed
Debug build: Passed, 0 errors, one pre-existing CS8602 warning
Application.Tests: Passed 42/42
Diagnostics/A1/FieldTrust regression: Passed 149/149
Non-UI tests: Passed 2526/2526
IdeOnly clean package: Passed, final package rerun after governance flush
Computer control: NotRun; no UI behavior changed
```

HLI-1A2 静态证据：9 个旧路径 0、Diagnostics/FieldTrust 算法副本 0、Application
Core-only、forbidden token 0、stale qualified name 0、exported allowlist 精确 18。

## 4. 当前关键边界

- A4-R1 可对明确的当前文件字段编辑请求形成真实本地提案并经用户 Apply。
- Apply 只改当前内存会话并形成一个 Undo 单元；不会自动保存。
- Custom endpoint 只能 advisory；官方 endpoint 才可进入 required authoring tool。
- 当前能力不等于任意自然语言编辑、Section 模板、多文件写入或素材生成。
- 自动重试、模型 fallback、深色主题、项目级替换继续后置。
- Legacy 不得恢复。

## 5. 当前风险与债务

| ID/Area | 状态 | 影响 |
|---|---|---|
| HLI-TD-001 | Partially repaid | Section/Reference/Diagnostics 已迁 Application；Preview 后续切片仍待处理 |
| HLI-TD-002 | Repaid | diagnostic core 已 neutral；IDE 只保留单向 ViewModel adapter |
| AGENT-AUTHORING-A1-TD-001 | Open / controlled | SemanticModel 可能重复构建，只影响潜在性能 |
| SEARCH-UIA-001 | Open | AvalonDock 浮动 child-HWND 阻止外部 UIA 穿透 Search 内容 |
| Mixed-DPI visual coverage | Manual | 特定多屏硬件状态未由自动化覆盖 |
| Real project Field Registry delta | Unknown | 仓库无真实 `.ini` corpus，无法统计实际 Unknown Key 增量 |

## 6. 下一安全入口

下一安全操作是先只读审计并制定：

```text
AUTOMATION-HLI-1B Headless Edit Preview code-fact audit and final contract
```

HLI-1A2 已完成，见 `Docs/AUTOMATION-HLI-1A2_StageLedger.md`。当前必须停止在
HLI-1A2；未经新的事实审计、最终契约和用户确认，不迁移 HLI-1B Preview。

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

旧累积状态已保存在：

- `Docs/Archive/Codex_CurrentPhase_Accumulated_Through_2026-08-22.md`
- `Docs/Archive/RA2IniEditor_IDE_Full_Codex_Context_Accumulated_Through_2026-08-22.md`
