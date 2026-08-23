# CONTENT-2B Projectile / Warhead Complete Profiles Code-Fact Audit

更新时间：2026-08-23  
状态：Completed / source gate passed with extension exclusions

## 1. 已有可复用权威

- `Ra2AutomationTemplateService` 是 profile 目录、参数和完整度的唯一所有者。
- `Ra2ContentTemplateCompiler` 继续把 profile 编译为 canonical `Ra2AutomationEditPlan`。
- Gateway、Preview、Workspace、Coordinator、Host Apply 和 Undo 链均已存在，本阶段不得复制。
- Field Registry snapshot 继续验证 Section kind、字段 schema 与 trust；内置 Skill 只提供工作流知识，
  不得绕过 schema gate。
- CONTENT-2A 已覆盖 direct-fire `Inviso` 弹体和基础 Warhead；CONTENT-2B 只补齐独立、可复用的
  原版弹道族与 YR core Warhead profile。

## 2. BuiltIn v3.2 字段事实

本阶段只使用当前 BuiltIn v3.2 中 exact Projectile / Warhead 且可自动创作的字段：

- Weapon 引用：`Projectile`、`Warhead`；
- Projectile 通用：`Image`、`AA`、`AG`；
- Arcing：`Arcing`、`SubjectToWalls`、`SubjectToElevation`、`SubjectToCliffs`；
- Homing：`ROT`；
- Warhead：`Verses`、`InfDeath`、`CellSpread`、`PercentAtMax`、`ProneDamage`、
  `Conventional`、`Wall`、`Wood`、`Rocker`、`Sparky`、`Tiberium`、`Bright`。

本阶段不修改 BuiltIn 数据，也不把旧 fallback 或用户 Global pack 当作 portable profile 的前提。

## 3. 引擎与扩展语义

- `Arcing=yes` 是独立弹道族；在 TS/YR 中使用固定速度 50，并忽略 Weapon `Speed`。
- Projectile `ROT>0` 表示追踪；`ROT=0` 不是可靠的直线弹道声明。
- `Arcing=yes` 与非零 `ROT` 组合存在引擎错误行为，因此必须拆成不同 profile。
- Phobos `Trajectory` 明确不能与 `Arcing`、`ROT`、`Vertical` 或 `Inviso` 混合。本阶段不开放
  `Trajectory.*`，也不伪装为原版弹道。
- YR `Verses` 是固定 11 个原生 ArmorTypes 槽。Ares 自定义 ArmorTypes 使用 `Versus.<armor>`；
  本阶段的 Warhead profile 因此明确命名为 YR core，不声称覆盖自定义护甲 override。
- `InfDeath` 的安全原版范围为 0..10；`CellSpread` 超过 11 在原版存在稳定性风险。

来源：

- https://modenc.renegadeprojects.com/Arcing
- https://modenc.renegadeprojects.com/ROT
- https://modenc.renegadeprojects.com/SubjectToCliffs
- https://modenc.renegadeprojects.com/SubjectToElevation
- https://modenc.renegadeprojects.com/SubjectToWalls
- https://modenc.renegadeprojects.com/CellSpread
- https://modenc.renegadeprojects.com/PercentAtMax
- https://modenc.renegadeprojects.com/ProneDamage
- https://modenc.renegadeprojects.com/InfDeath
- https://ares-developers.github.io/Ares-docs/new/additionalarmortypesandverses.html
- https://phobos.readthedocs.io/en/latest/New-or-Enhanced-Logics.html

## 4. 复用与差距裁决

| 能力 | 当前事实 | CONTENT-2B 裁决 |
|---|---|---|
| 新建 Section 并绑定既有 Weapon | compiler 已支持 `RequireExisting` + `CreateNew` | 直接复用 |
| Arcing 与 Homing | 字段充分，但互斥 | 两个独立 profile / capability |
| YR core Warhead | 12 个 exact/source-backed 字段充分 | 独立 profile |
| Ares `Versus.*` | 动态 key 与项目 ArmorTypes 需要专门模型 | 明确后置，不声称支持 |
| Phobos `Trajectory.*` | 版本化大字段族和互斥条件尚未 profile 化 | 模型调用前 fail closed |
| Apply/Undo/Save | 既有 Host 权威 | 零变化 |

## 5. 风险结论

实现风险为 R3：增加 internal route/capability 和 canonical profile 数据，但不新增 public 类型、
持久化格式、第二写入路径或 UI。主要风险通过“弹道族拆分、精确 tool schema、范围预检、
Field Registry trust gate、扩展请求本地拒绝”控制。

