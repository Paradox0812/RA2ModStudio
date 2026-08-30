# CONTENT-2D-2 — Project Multi-Document Transaction Stage Ledger

日期：2026-08-24
契约：`Docs/AUTOMATION-CONTENT-2D2_ProjectMultiDocumentTransactionFinalContract.md`

## Stage Result Ledger

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| 2D-2A | 代码事实审计、R4 数据/架构契约与自审 | audit、contract、governance/status docs | code anchors、doc links、scope/diff check | Completed | 是 |
| 2D-2B | Public project DTO + Gateway Preview | Application project contracts/service/Gateway/tests | focused Application 24/24；Application full 167/167 | Completed | 是 |
| 2D-2C | Project session store + snapshot capture | IDE project session store/tests | focused 5/5 | Completed | 是 |
| 2D-2D | Workspace atomic Apply/rollback | project preview wrapper/coordinator/Workspace/tests | focused cumulative 15/15；write count 0 | Completed | 是 |
| 2D-2E | Compound Undo/Redo + Shell projection | coordinator、`ShellWindow.xaml.cs`、boundary/tests | compound 8/8；Shell/owner focused 35/35 | Completed | 是 |
| 2D-2F | Multi-file Diff + full verification/docs | Diff projection/tests/docs | Diff 11/11；post-closure focused 35/35；Debug 0/0；Application 167/167；IDE 2626/2626；IdeOnly package 1195 files | Completed | 是 |

## 2D-2A 结论

- 当前生产实现只有一个 active editable session、单文档 Snapshot/Plan/Preview、单文档事务端口和单条语义 Undo。
- 两个单文档 Apply 串联不能满足 no-partial、rollback 或 compound Undo。
- 最终契约选择 Application 纯项目 Preview + IDE 唯一 project session store + Host 原子内存事务。
- 2D-2A 当时仅提出 4 个 Experimental 包装类型和一个 Gateway additive method；实施后 allowlist 为 63。
- Apply/Undo/Redo 不写磁盘；Save All、rules/art profile、AI schema 与退出确认 UI 保持后置。
- Apply 成功结果包含 affected/work/dirty counts；项目诊断通过内存覆盖读取全部 Preview 文档，刷新失败不回滚事务。

## 实施结论

- Application 新增 4 个 Experimental project DTO/result 类型和 `PreviewProject`；allowlist 63、catalog 8、Gateway methods 10。
- `Ra2AutomationEditPreviewService` 继续是唯一叶 Preview 引擎；项目失败态不返回 partial document payload。
- IDE project session store 成为活动/非活动文档的内存 owner；同项目切换保留 dirty，跨项目/关闭时 fail closed。
- Apply 使用 validate/prepare/replace-many，并在活动编辑器同步失败时恢复 store、editor 和 ProjectRevision。
- compound Undo/Redo 覆盖整组文档，任一 stale 成员整体拒绝；Save Current 仍只写当前文件。
- multi-file Diff 复用单文件 builder，增加 internal `FileHeader`，全局限制按项目累计。
- `ShellWindow.xaml`、布局、AutomationId、parser、Field Registry、Completion 与 Save/Backup 实现均未修改；
  Diagnostics 算法未改，仅在既有全项目诊断请求上增加 internal 内存文本覆盖输入。

## Deferred Governance Queue（已刷新）

| 类型 | 项目 | 结果 |
|---|---|---|
| Public API | 4 types + `PreviewProject` + capability | 已刷新到 `PublicApiLedger.md` |
| Architecture decision | project store owner + pure Application Preview | 已刷新到 `DecisionLog.md` |
| Technical debt | 首个 rules/art consumer、Save All/退出确认 UI | 保持后置，不属于本包缺陷 |
| Current status/context | CONTENT-2D-2 completed | 已刷新 |

## 当前停止点

CONTENT-2D-2 已完成。下一安全入口是首个 source-backed rules/art 多文档 consumer/profile 契约；
CONTENT-2C AI tuple 写入继续冻结，不得把项目 Preview 误写成自动 Apply/Save。
