# AI-REL-3 Provider Trust, Privacy, Resource and Observability Hardening

## 1. 契约状态

```text
Package: AI-REL-3
Risk: R4（网络端点、模型身份、隐私、成本与观测边界）
Contract state: Confirmed by user on 2026-07-20
Execution mode after approval: Continuous StagePackage
Per-stage approval after package approval: Not required
Current action: Completed through AI-REL-3I
Runtime implementation: Completed and verified
Automatic retry: Explicitly out of scope
```

本文件是 AI-REL-2 完成后的下一份连续契约。用户已确认先吸收最终自审修正，再进入连续执行。执行顺序为 `3-0 -> 3A -> 3C1 -> 3C2 -> 3B -> 3D -> 3E -> 3F1 -> 3F2 -> 3G -> 3H -> 3I`；每个 Task Card 都必须先自审和通过规定验证，遇到停线条件立即停止，不得带病进入下一阶段。

## 2. 官方模型事实与当前代码事实

### 2.1 DeepSeek 官方事实（2026-07-20 核对）

- OpenAI ChatCompletions 接口当前正式模型标识为 `deepseek-v4-flash` 与 `deepseek-v4-pro`。
- 两个 V4 模型默认启用 thinking；显式发送 `thinking.type=disabled` 才是非思考模式。
- `deepseek-chat` 与 `deepseek-reasoner` 将于 2026-07-24 15:59 UTC 停用，不再作为本项目可选模型。
- 官方 OpenAI 格式 Base URL 仍为 `https://api.deepseek.com`。

核对来源：

- [DeepSeek Your First API Call](https://api-docs.deepseek.com/)
- [DeepSeek Models & Pricing](https://api-docs.deepseek.com/quick_start/pricing)
- [DeepSeek Thinking Mode](https://api-docs.deepseek.com/guides/thinking_mode)
- [DeepSeek Create Chat Completion](https://api-docs.deepseek.com/api/create-chat-completion)

### 2.2 当前实现事实

1. `DeepSeekRa2AiClientFactory.DefaultModel` 与 `DeepSeekRa2AiClientOptions.Model` 当前默认均为 `deepseek-v4-pro`。
2. Shell 模型选择器当前只有 `Mock` / `DeepSeek`，默认 `Mock`；它实际选择的是 provider，不是模型。
3. Shell 生产路径保有 `FakeRa2AiClient`、默认 Fake Pipeline，以及 Mock/DeepSeek 两套终态文案分支。
4. `DEEPSEEK_MODEL` 可覆盖实际模型，导致 UI 只显示“DeepSeek”而不能准确说明实际请求模型。
5. 请求体发送 `temperature=0.2`、`stream=true`，但没有显式 thinking 模式和 `max_tokens`。V4 默认 thinking 时 temperature 不生效。
6. options 只要求 Base URL 是绝对 URI，未限制 scheme、远程明文 HTTP、userinfo、query 或 fragment。
7. 非空但无效的 Base URL/timeout 环境变量会静默回退默认值，配置错误不可见。
8. options 的 `ToString()` 会输出完整 Base URL。
9. Nearby text 已有 4000 字符上限；最近会话已有 6 轮、总计 6000、单轮 2000 字符上限，并带有限敏感文本清理。
10. 当前 user prompt 与显式 selected text 没有统一出站上限；PromptBox 没有 `MaxLength`；最终 PromptText 没有总字符预算。
11. transport 已有 1 MiB 累积正文硬上限，但没有 provider 输出 token 上限；Shell Markdown 控件数量和可见消息卡数量没有硬上限。
12. `Ra2AiResponse` 构造器允许 Success + ErrorMessage、Success + 非 Stop、失败 + FailureKind.None 等矛盾组合。
13. 现有失败分类、first-signal-wins、取消、总超时、首正文后 idle timeout、部分响应保留与上下文隔离已经验证，不得回退。
14. 部分 timeout 测试依赖 10–20 ms 墙钟延迟；没有真实 loopback HTTP/SSE 分片测试，也没有本地动态失败 UI 烟测。

## 3. 功能目标

1. 从产品 UI 和生产运行路径彻底移除 Mock；测试替身只能存在于测试项目。
2. 模型选择器仅提供 DeepSeek V4 Flash / V4 Pro，默认 V4 Flash，并保证 UI 选择就是本次请求使用的模型。
3. 显式冻结为非思考模式，避免模型默认值变化隐式改变时延、费用和 SSE 行为。
4. 收紧 endpoint/config 信任边界，使“缺失配置”和“无效配置”可区分、可诊断且不会静默改写。
5. 建立统一出站敏感文本清理与字符预算，避免过大选区、提示词和上下文无界进入网络请求。
6. 增加 provider 输出 token 上限和 Shell 渲染/历史资源上限。
7. 收紧响应状态组合，禁止矛盾结果进入 Pipeline、Shell 或对话上下文。
8. 增加不含敏感内容的单请求观测事实，能区分连接、首正文、流式消费和总耗时。
9. 用稳定的 transport/loopback/边界/UI 验证替代脆弱的极短墙钟测试。
10. 保持 AI-STREAM-0~3、AI-REL-1、AI-REL-2 已确认的生命周期和安全表现。

## 4. 非目标

- 不实现自动重试、退避、`Retry-After`、请求重放或失败后自动换模型。
- 不增加 thinking 开关、reasoning effort、工具调用、联网检索或 Agent 能力。
- 不保存 API Key、Base URL、模型选择、提示词、回复或诊断到磁盘。
- 不增加 API Key 输入框或通用设置页。
- 不读取或显示 provider 错误正文、原始异常 Message、请求 payload 或响应 payload。
- 不改变 INI parser、Field Registry、Completion、Hover、Quick Peek、Diagnostics、Save Preflight、Undo/Redo 或文件写入行为。
- 不修改 Shell 主布局、工具栏、菜单、项目浏览器、导航器、底部工具窗、状态栏或 docking 结构。
- 不恢复 legacy 工程、legacy MainWindow 或旧表格式编辑器。
- 不引入第三方包、日志框架、遥测 SDK 或持久化数据库。

## 5. 冻结的架构决策

### D1：Mock 的移除范围

- 删除生产项目中的 `FakeRa2AiClient.cs`。
- 删除 Shell 的 Fake 字段、默认 Fake Pipeline、`Ra2AiProviderMode`、Mock 分支和 Mock 文案。
- `IRa2AiClient` 继续作为测试与 provider 边界，不删除。
- 确定性测试改用测试文件内 private/internal stub、recording client 或 controlled client；不得把测试 Fake 再放回生产程序集。
- 历史阶段文档保留原始事实，不批量重写；CurrentPhase/FullContext 只登记其已被 AI-REL-3B supersede。

### D2：模型选择权威

新增内部模型枚举：

```csharp
internal enum DeepSeekRa2AiModel
{
    V4Flash = 0,
    V4Pro = 1
}
```

新增唯一映射：

```text
V4Flash -> deepseek-v4-flash
V4Pro   -> deepseek-v4-pro
```

- Shell 默认选择 `V4Flash`。
- UI 文案固定为 `DeepSeek V4 Flash` / `DeepSeek V4 Pro`。
- `DEEPSEEK_MODEL` 不再是生产配置源；相关常量、读取与文档说明删除。
- 每次点击发送时先捕获本次 model value，活动请求期间禁用 selector；后续 UI 选择变化不得影响已启动请求。
- Factory 的模型参数是唯一生产模型输入；不得按 `SelectedIndex == 1` 等脆弱索引隐式判断 provider。
- model catalog 同时提供 enum、显示名和 API ID；Shell 通过 typed `ItemsSource` / `SelectedValue` 取得 `DeepSeekRa2AiModel`。
- 禁止 Shell 解析显示文本、字符串 Tag、ComboBox 索引或 API ID 来反推 enum。

冻结 UI option：

```csharp
internal sealed record DeepSeekRa2AiModelOption(
    DeepSeekRa2AiModel Value,
    string DisplayName,
    string ApiModelId);
```

冻结签名：

```csharp
internal static DeepSeekRa2AiClientOptions CreateOptionsFromEnvironment(
    DeepSeekRa2AiModel model = DeepSeekRa2AiModel.V4Flash);

internal static IRa2AiClient CreateClientFromEnvironment(
    DeepSeekRa2AiModel model = DeepSeekRa2AiModel.V4Flash);
```

### D3：V4 思考模式与输出边界

- 两个 UI 模型都显式发送 `thinking: { "type": "disabled" }`。
- 本阶段不允许 Flash 与 Pro 通过不同 thinking 默认值产生隐式行为差异。
- `temperature=0.2` 保持不变，并在 options 中校验为有限数且位于 `[0, 2]`。
- 新增 `MaxOutputTokens`，默认 `8192`，允许范围 `1..32768`；请求体显式发送 `max_tokens`。
- 现有 1 MiB 累积字符上限继续作为 provider 不遵守 token 预算时的最终硬防线。
- 如果未来需要 thinking，必须新建契约，重新评估 reasoning_content、总超时、首正文等待、费用和会话上下文。

### D4：配置和 endpoint 信任边界

- `DEEPSEEK_API_KEY`：缺失或空白 -> MissingConfiguration。
- `DEEPSEEK_BASE_URL`：变量缺失/空白 -> 官方默认；变量非空但无效 -> 配置失败，不静默回退。
- `DEEPSEEK_TIMEOUT_SECONDS`：变量缺失/空白 -> 120 秒；变量非空但不是整数或不在 `10..600` -> 配置失败，不静默回退。
- 远程 endpoint 只允许 HTTPS。
- HTTP 只允许 loopback host：`localhost`、`127.0.0.1`、`::1`，仅服务于本地代理和 loopback 测试。
- 禁止 URI userinfo、query 和 fragment；允许 HTTPS/loopback HTTP Base URL 带普通 path 前缀。
- 已是 `/chat/completions` 的地址保持；否则只追加一次该 path。
- `ToString()` 不输出完整 Base URL、API Key、prompt 或 proxy 细节；只显示 `BaseUrl=***` 与非敏感数值配置。
- UI 只显示“官方端点/自定义端点”和“配置可用/缺失/无效”，不显示完整 endpoint 或 API Key。

配置检查与实际发送必须使用同一个不可变快照，禁止 Shell 和 client 各自重新读取环境变量：

```csharp
internal enum DeepSeekRa2AiConfigurationState
{
    Ready = 0,
    MissingApiKey,
    InvalidBaseUrl,
    InvalidTimeout,
    UnsupportedModel
}

internal sealed class DeepSeekRa2AiConfigurationSnapshot
{
    public DeepSeekRa2AiConfigurationState State { get; }
    public DeepSeekRa2AiModel Model { get; }
    public bool UsesCustomEndpoint { get; }
}
```

- Snapshot 内部持有已经读取并归一化的 options；不暴露 API Key 或完整 endpoint。
- Shell 在 AI 面板打开和每次发送前创建一次 snapshot；本次 UI 状态和 client 均消费该同一对象。
- 环境变量在 snapshot 创建后发生变化，只影响下一次 snapshot，不影响活动请求。
- `CreateClientFromEnvironment(model)` 可保留为兼容委托，但 Shell 的 canonical path 必须是 `CreateConfigurationSnapshot(model)` 后调用 `CreateClient(snapshot)`。

### D5：统一出站隐私与字符预算

新增无状态 `Ra2AiOutboundTextSanitizer`，由 conversation provider 与 PromptBuilder 共同复用。禁止复制两套正则/marker 清单。

清理范围：

- `sk-` / `ds-` 风格 token；
- Authorization/Bearer/API key/secret/token 等敏感行；
- DeepSeek 环境变量名及其同一行值；
- 原始请求/响应 payload、provider internal metadata 标记行。

清理只影响网络出站副本；Shell 中用户原始输入和恢复提示词保持原样，不把清理后的文本覆盖回编辑器或输入框。

冻结字符预算：

| 内容 | 上限 |
|---|---:|
| User request | 8,000；超限拒绝发送，不截断 |
| Explicit selected text | 16,000 |
| Nearby text | 4,000（保持现值） |
| Conversation context | 6 turns / 6,000 total / 2,000 per turn（保持现值） |
| Final PromptText | 65,536 |

PromptBox 不设置 `MaxLength`。用户主动输入超过 8000 字符时，Shell 必须在开始 request session、清空输入框或创建消息卡之前拒绝发送，保留完整原文并显示固定提示。不得静默截断用户问题。

最终 PromptText 优先级从高到低：应用安全规则与输出规则、完整且已通过 8000 字符检查的当前 user request、当前 section/key/caret、显式 selection、最新会话、最高分 Field Registry evidence、诊断摘要、nearby text。总预算不足时只从最低优先级起确定性裁剪，不得裁掉应用安全规则、输出规则或 user request。

新增内部准备事实：

```csharp
[Flags]
internal enum Ra2AiRequestPreparationFlags
{
    None = 0,
    SensitiveContentRedacted = 1,
    SelectedTextTruncated = 2,
    ContextTruncated = 4,
    TotalPromptTruncated = 8
}
```

`Ra2AiRequest` 增加只读 `PreparationFlags` 与 `PromptCharacterCount`。它们只存活于单次请求，不序列化、不持久化、不进入 conversation text。

### D6：响应不变量

`Ra2AiResponse` 改为受控 factory 创建，禁止调用者自由组合矛盾状态。冻结不变量：

| Kind | Text | FinishKind | FailureKind | ErrorMessage |
|---|---|---|---|---|
| Success | 必须非空 | 必须 Stop | 必须 None | 必须 null |
| Cancelled | 可含 partial | Unknown | None | null |
| Timeout | 可含 partial | Unknown | TotalTimeout / StreamingIdleTimeout / Unknown | 固定安全文本 |
| MissingConfiguration | 空 | Unknown | MissingConfiguration | 固定安全文本 |
| ProviderError | 通常空 | Unknown 或已有终止事实 | 非 None | 固定安全文本 |
| Incomplete | 必须含 partial 或明确非 Stop finish | 非 Stop 或 Unknown | 可为 None 或具体失败 | 固定安全文本或 null |

冻结 factory 族：

```text
CreateSuccess
CreateCancelled
CreateTimeout
CreateMissingConfiguration
CreateProviderFailure
CreateIncomplete
```

- 原始通用构造器改为 private。
- 非法输入由 factory 立即抛出 `ArgumentException`，不得静默改写成另一个终态。
- `ResponseKind`、`FinishKind`、`FailureKind` 三轴职责保持 AI-REL-2 定义。
- 只有合法 Success 才能成为 Completed/context eligible。

### D7：安全的单请求观测事实

新增内部不可变 `Ra2AiRequestDiagnostics`，由 transport 创建并随 response 返回：

```text
RequestId                 随机请求 ID，不含用户/机器信息
ModelId                   两个冻结模型之一
PromptCharacterCount      出站字符数
TimeToHeaders             从发送到响应头
TimeToFirstContent        首个 content delta；没有则 null
TotalDuration             包含有序 callback backpressure
ContentDeltaCount         正文 delta 数
ContentCharacterCount     正文字符数
HttpStatusCode            可选数字状态码
```

- 不记录 prompt、selection、context、response text、API Key、完整 endpoint、proxy、异常 Message 或堆栈。
- 不写文件、不上报遥测、不使用全局请求字典。
- Shell 在失败卡的“诊断详情”中最多显示 RequestId、model、耗时、delta/字符计数和数字状态码；复制回答正文时不得混入这些信息。
- FailureKind 仍是故障语义权威；Diagnostics 只提供事实，不重新分类。
- `Ra2AiStreamConsumerException` 继续表示本地消费端/展示端 callback 失败，不得伪装成 provider failure，也不强制合成为 `Ra2AiResponse`。
- consumer failure 发生时，Pipeline/Shell 沿现有异常边界终止；已创建的 request diagnostics 可用于内部审查，但不得为了返回 diagnostics 吞掉异常或改变既有资源释放顺序。

### D8：Shell 资源与透明度边界

- PromptBox 保留完整输入；超过 8000 字符时由发送前校验拒绝请求，不使用会静默截断粘贴文本的 `MaxLength`。
- 模型 selector 保留 `AiAssistant.ModelSelector` AutomationId，默认项是 Flash。
- SafetyFooter 改为明确说明：发送会联网到 DeepSeek、可能产生费用、会携带已披露的有界上下文、不会修改文件。
- 新增动态提示 AutomationId：`AiAssistant.RequestPreparationNotice`、`AiAssistant.RequestDiagnostics`。
- 如果发生敏感内容清理或裁剪，在当前请求卡显示固定提示；提示不包含被清理内容。
- ChatHistory 最多保留 60 个已终止消息卡；只在当前请求完成后从最旧端按完整 user/assistant 对清理，永不删除活动请求卡。
- Markdown 终态渲染上限：最多 256 blocks、64 code blocks、单表 200 rows、合计 1200 cells。超限剩余内容降级为单一可复制纯文本块，不丢弃 `Ra2AiResponse.Text`。
- streaming 阶段继续只使用一个轻量文本卡；不得恢复每 delta 创建控件。
- 清空聊天、恢复提示词、复制正文、自动滚动和 request ownership 行为保持不变。

## 6. 调用顺序与生命周期

```text
用户点击发送 / Enter
  -> Shell 验证原始 prompt 非空且不超过 8000 字符；失败则保留输入并停止
  -> Shell 捕获 typed DeepSeekRa2AiModel
  -> Factory 创建单次 DeepSeekRa2AiConfigurationSnapshot
  -> Shell 与 client 共享该 snapshot，不再读取第二次环境变量
  -> 当前 request session 启动
  -> Build bounded IDE context
  -> PromptBuilder 使用统一 sanitizer / budget 构造 Ra2AiRequest
  -> Shell 显示非敏感 preparation notice（如有）
  -> Snapshot 已包含 API key / Base URL / total timeout 读取结果
  -> Options validation result 提供 endpoint / model / numeric boundaries
  -> DeepSeek client 创建 request-local diagnostics
  -> POST SSE（thinking disabled, max_tokens=8192）
  -> AI-STREAM-2/3 有序 delta、idle timeout、coalesced rendering
  -> 受控 Ra2AiResponse factory 形成唯一合法终态
  -> Shell 原位 finalize，同步显示安全诊断事实
  -> 仅合法 Success 进入 completed conversation context
  -> terminal cleanup 后执行 60-card retention
```

模型、配置、预算、诊断与 response 均为单请求生命周期。不得新增静态活动请求状态、跨请求缓存、磁盘存储或后台重试。

## 7. 连续 Task Cards

### AI-REL-3-0 BaselineAndRollbackAnchor

目标：在无 Git 元数据的工作区建立可验证的 IDE-only 回滚锚点。

允许动作：

- 执行 `package-source-clean.ps1 -Profile IdeOnly`；
- 记录归档绝对路径、文件数量、大小和 SHA-256；
- 在本契约的 Stage Result Ledger 中登记锚点；
- 不通过整目录覆盖执行回滚，失败时只恢复当前 Task Card 文件。

Package hygiene：必须排除 `.git/.vs/bin/obj/artifacts/TestResults`、旧 zip、API Key、用户设置与本地工具配置。锚点失败即停线，不进入 3A。

### AI-REL-3A ModelIdentityAndFactoryAuthority

目标：建立 typed V4 enum/option/catalog、Flash 默认值、UI 模型选择所需的 factory 契约，移除 `DEEPSEEK_MODEL` 权威。

允许文件（最多 5 个）：

- `RA2IniEditor.IDE/AI/DeepSeekRa2AiModel.cs`（新增，包含 option/catalog）
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs`
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClientOptions.cs`
- `RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientFactoryTests.cs`

验收：默认 Flash；显式 Pro；环境模型不再覆盖；Factory 只输出两个官方 ID；旧 `deepseek-chat/reasoner` 不进入生产 Factory 请求。Direct options 的模型白名单与旧测试数据迁移统一留到 3C2。

### AI-REL-3C1 ConfigurationSnapshotAuthority

目标：建立一次读取、不可变、可安全展示状态的配置快照，使 UI 与实际发送共享同一配置事实。

允许文件：

- `RA2IniEditor.IDE/AI/DeepSeekRa2AiConfigurationSnapshot.cs`（新增，包含 state）
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs`
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClientOptions.cs`
- `RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientFactoryTests.cs`

验收：snapshot 区分 Ready/MissingApiKey/InvalidBaseUrl/InvalidTimeout/UnsupportedModel；只读一次环境变量；不暴露 key/endpoint；`CreateClient(snapshot)` 不重新读取环境变量。

### AI-REL-3C2 EndpointAndNumericTrustBoundary

目标：实现 endpoint、timeout、模型白名单、数值配置和 `ToString()` 的信任边界，并迁移 direct-options 测试数据。

允许文件（最多 5 个）：

- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClientOptions.cs`
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs`
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClient.cs`
- `RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs`

验收：direct options 与 Factory 均只接受两个官方模型；远程 HTTP/非 HTTP(S)/userinfo/query/fragment 被拒绝；loopback HTTP 与远程 HTTPS 可用；非空无效环境变量不回退；完整 endpoint 不出现在字符串或 UI。

### AI-REL-3B ProductMockRetirementAndModelSelector

目标：在配置安全边界已经验证后，从生产程序集和 Shell 删除 Mock/Fake，模型 selector 改为 typed Flash/Pro。

允许文件（5 个）：

- `RA2IniEditor.IDE/AI/FakeRa2AiClient.cs`（删除）
- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiClientTests.cs`
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`

验收：生产代码无 `FakeRa2AiClient`、`Ra2AiProviderMode`、Mock UI/文案；默认 Flash；Pro typed value 准确传入 snapshot/factory；测试替身仅位于测试文件；既有 AutomationIds 不变；超长 prompt 在启动 request session 前被拒绝并保留原文。

### AI-REL-3D SharedOutboundSanitizer

目标：提取并统一敏感文本清理，先替换 conversation 的重复私有实现。

允许文件：

- `RA2IniEditor.IDE/AI/Ra2AiOutboundTextSanitizer.cs`（新增）
- `RA2IniEditor.IDE/AI/Ra2AiConversationContextProvider.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiConversationContextProviderTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiOutboundTextSanitizerTests.cs`（新增）

验收：旧 conversation 行为保持；大小写、CRLF、多 token、marker 行、无关文本和误匹配边界有测试；不泄露被替换原文。

### AI-REL-3E PromptBudgetAndPreparationFacts

目标：把 sanitizer 用于 user prompt/selection/context，并建立最终 PromptText 预算及 preparation facts。

允许文件（最多 5 个）：

- `RA2IniEditor.IDE/AI/Ra2AiRequest.cs`
- `RA2IniEditor.IDE/AI/Ra2AiRequestPreparationFlags.cs`（新增）
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs`
- `RA2IniEditor.IDE/AI/Ra2CurrentDocumentAiContextProvider.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiPromptBuilderTests.cs`

验收：四级字符预算确定性生效；固定规则不被截断；secret 不在 PromptText；flags/字符数准确；原始 Shell prompt 不被覆盖。

### AI-REL-3F OutputBudgetDiagnosticsAndResponseInvariant

这是一个 R2 内部契约子包，必须先完成数据模型再改 producer；若单个 Task Card 超过 5 个文件，必须按 F1/F2 分开，不得突破预算。

F1：输出预算与观测事实。

- `DeepSeekRa2AiClientOptions.cs`
- `DeepSeekRa2AiClient.cs`
- `Ra2AiResponse.cs`
- `Ra2AiRequestDiagnostics.cs`（新增）
- `DeepSeekRa2AiClientTests.cs`

F2：受控 response factory 与 consumer 迁移。

- `Ra2AiResponse.cs`
- `DeepSeekRa2AiClient.cs`
- `IRa2AiClient.cs`
- `Ra2AiResponseTests.cs`
- `Ra2AiAssistantPipelineTests.cs`

验收：payload 含官方 model、thinking disabled、max_tokens=8192；观测事实不含敏感文本；所有生产者只创建合法 response；非法组合测试 fail fast；AI-REL-2 分类不回退。

### AI-REL-3G ShellTransparencyAndResourceBounds

目标：显示模型/联网/准备/诊断事实，并限制 PromptBox、历史和 Markdown 控件。

允许文件：

- `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2ShellWindowResponsibilityMapTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiMarkdownResponseParserTests.cs`（仅当 parser 合约断言需要；不得修改 parser 语义）

验收：新增/保留 AutomationIds 符合契约；大表/多 block 降级为纯文本；60-card retention 不碰活动请求；复制内容不含终态/诊断脚注；主 Shell 布局不变。

### AI-REL-3H DeterministicTransportAndFailureUiVerification

目标：补 BCL loopback HTTP/SSE、分片 UTF-8、动态 HTTP/timeout/connection-close 与 WPF failure smoke。

允许文件：

- `RA2IniEditor.Tests/IDE/DeepSeekRa2AiLoopbackIntegrationTests.cs`（新增）
- `RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs`
- `RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs`
- `RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs`
- 测试文件内的 private helper；不得新建生产 Fake

验收：不依赖第三方 server/package；不依赖 10–20 ms 精确调度；至少覆盖 headers、分片正文、异常 EOF、401、429、5xx、total timeout、首正文后 idle timeout、取消优先级和安全 UI 文案。

### AI-REL-3I FullVerificationUiSmokeAndDocumentationClosure

目标：全量验证、真实双模型最小烟测、清洁打包和治理文档收口。

文档允许文件：

- 本契约
- `Docs/Codex_CurrentPhase.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- 产品行为实际变化时再更新 `Docs/UserGuide.md` / `Docs/FeatureOverview.md` / `Docs/ReleaseChecklist.md`，如超过 5 文件则拆成 docs-only Task Card。

真实 provider 烟测限制：仅使用合成、无敏感内容提示；Flash 与 Pro 各最多一次；不自动重试；不打开或修改项目文件；配置缺失时记为 NotRun，不伪造通过。

## 8. 各阶段验证

每个代码 Task Card：

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "<该阶段相关测试过滤器>"
```

3I 最终门禁：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

最终 UI 烟测：

1. AI 面板默认显示 V4 Flash，列表只有 Flash/Pro，无 Mock。
2. 展开进阶区，确认联网/费用/上下文披露和 AutomationIds。
3. 缺失或无效配置显示固定安全提示，不泄露 endpoint/key。
4. Flash/Pro 各发送一条无敏感合成 prompt，确认实际 model 与 UI 一致。
5. 请求中切换/禁用、取消、部分响应、恢复提示词、非覆盖与上下文隔离保持。
6. 本地 loopback 触发 401/429/5xx/超时/异常 EOF，确认动态失败卡与控件复位。
7. 大 prompt、大 selection、大表和超过 60 卡场景不造成无界控件增长。

## 9. DeepSeek 子任务边界

AI-REL-3 的模型、网络、隐私、response factory、Shell 生命周期与 UI 契约均由 Codex 负责，不委派架构决策。

可选 DeepSeek 子任务只限：

- 3D 已冻结 sanitizer API 后的单文件边界测试草案；
- 3H 已提供真实 helper/API 后的单个 loopback 测试方法；
- 3I 文档格式整理。

每次委派必须提供真实 Exact API Inventory，最多 1–3 个文件，使用 `deepseek-v4-flash`，不得让 DeepSeek修改 public API、Shell 生命周期、endpoint 规则或 response 不变量。没有用户另行要求时，不主动委派。

## 10. Public / Internal Contract Ledger

外部 public API：无。所有新增契约均为 `internal`，不序列化、不持久化。

| Stage | Contract | Kind | Stability | Next use |
|---|---|---|---|---|
| 3A | DeepSeekRa2AiModel | internal enum | Stable | Factory + Shell |
| 3A | DeepSeekRa2AiModelOption / catalog | internal immutable data | Stable | Typed Shell selector + API id mapping |
| 3C1 | DeepSeekRa2AiConfigurationState | internal enum | Stable | Factory + Shell safe status |
| 3C1 | DeepSeekRa2AiConfigurationSnapshot | internal immutable authority | Stable | One-read config + client construction |
| 3E | Ra2AiRequestPreparationFlags | internal enum | Stable | PromptBuilder + Shell notice |
| 3E | Ra2AiRequest.PreparationFlags / PromptCharacterCount | internal DTO properties | Stable | Client diagnostics + Shell |
| 3F1 | Ra2AiRequestDiagnostics | internal immutable data | Experimental until 3I | Transport + failure UI |
| 3F2 | Ra2AiResponse factory family | internal construction contract | Stable after F2 tests | All clients/Pipeline/Shell |

不得新增外部 `public` 类型、接口、序列化字段或项目引用。如果实现发现必须这样做，立即 R4 停线并重新审查。

## 11. Decision Log

### Decision: 产品只保留真实 DeepSeek provider

- Status: Accepted（用户于 2026-07-20 确认修正版契约）
- Decision: 删除生产 Fake/Mock；测试替身仅在测试程序集。
- Rejected: 保留隐藏 Mock、Debug-only Mock、环境变量切换 Mock。原因是它们继续制造生产/测试路径分叉和 UI 误解。

### Decision: 模型选择和 thinking 模式正交

- Status: Accepted（用户于 2026-07-20 确认修正版契约）
- Decision: UI 只选 Flash/Pro；本包统一显式 non-thinking。
- Rejected: 让 Pro 隐式 thinking、Flash 隐式 non-thinking。原因是会把模型选择同时变成不可见的时延/费用策略。

### Decision: UI 模型选择覆盖环境模型

- Status: Accepted（用户于 2026-07-20 确认修正版契约）
- Decision: 删除 `DEEPSEEK_MODEL` 生产权威。
- Rejected: 环境变量继续最终覆盖。原因是 UI 会显示一个模型却发送另一个模型。

### Decision: 无持久化观测

- Status: Accepted（用户于 2026-07-20 确认修正版契约）
- Decision: 只随 response 返回请求局部安全事实。
- Rejected: 文件日志/遥测。原因是扩大隐私、生命周期和发布范围。

## 12. Deferred Governance Queue

### PublicApiLedger Pending Entries

无外部 public API。第 10 节内部契约在 3I 统一核对稳定性。

### TechnicalDebt Pending Entries

| Stage | Debt | Repayment trigger |
|---|---|---|
| 3F1 | Diagnostics 在 3I 前为 Experimental | 已由 3H 回环故障验证、3I 全量测试与 UI 烟测冻结；无遗留债务 |
| 3F2 | F2a 临时 internal response 构造器 | 已在同阶段 F2b 关闭并经 90 项定向测试验证；无遗留债务 |
| Future | thinking mode 未提供 | 用户明确需要且接受成本/时延契约时新建阶段 |

### CurrentStatus Pending Updates

| Area | Current | Next stable update |
|---|---|---|
| AI reliability | AI-REL-3 completed | 保持契约；后续变更另立阶段 |
| Model selector | Flash/Pro, Flash default | 保持 typed catalog 权威 |
| Retry | Not implemented | 保持不实现；不得隐式引入 |

### Stage Result Ledger

| Stage | Status | Verification evidence | Governance events |
|---|---|---|---|
| AI-REL-3-0 | Completed | 独立回滚锚点 `H:\RA2\RA2IniEditor_IDE\artifacts\RA2IniEditor.IDE.SourceClean.AI-REL-3-0.Rollback.zip`; 4,109,990 bytes; SHA-256 `C7D6B446E8BDB42147BCCC7D8E93D06432876C08DC2E14C9EC07F837F6614759`; 与原 904-entry 干净包哈希一致 | Rollback anchor protected from final-package overwrite |
| AI-REL-3A | Completed | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --filter FullyQualifiedName~DeepSeekRa2AiClientFactoryTests`: 15 passed, 0 failed; compile passed with one pre-existing nullable warning in BuiltInFieldRegistryPackLoaderTests | Typed model catalog frozen; Flash default; retired env model ignored |
| AI-REL-3C1 | Completed | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --filter FullyQualifiedName~DeepSeekRa2AiClientFactoryTests`: 17 passed, 0 failed; compile passed with one pre-existing nullable warning | One-read immutable snapshot; safe state; exact options instance shared with client |
| AI-REL-3C2 | Completed | Client/pipeline/factory targeted tests: 85 passed, 0 failed; first IPv6 loopback mismatch fixed in-card and reverified | Endpoint/model/numeric trust; non-thinking + max_tokens payload |
| AI-REL-3B | Completed | Shell/client boundary targeted tests: 102 passed, 0 failed; XAML/runtime compiled | Production Fake removed; typed Flash/Pro selector; pre-session 8000-char rejection |
| AI-REL-3D | Completed | Sanitizer/conversation targeted tests: 27 passed, 0 failed | Shared outbound sanitizer replaces conversation-private copy |
| AI-REL-3E | Completed | PromptBuilder/pipeline targeted tests: 30 passed, 0 failed | Shared scrub; deterministic four-level budgets; request-local preparation facts |
| AI-REL-3F1 | Completed | DeepSeek client targeted tests: 62 passed, 0 failed | Request-local safe diagnostics; callback backpressure included |
| AI-REL-3F2a/F2b | Completed | Response/client/pipeline targeted tests: 90 passed, 0 failed | Split to respect five-file budget; controlled factories complete; constructor private |
| AI-REL-3G | Completed | Shell/resource-bound targeted tests: 51 passed, 0 failed | Safe configuration/preparation/diagnostic facts; 60-card history bound; whole-response text fallback for Markdown limits |
| AI-REL-3H | Completed | Loopback/failure/UI-boundary targeted tests: 48 passed, 0 failed | BCL loopback covers fragmented UTF-8 SSE, 401/429/503, EOF, total/idle timeout and user cancellation without external network |
| AI-REL-3I | Completed | restore passed; build 0 warnings/0 errors; full tests 2171/2171; Flash and Pro live minimal requests each passed once; runtime AI-panel smoke passed; IdeOnly clean package generated after governance flush | Internal API/diagnostics frozen; product docs and context refreshed; package closure reached |

## 13. 连续执行停线条件

任一条件出现即停止并刷新已完成阶段、失败证据与下一安全入口：

- 官方 API 与本契约冻结 model/thinking/payload 不一致；
- 无法建立或校验 AI-REL-3-0 干净源码包回滚锚点；
- UI 展示的配置状态与创建 client 使用的配置快照不是同一个对象；
- 需要恢复 `DEEPSEEK_MODEL` 权威或新增第三种模型；
- 需要自动重试、fallback 到另一个模型或 provider；
- 需要保存 API Key、endpoint、prompt、response 或 diagnostics；
- 需要显示 provider 错误正文、异常 Message 或完整 endpoint；
- 需要修改 `IRa2AiClient` 外部 public 边界、项目引用或新增第三方依赖；
- 单个 Task Card 超过 5 文件且无法安全拆分；
- 需要修改 Shell 冻结区域、Field Registry、parser、completion、diagnostics、save、项目文件或 legacy；
- targeted build/test 失败且修复超出当前 Task Card；
- UI 截图/烟测失败且需要自由 XAML 重设计；
- 实际 diff 风险高于本契约且未被明确授权。

## 14. 最终验收标准

- 产品和生产程序集无 Mock/Fake；测试仍可确定性覆盖 client/pipeline。
- 选择器只有 V4 Flash/Pro，默认 Flash，请求 payload model 与 UI 一致。
- thinking 显式 disabled，max_tokens 固定受控；无旧模型别名。
- endpoint/config 无静默错误回退，无远程 HTTP，无 endpoint/key 泄露。
- prompt/selection/context/total prompt 均有确定性预算和共享清理策略。
- 原始 user prompt 超过 8000 字符时在 request session 创建前拒绝，完整输入保留且不自动截断。
- response 不可能构造矛盾状态；AI-REL-2 失败分类与 AI-REL-1 上下文隔离保持。
- 请求诊断能定位 headers/first-content/stream/total 阶段，不包含敏感正文。
- Shell prompt、历史、Markdown 控件有硬上限；完整 response text 仍可复制。
- loopback、targeted、full suite、UI smoke 与 clean package 均有明确证据。
- legacy 未恢复；Shell 主布局未改变；Field Registry 与编辑/保存语义未改变。
