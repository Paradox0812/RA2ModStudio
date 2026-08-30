# CONTENT-2E SuperWeapon / Support Power Complete Profiles Continuous Final Contract

状态：**Completed / automated verified**  
日期：2026-08-25  
风险：R3  
连续阶段：`2E-0 -> 2E-5`

## 0. 批准语句

批准后使用：

```text
批准 CONTENT-2E 最终契约，连续执行 2E-0 → 2E-5
```

批准后各阶段完成即自审并继续，不再逐阶段等待；任何 Stop Condition 命中时立即停止，不得静默扩大范围。

## 1. 目标与产品承诺

本连续包让 Work 模式可以：

1. 把“超级武器 / 超武 / 支援技能 / support power”识别为 SuperWeapon authoring；
2. 对两个高价值、官方文档闭合的 Ares 类型形成 typed complete proposal：
   - `Ares UnitDelivery Complete`；
   - `Ares GenericWarhead Complete`（引用项目中既存 Warhead）；
3. 在项目中生成 `[SuperWeaponTypes]` 注册、SuperWeapon Section、provider Building 绑定和必要效果引用；
4. 在主编辑区显示现有 Project Diff，用户显式 Apply 后形成一个 compound Undo；
5. 对其它原版/Ares/Phobos 超武类型使用现有 model-owned project plan，在来源/上下文不足时 clarification，
   不再被 blanket unsupported 拦截；
6. 不自动应用、不自动保存、不生成或写入素材文件。

“Complete”只表示本次 SuperWeapon 对象闭包完整：注册、provider 或明确 AlwaysGranted 策略、类型专用字段、
所有引用均能在捕获项目中解析。它不表示 IDE 会新建所有可复用依赖或保证实际游戏平衡。UnitDelivery 引用
既存 TechnoTypes；GenericWarhead v1 引用既存 Warhead。若用户要求同时创建新的 effect object，模型应明确
clarification 或进入后续 compound profile，不能偷偷降级为骨架。

## 2. 明确非目标

- 不承诺所有原版/YR/Ares/Phobos SuperWeapon 类型都具有 typed complete profile；
- 不实现 Phobos `LimboDelivery`、`Detonate.Weapon/Warhead`、`LaunchSW` 等 typed compound profile；
- 不生成 SHP/VXL/HVA/Cameo/CSF 素材，不验证素材文件存在；
- 不实现游戏进程 Hook、自动启动游戏或实际战局行为测试；
- 不修改 Field Registry 数据、优先级、Hover、Quick Peek、Diagnostics、Completion 或 Save Preflight；
- 不增加 retry、模型 fallback、provider 切换或第四次模型调用；
- 不修改 XAML、Dock、Shell 布局、Chat/Work 控件或 AutomationId；
- 不恢复 legacy。

## 3. 任务范围冻结

### 3.1 允许的实现文件

仅在实际依赖闭包需要时修改：

```text
RA2IniEditor.Application/Automation/**
RA2IniEditor.Application/Automation/Experimental/Ra2AutomationTemplateService.cs
RA2IniEditor.IDE/AI/Ra2AiInteractionRoute.cs
RA2IniEditor.IDE/AI/Ra2AiIntentAnalysisStage.cs
RA2IniEditor.IDE/AI/Ra2AiAuthoringToolCatalog.cs
RA2IniEditor.IDE/AI/Ra2AiAuthoringToolAdapter.cs
RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs（仅必要 Skill/capability 接线）
RA2IniEditor.IDE/AgentSkills/ra2-superweapon-authoring/SKILL.md
RA2IniEditor.IDE/AgentSkills/ra2-superweapon-ares-types/**
RA2IniEditor.IDE/AgentSkills/ra2-superweapon-phobos-extensions/**
RA2IniEditor.Application.Tests/**SuperWeapon**
RA2IniEditor.Tests/IDE/**SuperWeapon**
既有直接相关测试文件
Docs/AUTOMATION-CONTENT-2E_*.md
Docs/DecisionLog.md
Docs/PublicApiLedger.md
Docs/CurrentCapabilities.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/README.md
必要产品文档
```

### 3.2 禁止修改

```text
ShellWindow.xaml
Shell 主布局、菜单、工具栏、Dock、Project Explorer、底栏、状态栏
所有非必要 XAML / UI 主题
INI parser / serializer semantics
Field Registry provider priority 与 BuiltIn v3.2 数据
Hover / Quick Peek / AI Evidence
Diagnostics / Completion / Save Preflight
Backup / Rollback / Save / filesystem authority
自动 Apply / 自动 Save
Asset Provider / ASSET-HOST-1 / 素材写入
legacy solution / legacy editor
```

若测试证明必须修改 XAML、parser、Registry 数据或 public API，连续执行立即停止并报告。项目快照捕获所需的
`ShellWindow.xaml.cs` rules-only 窄边界例外已由用户于 2026-08-25 明确批准；它不得扩展到 XAML、布局或其它 Shell 行为。

## 4. 核心架构契约

### 4.1 双轨 authoring

```text
Known typed profile
  -> deterministic Template Expansion
  -> canonical Project Preview

Other source-backed SuperWeapon request
  -> existing model-owned project edit tool
  -> canonical Project Preview
```

两轨最终都必须进入同一个 Project Preview / Diff / Apply / Undo 权威。typed profile 提供更高确定性，
但不得成为未知合法 mod 内容的否决器。通用轨仍只接受项目成员的 Section/Field operation，不接受路径或
完整候选文件文本。

### 4.2 Profile 数据模型

允许新增一个 internal immutable profile catalog；不得新增 public DTO：

```text
Ra2SuperWeaponProfileDefinition (internal)
  ProfileId + Version                  // 唯一身份
  EngineFamily                        // Ares3；不从字段猜测
  SuperWeaponType                     // UnitDelivery / GenericWarhead
  OutputKind                          // CompleteObject
  RequiredArguments                   // 使用既有 descriptor/request 参数
  RegistrationSpec                    // [SuperWeaponTypes]
  ProviderBindingPolicy               // ExistingBuildingSlot / AlwaysGrantedExplicit
  EffectReferencePolicy               // Techno list / existing Warhead
  CompatibleTargetingPolicy           // type-specific
```

- catalog 进程生命周期、静态不可变、非序列化；
- expansion request、intent package、Skill resolution 均为 request lifetime；
- 不写项目配置，不缓存模型决定，不引入 schema migration；
- profile version 变化必须新增版本，不得静默改变旧版本输出。

### 4.3 rules 目标选择

- typed project template 只修改一个已捕获、属于同一 Project Snapshot 的 rules 文档；
- 候选仅为顶层 `rulesmd.ini` 或 `rules.ini`；
- 恰好一个候选时使用它；零个候选返回 typed missing-document；两个候选且上下文未明确目标时返回
  clarification/ambiguous，禁止静默优先、磁盘扫描或路径猜测；
- art 文档不是本阶段前置条件，也不得因存在 art 而产生空操作；
- current document 不属于 snapshot 时不得偷偷降级到当前文档 Apply。

### 4.4 最低本地门禁

本地门禁只覆盖结构和明确来源约束：

- captured snapshot/revision、项目成员身份、操作数和字符预算；
- Section/key/identifier 安全格式；
- `[SuperWeaponTypes]` 数字注册结构和 max+1 分配；
- provider/effect 引用必须在捕获项目中唯一解析；
- typed profile 固定的 `Type` 与官方枚举/互斥项；
- canonical Preview、stale、single-use、atomic rollback；
- 用户显式 Apply，绝不自动 Save。

Field Registry 的 Enum、未知字段、source priority 与 Diagnostics 只提供 advisory evidence，不得阻断通用
模型计划。禁止重现“旧枚举不包含新素材 ID，因此拒绝 AI 输出”的问题。

## 5. 类型契约

### 5.1 Ares UnitDelivery Complete

输入至少包含：

```text
superWeaponId
providerBuildingId 或明确 alwaysGranted=true（二选一）
providerSlot = SuperWeapon | SuperWeapon2（provider 模式）
uiName
name
rechargeTime
sidebarImage（只是 INI 资源引用，不检查素材文件存在）
deliveryTypeIds（1..16，逗号分隔 canonical identifiers）
deliveryOwner（官方 owner 枚举）
aiTargeting / target policy（按 UnitDelivery/Ares 约束）
```

成功闭包：

- `[SuperWeaponTypes]` 包含且只新增一次 `superWeaponId`；
- 新建唯一 `[superWeaponId]`，写入经 2E-0 来源矩阵冻结的 `Type/Action`、通用显示/充能字段和
  `Deliver.*` 字段；
- 每个 `deliveryTypeId` 唯一解析为 Infantry/Vehicle/Aircraft/Building 之一；
- provider 模式只修改唯一 Building 的指定槽；AlwaysGranted 模式不伪造 provider；
- 不创建 Techno、art Section 或素材。

### 5.2 Ares GenericWarhead Complete

输入至少包含：

```text
superWeaponId
providerBuildingId 或明确 alwaysGranted=true（二选一）
providerSlot = SuperWeapon | SuperWeapon2（provider 模式）
uiName
name
rechargeTime
sidebarImage
warheadId（必须是项目中既存、唯一 Warhead）
damage
aiTargeting / target policy（按 GenericWarhead/Ares 约束）
```

成功闭包：

- 与 UnitDelivery 相同的注册和 provider/AlwaysGranted 规则；
- 新建唯一 SuperWeapon Section，固定 `Type=GenericWarhead`，包含 `SW.Damage`、`SW.Warhead` 和经来源
  矩阵冻结的通用字段；
- `warheadId` 必须解析为 Warhead；若用户要求新建 Warhead，本 profile 返回 clarification，不生成空 Section；
- Warhead 缺少 CellSpread 时只产生可审阅风险提示，不擅自改变已有 Warhead；
- provider 是否需要 `DamageSelf=yes` 由用户意图决定，模型不得静默改动。

### 5.3 其它类型的通用轨

- 移除 complete SuperWeapon 的 blanket unsupported；
- 新增 `superweapon-project-edit` capability，复用现有通用 project edit tool；
- 第一轮必须选择 `ra2-superweapon-authoring`，并按 engine/type 增补 Ares 或 Phobos Skill；
- 不明确 engine/type/provider/effect 时返回 `needs_clarification`；
- 用户没有要求骨架时，不能只输出 Section 名和 TODO；
- 模型可以生成项目中合法的新字段/引用；Host 只执行第 4.4 节最低门禁；
- UI 将其标记为“模型主导 / 需复核”，不声称 typed complete。

## 6. Skill 契约

### 6.1 Skill 结构

- `ra2-superweapon-authoring` 升级为 core v2：对象图、注册、provider、availability、charge、targeting、
  sidebar/effect closure 和 clarification 规则；
- 新增 `ra2-superweapon-ares-types` v1：按 Type 分表记录专用字段、默认值和互斥组合；
- 新增 `ra2-superweapon-phobos-extensions` v1：只记录 Phobos additive extension，不覆盖 Ares base type；
- 每个 Skill 保存来源 URL、适用版本和“未知即澄清”规则；不包含脚本、网络调用或运行时下载。

### 6.2 选择规则

- 第一轮模型从同一 manifest 推荐；Host 保留最终解析权；
- typed Ares capability 强制 core + Ares types；
- 显式 Phobos 请求强制 core + Phobos extensions，必要时同时注入 Ares types；
- Field trust Skill 可追加但不能否决通用计划；
- 继续遵守最多 6 个 Skill、14 KiB 正文预算和稳定去重；不增加第三次 Skill 调用。

## 7. Work 路由与工具契约

新增 provider-visible capability IDs：

```text
ares-unitdelivery-superweapon-complete
ares-genericwarhead-superweapon-complete
superweapon-project-edit
```

规则：

- Chat 仍为 advisory 单调用；Work 正常仍为分析 + 执行两调用；
- “支援技能 / support power”归一为 `superweapon`，不能误判为 C# Skill/plugin；
- 明确 UnitDelivery/空投单位能力 -> typed UnitDelivery；
- 明确 GenericWarhead/区域伤害且引用既存 Warhead -> typed GenericWarhead；
- 其它明确类型 -> generic project edit；
- “做一个超武”但未给 engine/type/effect/provider -> clarification；
- unknown capability、tool JSON、template failure 继续走现有 typed failure 和最多一次 bounded repair；
- repair 不重跑 intent/Skill/HLI，不切模型，不扩大目标文档。

## 8. 阶段计划

### 2E-0 Source / Capability Matrix Freeze

交付：

- 冻结 YR Vanilla / Ares 3 / Phobos 三层矩阵；
- 冻结 UnitDelivery、GenericWarhead 的 exact tags、default/required/forbidden combinations；
- 冻结 provider 与 AlwaysGranted 两种闭包；
- 更新 core/Ares/Phobos Skill 来源矩阵；
- 建立 source-based test vectors。

门禁：若官方来源不能确认 `Type/Action` 或某个必需默认值，停止，不能凭记忆实现。

### 2E-1 Internal Profile Catalog + Rules-only Project Expansion

交付：

- internal immutable profile catalog；
- 复用现有 registration/compiler；
- `ExpandProjectTemplate` 支持 rules-only SuperWeapon templates；
- unique rules target resolver；
- public API/exported type/Gateway method/capability count零变化。

### 2E-2 Ares UnitDelivery Complete

交付：descriptor、definition、reference validation、provider/AlwaysGranted、registration、project plan、
focused tests。任何引用缺失、类型错误、Section 冲突或列表超限均返回 typed failure，零 partial plan。

### 2E-3 Ares GenericWarhead Complete

交付：descriptor、definition、existing-Warhead closure、targeting compatibility、provider/AlwaysGranted、
registration、project plan、focused tests。不得静默创建空 Warhead 或修改其 CellSpread。

### 2E-4 Work Routing + Skill Integration

交付：三个 capability ID、同义词路由、provider schema、tool adapter、必选 Skill、generic fallback、
bounded repair evidence。Chat 行为和其它 complete profile 输出必须零变化。

### 2E-5 End-to-End Verification + Documentation Closure

交付：

- headless template/profile/registration/reference/project preview tests；
- Work intent/tool/Skill/repair integration tests；
- Apply/compound Undo/no Save regression；
- 全量 Application/IDE tests、Debug build、IdeOnly clean package；
- `CurrentCapabilities` 必须标为 Partial：仅两个 typed profiles，其它为模型主导需复核；
- Stage Ledger、DecisionLog、PublicApiLedger 和上下文文档收口；
- 提供人工 DeepSeek + WPF 验收脚本，不代替自动证据。

## 9. 验证矩阵

### 9.1 正向

- rules.ini 与 rulesmd.ini 单候选分别成功；art 缺失不影响 rules-only profile；
- UnitDelivery：1 个与多个既存 TechnoType；四种允许的 Techno 家族；
- GenericWarhead：既存 Warhead、合法 damage、manual/AI targeting；
- provider `SuperWeapon` 与 `SuperWeapon2`；显式 AlwaysGranted；
- `[SuperWeaponTypes]` 空表、稀疏编号、重复执行 idempotency；
- proposal -> Project Diff -> Apply -> 一个 compound Undo -> 文本完全恢复；
- Apply 后仍未保存，dirty 状态符合现有编辑器语义。

### 9.2 负向

- 无 rules、rules/rulesmd 歧义、目标不在 snapshot；
- `[SuperWeaponTypes]` 重复编号、非数字 key、溢出；
- SuperWeapon ID 已存在、provider 缺失/重复/非 Building、槽位非法；
- delivery list 空、超限、标识符非法、引用缺失或类型错误；
- Warhead 缺失/重复/错误 kind；
- targeting 互斥组合；
- stale snapshot、过大项目、过多 operation、取消；
- generic route 未知字段不被 Registry Enum 阻断，但路径/项目成员/资源门禁仍拒绝；
- repair 不处理 transport/timeout/cancel/stale/resource/safety failure。

### 9.3 回归

- 现有 Weapon chain、dual armament、Projectile、Warhead、rules/art binding 输出 byte-for-byte 不变；
- Chat 单调用不变；普通 Work 两调用不变；repair 上限不变；
- Gateway public methods/capability catalog 与 exported type allowlist 不变；
- 除经批准的 `ShellWindow.xaml.cs` rules-only 项目快照捕获 wiring 外，Shell/XAML/AutomationId 不变；
  parser、Registry、Diagnostics、Completion、Save/Undo 契约不变；
- legacy 未恢复。

### 9.4 命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-restore --filter SuperWeapon
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-restore --filter SuperWeapon
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-restore
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

测试项目若过滤器不能命中，必须改为精确 fully-qualified-name 或运行该测试项目全量；不得把零测试当成功。

## 10. 人工验收用例

### 10.1 UnitDelivery

```text
工作模式。在当前项目的 rules 文件中，为既有建筑 [GAPOWR] 创建 Ares UnitDelivery 支援技能
[GAREINFORCEMENTS]，注册到 [SuperWeaponTypes]，由 GAPOWR 的 SuperWeapon2 提供；投送项目中
已经存在的 E1 和 FV，使用 invoker 所有权。请生成完整可用对象并只预览，不要自动应用或保存。
```

验收：Project Diff 只含 rules；有注册、provider、完整 SuperWeapon Section；没有 art/素材写入；Apply 后
Undo 可一次恢复。

### 10.2 GenericWarhead

```text
工作模式。在当前项目的 rules 文件中，为既有建筑 [NANRCT] 创建 Ares GenericWarhead 超武
[NAEMPBLAST]，使用项目中已存在的 [EMPWH]，伤害 1；请给出完整注册、provider 和 targeting 配置，
只预览，不自动应用，不修改 EMPWH，不保存。
```

验收：引用解析为既存 Warhead；没有新建空 Warhead；不擅自改 `DamageSelf` 或 `CellSpread`。

## 11. Stop Conditions

命中任意一项即停止连续执行：

1. 官方来源无法冻结某 typed profile 的关键 Type/Action/字段关系；
2. 需要新增或改变 public API、持久化格式、Apply/Undo/Save authority；
3. 超出已批准的 `ShellWindow.xaml.cs` 项目快照捕获窄边界，或需要修改 XAML、parser、Field Registry 数据或 Diagnostics/Completion；
4. 现有 Project Snapshot 无法在不接受路径的前提下标识 rules 成员；
5. 模板必须产生 art/素材操作才能通过，但用户没有要求；
6. focused 或 regression gate 失败且范围内无法修复；
7. 发现与现有 model-owned generic project decision 不兼容的双重内容权威。

## 12. 审查结论

契约自审：**通过；用户已批准 Shell 项目快照捕获窄边界例外，2E-0 → 2E-5 已连续完成并通过自动化门禁。**

通过理由：

- 优先复用现有注册、模板、Project Preview、Apply/Undo 和 Skill routing；
- typed profile 与通用 model-owned fallback 分工明确，不会让字段库重新成为内容否决器；
- 首包只覆盖两个官方文档闭合的 Ares 类型，不虚构“全面支持”；
- rules-only，不人为要求 art 或素材存在；
- public API、持久化、Shell UI/布局和写权限零变化；Shell 仅有已批准的 rules-only 捕获 wiring；
- 每个阶段均有明确输入、产物、门禁和 stop rule。

实现与验证证据见 `AUTOMATION-CONTENT-2E_SourceCapabilityMatrix.md` 和
`AUTOMATION-CONTENT-2E_StageLedger.md`。剩余不可消除风险：真实 DeepSeek 结构化参数服从度、
Ares/Phobos 版本差异、以及实际游戏行为只能通过人工/游戏内测试验证。它们必须在交付报告中标为
“未验证”，不能用单元测试冒充。
