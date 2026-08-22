# AUTOMATION-HLI-2B Gateway Consumer Stage Ledger

日期：2026-08-23
状态：Completed / Verified
最终契约：`Docs/AUTOMATION-HLI-2B_GatewayConsumerFinalContract.md`

## 1. 阶段结果

HLI-2B-1..2B-4 已连续完成。内置 AI 的明确当前文件编辑请求现在通过现有唯一
`Ra2IniEditPreviewService` 调用 `IRa2AutomationCapabilityGateway.Preview`。旧的 internal
`PreviewForHost` unlimited bypass 已删除，未新增第二 adapter。

Shell composition root 创建并共享一个 Gateway instance：同一实例注入 Host adapter，并在
provider 请求前从 `DocumentEditPreview` descriptor 读取资源限制。超过 8,388,608 UTF-16
字符的明确编辑会在本地拒绝并提示“尚未发送”；普通 advisory 仍可使用既有截断上下文。

## 2. Stage Result Ledger

| Task Card | 实现 | 自审结果 | 验证 |
|---|---|---|---|
| HLI-2B-1 Gateway Adapter Switch | adapter 改为 interface-injected typed Gateway；删除 `PreviewForHost` | 无第二 adapter、无算法复制、Workspace authority 不变 | Preview/Workspace/取消/7 MiB 与新契约测试通过 |
| HLI-2B-2 Budget Preflight | descriptor 唯一项/version/risk/字符/operation 校验；新增 internal resource availability | 无第二限制常量；preflight 位于 pipeline/session/send 前；advisory 不阻断 | HLI-2B + route tests 30/30 |
| HLI-2B-3 Regression Gates | public、A4、HLI-1C、transaction 与完整 non-UI 门禁 | allowlist 35；Provider/Prompt/Tool/Apply/Save/XAML 零语义变化 | Application 94/94；focused 78/78；full 2547/2547 |
| HLI-2B-4 Governance/Package | 契约、能力、API、决策、路线图和上下文收口 | Deferred governance 已 flush；未进入 HLI-2C | IdeOnly clean package Passed |

## 3. 代码与接口影响

- Public API：0 change；Application exported allowlist 精确保持 35。
- Persistence/wire：无变化。
- Application：只删除 internal unlimited bypass；public Preview engine/limits/failure 不变。
- IDE adapter：只消费 typed Gateway 并继续使用 `Ra2IniEditPreview.FromAutomation` guard。
- Shell：只增加 Gateway composition、descriptor preflight 和资源提示。
- XAML/AutomationId：0 change。
- Apply/Undo/Save：0 change；仍由 Workspace/Shell/用户拥有。
- Provider/prompt/tool schema/model/endpoint policy：0 change。

## 4. Verification Matrix

| Gate | 命令/证据 | 结果 |
|---|---|---|
| Restore | `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| Debug build | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed；0 warnings，0 errors |
| Application | `dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build` | Passed 94/94 |
| HLI-2B/A4/HLI-1C/Shell focused | 最终契约 filter | Passed 78/78 |
| Full non-UI | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | Passed 2547/2547 |
| New contract facts | `Ra2Hli2BGatewayConsumerContractTests` | 8 facts；Passed |
| Router additions | explicit resource reject + advisory preservation | Passed |
| 7 MiB boundary | existing performance case through new Gateway adapter | Passed |
| 8,388,609 boundary | typed `DocumentTooLarge`，无 candidate/active authority | Passed |
| Static source | no `PreviewForHost` / no Shell numeric duplicate / approved files only | Passed |
| Public reflection | exported allowlist 35；Gateway interface 5 methods | Passed |
| UI/computer control | 无视觉变化 | NotRun by contract |
| IdeOnly clean package | `tools/package-source-clean.ps1 -Profile IdeOnly` | Passed；禁止条目 0 |

## 5. 变更文件

Production/test：

```text
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationEditPreviewService.cs
RA2IniEditor.IDE/AI/Ra2AiInteractionRoute.cs
RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs
RA2IniEditor.Tests/IDE/Ra2Hli2BGatewayConsumerContractTests.cs
```

Governance：本台账及最终契约、CurrentCapabilities、PublicApiLedger、DecisionLog、
DevelopmentRoadmap、Codex_CurrentPhase、Compact Context 和 Docs README。

## 6. 剩余风险与停止点

- 8 MiB 以上当前文件的 AI 结构化编辑被明确收窄；advisory 不受该限制。
- 当前仍没有独立 Agent/CLI、public Apply/Save、wire contract 或 Job/Event/Artifact runtime。
- Shell preflight 是成本门禁，Gateway 执行限制仍是最终权威；非 Shell caller 无法绕过限制。
- 本阶段无视觉变化，因此不需要 UI/电脑操控验收。

当前停止点：HLI-2B Completed / Verified。下一推荐阶段是 HLI-2C 首个高层 Agent 闭环的
代码事实审计与最终契约；不得自动进入实现。
