# CONTENT-2D-2 — Project Multi-Document Transaction Code-Fact Audit

日期：2026-08-24
状态：Completed / read-only code-fact audit
风险：R4（Snapshot、rollback、跨文档状态所有权与 Undo 边界）

## 1. 审计目标

确认当前项目能否在不复制语义引擎、不绕过显式 Apply/Save 权限的前提下，为
`rulesmd.ini`、`artmd.ini` 等多个项目文档提供统一 Snapshot、Preview、原子内存 Apply、
失败回滚和 compound Undo/Redo。

本审计不修改生产代码，不把 rules/art profile、自动保存或磁盘事务纳入 2D-2。

## 2. 当前真实调用链

```text
Shell 当前编辑器
  -> Ra2AuthoringSnapshot.Capture（单个 editable session）
  -> Ra2AutomationCapabilityGateway.Preview（单个 document + plan）
  -> Ra2IniEditPreview / Ra2IniAuthoringWorkspace（单个 active preview）
  -> IRa2EditorTransactionPort.Apply（单个 preview）
  -> ShellEditorTransactionPort
  -> Ra2EditorSessionController.ApplyProgrammaticText（单个 session）
  -> Shell 当前 AvalonEdit + 单条 ProgrammaticSemanticUndoState
```

纯语义 Preview 位于 Application，最终内存状态提交位于 IDE Host；该 authority 分离是
正确的，2D-2 必须扩展而不能绕过。

## 3. 已有可复用事实

| 能力 | 当前权威 | 可复用结论 |
|---|---|---|
| 单文档不可变事实 | `Ra2AutomationDocumentSnapshot` | 继续作为项目快照叶节点 |
| 单文档结构计划 | `Ra2AutomationEditPlan` | 继续作为项目计划叶节点 |
| 语义 Preview | `Ra2AutomationEditPreviewService` / engine | 项目 Preview 逐文档委托，禁止复制算法 |
| Host snapshot | `Ra2AuthoringSnapshot` | 扩展为项目快照中的文档项，不改变现有 capture |
| 编辑会话与 revision | `Ra2EditableDocumentSession` / service | 继续表达文档身份、文本、dirty 与 edit revision |
| 显式确认与 active preview | `Ra2IniAuthoringWorkspace` | 继续作为唯一消费门，不能另建 Apply service |
| 单文档 currency | `Ra2IniEditPreviewCurrencyEvaluator` | 提取/复用逐文档事实，项目层先全量校验 |
| 当前文档事务 | `Ra2EditorSessionController.ApplyProgrammaticText` | 复用预构建 session 规则，提交所有权移到项目 store |
| 文本 Diff | `Ra2AuthoringDiffProjectionBuilder` | 项目层按文件组合，禁止第二 Diff 算法 |
| 文件发现 | `ProjectOpenService` / Project Explorer descriptors | 目标文档只能来自当前项目已发现的顶层 INI |
| 读取与编码 | `IIniFileStore` / `ReadonlyIniContentService` | 捕获非活动文档时复用；Apply 不调用 WriteText |
| Save/Backup/Rollback | `Ra2SaveCurrentFileService` 链 | 保持显式、逐文件保存；不成为项目 Apply 的一部分 |

`ManualFullDiagnosticsService` 已展示“当前活动编辑文本覆盖磁盘文本，其余文档从
`IIniFileStore` 读取”的只读模式，但它没有文档身份、会话持久化、Apply 或 Undo，不能被
直接当作项目事务实现。

## 4. 已确认缺口

### 4.1 Application 只有单文档 Preview

- Gateway 的 `Preview` 只接受一个 `Ra2AutomationDocumentSnapshot` 和一个 Plan。
- Result identity 只包含一个 DocumentId/Version/FilePath。
- 没有 project-session identity、项目 revision、文档集合上限或 aggregate failure。
- 依次调用两次 Preview 无法证明两个结果来自同一个项目时刻。

### 4.2 IDE 只有一个内存文档所有者

- `ShellWindow` 只有 `_editableSession`，非活动文件没有持久内存 session。
- `ShellViewModel.CurrentSnapshot` 是当前加载文件投影，不是项目状态容器。
- 切换文件前只处理当前 dirty session；窗口关闭没有文档 dirty 门禁。
- 因此把非活动候选直接写入编辑器或磁盘都会绕过当前所有权模型。

### 4.3 当前事务和 Undo 只能覆盖单文档

- `IRa2EditorTransactionPort.Apply` 只接受 `Ra2IniEditPreview`。
- `ApplyAuthoringPreviewTransaction` 只校验和替换当前 session/editor。
- `ProgrammaticSemanticUndoState` 只保存一对文本和一对 caret。
- 连续提交两个文件时，第二步失败会留下第一步已提交，且 Ctrl+Z 不能统一撤销。

### 4.4 当前 Diff 只有单文件输入

- `Ra2AuthoringDiffViewModel` 和 projection builder 从一个 Preview 生成 rows。
- 算法本身可复用，但没有文件头、逐文件统计或聚合资源上限。
- 2D-2 可以提供内部多文件 projection；首个 rules/art profile 才负责把它接入真实 AI 提案。

### 4.5 Save 不是项目事务

- 当前 Save 服务对一个 session 备份、写入，写失败后恢复一个文件。
- 2D-2 的 Apply/Undo/Redo 必须保持纯内存；把现有逐文件 Save 串起来不能形成安全的项目事务。
- 多文件原子磁盘保存、外部修改检测和 Save All 需要独立后续契约。

## 5. 路径与安全事实

- 当前项目只枚举项目根目录顶层 `*.ini`；这可作为 2D-2 v1 的文档 membership 权威。
- 模型/模板不得提交任意磁盘路径；Project Plan 只引用 Snapshot 中的 DocumentId。
- Host 必须以 Windows 大小写不敏感规则拒绝重复路径，并拒绝项目外、未发现、读取失败或
  超过单文档上限的目标。
- Apply、Undo、Redo 的 `IIniFileStore.WriteText` 调用次数必须为 0。

## 6. 结论

2D-2 有必要，且不能通过“对两个现有单文档事务做 foreach”实现。正确的最小演进路径是：

1. Application 增加只包装现有叶节点的项目 Snapshot/Plan/Preview；
2. IDE 建立唯一 project document session store，接管活动和非活动内存 session；
3. Workspace 继续拥有唯一 active preview 与显式确认门；
4. Host 使用 prepare-all / validate-all / commit-all 的内存事务；
5. compound Undo/Redo 只在所有成员仍匹配时原子执行；
6. 磁盘 Save、rules/art profile 和 AI tool schema 保持后置。

若不先迁移单一文档所有权，后续完整 Techno、SuperWeapon 和素材绑定都会在第一次跨文件
编辑时返工。
