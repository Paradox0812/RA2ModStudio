# CONTENT-2B Projectile / Warhead Complete Profiles Final Contract

更新时间：2026-08-23  
状态：Approved by continuous-execution authorization / self-review passed

## 1. 目标

让 Work 模式可把一个唯一存在的当前文档 Weapon 绑定到新的、字段非空且语义自洽的 Projectile
或 Warhead，并继续只生成可审阅的本地结构化 Preview。

## 2. Profile 契约

### 2.1 Arcing Projectile

```text
id: weapon-projectile-arcing-complete
version: 1
outputKind: CompleteObject
owner: one existing Weapon in the current document
sections created: 1 Projectile
operations: 8
parameters: weaponId, projectileId, image, antiAir, antiGround,
            subjectToWalls, subjectToElevation, subjectToCliffs
```

新 Projectile 固定写入 `Arcing=yes`，不写 `ROT`、`Vertical`、`Inviso` 或 `Trajectory`。

### 2.2 Homing Projectile

```text
id: weapon-projectile-homing-complete
version: 1
outputKind: CompleteObject
owner: one existing Weapon in the current document
sections created: 1 Projectile
operations: 5
parameters: weaponId, projectileId, image, rot, antiAir, antiGround
```

`rot` 必须为大于 0 的整数；新 Projectile 不写 `Arcing`、`Vertical`、`Inviso` 或 `Trajectory`。

### 2.3 YR Core Warhead

```text
id: weapon-warhead-yr-core-complete
version: 1
outputKind: CompleteObject
owner: one existing Weapon in the current document
sections created: 1 Warhead
operations: 13
parameters: weaponId, warheadId, verses, infDeath, cellSpread, percentAtMax,
            proneDamage, conventional, wall, wood, rocker, sparky, tiberium, bright
```

`verses` 必须恰有 11 个百分比 token；`infDeath` 为 0..10；`cellSpread` 为 0..11；
`percentAtMax` 与 `proneDamage` 必须为非负有限数。该 profile 只承诺 YR 原生 11 护甲槽，
不生成 Ares `Versus.*` override；当前文档存在 `[ArmorTypes]` 时整体拒绝并提示改用后续 Ares profile。

## 3. 路由与模型契约

- 明确 Arcing/抛物线/曲射弹体请求：只暴露 Arcing profile schema。
- 明确 Homing/追踪/制导/导弹弹体请求：只暴露 Homing profile schema。
- 明确完整 Warhead/弹头/范围伤害请求：只暴露 YR core Warhead schema。
- 只说“创建 Projectile”但未提供弹道族时保持 `EditAmbiguous`，不得猜测弹道族或发送模板工具。
- 明确 Phobos `Trajectory`、Straight、Bombard、Parabola、Vertical、Airburst/Splits 等未支持
  机制时，在网络调用前返回 `UnsupportedWorkCapability`。
- Weapon chain、Techno dual armament 和 skeleton 的既有优先级及 schema 保持不变。
- Chat 模式继续为 advisory only。

## 4. 权威与原子性

- Template service 是三个 profile 定义和本地参数范围的唯一所有者。
- AI tool catalog 只投影 route 对应的一个精确 required schema。
- Adapter 继续通用解析 template call；不得加入 raw INI 或第二套修复器。
- Compiler 继续生成唯一 plan；Preview、Diff、Apply、Undo、Save 权限不变。
- owner 缺失/重复/错 kind、新 Section 冲突、参数/schema/trust/stale/cancel/limit 任一失败时，
  必须零 partial plan、零 Apply。

## 5. 禁止范围

- 不修改 BuiltIn v3.2、provider priority、parser、diagnostics、completion、Hover、Save preflight。
- 不实现 Ares `Versus.*`、Phobos `Trajectory.*`、Airburst/Splits、注册列表或跨文件事务。
- 不修改 Shell、XAML、Dock、AutomationId 或项目文件。
- 不自动 Apply、Save、Undo、Redo，不引入持久化模板或 wire/CLI DTO。

## 6. 验收矩阵

1. Catalog 从 3 增至 6，三个新 descriptor 参数、版本、完整度精确。
2. Arcing 产生 1 Section / 8 operations，且不含 ROT/Vertical/Inviso/Trajectory。
3. Homing 产生 1 Section / 5 operations，`ROT>0`，且不含 Arcing/Vertical/Inviso/Trajectory。
4. Warhead 产生 1 Section / 13 operations，11 槽 Verses 与数值范围均受检。
5. 三类任一冲突、schema/trust 或参数失败均无 partial plan。
6. AI route、tool schema、prompt 与 adapter 对三类 profile 精确一致。
7. 未支持扩展弹道在模型调用前拒绝，工具列表为空。
8. Application public allowlist 保持 59；Gateway catalog/method count 不变。
9. Application/IDE 回归、Debug build、IdeOnly clean package 通过。

## 7. 自审结论

通过。拆分三个 profile 比条件式大 schema 更稳定：模型每次只看到一种参数形状；互斥弹道不会在
compiler 后才发现冲突；Warhead 范围由 Template service 在 canonical compiler 前统一预检。
没有新增 public API、持久化或写入权威，因此不会阻塞后续 HOST-1。

保留边界不是隐藏欠账：Ares custom armor 与 Phobos trajectory 都需要独立 source-backed profile，
不得在 CONTENT-2B v1 中用动态 key 或宽松字符串绕过 Field Registry。
