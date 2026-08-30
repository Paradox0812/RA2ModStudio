# CONTENT-2D-2 — Project Multi-Document Transaction Final Contract

日期：2026-08-24
状态：Accepted / implemented / verified
前置：`CONTENT-2D-0/1` completed，`GIT-BASELINE-1` completed
风险：R4

## 1. 阶段目标

为当前打开项目建立唯一、多文档、纯内存的结构化编辑事务：一次捕获目标文档，一次生成
按文件分组的统一 Preview，一次显式确认后原子更新全部内存 session；任一门禁或提交步骤
失败时保持全部文档原状，并提供一次项目级 compound Undo/Redo。

完成 2D-2 后，项目具备 rules/art profile 所需的事务底座，但本阶段不新增该 profile。

## 2. 强制不变量

1. `Ra2AutomationEditPreviewService` 仍是唯一逐文档语义 Preview 引擎。
2. `Ra2IniAuthoringWorkspace` 仍是唯一 active preview、显式确认和一次性消费门。
3. IDE project document session store 是全部活动/非活动内存文档的唯一所有者；Shell 只是活动投影。
4. Project Plan 只能引用同一 Project Snapshot 中的 DocumentId，不接受任意目标路径。
5. Preview 纯函数化；成功前不得修改 session、编辑器、磁盘、dirty、Undo 或 Problems。
6. Apply 必须 `validate all -> prepare all -> commit all`；任何失败都不得留下 partial state。
7. Apply/Undo/Redo 不自动 Save，不创建备份，不调用文件写入接口。
8. compound Undo/Redo 必须覆盖整组文档；任一成员 stale 时整组拒绝。
9. 现有单文档 Preview/Apply/Undo/Save 行为保持兼容。
10. Field Registry、parser、classifier、diagnostics 规则和 BuiltIn 数据不改变。

## 3. Public Experimental 数据契约

只增加 4 个类型，复用现有叶节点 DTO；预计 Application exported allowlist `59 -> 63`。

### 3.1 `Ra2AutomationProjectSnapshot`

```text
ProjectSessionId : Guid (non-empty, process-local)
ProjectRevision  : long (>= 0)
ProjectRootPath  : string (non-empty display/identity fact, not a write permission)
Documents        : IReadOnlyList<Ra2AutomationDocumentSnapshot>
```

约束：1..8 个文档；DocumentId 唯一；FilePath 按 OrdinalIgnoreCase 唯一；每个文档沿用 8 MiB
字符上限；项目 aggregate 最多 16 MiB 字符；全部 Field Registry revision 必须一致。

### 3.2 `Ra2AutomationProjectEditPlan`

```text
ProjectPlanId             : Guid
ExpectedProjectSessionId  : Guid
ExpectedProjectRevision   : long
DocumentPlans             : IReadOnlyList<Ra2AutomationEditPlan>
Summary / Origin          : existing bounded display semantics
```

约束：1..8 个 document plan；每个 ExpectedDocumentId 唯一；总 SectionCreation + FieldOperation
不超过 256；每个叶计划继续受现有 128 上限约束。计划顺序是稳定输出和提交顺序。

### 3.3 `Ra2AutomationProjectEditPreviewFailureKind`

```text
None = 0
InvalidProjectSnapshot
InvalidProjectPlan
StaleProject
DocumentNotFound
DuplicateDocumentTarget
DocumentPreviewFailed
ResourceLimitExceeded
Canceled
UnexpectedFailure
```

### 3.4 `Ra2AutomationProjectEditPreviewResult`

成功态包含 ProjectSessionId/Revision、ProjectPlanId、ProjectPreviewId、按 Plan 顺序的现有
`Ra2AutomationEditPreviewResult` 列表、总 operation/section creation 数和显式确认标记。

失败态的 DocumentPreviews 必须为空；允许携带失败 DocumentId/FilePath 和已有单文档
`Ra2AutomationEditPreviewFailureKind` 作为诊断证据，但不得携带 candidate/change/partial plan。

### 3.5 Gateway additive surface

- 新 capability id：`ini.project.edit.preview`，version 1，risk Edit，stability Experimental；
- `IRa2AutomationCapabilityGateway.PreviewProject(snapshot, plan, cancellationToken)`；
- `Ra2AutomationCapabilityGateway` 委托新的 internal project preview orchestrator；
- catalog 顺序在现有 7 项之后追加，现有 ID、方法签名和结果不变。

该 interface additive method 对自定义实现存在源码兼容影响；当前仓库只有 production Gateway 和
一个测试替身。实施时必须更新二者并以 reflection 锁定 10-method surface，不增加 generic Invoke。

## 4. Application Preview 算法

1. 校验 Project identity/revision、集合上限、唯一 DocumentId/Path 和 registry revision。
2. 校验 Plan identity、目标唯一性、aggregate work 上限和 Snapshot membership。
3. 按 DocumentPlans 顺序找到叶 Snapshot，并验证 document id/version/registry revision。
4. 对每个叶节点调用现有单文档 Preview service；不得复制 semantic engine。
5. 任一叶 Preview 失败，立即返回 `DocumentPreviewFailed`，清空全部成功叶 payload。
6. 全部成功后才生成 ProjectPreviewId 和 immutable ordered results。
7. 同输入必须得到相同 candidate/change/evidence 顺序；PreviewId 除外。
8. 取消在每个叶调用前后检查；取消结果无 partial payload。

## 5. IDE 项目文档状态模型

新增 internal `Ra2ProjectDocumentSessionStore`，替代 Shell `_editableSession` 作为 session owner：

| 数据 | 所有者 | 生命周期 |
|---|---|---|
| ProjectSessionId | store | 每次成功打开新项目重新生成 |
| ProjectRevision | store | 文档 membership 或任一内存文本变化时递增 |
| DocumentId/EditRevision | `Ra2EditableDocumentSession` | 项目打开期间稳定；文本变化时 revision 递增 |
| Original/Current/Dirty/Encoding | session | 保存或丢弃前持续存在，即使文件非活动 |
| ActiveFilePath | store | Shell 选择变化时更新，只决定 UI 投影 |
| Compound Undo entry | store/coordinator | 最多保留最近一次项目事务 |

Store 以当前 `ProjectOpenService` 已发现的顶层 INI descriptor 建立 membership；路径必须
`Path.GetFullPath` 后位于当前 root 且按 OrdinalIgnoreCase 唯一。不存在 membership 的文件不能由
模型/计划动态加入。

捕获规则：

- 活动文档必须先确认 AvalonEdit text 与 active session CurrentText 完全一致；
- 已缓存的非活动 session 使用内存 CurrentText；
- 未缓存目标通过 `IIniFileStore.ReadText` 创建 session，并保留编码/换行元数据；
- 读取失败、大文件延迟、只读、项目切换或 registry revision 无效时整体失败；
- Snapshot 捕获完成后，Preview 不再读取磁盘。

## 6. Host 原子 Apply / rollback

现有 Workspace 扩展项目 Preview/Apply，但不得创建第二 workspace：

```text
active preview gate
  -> validate project preview identity + explicit confirmation
  -> validate every target session currency
  -> prepare every updated session without store mutation
  -> store.TryReplaceMany(expected sessions, prepared sessions)
  -> synchronize active AvalonEdit projection when affected
  -> publish one compound undo entry
```

原子规则：

- currency 比较 ProjectSessionId、所有目标 DocumentId/EditRevision/Text 和 Field Registry revision；
- 任一 stale/no-op/readonly/identity mismatch 在 prepare 前失败；
- `TryReplaceMany` 在一个 store gate 内再次比较全部 expected session 后一次替换；
- active editor 同步失败时 store 恢复全部 before sessions，并尝试恢复 editor；
- 恢复 editor 也失败时进入 read-only fail-safe，结果仍为失败，绝不报告成功；
- 成功结果必须包含所有 before/after session evidence、受影响文件数、work count 和 dirty count；
- Problems 刷新保持后置：事务成功后按受影响文档重新分析；刷新失败不逆转已成功的内存提交。

## 7. Compound Undo / Redo

v1 延续现有“一条程序化语义 Undo”策略，但条目包含全部受影响文档：

- Undo 前，所有成员必须仍匹配 apply 后 DocumentId/EditRevision/Text；否则整组拒绝且不清除证据；
- Undo 预构建 before sessions，再用同一 `TryReplaceMany` 原子提交；
- Redo 对称校验 undo 后状态；
- 活动文档若属于事务，更新 AvalonEdit 和 caret；非活动文档只更新 store；
- 任何成员的后续用户编辑、单文档 Revert 或新结构化事务都会使旧 compound entry 失效；
- 保存某一成员不改变 CurrentText 时仍允许 Undo；Undo 后该文档按现有 OriginalText 比较重新变 dirty；
- 普通 AvalonEdit Undo 与现有单文档 semantic Undo 保持原路径；项目 compound entry 优先级最高。

## 8. Shell 与导航最小集成

本阶段允许且只允许修改 `ShellWindow.xaml.cs` 的 session ownership/wiring；不改主布局或 XAML。

- 同项目文件切换前，把当前 editor text 同步进 store；非活动 dirty session 不再丢失；
- 进入已有 session 的文件时加载其 CurrentText/encoding/dirty，而不是覆盖为磁盘文本；
- 状态栏使用现有文本区域显示项目 dirty file count，不新增控件或 AutomationId；
- 打开另一个项目或关闭窗口时，如 store 仍有任意 dirty session，先 fail closed 并提示用户逐文件保存、
  Revert 或 compound Undo；2D-2 不增加 Save All/Discard All 对话框；
- Save Current 继续调用现有 Save/Backup/Preflight 链，只更新 store 中当前 session；
- Apply/Undo/Redo 不调用 Save Current，不创建备份。

该最小策略避免静默数据丢失和新 UI 设计；项目级 Save All/退出确认对话框另立契约。

## 9. 统一 Diff 投影

2D-2 只建立可复用内部投影，不新增 AI profile：

- `Ra2AuthoringDiffProjectionBuilder` 增加 project-preview overload，逐文件复用现有 Build；
- 行模型增加 internal `FileHeader`，按 Project Plan 顺序输出文件名与相对路径；
- 总 visual rows/hunks 继续受现有上限，且按整个项目累计；
- 任一文件投影失败时整个 project Diff 标为不可用，不显示误导性 partial Diff；
- 现有单文件 ViewModel/XAML/AutomationId 与视觉行为零变化；
- 首个 rules/art profile 在后续阶段接入 project proposal 与真实 UI 展示。

## 10. 阶段拆分与逐阶段门禁

| Stage | 范围 | 必选门禁 |
|---|---|---|
| 2D-2A | 当前代码 characterization + 最终契约 | 文档自审、Git diff check |
| 2D-2B | Public project DTO + Gateway Preview | reflection allowlist 63、constructor/failure/no-partial/determinism/limits |
| 2D-2C | Internal project session store + snapshot capture | ownership/path/membership/active overlay/revision/concurrency tests |
| 2D-2D | Workspace project active preview + atomic Apply/rollback | precommit/postcommit failure injection、write count 0、single-doc regression |
| 2D-2E | Compound Undo/Redo + Shell session projection | two-doc undo/redo/stale/no-partial、navigation/save/close boundary tests |
| 2D-2F | Multi-file Diff projection + full verification/docs | focused matrix、Application full、IDE non-UI、Debug build、clean package |

每一阶段失败即停止，不削弱 currency、原子性或现有测试断言换取通过。

## 11. 允许文件

```text
RA2IniEditor.Application/Automation/Experimental/*Project*Contracts.cs
RA2IniEditor.Application/Automation/Experimental/IRa2AutomationCapabilityGateway.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationCapabilityGateway.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationCapabilityContracts.cs
RA2IniEditor.Application/Editing/*Project*Preview*.cs
RA2IniEditor.IDE/Editing/*Project*.cs
RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs
RA2IniEditor.IDE/Editing/IRa2EditorTransactionPort.cs
RA2IniEditor.IDE/Controllers/EditorSession/*（仅必要 additive/refactor）
RA2IniEditor.IDE/AuthoringDiff/Ra2AuthoringDiffProjection.cs
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs（仅 session overlay/status 接线）
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs（仅批准的 session/transaction wiring）
对应 Application.Tests / IDE Tests
本契约、Stage Ledger、PublicApiLedger、DecisionLog、CurrentPhase、Context、README
```

## 12. 禁止文件与行为

- 不修改 `ShellWindow.xaml`、主布局、Dock、菜单、工具栏或 AutomationId；
- 不修改 parser、Section classifier、Field Registry、BuiltIn v3.2、Completion/Hover/Diagnostics 规则；
- 不修改 Save writer、Backup/Rollback 持久化语义，不实现 Save All；
- 不新增 rules/art、Techno、SuperWeapon、Faction、AI tuple profile 或 AI tool schema；
- 不新增 raw INI 拼接、第二 Preview engine、第二 Workspace、第二 Apply authority；
- 不自动 Save、不写磁盘、不静默放弃 dirty session；
- 不公开 Host Apply/Undo/Save、Session store 或本地文件系统能力；
- 不恢复 legacy solution/editor。

## 13. 验收矩阵

### Application

1. Project DTO immutable、identity/path/document/work limits 精确；
2. duplicate id/path、missing target、stale version/revision、registry mismatch 精确失败；
3. 两文档 success 按 plan order 返回；
4. 第二文档失败时零 document preview payload；
5. cancellation/oversize/aggregate operations fail closed；
6. 现有单文档 Preview 结果与异常语义不变；
7. public allowlist 精确 63，Gateway catalog 8、methods 10。

### IDE Host

8. 活动 dirty text 与非活动 cached/disk text 捕获正确；
9. 项目外、未发现、重复、读取失败目标整体拒绝；
10. Preview 后任一目标编辑导致 Apply 整体 stale；
11. prepare 第二文档失败时所有 before session 保持引用/文本/revision 不变；
12. active editor 同步异常时全部 session 和 editor 回滚；
13. 成功 Apply 只产生一个 project compound entry；
14. Undo/Redo 两文档原子、顺序稳定；任一成员 stale 时零变化；
15. active/inactive dirty session 切换后仍保留；Save Current 只保存当前文件；
16. 打开新项目/关闭窗口在任意 dirty session 存在时 fail closed；
17. Apply/Undo/Redo 的 file write/backup 调用计数均为 0；
18. 现有单文档 Apply、Undo/Redo、Replace、Completion 和 Save 测试全通过。

### Diff / full gates

19. 两文件 Diff 有稳定 FileHeader、累计统计和全局上限；
20. project Diff 任一叶失败不显示 partial rows；
21. `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`；
22. Application.Tests full；IDE focused；IDE non-UI full；
23. `package-source-clean.ps1 -Profile IdeOnly`，禁入路径为 0。

真实 DeepSeek、电脑操控和物理视觉烟测不是 2D-2 必选门禁，因为本阶段不增加 AI profile 或
新可见布局；后续 rules/art consumer 阶段再进行端到端人工验收。

## 14. Public API Check

```text
API: ProjectSnapshot / ProjectEditPlan / ProjectEditPreviewFailureKind / ProjectEditPreviewResult
Kind: Experimental DTO/result/failure enum
Why existing API is insufficient: 单文档 identity/result 无法证明跨文件同一时刻和 no-partial
Expected next-stage use: rules/art binding、完整 Techno/SuperWeapon、独立 Host
Stability: Experimental
Compatibility risk: allowlist 59->63；Gateway interface additive method 影响自定义实现
Tests: reflection、constructor、parity、failure/no-partial、limits、consumer contract
Ledger action: Implemented；见 `Docs/PublicApiLedger.md`
```

## 15. 自我审查结论

契约已覆盖最容易导致返工的七个点：唯一 session owner、项目 identity/revision、aggregate
no-partial Preview、prepare/commit 两阶段、UI 同步失败回滚、compound Undo currency、非活动 dirty
session 生命周期。它还明确把磁盘事务、Save All、首个 rules/art profile 和 AI schema 留到消费者阶段，
避免把持久化权限混入当前内存 Apply。

剩余实施风险是 `ShellWindow.xaml.cs` 单体较大，2D-2E 的 owner 迁移可能暴露未被当前测试覆盖的
导航支路。契约已通过 characterization、每阶段门禁和禁止 compatibility shadow state 限制该风险。
用户已确认最终契约，2D-2B..2F 已按上述边界完成并通过自动化门禁。
