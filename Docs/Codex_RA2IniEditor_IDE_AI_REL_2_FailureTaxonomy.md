# AI-REL-2 Continuous Failure Taxonomy

## 1. 状态

```text
Package: AI-REL-2
Risk: R4 network/security boundary
Contract: Confirmed by user on 2026-07-20
Execution mode: Continuous StagePackage; no per-stage approval required
Current stage: AI-REL-2 completed
Governance mode: Deferred queue flushed at package completion
```

## 2. 目标与非目标

本包为 DeepSeek 请求建立内部结构化失败分类，区分鉴权/授权、限流、请求拒绝、provider 超时、服务不可用、网络/代理、本地总超时、流空闲超时、协议错误、响应超限和配置缺失，并由 Shell 显示固定安全提示。

本包不自动重试、不读取 `Retry-After`、不保存请求快照或 API Key、不读取 provider 错误正文、不修改 timeout 默认值、线上请求或 SSE payload，不修改 `IRa2AiClient`、Pipeline、PromptBuilder、请求生命周期、SSE parser、`ShellWindow.xaml`、AutomationId、Field Registry 或 legacy。

## 3. 架构与数据所有权

```text
DeepSeek HTTP/SSE
  -> DeepSeekRa2AiClient（分类与请求局部首终止原因）
  -> Ra2AiResponse（Kind + FinishKind + FailureKind + 安全 ErrorMessage）
  -> Ra2AiAssistantPipeline（原样透传）
  -> DeepSeekRa2AiFailureUiMessageFormatter（固定安全中文提示）
  -> Shell（终态、上下文资格与恢复提示词）
```

- `Ra2AiResponseKind` 只表达终态。
- `Ra2AiStreamFinishKind` 只表达模型完成原因。
- `Ra2AiFailureKind` 只表达失败原因。
- FailureKind 随单次响应存活，不序列化、不持久化、不进入对话历史。
- Shell 不解析或直接展示 `ErrorMessage`。

## 4. 冻结的内部契约

```csharp
internal enum Ra2AiFailureKind
{
    None = 0,
    MissingConfiguration,
    AuthenticationOrAuthorization,
    RateLimited,
    RequestRejected,
    ProviderRequestTimeout,
    ServiceUnavailable,
    NetworkOrProxy,
    TotalTimeout,
    StreamingIdleTimeout,
    ProtocolError,
    ResponseTooLarge,
    Unknown
}
```

`Ra2AiResponse` 在现有构造函数末尾增加可选 `failureKind = None`，并增加只读 `FailureKind`。构造器不校验或改写组合，确保 Fake 和旧测试兼容；AI-REL-2 只强制 DeepSeek producer 完整分类。

AI-REL-2B 验证通过后，枚举既有值语义、构造参数顺序和属性签名冻结。2C/2D 不得回头修改。

## 5. Transport 映射

| 输入 | FailureKind |
|---|---|
| 配置无效 | MissingConfiguration |
| HTTP 401 / 403 | AuthenticationOrAuthorization |
| HTTP 429 | RateLimited |
| HTTP 408 / 504 | ProviderRequestTimeout |
| 其他 5xx | ServiceUnavailable |
| 其他非成功 HTTP | RequestRejected |
| 无状态码 HttpRequestException / IOException | NetworkOrProxy |
| 错误 Content-Type、JSON/SSE/UTF-8、异常 EOF、缺少内容 | ProtocolError |
| 超过本地累计字符上限 | ResponseTooLarge |
| 本地总超时 | TotalTimeout |
| 流空闲超时 | StreamingIdleTimeout |

Content-Type 缺失继续按当前兼容行为尝试 SSE 解析。非成功 HTTP 响应正文不得读取。`ErrorMessage` 只能包含固定英文文本和数字状态码，不得包含正文、API Key、prompt、endpoint/代理详情、原始异常 Message 或堆栈。

## 6. 终止来源

请求开始后，用户取消、总超时和流空闲超时通过请求局部 `Interlocked.CompareExchange` 记录首个信号；后续信号不得覆盖。预取消继续立即返回 Cancelled。Idle timeout 只在首个 delta callback 成功完成后启动，并在后续 callback 成功完成后重置。无静态状态或请求字典。

## 7. Shell 契约

新增无状态 `DeepSeekRa2AiFailureUiMessageFormatter`，提供 standalone 和 partial-terminal 两个显式入口。Shell 仍只用 ResponseKind 决定 TurnState、错误样式和 AI-REL-1 上下文资格；FailureKind 只选择安全提示。无 XAML、布局、样式或 AutomationId 变更。“恢复提示词”仍只恢复文本、不发送、不覆盖已有输入。

## 8. Task Cards

| Stage | Goal | State |
|---|---|---|
| AI-REL-2A | ContractAndArchitectureReview | Completed |
| AI-REL-2B | FailureModelAndContractFreeze | Completed: build passed; targeted tests 10/10 |
| AI-REL-2C | TransportClassificationAndTerminationCause | Completed: build passed; targeted tests 63/63 |
| AI-REL-2D | SafePresentationAndShellIntegration | Completed: build passed; targeted tests 60/60 |
| AI-REL-2E | FullVerificationUiSmokeAndDocumentationClosure | Completed: cross-stage tests 263/263; full tests 2115/2115; live DeepSeek UI smoke passed |

## 9. 连续执行停线条件

- 需要修改 2B 冻结契约；
- 需要读取 provider 错误正文；
- 需要修改 `IRa2AiClient`、Pipeline、SSE parser、请求生命周期或 `ShellWindow.xaml`；
- 需要重试、Retry-After、持久化、新依赖或项目文件；
- 必需测试失败且修复超出当前 Task Card；
- UI 烟测失败且需要布局或新交互修改。

## 10. Stage Result Ledger

| Stage | Result | Review conclusion |
|---|---|---|
| 2A | 回归现有 response、transport、streaming、Shell 和恢复链路，冻结连续契约 | 通过；未发现必须先返工的接口冲突 |
| 2B | 新增内部失败枚举与 `Ra2AiResponse.FailureKind`，保持旧构造调用兼容 | 通过；无外部 public API、序列化或生命周期影响 |
| 2C | DeepSeek transport 完成 HTTP、网络、协议、容量与首终止原因分类 | 通过；未读取错误正文，未泄露原始异常信息，未引入自动重试 |
| 2D | Shell 通过无状态 formatter 显示固定安全中文提示 | 通过；TurnState、上下文资格、恢复提示词和 AutomationId 保持不变 |
| 2E | 完成跨阶段测试、全量测试、真实 UI/DeepSeek 烟测、文档和源码包收口 | 通过；真实服务本次成功，因此故障提示由确定性单元/边界测试覆盖 |

## 11. Verification Matrix

| Layer | Evidence | Result |
|---|---|---|
| Restore | `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| Build | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed；0 errors；1 个既有测试 CS8602 warning |
| 2B targeted | response / pipeline tests | 10/10 passed |
| 2C targeted | DeepSeek client / factory / pipeline tests | 63/63 passed |
| 2D targeted | formatter / Shell / context tests | 60/60 passed |
| Cross-stage | `Ra2Ai|DeepSeek|IdeShellBoundaryTests|WpfAutomationHarnessBoundaryTests` | 263/263 passed |
| Full suite | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | 2115/2115 passed |
| UI smoke | Debug WPF 启动、AI 面板、进阶区、DeepSeek 选择、一次无敏感内容请求、完整回复与控件复位 | Passed |
| Clean package | `powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly` | Passed；903 files；`artifacts/RA2IniEditor.IDE.SourceClean.zip` |

UI 烟测没有人为制造 401、429、5xx、网络中断或超时；这些分支由 transport 与 formatter 的确定性测试验证。烟测期间没有打开项目或编辑器文件，未发生文件写入。

## 12. Governance Flush

### Internal Contract Ledger

| Stage | API | Kind | Reason | Expected next use | Stability |
|---|---|---|---|---|---|
| 2B | Ra2AiFailureKind | internal enum | 结构化失败原因 | 2C transport / 2D Shell | Stable |
| 2B | Ra2AiResponse.FailureKind | internal DTO property | 跨层透传分类 | 2C / 2D | Stable |

### Accepted decisions

- 终态、模型完成原因与失败原因采用正交三轴模型。
- 分类发生在 transport；Shell 不解析诊断字符串。
- 请求终止原因采用 first-signal-wins。
- 本包不引入重试策略。

### Technical debt

无。本包没有兼容 shim、TODO、部分验证、临时行为或遗留代码债。

## 13. Package conclusion

AI-REL-2 已完成，不自动进入重试策略。若未来需要自动重试、`Retry-After`、退避、请求快照或遥测，必须以新的用户确认契约重新评估重复请求、费用、隐私和请求幂等性。

## 14. AI-REL-TD-001 narrow reliability amendment — 2026-07-21

The user separately authorized a narrow repair for a full-suite race in the existing `first-signal-wins` termination contract. The linked cancellation token could propagate total-timeout cancellation into the HTTP handler before the `TotalTimeout` attribution callback ran; if that handler synchronously cancelled the user token, `UserCancellation` could be recorded first.

`DeepSeekRa2AiClient` now uses one request-local cancellation source. User cancellation, total timeout, and streaming idle timeout each atomically claim the existing `RequestTerminationCause` before the winning source propagates cancellation to the HTTP/SSE request. Losing sources do not overwrite the recorded cause.

Contract impact:

- No public API, enum, DTO, failure kind, error text, timeout value, SSE behavior, or model policy changed.
- No automatic retry, request replay, model fallback, persistence, or new dependency was introduced.
- The existing `SendAsync_LateUserCancellationDoesNotOverrideEarlierTotalTimeout` regression remains unchanged and is the contract test.

Verification:

- Solution build: passed with 0 errors and one pre-existing CS8602 warning.
- Exact regression: 20/20 passed.
- `DeepSeekRa2AiClientTests`: 62/62 passed.
- Full suite: two consecutive runs passed 2278/2278.
- IdeOnly clean package: passed with 934 source files.

Status: `AI-REL-TD-001` resolved. The AI-REL-2 prohibition on automatic retry remains unchanged.
