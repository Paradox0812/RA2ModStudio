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

### Implemented / Acceptance Pending

- 部分 UI-MODERN/M4-R2/Visual Fix 自动化门禁已完成，但对应真实 WPF 截图或
  特定硬件视觉验收不能由文档整理任务补记为通过。

### Contracted / Not Implemented

- HLI-1A1 Document Query Slice 最终契约已生成并自审，生产迁移尚未获得确认。
- `RA2IniEditor.Application`、Capability Gateway、独立 Agent/CLI、Job/Event/
  Artifact、素材/图标/SHP/VXL 流水线和 Runtime Test Host 均未实现。

## 3. 最新完整实现证据

来源：`Docs/AGENT-AUTHORING-1-R1_A4_R1_StageLedger.md`

```text
Restore: Passed
Debug build: Passed, 0 warnings, 0 errors
Non-UI tests: Passed 2519/2519
IdeOnly clean package: Passed, 1049 files
Computer control / live provider: NotRun by A4-R1 scope
```

HLI-1A0 另有本轮直接证据：characterization tests 7/7，通过 1/4/7 MiB 两次构建
一致性样本。该阶段没有生产代码变更；完整 2519 测试和 clean package 未重跑。

HLI-1A1 契约阶段最新直接证据：API allowlist 15/15、迁移源清单 22/22、残留
Classification using 3/3、Query dependency regression 54/54、Debug build 0 warnings /
0 errors。该证据只验证契约基线，不代表 Application 已实现。

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
| HLI-TD-001/002 | Open / controlled | UI-neutral algorithms 和 diagnostic presentation coupling 仍位于 IDE assembly |
| AGENT-AUTHORING-A1-TD-001 | Open / controlled | SemanticModel 可能重复构建，只影响潜在性能 |
| SEARCH-UIA-001 | Open | AvalonDock 浮动 child-HWND 阻止外部 UIA 穿透 Search 内容 |
| Mixed-DPI visual coverage | Manual | 特定多屏硬件状态未由自动化覆盖 |
| Real project Field Registry delta | Unknown | 仓库无真实 `.ini` corpus，无法统计实际 Unknown Key 增量 |

## 6. 下一安全入口

下一阶段只应先确认：

```text
AUTOMATION-HLI-1A1 Document Query Slice Final Contract
```

契约：`Docs/AUTOMATION-HLI-1A1_DocumentQuerySliceFinalContract.md`。
HLI-1A1 是 R3 生产迁移并包含 R2 Experimental public API。未确认前不得创建
Application/Application.Tests、移动 22 个生产文件或修改 solution/project references。

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

旧累积状态已保存在：

- `Docs/Archive/Codex_CurrentPhase_Accumulated_Through_2026-08-22.md`
- `Docs/Archive/RA2IniEditor_IDE_Full_Codex_Context_Accumulated_Through_2026-08-22.md`
