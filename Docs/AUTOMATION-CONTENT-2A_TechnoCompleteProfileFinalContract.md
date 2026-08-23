# CONTENT-2A Techno Complete Profile Final Contract

更新时间：2026-08-23  
状态：Approved by continuous-execution authorization / self-review passed

## 1. 目标

为一个唯一存在的当前文档 TechnoType 生成 Primary 与 Secondary 两套完整 direct-fire 武器链，
并保证“循环/交替开火”不会被错误解释为普通双武器配置。

## 2. 模板契约

```text
id: techno-primary-secondary-direct-fire-complete
version: 1
outputKind: CompleteObject
owner: one existing compatible TechnoType in the current document
sections created: 6 (2 Weapon + 2 Projectile + 2 Warhead)
operations: 30
apply/save: never performed by template
```

参数为 `ownerSectionId`，以及 `primary*`、`secondary*` 各 13 项：

```text
WeaponId, ProjectileId, WarheadId,
Damage, Rof, Range, ProjectileSpeed,
Verses, InfDeath, CellSpread, PercentAtMax,
AntiAir, AntiGround
```

每个 `Verses` 必须恰有 11 个百分比 token；布尔值由既有 compiler 按 Field Registry 的
Yes/No style 规范化。全部参数必填；ID 冲突、owner 缺失/重复、schema/trust 失败、超限或取消均整体失败。

## 3. 意图与路由

- 明确“主副武器/双武器/两套武器链”且要求结构化修改：使用本 profile。
- 只新增一条同轴/副武器链：继续使用既有 single-chain complete profile。
- 明确“骨架/框架/占位”：继续使用 skeleton profile。
- 包含“循环开火/交替开火/轮换开火/cyclic/alternate fire”：返回
  `UnsupportedWorkCapability`，零工具、零网络、零 proposal。
- Chat 模式仍只提供 advisory，不获得 authoring tool。

## 4. 权威与兼容

- Template service 是 profile/parameter/definition 唯一所有者。
- AI tool catalog 只投影当前 route 的一个 required tool schema。
- Adapter 继续通用解析 template request，不增加 JSON repair 或模板特判写入。
- Compiler 继续产生唯一 `Ra2AutomationEditPlan`；Preview、Diff、Apply、Undo 原样复用。
- Application public allowlist 预计保持 59；Gateway catalog/method count 不变。
- 既有 template id/version/descriptor 行为保持兼容。

## 5. 禁止范围

- 不修改 BuiltIn v3.2、provider priority、parser、diagnostics、completion、Hover、Save preflight。
- 不实现 Gattling/Cycle、不把 `Burst` 当作双武器交替。
- 不创建 type-list 数字索引、注册表、跨文件事务、素材、自动 Apply/Save。
- 不修改 Shell、XAML、Dock 或 AutomationId。

## 6. 验收矩阵

1. Catalog 暴露 3 个模板；新 descriptor 为 27 required parameters。
2. Service 成功生成 6 个新 Section 和 30 个有序 operations。
3. owner 的 `Primary` 和 `Secondary` 同属一个原子 Plan。
4. 任一参数、Verses、schema、trust 或 Section 冲突失败时没有 partial plan。
5. AI dual route 只获得新 profile 的精确 schema；prompt 明示不是 cyclic fire。
6. cyclic/alternate 请求在发送前本地拒绝，工具列表为空。
7. Adapter 只生成 Preview plan，不调用 transaction Apply。
8. 完整 Application/IDE non-UI 回归、Debug build、IdeOnly clean package 通过。

## 7. 自审

通过：单一数据所有者、单一 Preview/Apply 链、旧模板版本兼容、public 类型零增加、失败原子性、
网络成本门禁和语义诚实性均有自动测试入口。

保留风险：27 参数对模型是较大 schema，真实 DeepSeek 稳定性仍需用户后续实机验收；真正 Gattling
必须先完成独立 source-backed Field Registry/profile 契约，不能在本阶段偷偷降级。

