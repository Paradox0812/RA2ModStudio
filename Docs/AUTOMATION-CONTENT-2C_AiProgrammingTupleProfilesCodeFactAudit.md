# CONTENT-2C AI Programming Tuple Profiles 代码事实审计

更新时间：2026-08-23  
状态：Completed / audit only  
契约状态：Deferred by user  
实现状态：Not started

## 1. 本轮结论

当前项目已经具备可靠的单文档结构化编辑、Preview、Diff、显式 Apply、一次 Undo 和
Problems 刷新链路，但**尚未实现 AI Programming Tuple Profiles**。现有 CONTENT 模板编译器
只能处理具有固定字段名、且能通过 Field Registry 精确 schema 查询的字段；它不能安全表达：

- `[TaskForces]`、`[ScriptTypes]`、`[TeamTypes]` 的动态数字注册键；
- TaskForce 对象 Section 中的 `0=count,unit` 成员元组；
- Script 对象 Section 中的 `0=action,argument` 动作元组；
- `[AITriggerTypes]` 中以触发器 ID 为 key、固定 18 个值槽的完整行；
- 上述对象之间的引用闭包、ID 冲突和动态索引分配。

因此，Work 模式当前可以生成已注册的 Weapon / Projectile / Warhead / Techno profile，
但对“创建完整 AI 编队/触发器”的请求必须保持 unsupported/fail closed。内置
`ra2-ai-programming` Skill 目前只提供知识和约束，不提供新的写入 capability。

用户已明确要求本轮审计后停止。本轮不制定最终实现契约、不修改生产代码、不开放 AI 写入。

## 2. 审计范围

已核对：

- `Ra2AutomationTemplateService` 的现有 profile catalog；
- `Ra2ContentTemplateCompiler` 的模板编译、字段 schema 和 trust 门禁；
- `Ra2AutomationEditPreviewEngine` 的 Preview 处置；
- `Ra2SectionClassifier` 对 AI registry 与数字键的识别；
- BuiltIn v3.2 的 TaskForce / Script / TeamType / AITrigger 精确字段覆盖；
- Chat / Work 路由、AI tool catalog 与 `ra2-ai-programming` Skill；
- 既有 Gateway、Workspace、Host Apply、Undo、Save 边界；
- RA2/YR AI 配置的公开语义资料。

未执行：

- 生产代码修改；
- profile、tool schema、prompt 或 UI 修改；
- 真实 DeepSeek 调用；
- WPF 人工烟测；
- build/test/package。文档审计不改变运行时，因此只执行文档差异检查。

## 3. RA2/YR AI 数据事实

### 3.1 TaskForce

- `[TaskForces]` 使用数字键注册 TaskForce ID；
- TaskForce 对象 Section 使用从 `0` 开始的有序成员行；
- 每行语义是 `数量,单位类型 ID`；
- 原版有效成员槽为 `0..5`；`Name` 和 `Group` 是普通命名字段。

来源：https://modenc.renegadeprojects.com/TaskForces

### 3.2 ScriptType

- `[ScriptTypes]` 使用数字键注册 Script ID；
- Script 对象 Section 使用从 `0` 开始的有序动作行；
- 每行语义是 `动作编号,参数`；
- 原版动作槽为 `0..49`；动作 `0` 是攻击指定 quarry，动作 `49` 可登记成功。

来源：

- https://modenc.renegadeprojects.com/ScriptTypes
- https://modenc.renegadeprojects.com/ScriptTypes/ScriptActions

### 3.3 TeamType

- `[TeamTypes]` 使用数字键注册 TeamType ID；
- TeamType 对象 Section 用命名字段引用 TaskForce 与 Script；
- 完整对象至少需要处理 `Name`、`TaskForce`、`Script`、`House`、`Priority`、`Max`、
  `TechLevel`、`Group` 和行为布尔值，而不是只创建空 Section。

来源：https://modenc.renegadeprojects.com/TeamTypes

### 3.4 AITriggerType

- `[AITriggerTypes]` 不是“注册列表 + 独立对象 Section”，而是以 Trigger ID 为 key 的固定元组；
- value 有 18 个位置相关槽：名称、Team1、OwnerHouse、TechLevel、ConditionType、
  ConditionObject、Comparator、三项权重、Skirmish 标志、unused、Side、BaseDefense、Team2、
  Easy/Medium/Hard 三个启用位；
- Comparator 是结构化十六进制值，不应作为任意自由文本交给模型；
- 全局 `ai.ini` / `aimd.ini` 触发器默认启用；地图局部触发器另受
  `[AITriggerTypesEnable]` 控制，不能混入同一 v1 profile 假定。

来源：

- https://modenc.renegadeprojects.com/AITriggerTypes
- https://modenc.renegadeprojects.com/AITriggerTypesEnable

## 4. 当前代码事实

### 4.1 已有模板能力

`Ra2AutomationTemplateService` 当前有 6 个 catalog profile：

1. `weapon-projectile-warhead-skeleton`；
2. `weapon-projectile-warhead-direct-fire-complete`；
3. `techno-primary-secondary-direct-fire-complete`；
4. `weapon-projectile-arcing-complete`；
5. `weapon-projectile-homing-complete`；
6. `weapon-warhead-yr-core-complete`。

它们最终都由 `Ra2ContentTemplateCompiler` 生成既有 `Ra2AutomationEditPlan`，继续复用
Gateway、Preview、Workspace、Host Apply 和 Undo；当前没有 AI Programming profile。

### 4.2 编译器边界

当前 `Ra2ContentTemplateFieldSpec` 的 key 是定义时固定字符串，value 才能来自参数。
编译器会对每个字段调用 `GetFieldSchema(sectionKind, field.Key)`：

- schema 不存在即 `FieldSchemaNotFound`；
- blocked trust 即拒绝；
- value 不满足 schema 即拒绝；
- 只有全部通过才生成 `UpsertField` operation。

这一设计适合普通命名字段，也正确保护现有 profile；但它没有 dynamic key、顺序 tuple、
固定 arity tuple、next numeric index 或 tuple component 类型。

### 4.3 Field Registry 覆盖

BuiltIn v3.2 当前精确覆盖：

| Section kind | 精确行数 | 可用于 CONTENT-2C 的事实 |
|---|---:|---|
| TaskForce | 2 | `Name`、`Group` 可用；数字成员键无 schema |
| Script | 1 | 只有旧 `x` guardrail；数字动作键无可创作 schema |
| TeamType | 32 | 命名字段覆盖较完整，可复用 |
| AITrigger | 0 | 18 槽注册元组无 schema |

`LocalRa2FieldDefinitionProvider` 只执行精确 key 查询，没有数字键或 wildcard schema。
为 CONTENT-2C 批量伪造 `0`、`1`、任意 Trigger ID 字段行会污染字段库，也不能表达元组 arity，
不应采用。

### 4.4 分类、Preview 与诊断

- `Ra2SectionClassifier` 已认识 `TaskForces`、`ScriptTypes`、`TeamTypes`、`AITriggerTypes`；
- 它会收集数字 key 的 value，并可从前三个 registry 的引用推断对象 Section 类型；
- Preview 对未知 Section/field 可以给出 Caution，但现有模板编译器会在此之前因 schema 缺失失败；
- 诊断对部分数字键采取保守策略，不等同于已经验证 tuple 的长度、槽位类型和引用闭包。

结论：分类器已有一部分“读懂”能力，但不能反推为“可安全生成”。

### 4.5 Chat / Work 与 Skill

- Chat 模式不暴露编辑工具；
- Work 模式只允许已登记的结构化能力进入 Preview；
- `Ra2AiInteractionRoute` 已能识别 `ai-programming` domain，并能返回
  `UnsupportedWorkCapability`；
- `ra2-ai-programming/SKILL.md` 明确要求：在 typed AI profile 能验证 tuple arity 和
  reference closure 之前，Work 必须不可用，不能用空 Section 冒充 AI 完整对象。

Skill 是 prompt 知识层，不增加 Gateway capability、Apply 或 Save 权限。

## 5. 当前实际实现了什么

### 5.1 IDE 与语言能力

- AvalonEdit 源码编辑、Project Explorer / Navigator、Dirty、Undo/Redo；
- Field Registry 的 Project > Global > BuiltIn 优先级、Completion、Hover、Quick Peek；
- 当前文档与项目诊断、Problems、Find References；
- 项目级查找与当前文件 Replace All Preview；
- Save Preflight、编码写入、backup/rollback。

### 5.2 AI 与高层接口

- DeepSeek V4 Flash / Pro、SSE 流式响应、取消/超时/断流失败分类；
- Chat / Work 显式模式；
- 当前文件 Query、Validate、Reference Resolve、Field Schema、CreateSection；
- typed Gateway 到 canonical Edit Preview；
- 主工作区 unified Diff、显式 Apply、一次 Undo、Apply 后 Problems 刷新；
- 不自动 Apply、不自动 Save，Apply/Save 仍由 IDE Host 所有。

### 5.3 已有完整对象 profile

- single direct-fire Weapon / Projectile / Warhead；
- 现有 Techno 的 Primary / Secondary 双 direct-fire 链；
- 绑定既有 Weapon 的 Arcing Projectile；
- 绑定既有 Weapon 的 Homing Projectile；
- YR core Warhead；
- 另有明确标注的 skeleton profile，仅在用户明确要求骨架时使用。

### 5.4 当前没有实现

- TaskForce / Script / TeamType / AITrigger 的完整创建与注册；
- AITrigger 18 槽 typed tuple 编码、Comparator 编码和引用闭包验证；
- 项目级或多文件原子编辑；
- 独立 Agent Host / CLI / wire protocol；
- 自动 Apply、自动 Save、无人值守写入；
- SuperWeapon / Faction complete profile；
- Cameo/Icon、SHP、VOX/VXL 素材流水线；
- Job Runtime、Artifact Registry 和游戏运行时验证。

## 6. 复用与差距裁决

| 能力 | 当前事实 | 审计裁决 |
|---|---|---|
| Document Query / Validate | Application + Gateway 已有 | 直接复用 |
| Section create / named field upsert | canonical compiler 已有 | 直接复用 |
| Preview / Diff / Apply / Undo | Host 链已验证 | 零复制、零新写入路径 |
| 动态数字 key | 当前模板规格不支持 | 未来需 compiler 内部 typed key source |
| TaskForce / Script tuple | Field Registry 无精确 schema | 未来需领域 grammar，不能伪造字段行 |
| AITrigger 18 槽行 | 无 typed model | 未来需固定 arity/value object 与 encoder |
| 数字注册索引 | classifier 能读，compiler 不能分配 | 未来需 deterministic conflict-checked allocator |
| 引用闭包 | 普通 Reference 可查，完整 AI 图未验证 | 未来需 profile 级 closure gate |
| Field Registry | 适合普通命名字段 | 本阶段不得修改其优先级或塞入动态假字段 |
| Skill | 已有知识与拒绝边界 | 不得当作 capability |

## 7. 风险评估

本次审计与文档更新为 **R0**，不改变运行时。

若未来恢复 CONTENT-2C，实现风险应按 **R3** 管理，原因是它会扩展内部编译语义，且错误的
槽位、索引或引用可以生成“语法合法但游戏不可用”的 AI 配置。风险不来自现有 Host 写入链，
而来自新增的 tuple grammar、ID/index 分配、Comparator 编码和引用闭包。

不能接受的实现捷径：

- 让模型直接返回 raw INI 并绕过 template/compiler；
- 把数字 key 统一当普通字符串字段并跳过类型验证；
- 在 BuiltIn Field Registry 中枚举任意数字键或 Trigger ID；
- 只创建空 TaskForce/Script/TeamType/AITrigger 骨架却标记 complete；
- 新建第二套 Preview、Apply、Undo 或 Save 通道；
- 未经单独契约把全局 AI 与地图局部 AI 混为一体。

## 8. 审计结论与停止点

1. 当前底层编辑与 Host 事务能力足够承载未来 CONTENT-2C，无需抽换架构。
2. 当前模板数据模型不足以安全表达 AI programming tuple；直接添加 profile 会失败或失真。
3. 正确扩展点位于 `Ra2ContentTemplateCompiler` 内部 typed structured-entry 层，而不是 Field Registry、
   parser、诊断或新的写入服务。
4. 在用户本轮范围内，CONTENT-2C 只完成审计；最终契约、代码、测试和 AI 写入均保持未开始。
5. 若未来重启，应先为 typed key/tuple/closure 制定独立最终契约并获批，然后才进入实现。
