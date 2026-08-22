# AGENT-AUTHORING-1-R1 A2 阶段台账

日期：2026-07-28  
契约：`Docs/AGENT-AUTHORING-1-R1_A2_SingleDocumentPlanPreviewContract.md`  
状态：实现与自动化验证完成

## 1. 阶段结果

| 阶段 | 结果 | 关键证据 |
|---|---|---|
| A2-P0 回滚锚点与最终契约 | Completed | PreChange 包 1003 entries；禁止条目 0 |
| A2-A 创作快照 | Completed | 会话身份、修订、编辑器文本和 Registry Snapshot 一致性门禁 |
| A2-B 操作与计划 | Completed | `UpsertField` / `ReplaceFieldValue`；1..128 操作；防御性复制 |
| A2-C 预览与诊断差异 | Completed | 不可变候选文本、ChangeSet、逐项证据、诊断多重集差异 |
| A2-D 确定性预览服务 | Completed | 原文坐标规划、冲突/no-op 拒绝、格式与注释保留、无 I/O |
| A2-E 时效性判定 | Completed | DocumentId、EditRevision、会话/编辑器文本、Registry Revision |
| A2-F 边界/取消/性能 | Completed | A2 定向 39/39；1/4/7 MiB、取消与禁止依赖测试 |
| A2-G 包级验证与治理 | Completed | 相关回归 104/104；全量非 UI 2419/2419；Debug build 0/0 |

## 2. 代码结果

### 新增生产契约

- `Editing/Ra2AuthoringSnapshot.cs`：捕获不可变单文档创作输入和显式失败。
- `Editing/Ra2IniEditOperation.cs`：定义首版两类结构化字段操作。
- `Editing/Ra2IniEditPlan.cs`：绑定文档、编辑修订和字段库修订。
- `Editing/Ra2IniEditPreview.cs`：承载候选文本、ChangeSet、操作证据和诊断差异。
- `Editing/IRa2IniEditPreviewService.cs`：只读预览端口。
- `Editing/Ra2IniEditPreviewService.cs`：确定性、纯内存的单文档规划器。
- `Editing/Ra2IniEditPreviewCurrency.cs`：纯时效性检查，不拥有或应用预览。

所有新增生产类型均为 `internal`。没有新增外部 public API、依赖、项目文件或序列化格式。

### 复用与所有权

- 编辑会话继续由 `Ra2EditableDocumentSession` 拥有。
- 字段知识版本继续由 `Ra2FieldRegistryProviderSnapshot` 表达。
- 当前/候选分析复用 A1 的 `IRa2IniLanguageAnalysisService`。
- 文本变化复用 `Ra2TextChangeSet`，插入换行策略复用 `Ra2AddPropertyInsertPlanner`。
- A2 Preview 只是不可应用的值对象；未来 Store、消费和事务所有权保留给 A3。

## 3. 行为边界

- 不修改 Session、AvalonEdit、Undo/Redo、脏状态或文件。
- 不调用 Save、Writer、`File.Write*` 或 Field Registry 写入接口。
- 不接入 Shell、Dock、XAML、ViewModel、AI 或 Search。
- 不修改 parser、diagnostics、Completion、Hover、Quick Peek、Save Preflight 和 BuiltIn。
- 不把未知/低可信字段静默视为已验证；预览证据保留字段可信度。
- 成功预览始终要求显式确认。

## 4. 验证矩阵

| Gate | 命令 / 范围 | 结果 |
|---|---|---|
| Restore | `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| IDE-only build | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed，0 warnings / 0 errors |
| A2 contracts/services | A2 Snapshot/Plan/Preview filters | Passed，39/39 |
| Related regression | A0/A1、Registry Snapshot、Session、ChangeSet、Replace/Insert Planner 与 A2 | Passed，104/104 |
| Boundary/performance | `Ra2IniEditPreviewBoundaryAndPerformanceTests` | Passed，6/6 |
| Full non-UI suite | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | Passed，2419/2419 |

本机记录型性能证据：

| 文档规模 | Preview 记录时间 | 结果 |
|---|---:|---|
| 1 MiB | 84 ms | 成功，源文本不变 |
| 4 MiB | 332 ms | 成功，源文本不变 |
| 7 MiB | 531 ms | 成功，源文本不变 |

这些时间是本机单次记录，不是跨设备 SLA。现有 `AGENT-AUTHORING-A1-TD-001`
继续保持 Open / Controlled；当前证据不足以授权在 A2 内重构诊断链。

## 5. Diff Intent Table

| 变更组 | 意图 | 明确非意图 |
|---|---|---|
| `IDE/Editing/Ra2Authoring*` | 捕获一致的只读创作输入 | 新编辑会话或工作区 |
| `IDE/Editing/Ra2IniEdit*` | 计划、预览、证据与 stale 判定 | Apply、Store、Undo、Save |
| A2 Tests | 锁定契约、边界、确定性和性能事实 | 放松既有语义 |
| Docs | 固化契约、证据和下一安全入口 | 改写历史阶段 |

## 6. Deferred Governance Queue

### PublicApiLedger Pending Entries

- 无外部 public API 变更。
- A2 internal 契约为 Experimental；A3 只能消费，不得未经新契约公开或序列化。

### TechnicalDebt Pending Entries

- 无新增 A2 技术债。
- `AGENT-AUTHORING-A1-TD-001` 保持 Open / Controlled；不得在 A3 顺手偿还。

### DecisionLog Candidate Entries

- Accepted：多操作均相对同一原始 Snapshot 解析，不采用逐项变异语义。
- Accepted：A2 不拥有可应用 Preview；workspace-owned Store 与单次消费属于 A3。
- Accepted：Search Replace Plan 保持 Search 专用，不泛化为 Authoring Plan。

### CurrentStatus Pending Updates

- 已在本次治理 flush 中更新 `Docs/Codex_CurrentPhase.md` 和
  `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`。

## 7. 包证据

- PreChange：`artifacts/RA2IniEditor.IDE.SourceClean.AGENT-AUTHORING-A2.PreChange.Rollback.zip`
- Final：`artifacts/RA2IniEditor.IDE.SourceClean.AGENT-AUTHORING-A2.Final.zip`
- 最终归档条目数、大小、禁止条目和 SHA-256 在交付摘要中记录，避免归档文档自引用。

## 8. 下一安全入口

`AGENT-AUTHORING-1-R1 A3 EditorTransactionPortContract`

A3 必须先单独确认：

- workspace-owned Preview Store；
- PreviewId 单次消费与失效；
- Apply 前完整 currency 复检；
- 候选文本接入既有编辑会话、脏状态和一次语义 Undo；
- 不自动保存、不绕过 Save/Preflight/Backup/Rollback。

在 A3 契约确认前，不得把 A2 Preview 接入 Shell、AI 或任意写入路径。
