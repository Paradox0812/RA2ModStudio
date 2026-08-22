# AGENT-AUTHORING-1-R1 A3 阶段台账

日期：2026-07-28  
契约：`Docs/AGENT-AUTHORING-1-R1_A3_EditorTransactionPortContract.md`  
状态：实现与自动化验证完成；按用户要求在 A3 停止

## 1. Stage Result Ledger

| 阶段 | 目标 | 结果 | 验证证据 | 下一入口满足 |
|---|---|---|---|---|
| A3-P0 | 最终契约与回滚锚点 | Completed | PreChange 包 1019 entries，禁止条目 0 | 是 |
| A3-A | 会话级程序化文本事务 | Completed | 身份/修订/原文/no-op 门禁；只调用一次 `UpdateText` | 是 |
| A3-B | 单活动 Preview 所有权 | Completed | 确认门禁、一次消费、显式失效和 generation 竞争测试 | 是 |
| A3-C | Shell-owned 事务端口 | Completed | currency 复检、Editor/Session/Undo 原子区和补偿静态边界 | 是 |
| A3-D | 契约与集成测试 | Completed | A3 定向及受影响边界 23/23 | 是 |
| A3-E | 包级验证与治理收口 | Completed | Debug build 0/0；完整非 UI 2436/2436；IdeOnly clean package | 是 |

## 2. 实现结果

### Editing 边界

- `Ra2IniEditApplyRequest` 只接受 `PreviewId` 与显式确认，不接受调用方提供的实时状态。
- `Ra2IniEditApplyResult` 成功时携带完整 Editor/Session/Undo 证据；失败时不携带提交证据。
- `Ra2IniAuthoringWorkspace` 只拥有一个活动 Preview，以 generation 防止较旧异步结果覆盖新结果。
- 确认后的 Preview 在调用事务端口前即被 claim；成功、stale 或失败后均不能重放。
- `IRa2EditorTransactionPort` 是结构化 Preview 进入实时编辑状态的唯一内部端口。

### Session Controller

- `ApplyProgrammaticText` 再次检查可编辑状态、DocumentId、EditRevision、当前文本和 no-op。
- 成功路径只调用一次 `IRa2EditableDocumentSessionService.UpdateText`。
- 返回会话必须保持 DocumentId、修订恰好 `+1`，且 CurrentText 等于候选文本。
- 全部新增/变更类型与方法均为 `internal`；无外部 public API 变更。

### Shell 窄接入

- `ShellWindow.xaml.cs` 私有适配器在提交瞬间读取实时 Session、AvalonEdit 文本、Registry Revision 和 Caret。
- 先使用既有 `Ra2IniEditPreviewCurrencyEvaluator` 复检，再生成更新后的不可变 Session。
- Editor 文本、Session 引用、单个 `ProgrammaticSemanticUndoState` 与 AvalonEdit Undo 清理形成核心提交区。
- 编辑器同步异常时尝试恢复原文和 Caret；恢复失败则退为只读，避免不一致状态进入保存链。
- 用户文本、程序化文本、会话进入/退出及 Registry Reload 均使活动 Preview 失效。
- `ShellWindow.xaml`、Dock、布局、AutomationId 和用户入口未改变；A3 仍不是用户可见功能。

## 3. 验证矩阵

| Gate | 命令 / 范围 | 结果 |
|---|---|---|
| Restore | `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| IDE-only build | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed，0 warnings / 0 errors |
| A3 targeted | Controller/Workspace/Apply/Shell Boundary/A2 Boundary | Passed，23/23 |
| Full non-UI suite | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | Passed，2436/2436 |
| UIA | NotRun | A3 没有新增用户入口或控件，不是本阶段门禁 |
| IdeOnly package | `tools/package-source-clean.ps1 -Profile IdeOnly` | Passed；最终指标见交付摘要 |

## 4. Diff Intent Table

| 文件 / 组 | 变更意图 | 在允许范围 |
|---|---|---|
| `Editing/Ra2IniEditApplyResult.cs` | Apply 请求、结果与失败不变式 | 是 |
| `Editing/IRa2EditorTransactionPort.cs` | 唯一内部实时提交端口 | 是 |
| `Editing/Ra2IniAuthoringWorkspace.cs` | 单活动 Preview 所有权与消费 | 是 |
| `Controllers/EditorSession/Ra2EditorSessionController.cs` | 会话级程序化事务 | 是 |
| `Views/ShellWindow.xaml.cs` | 私有端口、currency、补偿和失效接点 | 是，用户明确授权的窄 code-behind 范围 |
| A3 tests | 契约、并发、失效与 Shell 静态集成 | 是 |
| A2 boundary test | 将 A3 Editing 文件纳入无 UI/AI/Writer 边界 | 是 |
| A3 docs | 契约、台账、当前状态和胶囊 | 是 |

## 5. 明确未变

- 无 `ShellWindow.xaml` 或其他 XAML 修改。
- 无 AI、Search、Save、Writer、Backup、Rollback 修改。
- 无 parser、diagnostics、Completion、Field Registry 实现或 BuiltIn 数据修改。
- 无项目文件、依赖、目录移动、序列化格式或外部 public API 修改。
- legacy 未恢复。

## 6. Deferred Governance Queue

### PublicApiLedger Pending Entries

- 无外部 public API 变更。
- A3 的 Workspace、Apply、Transaction Port 和 Controller 扩展均为 internal / Experimental。

### TechnicalDebt Pending Entries

- `AGENT-AUTHORING-A1-TD-001` 保持 Open / Controlled，本阶段未改动诊断链。
- A3 编辑器同步异常补偿目前由代码边界与静态测试覆盖；没有为了注入 AvalonEdit 异常而新增测试钩子。
- A3 保留现有单状态程序化语义 Undo，不提供多级结构化事务历史。

### DecisionLog Candidate Entries

- Accepted：活动 Preview 由 Workspace 单槽所有，调用方只能提交 PreviewId 与显式确认。
- Accepted：实时 Session/Editor/Registry/Caret 只能由 Shell-owned Port 在提交瞬间读取。
- Accepted：A3 只修改内存编辑状态，不接入 Save，也不增加用户入口。

### CurrentStatus Pending Updates

- 已在本次治理 flush 中更新 `Docs/Codex_CurrentPhase.md`、
  `Docs/RA2IniEditor_IDE_Full_Codex_Context.md` 和 A3 Context Capsule。

## 7. 包与回滚

- PreChange：`artifacts/RA2IniEditor.IDE.SourceClean.AGENT-AUTHORING-A3.PreChange.Rollback.zip`
- Final：`artifacts/RA2IniEditor.IDE.SourceClean.AGENT-AUTHORING-A3.Final.zip`
- 逐文件哈希差异与最终 SHA-256 在交付摘要中记录，避免归档文档自引用。

## 8. 下一安全入口

`AGENT-AUTHORING-1-R1 A4`

A4 必须先单独形成并确认用户入口契约，限定：

- 谁创建 `Ra2AuthoringSnapshot` 和 `Ra2IniEditPlan`；
- 如何展示 Preview、诊断差异和显式确认；
- 如何调用现有 Workspace，而不绕开 PreviewId 单次消费；
- 不自动保存，不允许 AI 直接提交未经用户确认的编辑。

A3 已按用户要求停止，不自动进入 A4。
