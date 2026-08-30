# CONTENT-2E SuperWeapon / Support Power 代码事实审计

状态：**Completed / contract input only**  
日期：2026-08-25  
风险：R3（内容编排、Work 路由和完整对象语义；不涉及持久化或新写权限）

## 1. 审计结论

当前 Agent 已经具备完成 SuperWeapon 写入所需的大部分通用基础设施，但尚未具备可对外宣称的
“完整超级武器 / 支援技能”能力。缺口不是 INI 编辑器、Project Diff 或事务，而是：

1. 没有任何 SuperWeapon complete template/profile；
2. Work 第一阶段把完整 SuperWeapon 明确归类为 unsupported；
3. `ra2-superweapon-authoring` 只有通用流程知识，没有按引擎、类型拆分的可执行知识；
4. 没有 SuperWeapon 专用的 provider、注册、效果对象和 AI targeting 闭包测试；
5. “支援技能”尚未稳定归一到 `superweapon` domain。

因此当前能力是“可查询字段、可由模型自由生成通用项目修改”，不是“具有来源约束和闭包证据的
完整 SuperWeapon authoring”。

## 2. 已存在并必须复用的能力

| 事实 | 现有权威 | 结论 |
|---|---|---|
| 数字注册表编译 | `Ra2ContentTemplateRegistrationSpec` + `ExplicitNumberedList` | 已原生包含 `[SuperWeaponTypes]`，不得另建注册器 |
| 当前文档模板编译 | `Ra2ContentTemplateCompiler` | 可生成 Section/Field/注册 operation，继续作为唯一模板编译器 |
| 项目模板入口 | `IRa2AutomationTemplateService.ExpandProjectTemplate` | 已有 public Experimental 入口，不需要新增 Gateway 方法 |
| Project Snapshot/Plan/Preview | `Ra2AutomationProjectSnapshot`、`Ra2AutomationProjectEditPlan`、`PreviewProject` | 已能生成可视化 Project Diff |
| Apply/Undo | 现有 project transaction coordinator | 显式 Apply、compound Undo、失败回滚已存在；不得建立第二事务系统 |
| Work 两阶段调用 | Intent analysis + execution | 第一轮已能从同一 Skill manifest 选择 Skill，第二轮消费已解析正文 |
| 有界查询 | `AGENT-CONTEXT-3` | 可读取捕获的 `current/rules/art` Section/引用，模型不能提交路径 |
| 一次修复 | `AGENT-REPAIR-1` | 只允许 typed、模型可修正的结构化失败；不得扩展为开放重试 |
| 通用内容自由 | NF6 model-owned project plan | Field Registry / Diagnostics 只能提供 advisory evidence，不能否决未知合法 mod 内容 |

## 3. 当前硬缺口

### 3.1 Application 内容层

- `Ra2AutomationTemplateService` 当前目录只有 Weapon、Projectile、Warhead 与 Techno rules/art binding；
- `ExpandProjectTemplate` 当前只接受 `techno-rules-art-asset-binding`；
- `Ra2ContentProjectTemplateCompiler` 只实现 rules/art pair 编译，没有 rules-only 项目模板；
- `[SuperWeaponTypes]` 虽已在注册类型目录中，但没有模板真正声明该 registration；
- 没有 provider Building、SuperWeapon Section 和 effect reference 的组合闭包。

### 3.2 IDE Agent 路由

- `Ra2AiInteractionRoute.LooksLikeUnsupportedCompleteObject` 直接匹配“超级武器”；
- `Ra2AiIntentAnalysisStage` 的系统约束明确声明 complete SuperWeapon unsupported；
- authoring capability allowlist 没有 SuperWeapon capability；
- provider-visible schema 也没有 SuperWeapon capability ID；
- domain 能识别 `superweapon`，但“支援技能 / support power”同义词覆盖不足。

### 3.3 知识层

- `ra2-superweapon-authoring` 已说明类型、provider、targeting、effect closure 和 cameo 的必要性；
- 但它没有冻结 Ares 各 Type 的专用默认值、互斥组合和 Phobos additive extension；
- 现有 BuiltIn v3.2 中有 150 个 `SuperWeapon` 适用字段且均为 source-backed 数据，但字段条目只应作为
  查询与风险提示，不能替代对象级 Profile，也不能形成枚举式否决器。

## 4. 来源结论

本阶段只把官方 Ares 与 Phobos 文档作为运行时 Skill 内容的来源；社区页面只能作为后续交叉验证，
不得覆盖官方定义。

- Ares 说明原版 YR 对新建/修改 SuperWeapon 限制很大，而 Ares 提供新的可定制类型；新类型应在
  `rulesmd.ini` 中定义并由 provider building 引用：
  https://ares-developers.github.io/Ares-docs/new/superweapons/index.html
- Ares 明确声明每个 SuperWeapon Type 有自己的通用默认值和专用字段，因此不能建立万能混合模板：
  https://ares-developers.github.io/Ares-docs/new/superweapons/types/index.html
- `UnitDelivery` 的核心闭包是 `Deliver.Types`、owner 与 placement semantics：
  https://ares-developers.github.io/Ares-docs/new/superweapons/types/unitdelivery.html
- `GenericWarhead` 的核心闭包是 `SW.Damage` + `SW.Warhead`，并需要考虑 Warhead 的 CellSpread：
  https://ares-developers.github.io/Ares-docs/new/superweapons/types/genericwarhead.html
- AI targeting 的 dependent defaults 和互斥组合可能让 SuperWeapon 永远无法发射：
  https://ares-developers.github.io/Ares-docs/new/superweapons/targeting.html
- Phobos 在 Ares/原版基础上提供额外 SuperWeapon 与 effect bridge；这些应作为 additive modifier，不能
  混入 Ares base profile：
  https://phobos.readthedocs.io/en/latest/New-or-Enhanced-Logics.html

## 5. 架构裁决输入

### 5.1 规范路径

```text
用户 Work 请求
  -> 第一轮 intent + Skill manifest 选择
  -> 有界 current/rules/art 查询
  -> 已知类型：现有 Template Gateway + project template compiler
     未知/高级类型：现有 model-owned project edit tool
  -> canonical Project Preview
  -> UI Project Diff
  -> 用户显式 Apply
  -> compound Undo
```

不允许出现第二 parser、第二字段注册表、第二 Project Diff、第二 Apply/Undo 或直接文件写入。

### 5.2 数据所有权

- SuperWeapon Profile 目录：Application internal、进程生命周期、不可变、非序列化；
- identity：`profileId + version`；profile ID 同时编码引擎与类型；
- 模板参数：request lifetime，继续使用既有 `Ra2AutomationTemplateExpansionRequest`；
- Skill manifest/resolution：IDE internal、request lifetime、非持久化；
- Plan/Preview：沿用已存在的 immutable Experimental DTO；
- Apply/Undo：沿用 editor session / project transaction authority；
- 不新增配置文件、缓存、项目元数据或迁移。

### 5.3 Public API 预审

预期 public API diff 为零：

- 新 profile 是既有 descriptor catalog 数据；
- project template 继续通过既有 `ExpandProjectTemplate`；
- 新编译、profile catalog、target resolver 和 validation 均为 internal；
- provider-visible capability/tool JSON 属 IDE Experimental wire shape，必须登记并做严格回归，但不增加
  Application exported API；
- 不新增 Apply、Save、filesystem、path 或 asset provider 权限。

若实现事实证明必须增加 public DTO、enum、Gateway 方法或持久化字段，必须停止连续执行并重新审批。

## 6. 复用裁决

| 需求 | 复用 | 禁止方案 |
|---|---|---|
| `[SuperWeaponTypes]` 注册 | `Ra2ContentTemplateRegistrationSpec` | 新 numbered-list helper |
| Section/Field 操作 | `Ra2ContentTemplateCompiler` | 字符串拼接完整 INI |
| rules 目标 | Project Snapshot 成员 | 模型路径、递归磁盘扫描 |
| 引用核实 | Gateway `GetSection/ResolveReference` | IDE 私有 parser/index |
| Diff | `PreviewProject` + 现有 AuthoringDiff | 新 SuperWeapon 预览窗口 |
| Apply/Undo | 现有 transaction coordinator | 模板直接写文件 |
| 知识注入 | `Ra2AgentSkillCatalog` | 把全部文档塞入全局 system prompt |
| 未知合法内容 | model-owned project plan | Field Registry enum 阻断 |

## 7. 风险分级

本任务为 R3，理由是它改变 complete-object authoring 能力和 provider-visible route/schema，但不增加文件
权限、自动保存、持久化或 public API。治理结论为 `StopForReview`：本审计和契约可落盘；运行时代码必须
等待用户明确批准。

