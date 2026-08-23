# AGENT-MODE-1A Direct-fire Complete Profile Source Audit

更新时间：2026-08-23  
状态：Passed / implemented

## 结论

`weapon-projectile-warhead-direct-fire-complete@1` 的最小字段集合通过来源门禁。它只定义
“现有 TechnoType 武器槽 + 一个可连接的直射 Weapon/Projectile/Warhead 链”，不宣称覆盖所有武器逻辑。

## 来源和本地证据

- ModEnc `Projectile`：Weapon 的 `Projectile=` 指向弹体；Projectile 在命中/到达目标后触发 Warhead。
  <https://modenc.renegadeprojects.com/Projectile>
- ModEnc `Warhead`：Warhead 是 Weapon 的伤害响应对象，并控制 armor/Verses 等伤害行为。
  <https://modenc.renegadeprojects.com/Warhead>
- 本地 BuiltIn v3.2 精确行：Vehicle/Infantry/Aircraft/Building/Techno 的 Primary/Secondary；Weapon 的
  Damage/ROF/Range/Projectile/Speed/Warhead；Projectile 的 Inviso/Image/AA/AG；Warhead 的
  Verses/InfDeath/CellSpread/PercentAtMax，均为来源核验行而非 wrong-context guardrail。
- 本地类型边界：`Ra2SectionKind` 已区分 Techno 子类、Weapon、Projectile、Warhead；Template Compiler
  可使用 captured Field Registry snapshot 校验字段和值。

## 冻结不变量

成功展开必须原子包含：

1. 一个唯一现有 TechnoType 的 Primary 或 Secondary 绑定；
2. 一个含 6 个核心字段的 Weapon；
3. 一个含 Inviso/Image/AA/AG 的 Projectile；
4. 一个含 Verses/InfDeath/CellSpread/PercentAtMax 的 Warhead；
5. 三个新 Section ID 不冲突，且每个 Section 非空；
6. `Verses` v1 恰为 11 个百分比 token；
7. 任一 Section、字段、trust、参数、版本或资源门禁失败时无 partial plan。

## 明确不包含

Burst、EliteWeapon、声音、动画、custom trajectory、注册列表、素材文件、自动平衡、多文件写入、Apply、
Save。需要这些能力时必须进入后续 Profile/Skill/Host 阶段。

