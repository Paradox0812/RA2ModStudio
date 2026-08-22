# Context Capsule — AGENT-AUTHORING-1-R1 A2

## 1. 当前状态

`AGENT-AUTHORING-1-R1 A2` 已完成实现与自动化验证。当前代码具备 UI 无关、
纯内存、单文档的结构化编辑计划与预览能力，但仍不具备 Apply 或用户可见入口。

## 2. 当前架构事实

- `Ra2AuthoringSnapshot` 绑定 DocumentId、EditRevision、编辑器文本和 Registry Revision。
- `Ra2IniEditPlan` 首版只支持 `UpsertField` 与 `ReplaceFieldValue`。
- 同一 Plan 的全部操作相对同一原始 Snapshot 解析。
- `Ra2IniEditPreview` 包含候选文本、`Ra2TextChangeSet`、逐项操作证据和前后诊断差异。
- `Ra2IniEditPreviewCurrencyEvaluator` 只判断 stale；它不存储、不消费、不应用 Preview。
- 所有 A2 生产契约均为 `internal` / Experimental。

## 3. 必须保持的边界

- A2 不修改编辑会话、编辑器、Undo/Redo、脏状态或磁盘。
- A2 不包含 Preview Store、Apply、Save、Writer、AI 或 UI。
- 不修改 parser、diagnostics、Completion、Field Registry、Hover、Save Preflight 或 BuiltIn。
- 不把 Search Replace Plan 泛化为 Authoring Plan。
- 未知或低可信字段只能作为可见证据存在，不能伪装为 source-verified。

## 4. 关键文件

- `Docs/AGENT-AUTHORING-1-R1_A2_SingleDocumentPlanPreviewContract.md`
- `Docs/AGENT-AUTHORING-1-R1_A2_StageLedger.md`
- `RA2IniEditor.IDE/Editing/Ra2AuthoringSnapshot.cs`
- `RA2IniEditor.IDE/Editing/Ra2IniEditPlan.cs`
- `RA2IniEditor.IDE/Editing/Ra2IniEditPreview.cs`
- `RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs`
- `RA2IniEditor.IDE/Editing/Ra2IniEditPreviewCurrency.cs`

## 5. 验证基线

- IDE-only Debug build：0 warnings / 0 errors。
- A2 与相关复用边界回归：104/104。
- 全量非 UI 测试：2419/2419。
- 1/4/7 MiB 记录型预览测试：全部通过，源文本未改变。
- UI/真实产品烟测：Not Applicable；A2 没有 UI 或 Apply 接线。

## 6. 开放风险

- `AGENT-AUTHORING-A1-TD-001` 仍为 Open / Controlled；A2 记录型性能证据没有授权重构诊断内部。
- A2 Preview 当前由调用方直接持有；在 A3 Store/消费契约前不得认为它具有事务所有权。
- 性能数字仅为当前机器记录，不是 SLA。

## 7. 下一推荐阶段

`AGENT-AUTHORING-1-R1 A3 EditorTransactionPortContract`

允许范围：

- 设计 workspace-owned Preview Store、单次消费、currency 复检和既有编辑事务接入。

禁止范围：

- 自动保存、磁盘写入新通道、跨文档事务、Shell/AI/UI 接线、parser/diagnostics 重构。

停止条件：

- 需要新增外部 public API、改变 Save/Undo 生命周期，或无法复用现有
  `Ra2EditableDocumentSession` 与程序化语义 Undo。

## 8. 新上下文必读顺序

1. `AGENTS.md`
2. `Docs/Codex_CurrentPhase.md`
3. `Docs/AGENT-AUTHORING-1-R1_A2_SingleDocumentPlanPreviewContract.md`
4. `Docs/AGENT-AUTHORING-1-R1_A2_StageLedger.md`
5. 本胶囊

不要从 A2 类型存在推断“AI 已经能修改文件”或“项目已经有 Agent Apply”。
