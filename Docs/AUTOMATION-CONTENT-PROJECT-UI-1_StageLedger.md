# CONTENT-PROJECT-UI-1 Stage Result Ledger

日期：2026-08-24  
状态：Completed / automated verified；real-provider 与 WPF visual acceptance pending  
风险：R4  
契约：`AUTOMATION-CONTENT-PROJECT-UI-1_WorkProjectProposalEndToEndFinalContract.md`

## 1. 交付结果

Work 模式现可把既有 `techno-rules-art-binding` project capability 接入唯一生产链：第一次
DeepSeek 调用返回并校验意图事实包，第二次只允许 `expand_ini_project_content_template`；IDE 在
调用前后捕获同一 rules/art Project Snapshot，随后复用 Gateway `ExpandProjectTemplate`、Workspace
`PreviewProject/ApplyProject`、现有多文件 Diff 与 compound Undo。Proposal 明确区分 Document/Project，
素材 Manifest 只显示为非阻塞待办，不调用 Provider、不要求素材存在、不自动保存。

## 2. 阶段台账

| 阶段 | 状态 | 结果 |
|---|---|---|
| CPUI-1A Contract | Completed | 代码事实、边界、UI 契约、失败语义与门禁获用户确认 |
| CPUI-1B Route / Tool | Completed | project capability、专用工具 schema、两阶段路由和 provider prompt 接入 |
| CPUI-1C Contracts / Coordinator | Completed | Document/Project strict union、项目准入、adapter、Preview/Apply/Dismiss/stale 权威接入 |
| CPUI-1D Shell Wiring | Completed | 模型调用前后捕获精确 project targets；显式 Apply 走现有项目事务；未改 Shell XAML |
| CPUI-1E Proposal / Diff UI | Completed | 两文件摘要、非阻塞素材待办、文件头分组、窄宽度路径收缩和 AutomationId |
| CPUI-1F Verification / Docs | Completed | restore/build/full tests 通过；产品、开发、路线、决策和阶段文档已更新；干净包门禁见下表 |

## 3. 运行时代码与测试文件

- `RA2IniEditor.IDE/AI/Ra2AiInteractionRoute.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAuthoringToolCatalog.cs`
- `RA2IniEditor.IDE/AI/Ra2AiIntentAnalysisStage.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs`
- `RA2IniEditor.IDE/AI/Ra2AiEditProposalContracts.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAuthoringToolAdapter.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAuthoringCoordinator.cs`
- `RA2IniEditor.IDE/AI/Ra2AiProposalPreparationRunner.cs`
- `RA2IniEditor.IDE/AI/Ra2AiEditProposalViewModel.cs`
- `RA2IniEditor.IDE/AI/Ra2AiEditProposalView.xaml`
- `RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs`
- `RA2IniEditor.IDE/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/ViewModels/AuthoringDiffDocumentViewModel.cs`
- `RA2IniEditor.IDE/Views/AuthoringDiffDocumentView.xaml`
- `RA2IniEditor.IDE/Views/AuthoringDiffDocumentView.xaml.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiProjectAuthoringIntegrationTests.cs`
- 既有 Pipeline、Shell boundary、HLI workspace 与 UI contract 测试的增量断言。

`ShellWindow.xaml`、项目文件、legacy 工程均未修改或恢复。

## 4. 数据与权威边界

- Proposal 为严格 tagged union：Document 只能携带单文档 Plan；Project 必须携带 Project Plan 与 Manifest。
- Project target 只接受唯一 `rulesmd.ini + artmd.ini` 或 `rules.ini + art.ini`，顺序固定 rules -> art。
- Project currency 比较 root、路径/顺序、document id/version/text、provider identity/revision；漂移即 stale。
- Application compiler 仍是字段/Section/操作合法性的最终事实源；AI adapter 不复制编译逻辑。
- Workspace active slot、Apply/Dismiss/supersede、项目 session store 与 compound Undo 仍是唯一 Host 权威。
- Manifest 无 Apply、Save、文件或 Provider 权限；缺少 SHP/Cameo 文件不阻塞 INI Proposal。
- 无 public API 或持久化格式变化；导出类型 77、Gateway catalog 9、interface methods 11 保持不变。

## 5. UI 契约结果

- 新增 `AiAssistant.EditProposalCard.ProjectSummary`。
- 新增 `AiAssistant.EditProposalCard.AssetManifestSummary`。
- 新增 `Shell.AuthoringDiff.FileHeader`。
- Project Proposal 显示两文件和非阻塞资产摘要；Document Proposal 不出现空白项目区域。
- Project Diff 以文件头分组，rules 在前、art 在后；窄于 640 DIP 时收起次要相对路径，操作仍可达。
- 未修改 Dock、菜单、工具栏、窗口布局或主题。

## 6. Verification Matrix

| 检查 | 命令/证据 | 结果 |
|---|---|---|
| Restore | `dotnet restore .\RA2IniEditor.IDE.sln` | Passed；all projects up-to-date |
| Debug Build | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed；0 errors；1 个既有 nullable warning |
| Application full | `dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build` | Passed 186/186 |
| IDE non-UI full | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | Passed 2641/2641 |
| CPUI focused | 项目准入、route/tool、adapter、coordinator、pipeline、Shell/UI contracts | Passed 425/425 |
| Public baseline | 现有结构门禁 | Passed；77 / 9 / 11 unchanged |
| Diff hygiene | `git diff --check` | Passed；仅既有 LF -> CRLF 提示，无 whitespace error |
| Clean package | `tools/package-source-clean.ps1 -Profile IdeOnly` | Passed；`artifacts/RA2IniEditor.IDE.SourceClean.zip`，1209 files |

## 7. Diff Intent Audit

| 变更区域 | 意图 | 是否越界 |
|---|---|---|
| AI route/tool/prompt | 让两阶段 Work 精确选择既有 project template | 否 |
| Proposal contracts/coordinator | 把现有单文档 active proposal 权威扩展为严格 Document/Project 分支 | 否 |
| Shell code-behind | 捕获 project snapshot、投影 Proposal、调用既有 Project Apply | 否；契约明确允许 |
| Proposal/Diff UI | 展示两文件、Manifest 待办与文件分组 | 否 |
| Application / parser / registry / diagnostics | 未修改 | 否 |
| Asset Provider / persistence / Save | 未接入、未调用、未扩权 | 否 |

## 8. Deferred Governance Queue

- Public API ledger：无变更，不新增条目。
- Persistence ledger：无变更。
- Architecture decision：已在 `DecisionLog.md` 接受并记录。
- Technical debt：未引入临时双权威、兼容旁路或 TODO 实现。
- 人工验收不是自动门禁替代项，保留为显式剩余风险。

## 9. 人工验收脚本

1. 在包含唯一 `rulesmd.ini + artmd.ini`（或 classic pair）的项目中打开 rules 文档，选择 Work。
2. 请求为一个既有 Techno 绑定明确的 art Section、body asset ID 与 Cameo ID，仅预览 rules/art INI 修改。
3. 确认 Proposal 显示两文件及非阻塞素材待办，Project Diff 按 rules -> art 分组。
4. 点击 `应用到项目`，确认两份内存文档均 dirty 且磁盘未自动写入。
5. 执行一次 Ctrl+Z、一次 Ctrl+Y，确认两个文档同时撤销、同时重做。
6. 仅保存当前文件，确认另一目标仍 dirty；关闭时仍由既有未保存确认处理。
7. 删除/缺失实际 SHP/Cameo 文件重复请求，确认 INI Proposal 不被阻塞且没有创建资产文件。

## 10. 剩余风险与下一阶段

- 已验证：本轮真实 DeepSeek 第二阶段返回完整 project tool 五参数 schema；长期 provider 漂移仍由严格
  schema 与本地 compiler fail closed。
- 未验证：1920x1080、窄 AI 面板和 Project Diff 的真实 WPF 视觉与键盘体验。
- 未验证：用户侧 Apply、compound Undo/Redo、Save Current 的端到端手工交互。
- 明确未实现：完整 Techno 创建、资产生成/落盘、自动 Save、独立 Agent Host。

建议先完成上述人工验收；通过后进入 source-backed Techno/SuperWeapon/Faction profile 扩展。
`ASSET-HOST-1` 按用户裁决继续冻结，不是 INI rules/art binding 的前置。

## 11. Post-completion narrow fix：CPUI-1-NF1 Intent Completion Compatibility（已由 NF2 取代）

- 实机症状：明确 rules/art binding 请求得到 HTTP 200，但 UI 报“DeepSeek 返回了无法完整解析的响应”；
  诊断为 0 个可见文本 delta。
- 真实合成调用证据：DeepSeek 正常返回唯一 `analyze_ra2_authoring_intent` tool call，capability/domain 均正确，
  但把该完整指定的有界绑定分类为 `completion_level=complete`；旧校验只接受 `field`，导致工具调用在
  本地意图包校验阶段被误报为协议错误。
- 该修复只覆盖了当时观察到的 completion 变体，没有解决同一 capability 返回 `domain_intent_id=techno`
  时仍被拒绝的根因；不得再把 NF1 视为完整修复。完整裁决与证据见 NF2。
- 权限影响：0；不新增工具、operation、Apply/Save/asset 权限，不加入通用 JSON 容错或自动重试。
- 验证结果：Debug build 通过（0 errors、1 个既有 nullable warning）；Project/Pipeline 36/36、
  全部 `Ra2Ai*` 386/386 通过。真实修复后 GUI 复验仍由用户执行。

## 12. Post-completion root fix：CPUI-1-NF2 Capability-authoritative Intent Normalization

- 实机复现：用户原始 rules/art prompt 得到 HTTP 200、259 个 SSE event、257 个 tool-call fragment、
  0 个无效事件和 `finish_reason=tool_calls`；DeepSeek 返回完整 `analyze_ra2_authoring_intent`，其中
  capability=`techno-rules-art-binding`、domain=`techno`、completion=`field`。旧本地代码因只接受
  domain=`art-animation` 而拒绝该完整响应，UI 又把本地语义拒绝投影为通用 ProtocolError。
- 根因：明确 capability 已唯一限定项目工具与本地 compiler，但 provider 派生的 domain/completion
  元数据仍拥有第二次否决权；NF1 继续扩大单值白名单会重复返工。
- 修复：先保持根对象、唯一字段、枚举、domain allowlist、capability/outcome 严格校验；当 authoring
  capability 精确为 `techno-rules-art-binding` 时，把非权威 domain/completion 归一化为
  `art-animation + Field`，再进入原有 project availability、专用 tool schema、adapter 与 compiler。
  未知 domain/capability、错误工具名、非法 JSON 和额外根字段仍整体拒绝。
- 第二阶段真实探测：正式客户端形态（`thinking=disabled`、`tool_choice=required`）得到 HTTP 200、
  220 个 SSE event、0 个无效事件、正确的 `expand_ini_project_content_template` 与完整五参数 proposal；
  现有 adapter 可接受其可选 `message`，因此 tool schema、adapter、Gateway compiler 与 Project Diff
  不需要放宽。第一阶段复现用量 1,393 tokens，第二阶段生成用量 1,314 tokens，合计 2,707 tokens；
  一次参数错误的预检在生成前被 HTTP 4xx 拒绝。
- 权限影响：0；不新增 capability/tool/operation，不改变 Project Snapshot、Preview、Apply、Undo/Redo、
  Save、Manifest、Asset Provider、transport、SSE、timeout、retry 或 model policy。
- 自动验证：Debug build 0 errors（1 个既有 CS8602 warning）；Project/Pipeline 39/39；全部
  `Ra2Ai*` 389/389；Application 186/186；IDE non-UI 2645/2645。GUI 最终复验仍由用户执行。

## 13. Post-completion observability fix：CPUI-1-NF3 Local Rejection Visibility

- 再次实机症状：NF2 后相同 rules/art prompt 仍显示“DeepSeek 返回了无法完整解析的响应”，HTTP 200、
  `Deltas=0`、`Characters=0`。一次获授权的最小真实探针得到完整 `analyze_ra2_authoring_intent`
  tool call：capability=`techno-rules-art-binding`、domain=`art-animation`、completion=`field`，严格通过
  当前 parser；用量 prompt 1,044、completion 324、total 1,368 tokens。
- 根因：Pipeline 把“意图包未通过本地 Work 契约”和“Project availability 不可用”都伪装成
  `ProviderError/ProtocolError`；Shell 又按 FailureKind 使用固定 provider 文案，导致已经存在的
  NoProject、PairMissing、PairAmbiguous、SnapshotUnavailable、ReadOnly、ResourceLimitExceeded
  具体原因全部丢失。旧截图因此不能证明 Provider 响应损坏。
- 修复：新增 internal、瞬态、非序列化 `Ra2AiResponseKind.LocalRejection` 与工厂不变量；它必须
  `FailureKind=None`、无 `ErrorMessage`、无 ToolCalls，并只携带本地生成的安全显示消息。Pipeline
  对六种项目准入结果返回精确原因；Shell 在 provider formatter 之前直接显示该安全消息。真实
  Provider/transport failure 继续走原有 FailureKind formatter，不显示 provider body。
- 权限影响：0；不扩大扫描范围、不创建缺失文件、不放宽 JSON/tool schema，不改变 Project Snapshot、
  compiler、Preview、Apply、Undo/Redo、Save、asset、SSE、timeout、retry 或模型策略。
- 自动验证：Debug build 0 errors（1 个既有 CS8602 warning）；Response/Pipeline/Shell focused
  82/82；全部 `Ra2Ai*` 395/395；Application 186/186；IDE non-UI 2651/2651；`git diff --check`
  通过（仅既有 LF -> CRLF 提示）；IdeOnly clean package 1209 files。
- 手工复验预期：旧通用错误将被具体本地原因替换。若显示 pair missing/ambiguous，用户需打开含唯一
  顶层 `rulesmd.ini + artmd.ini` 或 `rules.ini + art.ini` 配对的真实项目；创建缺失文件不属于本契约。

## 14. Post-completion usability/root fix：CPUI-1-NF4 Project Readiness and Session Authority

- 实机事实：用户实际项目根目录是 `H:\RA2\YR_Test`，其中顶层存在唯一 `rulesmd.ini + artmd.ini`；
  `artmd.ini` 可以为空，同时还存在 `ddraw.ini`、`RA2MD.INI`、`Register.ini`。该目录按现有 admission
  契约应当成功，额外 INI 与空 art 文件都不是拒绝理由。
- 根因风险：Shell 过去从 `ProjectExplorer.Items` 这一 UI 投影重建 project session 与捕获 Work 上下文。
  UI 投影不是项目成员权威来源，可能使实际已打开的文件集合与 Work 准入看到的集合不一致。
- 修复：`ShellViewModel` 在成功打开项目时保存不可变 `ProjectOpenResult.Files`；
  `Ra2ProjectDocumentSessionStore` 保存不可变、标准化的 `MemberFilePaths`；Shell 初始化 session、判断
  rules/art pair 和捕获 snapshot 都只消费这些会话成员，不再依赖项目浏览器控件内容。
- 可用性：Work 上下文摘要直接显示完整项目根路径与“配对已识别”或明确缺失/歧义原因；切换 Work、
  正常打开项目和自动化打开项目后立即刷新。提示词不再要求用户重复文件名、仅预览、不保存和不生成
  素材等本地已强制边界，只需说明目标 Section、art Section、body 与 Cameo ID。
- 保持边界：仍只扫描已打开项目的顶层唯一 md/classic pair；不递归猜目录、不读取最近路径、不创建
  rules/art 或素材文件、不自动 Apply/Save，不改变 parser、Field Registry、Diagnostics、Undo/Redo、
  provider、transport 或模型策略。
- 自动验证：Debug build 通过（0 errors、1 个既有 CS8602 warning）；项目打开、session、admission、
  pipeline 与 Shell 边界聚焦测试 88/88；Application 186/186；IDE non-UI 2652/2652 通过。
  洁净包结果记录在最终阶段报告中。
- 人工复验：打开 `H:\RA2\YR_Test` 后切换 Work，应在上下文摘要看到完整路径与
  `rulesmd.ini + artmd.ini 配对已识别`；随后可直接输入
  `给 HTNK 绑定美术：Art=HTNKART，Body=HTNKBODY，Cameo=HTNKICON。`

## 15. Post-completion root refactor：CPUI-1-NF5 Profile Validation Layering and Legacy Enum Isolation

- 实机症状：同一已识别 rules/art 项目中，Work 对
  `给 HTNK 绑定美术：Art=HTNKART，Body=HTNKBODY，Cameo=HTNKICON。` 返回
  “内容模板参数不符合当前 Profile 的约束”。
- 真实 provider 证据：一次获授权的 DeepSeek V4 Flash 第二阶段探针返回了正确的 project template、
  version、`ownerSectionId=HTNK`、`artSectionId=HTNKART`、`bodyAssetId=HTNKBODY`、
  `cameoAssetId=HTNKICON` 和合法 `assetBrief`；用量为 prompt 611、completion 214、total 825 tokens。
  因此该次失败不是模型没有遵守工具契约。
- 确认根因：当前 Global 用户字段包把 `Vehicle.Image` 定义为只包含既有样本值的 Enum，且列表中没有
  `HTNKART`。旧 compiler 把这类学习/迁移得到的观测值列表当成封闭集合，导致合法的新素材引用在
  Template Profile 内被错误否决；UI 又只显示通用 Profile 错误。
- 架构修复：模板字段规格新增 internal、不可变、非序列化的验证策略。默认仍使用 effective schema；
  只有 source-backed rules/art `Image` 与 `Cameo` 引用采用 `OpenReference`：字段必须存在、不得
  Blocked、仍经过 trust 门禁且值必须是安全 RA2 identifier，但旧观测 Enum 不再拥有封闭世界否决权。
- Adapter 收口：project tool 只要求四个 ID；`assetBrief` 改为可选并由 Host 确定性生成。该唯一
  project profile 支持参数名大小写归一、空 brief 忽略以及 body/Cameo 单个 `.shp` 后缀去除；未知参数、
  大小写重复参数、四个必填 ID 缺失、非法 identifier 和 body/Cameo 冲突继续 fail closed。
- 可观察性：补充缺失 Section、Section kind、字段/参数非法、重复素材 ID、brief 长度等安全本地错误映射，
  不再把所有 compiler 拒绝压缩成同一句 Profile 错误。
- 保持边界：未修改用户 Global/BuiltIn 字段包、provider priority、Field Registry loader、public API、
  持久化格式、Project Snapshot、Preview/Apply、compound Undo/Redo、Save、素材生成、transport、SSE、
  timeout、retry 或 model policy；Shell XAML 与 AutomationId 均未改变。
- 自动验证：Debug build 通过（0 errors、1 个既有 CS8602 warning）；Application focused 48/48、
  IDE focused 103/103、Application full 188/188、IDE non-UI full 2656/2656 通过。最终 package 结果见
  本轮交付报告；IdeOnly clean package 1209 files，压缩包二次检查 forbidden entries=0。
- 人工复验：打开 `H:\RA2\YR_Test`、确认 Work 摘要显示唯一配对，再发送上述原始短提示词；应出现
  rules/art Project Proposal，而不是 Profile 约束错误。完整 GUI Project Diff/Apply/Undo 仍由用户验收。

## 16. Authority refactor：CPUI-1-NF6 Model-Owned Project Plans / Minimum-Safety Host

- 用户裁决：rules/art Work 项目的内容构建由 DeepSeek 负责。字段库、Diagnostics、旧 Enum 样本、
  固定 Profile 与本地对象完整度判断只能提供审阅证据，不得否决模型生成的新字段、新 Section 或
  mod-specific 引用。模型无法确定必要目标或意图时，应返回 `needs_clarification`，而不是由 Host
  猜测或用旧 Profile 代替模型裁决。
- 生产路由：Work 的 rules/art 第二阶段改用通用结构化工具 `preview_ini_project_edit_plan`。模型返回
  `rules` / `art` 两个符号目标及有界 Upsert/Replace 操作；不得传入文件路径、snapshot id、revision、
  Apply、Save 或素材写入指令。旧 `expand_ini_project_content_template` 与固定 project template 仍保留
  给 headless/兼容调用方，但不再暴露给生产 AI 项目路由。
- 计划投影：Adapter 只把符号目标映射到请求前捕获且请求后复核的唯一 rules/art snapshot。Upsert 指向
  不存在的 Section 时，Host 以 `SectionKind.Unknown` 确定性补充 Section-create operation；模型无需受
  本地 SectionKind/Profile 目录限制。Project Plan 继续复用 canonical Project Preview、Project Diff、
  显式原子 Apply、single-use/stale 门禁、失败回滚及 compound Undo/Redo。
- 权威分层：项目 Preview 中 Field Registry trust、未知字段与新增 diagnostics 只产生 Caution/审阅证据，
  不再把显式 ApplyPolicy 变成 Blocked。对于 `ExpectedSectionKind.Unknown` 的模型计划，Blocked/obsolete
  registry evidence 同样降为 Caution；具体 SectionKind 的 headless 模板仍保持既有 fail-closed 行为。
- 最低安全边界保留：合法 JSON/工具名/唯一属性、只允许当前已捕获 rules/art 文档、安全 INI identifier、
  有界文档数/操作数/字符串长度、canonical parser/Preview、显式 Apply、禁止自动 Save、snapshot 一致性、
  单次使用和项目事务原子回滚。不存在任意路径、原始全文替换、素材生成或静默写盘权限。
- Manifest：通用模型计划不要求 Asset Manifest；有素材待办时可由后续独立能力表达，但它不再是 INI
  Project Proposal 成功的前置条件。UI 可显示一份或两份实际发生变化的文档。
- API 影响：没有新增 public DTO、enum、Gateway 方法或序列化 shape；Experimental Preview 的语义变更
  已登记到 `Docs/PublicApiLedger.md`。Field Registry 数据、provider priority、parser、Completion、Hover、
  Save、Undo/Redo 实现、Shell XAML、AutomationId 与 legacy 均未修改。
- 自动验证：首次 build 因 Adapter 缺少 `System.IO.Path` using 失败，补齐后 Debug build 通过（0 errors、
  1 个既有 CS8602 warning）；Application focused 20/20、IDE focused 68/68、Application full 188/188、
  IDE full 2659/2659 通过。未执行真实 DeepSeek 调用或电脑操控；新工具的真实 provider/UI 流程待用户
  使用 `H:\RA2\YR_Test` 手工验收。
