# AGENT-AUTHORING-1 高层 INI 编写架构契约

状态：研究与方案完成；运行时实现待后续独立阶段  
日期：2026-07-23  
风险：R3（IDE 内部写入编排）/ R4（外部 Agent 进程桥接）

## 1. 目标

为未来 Codex、其他 Agent 和当前 AI 助手提供同一条高层 INI 编写路径：

- 使用项目现有解析、字段词典、语义模型、诊断和保存前检查；
- 以结构化意图生成可审查的文本变更；
- 用户在编辑器中实时看到已应用的更改；
- 保留原始注释、顺序、换行和未知字段；
- 写入内存编辑会话，不绕过 Undo/Redo、诊断和用户保存确认；
- 外部 Agent 与内置 AI 不各自维护一套写入实现。

## 2. 当前代码事实

### 2.1 不存在统一“编译器”

当前所谓“编译规范”实际分布在：

1. `RA2IniEditor.Core`
   - `IniParser.Parse`
   - `IniValidator.Validate`
   - `IniSerializer.Serialize`
2. IDE TextModel
   - `IRa2IniTextDocumentParser`
   - `Ra2IniTextDocumentParser`
   - 精确行、键、值和注释 Span
3. IDE Language
   - `Ra2DocumentSemanticModelBuilder`
   - Section 分类、字段识别、引用符号
4. Diagnostics
   - 结构、字段、引用、链路和保存前诊断
5. Field Registry
   - `IRa2FieldDefinitionProvider`
   - `FieldRegistryRuntimeService.CurrentProvider`
   - Project > Global > BuiltIn 有效优先级

因此不能把一个现有类型包装后宣称为“编译器”；必须先建立只读语言服务门面。

### 2.2 当前程序化编辑能力

`Ra2TextChangeApplier` 只支持单一 Span 替换。Completion 和字段插入都先生成
单个 `Ra2TextChange`，再更新 `Ra2EditableDocumentSession`。

当前 Shell 通过 `SetEditorTextFromProgram` 同步 AvalonEdit，并通过
`Ra2EditorSessionController.UpdateTextFromUser` 重新同步编辑会话。

当前缺失：

- 文档版本前置条件；
- 多变更原子事务；
- 结构化编辑意图；
- 变更前后诊断对比；
- Agent 预览、冲突和拒绝结果；
- 多文件事务与回滚；
- 与 UI 无关的统一 Apply 端口。

### 2.3 保存边界

`Ra2SaveCurrentFileService` 已有保存计划、备份、写入失败回滚和
`MarkSaved`。Agent 不应直接调用文件写入器，也不应自动保存。

## 3. 核心架构决定

采用“只读语言门面 + 结构化计划 + 预览 + 内存事务应用 + 用户保存”的五层架构。

```text
Codex / 内置 AI / 其他 Agent
              |
              v
      Agent Capability Adapter
              |
              v
       Authoring Workspace
       |       |       |
       v       v       v
 Language   Edit Plan  Preview
 Services   Planner    Validator
              |
              v
      Editor Transaction Port
              |
              v
 Existing editable session + AvalonEdit
              |
              v
 Existing save preflight / backup / writer
```

Agent 只能提出计划；计划通过版本、语义和诊断检查后，才能由 IDE 内的事务端口应用。

## 4. 非目标

- 不允许 Agent 直接访问 WPF 控件、AvalonEdit 文档或 Shell code-behind。
- 不允许 Agent 直接写磁盘、绕过保存前检查或自动确认警告。
- 不在首阶段支持跨文件原子提交。
- 不把 Field Registry 的可变运行时服务直接暴露给外部进程。
- 不允许逐 token 修改源码；流式输出与源码应用必须解耦。
- 不用自然语言字符串替代结构化失败结果。

## 5. 建议的数据模型

以下是设计候选，首个实现阶段应保持 `internal`；稳定后再单独评审是否公开。

### 5.1 `Ra2AuthoringSnapshot`

- `DocumentId`
- `FilePath`
- `Version`
- `Text`
- `CaretOffset`
- `Selection`
- `IsEditable`
- `IsDirty`
- `FieldRegistryRevision`

所有计划必须绑定 Snapshot 的 Version 和 Field Registry Revision。

### 5.2 `Ra2IniEditOperation`

采用封闭的操作种类，而非任意 UI 命令：

- `InsertSection`
- `UpsertField`
- `ReplaceFieldValue`
- `RemoveField`
- `InsertRawText`（受限逃生口，默认禁用）

每个操作包含：

- Section 定位器；
- 字段 Key；
- 新值或插入内容；
- 重复项策略；
- 缺失 Section 策略；
- 用户可读意图；
- 来源 Agent/会话标识。

### 5.3 `Ra2IniEditPlan`

- `PlanId`
- `DocumentId`
- `ExpectedVersion`
- `ExpectedFieldRegistryRevision`
- `Operations`
- `Summary`
- `Origin`
- `RequiresExplicitConfirmation`

### 5.4 `Ra2IniEditPreview`

- 规范化后的有序文本变更；
- 完整候选文本；
- Caret 建议；
- 变更摘要；
- 变更前/后诊断；
- 新增 Error/Warning 数量；
- 未知字段和低可信字段证据；
- 冲突与拒绝原因；
- 是否允许应用。

### 5.5 `Ra2IniEditApplyResult`

必须区分：

- `Applied`
- `NoChanges`
- `StaleDocument`
- `FieldRegistryChanged`
- `ReadOnly`
- `InvalidPlan`
- `OverlappingChanges`
- `DiagnosticsRejected`
- `UserConfirmationRequired`
- `UnexpectedFailure`

不得用 `bool`、`null` 或无效果成功吞掉这些状态。

## 6. 建议的内部接口

这些签名是后续契约输入，不是本阶段 public API：

```csharp
internal interface IRa2IniLanguageServices
{
    Ra2IniAnalysisResult Analyze(Ra2AuthoringSnapshot snapshot);
}

internal interface IRa2IniEditPlanner
{
    Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        IRa2FieldDefinitionProvider fieldProvider);
}

internal interface IRa2EditorTransactionPort
{
    Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview);
}

internal interface IRa2IniAuthoringWorkspace
{
    Ra2AuthoringSnapshot CaptureCurrent();
    Ra2IniEditPreview Preview(Ra2IniEditPlan plan);
    Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview);
}
```

`IRa2EditorTransactionPort` 是唯一允许接触当前编辑会话的适配边界；
Language、Planner 和 Agent Adapter 均不得引用 WPF。

## 7. 调用顺序与实时可见性

1. IDE 捕获不可变 `Ra2AuthoringSnapshot`。
2. Agent 读取受限语言能力、字段词典和当前上下文，生成结构化 `Ra2IniEditPlan`。
3. Planner 在后台线程解析并生成规范化、无重叠的文本变更。
4. Preview 对候选文本运行结构、字段、引用、链路和保存前相关只读检查。
5. UI 显示 Diff 与风险；低风险计划可按用户策略确认，高风险计划必须显式确认。
6. Apply 回到 UI 线程，重新校验文档 Version 和词典 Revision。
7. 事务端口在一个 AvalonEdit Undo group 中应用有序变更，并只同步一次编辑会话。
8. 编辑器立即显示结果，脏状态、状态栏、诊断和补全读取同一份新文本。
9. 用户可用一次 Undo 撤销整个 Agent 事务。
10. 只有用户执行保存时，才进入现有 preflight、backup 和 writer。

“实时看到更改”指每个已验证事务批次立即出现在编辑器中；不指逐 token 写源码。
建议默认以一个逻辑对象或一个 Section 为批次，避免半行、半键值和临时无效状态。

## 8. 当前 AI 助手接入

当前 AI Pipeline 保持生成职责，不直接获得编辑器引用。新增接入应分两步：

1. 模型输出受约束的 `Ra2IniEditPlan` JSON/工具调用；
2. IDE 解析、预览并通过 `IRa2IniAuthoringWorkspace` 应用。

聊天流式文本继续用于解释和进度；编辑计划事件使用独立通道。取消生成时：

- 尚未 Apply 的 Preview 丢弃；
- 已完成的事务不自动回滚，可由用户 Undo；
- 不允许取消发生在半个事务中。

原有 advisory-only 行为在 Agent 写入功能正式启用前保持不变。

## 9. Codex / 外部 Agent 接入

外部接入是独立 R4 阶段，建议通过本地能力桥，而非直接加载 IDE 程序集：

- 传输：本机命名管道或 loopback JSON-RPC；若采用 MCP，应仅暴露同等能力。
- 能力：`capture_snapshot`、`query_fields`、`preview_edit_plan`、`apply_preview`。
- 每次 Apply 必须带短期 capability token、DocumentId、ExpectedVersion 和 PreviewId。
- 路径必须限制在当前项目根目录；默认只允许当前活动文档。
- 所有 Apply 记录来源、摘要、时间、旧/新版本和结果。
- 外部进程不得获得 API Key、全局字段包写权限或任意文件系统能力。
- IDE 关闭、切换项目或文档版本变化时，所有旧 Preview 立即失效。

## 10. 复用与迁移路线

### Stage A1：语言服务归一化（R2）

- 建立 internal `IRa2IniLanguageServices`。
- 组合现有 Core parser/validator、SemanticModelBuilder 和诊断。
- 不删除或替换现有入口。
- 验收：同一 Snapshot 的诊断结果与现有路径等价。

### Stage A2：单文档计划与预览（R2/R3）

- 定义 Snapshot、Operation、Plan、Preview、FailureKind。
- 首批只支持 `UpsertField` 和 `ReplaceFieldValue`。
- 复用 TextModel Span 与 Field Registry provider。
- 验收：不改变输入文本；Preview 可重复且稳定。

### Stage A3：编辑器事务端口（R3）

- 增加版本校验和单文档原子 Apply。
- 使用 AvalonEdit Undo group；一次同步编辑会话。
- 不自动保存。
- 验收：一次 Undo 完整撤销；并发编辑导致 `StaleDocument`。

### Stage A4：内置 AI 工具调用（R3）

- AI 只能产生结构化计划。
- 增加 Preview/确认 UI 和 apply policy。
- 保持普通聊天与流式响应兼容。

### Stage A5：外部 Agent 能力桥（R4）

- 本地 IPC/MCP 适配器。
- 能力、路径、版本和审计门禁。
- 先只读，再开放当前文档 Preview，最后才开放 Apply。

### Stage A6：多文件事务（R4，后置）

- 独立工作区事务、文件锁、失败补偿和统一用户确认。
- 在完成单文档可靠性验证前不得提前实施。

## 11. 需要优先偿还的现有限制

1. 当前 `Ra2TextChange` 只支持单变更，需新增有序、非重叠变更集合。
2. 当前编辑会话没有自身 Version；不能借用加载 Snapshot Version 代替编辑版本。
3. 当前程序化 Undo 是 Shell 内的单状态实现，不足以承载连续 Agent 事务。
4. Field Registry 当前没有稳定 Revision；需由运行时服务在 Reload 时递增内部代次。
5. 诊断入口同时存在 public readonly service 和 internal semantic builder，需要门面统一但不能复制算法。
6. Shell code-behind 持有编辑器同步细节，必须通过窄事务端口隔离，而不是继续增加 Agent 分支。

## 12. 验证策略

- 纯 Planner：确定性单元测试、重复执行、换行保留、注释保留、重复 Key 策略。
- Preview：前后诊断差异、未知/低可信字段、重叠变更拒绝。
- Transaction：版本冲突、只读、单次 Undo/Redo、Caret、滚动位置和脏状态。
- AI：无效 JSON、越权操作、取消、超时、重复 Apply、旧 Preview。
- 外部桥：项目路径逃逸、过期 token、IDE 重启、切换文件、并发用户编辑。
- 兼容：现有 Completion、字段插入、保存前检查和 Field Registry 优先级测试全部保持。

## 13. Public API 与安全结论

- 本阶段无 public API 变更。
- A1-A4 首选 internal 契约，并通过 `InternalsVisibleTo` 测试。
- 只有外部桥协议稳定后，才评审可见协议；不得直接公开 WPF、编辑会话或
  `FieldRegistryRuntimeService`。
- 任何 Apply 能力默认需要用户可见确认和可撤销事务。

## 14. 自审结论

- Architecture：通过。Agent 与 UI、文件写入、字段库可变服务隔离。
- Reuse：通过。复用现有解析、语义、诊断、字段 provider、TextModel 和保存链路。
- Data ownership：通过。Snapshot/Plan/Preview/ApplyResult 的所有权和生命周期明确。
- Concurrency：通过。ExpectedVersion 和 Preview 失效规则覆盖用户实时编辑冲突。
- Safety：通过。不自动保存，不逐 token 写源码，不把外部进程接到 WPF。
- Evolvability：通过。内置 AI 和外部 Agent 共用 Authoring Workspace。
- Remaining gate：运行时实现涉及 R3/R4 和新契约，必须按 A1-A5 分阶段实施并逐阶段验证。

