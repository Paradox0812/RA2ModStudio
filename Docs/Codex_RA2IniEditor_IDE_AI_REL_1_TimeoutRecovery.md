# AI-REL-1 Timeout Recovery

## 1. 状态

```text
实现状态：Completed
契约确认：用户已确认 AI-REL-1 最终契约
Task Cards：AI-REL-1A、AI-REL-1B、AI-REL-1C 已完成；AI-REL-1D 人工烟测已由用户确认通过
日期：2026-07-20
```

## 2. 功能目标

- 超时、取消、不完整、服务错误、配置缺失、消费端异常和流式一致性失败后，不把该次用户提示或失败助手轮次带入下一次 AI 请求。
- 失败或不完整卡片继续保留已收到文本与复制能力。
- 在失败卡片上提供“恢复提示词”，仅将原始提示文本回填到空输入框，不自动发送。
- 保持 AI-STREAM-0 至 AI-STREAM-3 的单请求、取消、流式传输和终结语义不变。

## 3. 明确非目标

- 不自动重试，不做即时重发、退避或 AttemptId / ExchangeId。
- 不冻结或持久化完整请求快照，不保存 API Key。
- 恢复后再次发送使用当时选择的模型和当前 IDE 上下文。
- 不修改 DeepSeek transport、SSE parser、Pipeline、PromptBuilder 或请求生命周期。
- 不修改 `ShellWindow.xaml`、Field Registry、Completion、Hover、Diagnostics、Save Preflight 或 legacy。

## 4. 内部接口契约

### `Ra2AiConversationTurn.IsContextEligible`

```csharp
public bool IsContextEligible { get; init; } = true;
```

该成员位于 internal 类型中，不是外部 public API。生命周期仅限当前 Shell 聊天卡片，不序列化、不持久化。

`Ra2AiConversationContextProvider` 仅复用同时满足以下条件的轮次：

```text
State == Completed && IsContextEligible
```

过滤发生在 `LastTurns` 和字符预算计算之前；Provider 创建新副本，不反写源轮次。

## 5. Shell 终结语义

| 实际终结状态 | 助手轮次 | 发起用户轮次 | 恢复动作 |
|---|---|---|---|
| `Completed` | 可进入上下文 | 可进入上下文 | 不显示 |
| `Incomplete` | 不可进入上下文 | 不可进入上下文 | 显示 |
| `Error` | 不可进入上下文 | 不可进入上下文 | 显示 |

资格以最终渲染的 `Ra2AiConversationTurnState` 为准，因此即使原始响应种类为 Success，流式一致性校验失败仍按 Error 处理。

每个流式响应句柄持有其发起用户卡片和动作面板。若异常发生在用户卡片创建之后、流式卡片创建之前，静态失败卡片执行相同的用户资格关闭和恢复动作接入。

## 6. 恢复提示词契约

- 按钮文本：`恢复提示词`
- 按钮 AutomationId：`AiAssistant.RestorePromptButton`
- 状态 AutomationId：`AiAssistant.RestorePromptStatus`
- Automation Name：`恢复提示词`
- HelpText：`仅恢复文本，不会自动发送；再次发送可能产生服务费用。`
- 输入框为空或仅空白时：写入提交时已 Trim 的提示，聚焦输入框，光标移到末尾。
- 输入框已有内容时：绝不覆盖，显示 `输入框已有内容，未覆盖。`
- 恢复成功后显示：`提示词已恢复到输入框，尚未发送。`
- 该点击处理程序不调用 Generate、Pipeline、DeepSeek 或任何网络路径。

## 7. 修改范围

### 代码与测试

- `RA2IniEditor.IDE/AI/Ra2AiConversationTurn.cs`
- `RA2IniEditor.IDE/AI/Ra2AiConversationContextProvider.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiConversationContextProviderTests.cs`
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`

### 文档

- 本文档
- `Docs/Codex_CurrentPhase.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `Docs/KnownIssues_v0.5.0-preview.md`
- `Docs/UserGuide_v0.5.0-preview.md`

## 8. Stage Result Ledger

| Stage | 目标 | 验证 | 状态 |
|---|---|---|---|
| AI-REL-1A | 上下文资格模型与 Provider 过滤 | Provider 定向测试 15/15 | Completed |
| AI-REL-1B | Shell 请求归属、失败恢复和自动化边界 | IDE-only build；AI/DeepSeek/Shell 定向测试 231/231 | Completed |
| AI-REL-1C | 全量回归、真实窗口检查、文档与洁净包 | 全量测试 2083/2083；AI 面板/进阶区可见；洁净包 898 个文件 | Completed |
| AI-REL-1D | 恢复提示词动态人工烟测 | 用户确认恢复、焦点、非覆盖、无自动发送和上下文隔离测试通过 | Completed |

## 9. Verification Matrix

| 检查 | 状态 | 证据 |
|---|---|---|
| IDE-only build | Passed | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`，0 警告 / 0 错误 |
| Provider 定向测试 | Passed | 15/15 |
| AI / DeepSeek / Shell / Automation 定向测试 | Passed | 231/231 |
| 全量测试 | Passed | 2083/2083 |
| 真实 WPF 启动与静态布局 | Passed | Debug IDE 可启动；AI 面板和进阶模型区可见；未打开项目或文件 |
| 恢复按钮动态交互 | Passed | 2026-07-20 用户明确确认第 12 节人工测试通过 |
| IdeOnly clean package | Passed | `artifacts/RA2IniEditor.IDE.SourceClean.zip`，最终归档 898 个文件 |

## 10. 决策记录

### Decision: 恢复文本而不重试请求

- Status: Accepted
- Context: 超时或失败后自动重试可能重复计费，也可能在上下文变化后产生不同请求。
- Decision: 将失败请求的两轮上下文均隔离；只提供显式文本恢复，由用户决定是否再次发送。
- Rejected Alternatives: 自动重试、失败后立即重发、冻结完整请求快照。
- Consequences: 安全边界清晰且无隐式费用；再次发送使用当前模型和当前上下文，不保证与原请求完全相同。

## 11. Resolved Verification Debt

| ID | 区域 | 原因 | 原风险 | 偿还证据 | 状态 |
|---|---|---|---|---|---|
| AI-REL-1-UI-VERIFY | 动态恢复控件 | Codex WPF UIA ValuePattern / 桌面悬浮层曾阻塞自动交互 | 焦点、非覆盖提示和窄宽度布局缺少真实点击证据 | 2026-07-20 用户确认第 12 节人工测试通过 | Resolved |

AI-REL-1 没有剩余代码捷径、兼容适配器、TODO 或验证债务。

## 12. 人工烟测步骤（已完成）

1. 启动 Debug IDE，不打开项目文件。
2. 打开右侧 AI 面板，在进阶区选择 DeepSeek。
3. 输入不含敏感信息的合成提示并发送；在请求进行时取消，或使用短超时环境触发 Timeout。
4. 确认失败/不完整回答仍可见、可复制，且出现“恢复提示词”。
5. 保持输入框为空，点击恢复：原提示应恢复，输入框获得焦点，光标位于末尾，状态显示“尚未发送”。
6. 在输入框预先输入不同文本，再次点击恢复：已有文本不得被覆盖，并显示“输入框已有内容，未覆盖。”
7. 确认点击恢复不会启动请求；只有再次点击发送才可能产生服务费用。
8. 再次发送前确认对话上下文摘要未计入上一次失败请求的用户轮次和助手轮次。

验收结果：用户于 2026-07-20 明确确认上述测试通过。

## 13. 下一安全入口

`AI-REL-2A FailureTaxonomyContract`：只设计鉴权、限流、服务不可用、网络/代理、总超时、流空闲超时、协议错误和配置缺失的内部失败分类与安全提示契约。该阶段必须先输出精确契约并等待用户确认，不得直接修改 transport、Shell 或 public API。
