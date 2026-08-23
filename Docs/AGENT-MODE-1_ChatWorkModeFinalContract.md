# AGENT-MODE-1 Chat / Work Mode Continuous Final Contract

日期：2026-08-23  
状态：Implemented / automated verification complete; visual acceptance pending  
风险等级：R3（AI 工具授权路由、AI 面板交互、Experimental Template 元数据）

## 0. 结论

本连续包把当前由提示词关键词隐式推断的“咨询/修改能力”改为用户在 UI 中显式选择的两个模式：

- `Chat / 聊天`：只解释、分析和给出建议，永远不声明任何编辑工具，也不产生 Apply 权限；
- `Work / 工作`：只通过现有结构化 Plan -> Preview -> Diff -> 用户显式 Apply 链修改当前文档；
- 只有用户明确使用“骨架、框架、占位、空结构、skeleton、scaffold、placeholder”等表达时，
  Work 才能选择 skeleton profile；
- 未明确要求骨架时，受支持的对象请求必须选择 complete-object profile；不支持的对象必须明确失败或请求
  澄清，禁止静默退化为骨架；
- 本阶段首个 complete-object 范围严格限定为“当前文档中，把一个现有 TechnoType 的 Primary/Secondary
  绑定到新建的 direct-fire Weapon / Projectile / Warhead 完整链路”；单位、建筑、SuperWeapon、素材等
  其他完整 Profile 仍属于后续 CONTENT 包。

本契约不会把 Chat/Work 复用为底层 Capability enum，也不会让模型获得 Apply、Save、Undo、Shell 或文件
系统权限。用户模式、模型能力、模板完整度和最终写入权限是四个独立维度。

## 1. 代码事实回归

### 1.1 当前真实行为

1. `Ra2AiInteractionRouter.Resolve(prompt, availability)` 仅依赖提示词关键词决定：
   `Advisory / EditExplicit / TemplateExplicit / EditAmbiguous / EditUnavailable`。
2. 当前 UI 没有用户模式状态，也没有 Chat/Work 选择控件。
3. 当前 `LooksLikeTemplateRequest` 只要看到“武器链”或 Weapon + Projectile + Warhead，就选择
   `CurrentDocumentTemplatePreview`。
4. 当前 template tool schema 只允许
   `weapon-projectile-warhead-skeleton@1` 和 `weaponId/projectileId/warheadId` 三个参数。
5. 当前模板只创建三个 Section，并只写 Weapon.`Projectile`、Weapon.`Warhead` 两条引用；Projectile 和
   Warhead 为空，也不把 Weapon 绑定回现有单位。因此截图中的结果是现有契约的确定输出，而不是模型
   偶然漏写。
6. 现有 `Ra2ContentTemplateCompiler`、Gateway Preview、Coordinator、主工作区 Diff 和显式 Apply 是唯一
   可复用权威，不允许新建第二套 patch/apply 路径。
7. 当前 `SetAiAssistantSendingState` 已在请求期间禁用模型选择器，可复用同一生命周期约束模式选择器。
8. 当前未决提案拥有唯一 active slot；新请求会使旧提案 superseded。模式切换必须服从该生命周期，不能
   隐式丢弃未处理提案。

### 1.2 直接根因

当前错误不是“DeepSeek 没理解完整对象”，而是本地 Router、Prompt 和 Tool Schema 共同强制它只能请求
关系骨架。若只改 prompt 或 UI 文案而不拆分 UserMode / CapabilityMode，并不能修复问题。

## 2. 任务上下文摘要（实施前强制项）

### 2.1 当前任务目标

完成可见、可键盘操作、请求期间稳定的 Chat/Work 模式，并让 Work 对受支持的 direct-fire weapon chain
生成完整可预览对象；显式 skeleton 请求继续保留现有骨架能力。

### 2.2 允许修改的文件

核心候选：

```text
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationTemplateContracts.cs
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationTemplateService.cs
RA2IniEditor.Application/Automation/Ra2ContentTemplateCompiler.cs
RA2IniEditor.IDE/AI/Ra2AiUserMode.cs                         (new)
RA2IniEditor.IDE/AI/Ra2AiInteractionRoute.cs
RA2IniEditor.IDE/AI/Ra2AiAuthoringToolCatalog.cs
RA2IniEditor.IDE/AI/Ra2AiPromptBuildRequest.cs
RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs
RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs
RA2IniEditor.IDE/AI/Ra2AiRequest.cs                          (仅请求模式快照)
RA2IniEditor.IDE/Themes/IdeWorkspaceStyles.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml                      (仅 AI 面板内部)
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs                   (仅 AI 模式/发送接线)
对应 RA2IniEditor.Application.Tests / RA2IniEditor.Tests 测试
本契约、Stage Ledger、CurrentPhase、Context、Capabilities、Decision/Public API 文档
```

若实现证明无需修改其中某个候选文件，应保持不变；不得为了符合清单制造改动。

### 2.3 禁止修改的文件和能力

```text
BuiltIn v3.2 字段数据
Field Registry provider priority / load / apply / rollback / learning
INI parser、Diagnostics、Completion、Hover、Quick Peek、Save Preflight
Undo/Redo 实现、Save/Backup/Rollback 实现
DeepSeek transport、SSE、timeout、retry、model catalog
AvalonDock 全局布局、Project Explorer、底部 Problems/Output、菜单/工具栏
独立 Agent Host、CLI、IPC/wire、Job/Event/Artifact
Icon/VOX/VXL/SHP 素材流水线
legacy solution/editor
项目文件和依赖版本
```

### 2.4 语义边界

- current-document only；
- Work 只产生候选 Plan，不自动 Apply/Save；
- Apply 继续由现有 IDE Host/User authority 完成；
- 不引入多文件事务和注册表数字索引维护；
- Field Registry 只验证字段事实、类型和 trust，不定义对象完整度或玩法默认值；
- complete profile 的结构由 Content Profile 定义，参数值由结构化 Work 请求提供并在 Diff 中可见；
- custom endpoint 仍是 advisory-only，不获得 Work 工具权限；
- 未支持类型 fail closed，不能生成空 Section 冒充完成。

### 2.5 必须保留和新增的 AutomationId

保留当前所有 `AiAssistant.*` AutomationId，新增：

```text
AiAssistant.ModeSelector
AiAssistant.ChatModeButton
AiAssistant.WorkModeButton
AiAssistant.ModeSummary
```

不重命名 `AiAssistant.PromptBox`、`ModelSelector`、`GenerateButton`、`CancelButton`、`SafetyFooter`、
`EditProposalCard.*` 或 `Document.AuthoringDiff`。

### 2.6 验证命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

另外运行 focused filters：Mode Router、Prompt/Tool Contract、Template Compiler/Service、AI Shell Boundary、
Authoring Transaction、Undo/Redo Boundary、Diff UI Contract。

不调用真实 DeepSeek，不使用电脑操控作为代码完成门禁；实现后请求用户在 1920x1080 和窄右侧面板各做
一次视觉/交互验收。

### 2.7 是否需要实施前批准

用户已于 2026-08-23 明确授权契约自审通过后连续执行。1A 来源门禁通过，1A -> 1F 已连续实现；自动
验证完成后，物理 UI 仍保留人工视觉验收边界。

## 3. Architecture Check

| 项 | 冻结决定 |
|---|---|
| 触达层 | Application Content Template、IDE AI route/prompt、WPF AI panel、tests/docs |
| canonical owner | UserMode：当前 AI 面板会话；Capability：Router/ToolCatalog；Plan/Preview：Gateway；Apply：IDE Host |
| 复用入口 | 现有 TemplateService/Compiler、PromptBuilder、Pipeline、RequestLifecycle、Coordinator、Diff |
| 生命周期 | 应用启动默认 Chat；同一进程内跨文档保持；不持久化；每次发送前复制为不可变 request snapshot |
| 并发 | 活动请求期间模式禁用；存在未处理 active proposal 时模式禁用；Apply/Dismiss 后恢复 |
| public API | UserMode 不公开；Template descriptor 增加 additive Experimental 完整度元数据 |
| 被拒绝的捷径 | 把 Chat/Work 塞进 Capability enum；只改 prompt；用模板 ID 字符串猜完整度；完整请求回退 skeleton |

## 4. Data Model Check

| 状态/类型 | 所有者 | 可变性 | 真值来源 | 失效/清理 |
|---|---|---|---|---|
| `Ra2AiUserMode` | IDE AI panel session | 仅 idle 且无 active proposal 时可变 | 用户显式 UI 选择 | 窗口关闭；不落盘 |
| `Ra2AiInteractionRoute` | 单次发送 | immutable | UserMode + prompt + local availability | 请求完成 |
| `Ra2AiCapabilityMode` | 单次发送 | immutable | Router | 请求完成 |
| Template completeness | Application catalog | immutable | source-audited Content Profile | 版本升级，不原地变义 |
| Template arguments | provider tool call | immutable/untrusted | DeepSeek structured arguments | 本地严格验证后编译或整体拒绝 |
| EditPlan/Preview | canonical Gateway/Workspace | immutable/versioned | captured snapshot + registry revision | stale/cancel/supersede |
| Apply authority | IDE Host | single-use | 用户点击 Apply | apply/dismiss/stale |

禁止新增持久化 schema。Chat/Work 不写入 layout XML、settings、project 文件或对话跨会话存储。

## 5. 模式与路由契约

### 5.1 用户模式

```csharp
internal enum Ra2AiUserMode
{
    Chat = 0,
    Work = 1
}
```

它描述用户意图，不等于底层工具列表。`Ra2AiInteractionRoute` 必须携带本次 `UserMode` 快照。

### 5.2 确定性矩阵

| UI 模式 | 提示词事实 | 本地结果 | 模型工具 |
|---|---|---|---|
| Chat | 任意文本，包括“修改/创建” | Advisory | none |
| Work | 明确“不要修改/只解释” | Advisory（选中模式不自动改变） | none |
| Work | 精确字段赋值/替换 | FieldEditPreview | `preview_ini_edit_plan` required |
| Work | 明确 skeleton marker + 武器链 | SkeletonTemplatePreview | skeleton template required |
| Work | 非 skeleton 的 direct-fire 武器链/同轴机枪请求 | CompleteWeaponChainPreview | complete template required |
| Work | 支持目标但关键身份无法确定 | NeedsClarification | 不生成 proposal；可由 required tool 返回 clarification |
| Work | 未支持的 Unit/Building/SuperWeapon/asset 完整对象 | UnsupportedWorkCapability | 本地明确提示；不发送、不收费 |
| Work | 配置、端点、可编辑快照或资源门禁失败 | WorkUnavailable | 本地明确提示；不发送 |

优先级：explicit no-edit > UI UserMode > explicit skeleton marker > supported complete profile > exact field edit >
clarification/unsupported。任何普通“武器链”关键词都不得再命中 skeleton。

### 5.3 skeleton marker

中文：`骨架`、`框架`、`占位`、`空结构`、`只建结构`。  
英文：`skeleton`、`scaffold`、`placeholder`、`empty structure`。

“搭建/构建/创建”本身不是 skeleton marker；例如“搭建一套可用武器链”必须走 complete profile。

## 6. 首个完整对象 Profile

### 6.1 稳定身份与范围

```text
template id: weapon-projectile-warhead-direct-fire-complete
version: 1
output kind: CompleteObject
scope: current document
owner: one existing, uniquely resolved TechnoType section
created: one Weapon + one Projectile + one Warhead section
```

现有 `weapon-projectile-warhead-skeleton@1` 保持原义，标记 `Skeleton`，不改版本、不悄悄扩充。

### 6.2 参数契约

完整 Profile 至少接收以下结构化参数；全部作为字符串进入现有 bounded template argument transport，再由
本地 parameter kind + effective field schema 二次验证：

```text
ownerSectionId       Identifier, required
ownerWeaponSlot      String enum Primary|Secondary, required
weaponId             Identifier, required
projectileId         Identifier, required
warheadId            Identifier, required
damage               Integer, required
rof                  Integer, required
range                Float, required
projectileSpeed      Integer, required
verses               String, required (严格 11 个百分比 token)
infDeath             Integer, required
cellSpread           Float, required
percentAtMax         Float, required
antiAir              Boolean, required
antiGround           Boolean, required
```

模型可以根据“同轴机枪、轻型、防空”等自然语言提出数值，但这些值必须出现在 Diff 中；本地不把它们写入
字段库，也不宣称它们是官方平衡值。若模型无法给出全部 required 参数，必须返回 needs_clarification。

### 6.3 编译后完整度不变量

成功 Plan 必须原子地包含：

1. 现有 `ownerSectionId` 上 `Primary=<weaponId>` 或 `Secondary=<weaponId>` 一条绑定；
2. Weapon：`Damage`、`ROF`、`Range`、`Projectile`、`Speed`、`Warhead`；
3. Projectile：`Inviso=yes`、`Image=none`、`AA=<antiAir>`、`AG=<antiGround>`；
4. Warhead：`Verses`、`InfDeath`、`CellSpread`、`PercentAtMax`；
5. 三个新 Section 均非空，引用闭合，Section ID 不冲突；
6. 所有字段通过 captured effective Field Registry 的 schema/trust 门禁；
7. 任何一项失败则无 partial plan、无 candidate text、无 active proposal。

该 Profile 定义“可连接、可预览的 direct-fire 最小完整链路”，不承诺自动平衡、声音/动画素材、注册列表、
Elite weapon、Burst、弹道特效或多文件绑定。完整度针对 Profile，不等于包含引擎所有可选字段。

### 6.4 Existing Section 支持

为避免另建组合 patch，`Ra2ContentTemplateCompiler` 只做 additive internal 扩展：Section spec 支持
`CreateNew` 与 `RequireExisting` 两种 target mode。

- `CreateNew` 保持当前行为；
- `RequireExisting` 必须唯一解析现有 Section，并使用它的实际 `Ra2SectionKind` 查询字段 schema；
- complete profile 的 owner 使用 `RequireExisting`；
- existing section 缺失、重复、类型不支持或 owner slot schema 不可用时整体失败；
- 最终仍只生成一个现有 `Ra2AutomationEditPlan`，不增加第二 Preview/Apply 服务。

### 6.5 Template 完整度元数据

在 Experimental descriptor 上 additive 增加：

```text
Ra2AutomationTemplateOutputKind.Skeleton
Ra2AutomationTemplateOutputKind.CompleteObject
Ra2AutomationTemplateDescriptor.OutputKind
```

不得通过 ID 包含 `skeleton`、DisplayName 或 Summary 文本猜测能力。该 API 继续标记 Experimental，并在
PublicApiLedger 中登记；不创建 wire DTO。

## 7. 精确 UI 契约

### 7.1 位置和尺寸

在现有 AI composer 卡片内部、输入框正上方增加一行紧凑 mode bar；不增加新的 Dock、不移动上下文、聊天
记录、模型选择或发送按钮。

```text
高度：24 DIP control + 4 DIP bottom gap
composer 左右 padding：沿用 6 DIP
Chat segment 最小宽：52 DIP
Work segment 最小宽：52 DIP
模式摘要：占剩余宽度，单行 CharacterEllipsis
```

1920x1080、默认右侧工具井和 320 DIP 窄面板均不得产生水平滚动或遮挡发送按钮。

### 7.2 视觉

- 一个连续的 segmented control，而不是两个原生 RadioButton 圆点；
- 未选：透明/Surface 背景、无重阴影；
- hover：`UiSurfaceHoverBrush`；
- 选中：`UiAccentSoftBrush` + 1 DIP `UiAccentBrush`，文字使用 accent pressed；
- 外轮廓 CornerSmall；两段之间只有 1 DIP divider，不出现双边框；
- 禁用：保持可识别选中状态并降至约 0.72 opacity；
- 使用既有 UI token 和 `UiFontFamily`，不硬编码系统字体或颜色。

### 7.3 文案

| 模式 | segment | `AiAssistant.ModeSummary` | SafetyFooter |
|---|---|---|---|
| Chat | 聊天 | 解释与建议 | 聊天模式只提供解释与建议；发送会联网，不修改文件。 |
| Work | 工作 | 结构化修改 · 预览后应用 | 工作模式只生成本地预览；仅点击应用后修改当前文件，不自动保存。 |

空状态的既有两个 TextBlock 随模式更新，但保留 AutomationId：

- Chat：`询问 INI 规则、字段或当前文档内容。` / `不会生成可应用修改。`
- Work：`描述当前文件要完成的修改。` / `先生成结构化 Diff，再由你决定是否应用。`

### 7.4 交互和无障碍

- 默认选中 Chat；整个 segment 可点击；
- Tab 可进入，左右方向键切换，Space/Enter 选择；
- `AutomationProperties.Name` 分别为“聊天模式”“工作模式”；
- 活动请求期间禁用两个 segment；
- 存在未 Apply/Ignore 的 active proposal 时禁用，并以 ToolTip 提示“请先应用或忽略当前修改建议”；
- Clear 不重置模式；文档切换不重置；应用重启恢复 Chat；
- 模式变化立即更新摘要、空状态和 SafetyFooter，不发送请求、不清空输入、不清空历史；
- 当前 Diff/Proposal 生命周期不因模式控件事件被隐式销毁。

## 8. 连续实施计划

### AGENT-MODE-1A — Source/Profile Gate

1. 复核 Weapon/Projectile/Warhead 和 owner Primary/Secondary 的 ModEnc/字段库证据；
2. 冻结 direct-fire Profile 字段、参数类型和 completeness invariants；
3. 任一 required 字段为 guardrail/obsolete/non-existent 或无法验证时停止，不以 inferred fallback 冒充完成；
4. 产出 `AGENT-MODE-1A_DirectFireCompleteProfileSourceAudit.md`。

### AGENT-MODE-1B — Headless UserMode / Router

1. 新增 internal `Ra2AiUserMode`；
2. Route 携带 send-time mode snapshot；
3. Chat 强制零工具；Work 执行确定性矩阵；
4. skeleton marker 独立解析；普通“武器链”不得命中 skeleton；
5. 不支持/含糊/不可用均为 typed local result，禁止发送。

### AGENT-MODE-1C — Complete Content Profile

1. Template descriptor 增加 OutputKind；
2. Compiler additive 支持 RequireExisting owner target；
3. 注册 complete direct-fire template，保留 skeleton v1 原义；
4. Tool schema 对 skeleton 和 complete 使用互斥 capability 定义；
5. 完整 Profile 生成 owner binding + 三个非空 Section；
6. 全失败原子性、stale、limits、cancellation 保持。

### AGENT-MODE-1D — Prompt / Pipeline Integration

1. PromptBuildRequest/Request 记录 UserMode snapshot；
2. Work field、skeleton、complete 三种 capability 使用互斥 required tool contract；
3. complete route 不允许模型返回 skeleton template id；
4. plain text 替代 required tool 继续映射 typed failure；
5. 现有 streaming、failure taxonomy、request diagnostics 不改语义。

### AGENT-MODE-1E — Explicit UI / Shell Wiring

1. 实现专用 segmented style；
2. 在 composer 上方接入 Chat/Work；
3. 发送前捕获 mode，活动请求/提案期间锁定；
4. 动态摘要、空状态和 footer；
5. Work proposal 自动打开现有主工作区 Diff；
6. Shell Dock topology、浮动宽高、菜单和布局存储零变化。

### AGENT-MODE-1F — Verification / Documentation Closeout

1. focused + Application full + IDE full + clean package；
2. 静态 XAML/AutomationId、public API allowlist、dependency direction 检查；
3. Undo/Redo 只验证不修改；若复现用户报告，单列 `AI-AUTHORING-UNDO-1`，不得夹带修复；
4. 更新 Stage Ledger、DecisionLog、PublicApiLedger、CurrentPhase、Context、Capabilities、UserGuide；
5. 请求用户完成 1920x1080 与窄面板手工 UI 验收。

## 9. 测试矩阵

| 场景 | 必须结果 |
|---|---|
| Chat + “修改当前文件 Strength=150” | 零工具 advisory；无 proposal/Apply |
| Work + 同一请求 | field edit tool；可见 Diff；不自动 Apply |
| Work + “搭一个武器链” | complete route 或 clarification，绝不 skeleton |
| Work + “只搭骨架武器链” | skeleton route |
| Work + HTNK 同轴机枪 | owner Secondary/Primary + 3 个非空 Section 的一个原子 Preview |
| complete tool 返回 skeleton id | 本地拒绝 |
| complete 参数缺 1 项 | clarification/failure；无 partial plan |
| owner 不存在/重复 | failure；无 partial plan |
| Section ID 冲突 | failure；无 partial plan |
| 字段 trust blocked | failure；无 partial plan |
| request active | mode selector disabled |
| proposal active | mode selector disabled，Apply/Ignore 后恢复 |
| 关闭 Diff | proposal 保留，可从卡片重开 |
| Apply | 现有单次 transaction；不 Save；Problems refresh |
| Undo smoke | 验证现有一事务撤销；失败则本阶段不宣称通过 |
| custom endpoint + Work | 本地拒绝；不发送 |
| 8 MiB 超限 + Work | 本地拒绝；不发送 |
| narrow pane | selector/summary ellipsis，无横向滚动和按钮遮挡 |

## 10. Public API Ledger 候选

仅允许以下 additive Experimental 变化：

```text
Ra2AutomationTemplateOutputKind                       new enum
Ra2AutomationTemplateDescriptor.OutputKind            new property
```

`Ra2AiUserMode`、route kinds、UI state、complete profile definition、existing-section target mode 均保持 internal。
若 1A 证明 complete profile 无法通过 source gate，则上述 public diff 也不实施。

## 11. 自审

### 11.1 已通过

- Reuse-first：继续使用 TemplateCompiler -> EditPlan -> Gateway Preview -> Workspace -> Host Apply；
- Authority：模式不授予 Apply；模型只有结构化 proposal 权限；
- Data ownership：字段库与对象 Profile 分离；
- Anti-placeholder：complete 和 skeleton 有机器可读差异，互斥 tool schema；
- Compatibility：skeleton v1 原义不变；Chat 为默认，现有 advisory overload 保持；
- Failure atomicity：缺参、冲突、低可信、stale、cancel、limit 均无 partial plan；
- UI：只改 AI panel 内部，不改变 Dock topology；
- Host future：不冻结 wire/session DTO，但 descriptor 已能机器区分完整度；
- Cost：含糊/不支持/本地不可用在调用模型前停止，避免无效付费请求。

### 11.2 受控剩余风险

1. “完整”是 Profile-relative，不等于枚举引擎所有可选 flag；UI/文档必须使用“direct-fire 完整链路”，
   不能宣传成任意对象生成。
2. complete Profile 的 exact fields 必须通过 1A source gate；本契约列出的候选字段不是跳过来源审计的授权。
3. 模型提出的平衡值仍可能不理想，但全部可在 Diff 中审核，不自动应用。
4. 用户报告的 AI Apply Undo 需纳入验证；若复现，必须由独立 Undo 契约修复。
5. 真实 DeepSeek 结构化参数服从性仍需用户手工验收；代码测试只使用 fake/loopback，不消耗 API。

### 11.3 最终审查结论

契约在“模式分离、显式 UI、骨架门禁、首个完整武器链、单一 Preview/Apply 权威”范围内足够可靠，且避免
了当前最容易返工的三种做法：把模式混入 Capability、让完整请求继续走 skeleton、另建组合 patch 路径。

它不批准任意 Unit/Building/Asset 完整对象生成，也不批准独立 Host。完成本包后，下一阶段应是
`CONTENT-2A Techno Complete Profiles`，随后再进入 `AI-AUTHORING-UNDO-1`（若验证失败）和
`HOST-1 Independent Agent Host`。
