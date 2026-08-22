# AUTOMATION-HLI-1A1 Document Query Slice Stage Ledger

状态：Completed / Verified
日期：2026-08-22
权威契约：`Docs/AUTOMATION-HLI-1A1_DocumentQuerySliceFinalContract.md`
基线提交：`bc20efccb94ebdc4d363942c3f00464d5e9c01ba`
风险：R3 程序集迁移 + R2 Experimental public API

## 1. 结果

HLI-1A1 已在真实主仓库完成。新增 `RA2IniEditor.Application` 和
`RA2IniEditor.Application.Tests`；22 个 Query foundation 文件从 WPF IDE assembly
原子迁移到 `net8.0` Application，IDE 和现有测试改为消费同一份 internal 实现。

新增且仅新增契约冻结的 15 个
`RA2IniEditor.Application.Automation.Experimental` exported types，交付：

- `ini.document.section.get`
- `ini.document.references.find`

没有实现 Diagnostics、Preview、Apply、Save、Gateway、CLI、IPC、MCP 或项目级搜索。

## 2. Stage Result Ledger

| Stage | Goal | Files Touched | Verification | State After Stage | Next Entry Satisfied |
|---|---|---|---|---|---|
| HLI-1A1-0 | 建立可审计本地基线 | `.git` 本地元数据；生产文件未改 | baseline 54/54；提交 `bc20efc` | Completed | 是 |
| HLI-1A1-1 | 新建 Application/Application.Tests 和 solution graph | solution、2 个新 project、IDE/Tests references | restore；Debug solution build | Completed | 是 |
| HLI-1A1-2 | 原子迁移 22 个 internal 文件 | Application Classification/Language、IVT/global using、兼容 using/tests | 22/22；旧路径 0；逐行迁移等价 0 failures；54/54 | Completed | 是 |
| HLI-1A1-3 | 实现 15-type Experimental query contract | `Automation/Experimental/**`、Application.Tests | reflection exact allowlist；Application.Tests 31/31 | Completed | 是 |
| HLI-1A1-4 | IDE integration 与完整回归 | 现有静态边界断言的最小路径/类型接线 | existing tests 2526/2526 | Completed | 是 |
| HLI-1A1-5 | API、依赖、diff、包和治理收口 | audits、本文及当前状态文档 | static audits passed；IdeOnly package 1086 files | Completed | 停止于 HLI-1A1 |

## 3. AgentPilot Lite / Luna 执行记录

用户授权后只执行了一次 Luna 请求；没有 retry、换模型或 provider fallback。

```text
Provider: luna
Model: gpt-5.6-luna
Reasoning effort: max (runtime-enforced)
TaskResult: failed
Backend exit: 0
Workspace/scope/diff-check: passed
Commands gate: failed at first solution build
Retained worktree:
H:\AgentPilotLite.Workspaces\RA2IniEditor-HLI-1A1\RA2-HLI-1A1-DocumentQuerySlice-9b0b40f8
```

失败原因不是生产设计错误，而是最终契约的直接消费者清单遗漏一个完全限定旧类型：
`Ra2IniTextDocumentLineSpanTests.cs` 使用
`RA2IniEditor.IDE.Language.Ra2TextSpan`。主 Agent 没有重试模型，而是在保留候选上
完成代码审查和窄边界兼容修复；随后又通过完整构建发现并修复 WPF
`Application` 同名命名空间冲突，以及两份旧静态边界断言。没有添加 shim、type
forwarding 或第二套实现。

AgentPilot 返回的实际 usage：

| Metric | Value |
|---|---:|
| Input tokens | 9,029,741 |
| Output tokens | 74,076 |
| Total tokens | 9,103,817 |
| Cached input tokens | 8,775,424 |
| Reasoning output tokens | 41,575 |
| Total duration | 1,744,410 ms |

## 4. Public API 与数据边界

- Exported types：精确 15 个，reflection 针对整个 Application assembly，而不是只筛
  Experimental namespace。
- Internal foundation：22 个类型全部保持 internal；IVT 仅 IDE、Tests、Application.Tests。
- Application 仅引用 Core；无 WPF、IDE、Infrastructure、Avalon、I/O 或 runtime singleton。
- Snapshot 由 host 捕获并拥有；service 无状态，每次调用构建 invocation-local model。
- Section/Reference 失败使用 typed failure，limit/cancel/analysis 不返回 partial payload。
- 不新增序列化、持久化或文件写入格式。

## 5. Verification Matrix

| Step | Status | Evidence |
|---|---|---|
| Baseline Query regression | Passed | 54/54 before migration |
| Restore | Passed | `dotnet restore .\RA2IniEditor.IDE.sln` |
| Build / Compile | Passed | Debug solution；0 errors；1 条既有 CS8602 warning |
| Headless contract tests | Passed | Application.Tests 31/31 (`net8.0`) |
| Migrated dependency regression | Passed | 54/54 |
| Existing full regression | Passed | RA2IniEditor.Tests 2526/2526 |
| Static architecture/API audit | Passed | 22/22、old 0、Core-only、forbidden 0、stale qualified 0、diff-check passed |
| Source package | Passed | `artifacts/RA2IniEditor.IDE.SourceClean.zip`；1086 entries；10.29 MiB；forbidden 0 |
| UI / computer control | NotRun | 本阶段无 UI 行为变化，契约不要求 |

既有 warning 位于
`RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs:1961`，在
HLI-1A0 及更早台账中已记录；本阶段未修改该 Field Registry 测试。

## 6. Diff Intent Table

| File group | Change Type | Reason | In Allowed Scope |
|---|---|---|---|
| `RA2IniEditor.Application/**` | Add/move | 建立 Core-only Query authority | Yes |
| `RA2IniEditor.Application.Tests/**` | Add | 证明真正 headless 边界与失败语义 | Yes |
| IDE Classification/Language 22 old paths | Delete/move | 消除 WPF assembly 内唯一实现 | Yes |
| solution / IDE / Tests project wiring | Modify | 引用新 assembly、IVT/global using | Yes |
| 兼容 consumers / boundary tests | Modify | 清退完全限定旧 namespace 和同步架构断言 | Yes；实施审查补充 |
| HLI/API/Decision/CurrentStatus/Roadmap docs | Modify | 正常 governance flush | Yes |

Shell/XAML、AutomationId、TextModel、Diagnostics、Preview/Apply/Save、Field Registry
runtime/data、Completion/Hover/AI/Search、Infrastructure、Core 和 legacy 均未修改。

## 7. Deferred Governance Queue

### Flushed

- PublicApiLedger：15 个 API 更新为 Implemented / Experimental。
- DecisionLog：既有 HLI-1A1 decision 从 Proposed 更新为 Accepted。
- CurrentStatus/Context/Roadmap/Capabilities：更新为 HLI-1A1 Completed / Verified。

### Remaining

- Technical debt：本阶段无新增。兼容漏项已在当前阶段修复，未留下 shim/TODO。
- 既有 CS8602：维持历史基线记录，等待独立 Field Registry 测试卫生任务。
- HLI-1A2：尚未契约或实施。

## 8. Stop Rule

已停止于 HLI-1A1。下一安全入口是 HLI-1A2 Diagnostics 的只读代码事实回归、
依赖锥确认和最终契约；不得据此自动开始生产迁移。
