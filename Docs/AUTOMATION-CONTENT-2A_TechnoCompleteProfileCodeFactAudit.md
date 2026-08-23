# CONTENT-2A Techno Complete Profile Code-Fact Audit

更新时间：2026-08-23  
状态：Completed / source gate passed with cyclic-fire exclusion

## 1. 已有可复用权威

- `Ra2AutomationTemplateService` 是模板目录和完整度数据所有者。
- `Ra2ContentTemplateCompiler` 只把定义编译为 canonical `Ra2AutomationEditPlan`。
- Gateway、Preview、Workspace、Coordinator 和显式 Apply/Undo 链不需要新增实现。
- Field Registry snapshot 继续验证 Section kind、字段类型与 trust；模板不得绕过它。
- MODE-1 已有单条 direct-fire complete profile：现有 Techno slot + 3 个新 Section + 15 operations。

## 2. 字段事实

BuiltIn v3.2 中可作为本阶段 portable source gate 的字段：

- Techno：`Primary`、`Secondary`；
- Weapon：`Damage`、`ROF`、`Range`、`Projectile`、`Speed`、`Warhead`；
- Projectile：`Inviso`、`Image`、`AA`、`AG`；
- Warhead：`Verses`、`InfDeath`、`CellSpread`、`PercentAtMax`。

BuiltIn v3.2 当前没有可供 compiler 使用的 `IsGattling`、`WeaponCount`、`WeaponStages`、
`WeaponN`、`EliteWeaponN`、`StageN`、`EliteStageN`、`RateUp`、`RateDown` 或 `Gattling.Cycle`
schema。旧 v2/v3 数据或用户 Global pack 不能作为 portable BuiltIn profile 的运行前提。

## 3. 引擎语义结论

- `Primary` / `Secondary` 表达武器槽和目标选择，不保证轮流或循环发射。
- Weapon `Burst` 表达同一 Weapon 的一次攻击连发，不会在两种 Weapon 之间交替。
- YR Gattling 系统用 `IsGattling`、`WeaponStages`、成对 `WeaponN`、`StageN`、
  `RateUp/RateDown` 实现分阶段选择；每个 stage 有 AG/AA 两个槽，不等于“一发主炮、一发同轴”的固定交替。
- Ares `Gattling.Cycle=yes` 只在最后阶段后回到第一阶段，而且官方明确说明不能保证某 stage 固定发射次数。

来源：

- https://modenc.renegadeprojects.com/Primary
- https://modenc.renegadeprojects.com/Secondary
- https://modenc.renegadeprojects.com/Burst
- https://modenc.renegadeprojects.com/Gattling_Weapon_System
- https://ares-developers.github.io/Ares-docs/new/gattlingcycle.html
- https://github.com/Phobos-developers/Phobos/blob/develop/docs/New-or-Enhanced-Logics.md

## 4. 复用与差距

| 能力 | 当前事实 | CONTENT-2A 裁决 |
|---|---|---|
| 单武器槽完整链 | 已实现 | 保持 v1 兼容 |
| Primary + Secondary 两条完整链 | 同一 compiler 可组合 | 新增独立 profile |
| 循环/交替开火 | 字段 schema 与精确行为契约不足 | 模型调用前 fail closed |
| 新建完整 Techno + 类型列表注册 | 当前文档索引/注册能力未实现 | 后置，不伪造完整对象 |
| Apply/Undo/Save | 既有 Host 权威 | 零变化 |

## 5. 风险结论

本阶段为 R3：扩展 canonical template 数据与 AI 内部路由，但不增加 public 类型、持久化、
第二写入路径或 UI。通过“独立 profile + 精确意图拒绝 + Field Registry source gate”控制风险。

