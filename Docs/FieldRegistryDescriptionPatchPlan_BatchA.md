# Field Registry Description Patch Plan - Batch A

Phase: FR-DQ-2B-PatchPlan Batch A Canonical Context Patch Plan

This document prepares a patch plan from `FieldRegistryDescriptionVerification_BatchA.md` and the effective P0 candidate policy. It does not modify Field Registry JSON, source code, XAML, Hover, Quick Peek, AI Evidence, provider priority, parser, diagnostics, completion, or save preflight.

## Scope

Batch A verification confirms that combat field descriptions should be applied only to canonical section-kind rows. Broad effective fallback rows are not patch targets.

Allowed canonical target rows:

- `Damage / Weapon`
- `Damage / Animation`, only if the row exists
- `ROF / Weapon`
- `Range / Weapon`
- `Projectile / Weapon`
- `Warhead / Weapon`
- `Warhead / Animation`, only if the row exists
- `Verses / Warhead`
- `CellSpread / Warhead`
- `PercentAtMax / Warhead`
- `AA / Projectile`
- `AG / Projectile`

Rows that must not receive these canonical descriptions unless a later source proves the field is valid in that section kind:

- `Techno`
- `Unit`
- `Infantry`
- `Vehicle`
- `Aircraft`
- `Building`
- `Unknown`
- `Global`

## Row Existence Check

Read-only scan result in this workspace:

| Key | Target SectionKind | Row Exists | Current Source |
|---|---|---:|---|
| Damage | Weapon | Yes | BuiltIn v3.2 |
| Damage | Animation | No | Not found |
| ROF | Weapon | Yes | BuiltIn v3.2 |
| Range | Weapon | Yes | BuiltIn v3.2 |
| Projectile | Weapon | Yes | BuiltIn v3.2 |
| Warhead | Weapon | Yes | BuiltIn v3.2 |
| Warhead | Animation | No | Not found |
| Verses | Warhead | Yes | BuiltIn v3.2 |
| CellSpread | Warhead | Yes | BuiltIn v3.2 |
| PercentAtMax | Warhead | Yes | BuiltIn v3.2 |
| AA | Projectile | Yes | BuiltIn v3.2 |
| AG | Projectile | Yes | BuiltIn v3.2 |

## Patch Plan

| Key | Target SectionKind | Existing Effective Status | Proposed Description Zh | SourceTrust | Source | ReadyToApply | DoNotApplyTo | Notes |
|---|---|---|---|---|---|---:|---|---|
| Damage | Weapon | Valid | 设置武器造成的基础伤害点数。实际应用到目标前，该数值会继续受到 Warhead 的 Verses 等伤害倍率或特殊逻辑影响。 | Community | ModEnc: Damage | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Weapon row exists. This is a richer replacement / refinement candidate, not a broad fallback patch. |
| Damage | Animation | Missing row | 设置动画每帧或落地、碰撞时造成的伤害；具体行为取决于动画类型、Bouncer、ExpireAnim、DamageRadius 和相关 Warhead 设置。 | Community | ModEnc: Damage | false | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global, ArtObject | Target row does not exist in the scanned BuiltIn / Global packs. Do not create or patch `ArtObject` as a substitute in this phase. |
| ROF | Weapon | Valid | 设置武器发射后的再装填 / 射击间隔，单位为游戏帧。该值参与 RearmDelay 计算，数值越大通常表示射击间隔越长。 | Community | ModEnc: ROF | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Weapon row exists. Do not apply to country or difficulty multiplier meanings. |
| Range | Weapon | Valid | 设置武器的最大射程，单位为格。目标距离在该范围内时可正常攻击；特殊值与高低差、GuardRange 等逻辑可能影响实际行为。 | Community | ModEnc: Range | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global, SuperWeapon, Sound | Canonical Weapon row exists. `Range / SuperWeapon` and `Range / Sound` are different meanings and are intentionally excluded. |
| Projectile | Weapon | Valid | 设置该武器使用的 Projectile section。Projectile 定义弹体的移动 / 表现方式，并在命中或到达目标后触发对应 Warhead 造成效果。 | Community | ModEnc: Projectile | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Weapon row exists. Do not write to broad Techno fallback rows. |
| Warhead | Weapon | Valid | 设置该武器命中后使用的 Warhead section。Warhead 决定伤害倍率、范围扩散、命中特效以及若干特殊效果。 | Community | ModEnc: Warhead | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Weapon row exists. |
| Warhead | Animation | Missing row | 在 art(md).ini 动画上使用时，指定动画造成伤害时采用的 Warhead；通常需要与 Damage / ExpireAnim 等设置配合。 | Community | ModEnc: Warhead | false | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global, ArtObject | Target row does not exist in the scanned BuiltIn / Global packs. Do not patch broad `ArtObject` as an Animation row replacement. |
| Verses | Warhead | Valid | 设置 Warhead 对各 Armor 类型的伤害倍率列表。列表顺序需对应当前游戏的 Armor 类型；RA2/YR 中常见顺序为 none、flak、plate、light、medium、heavy、wood、steel、concrete、special_1、special_2。 | Community | ModEnc: Verses | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Warhead row exists. Detailed 0% / 1% targeting side effects can stay in longer docs, not necessarily Hover. |
| CellSpread | Warhead | Valid | 设置 Warhead 的伤害扩散半径。没有 CellSpread 时通常只影响命中格；数值越大，影响范围越大。伤害随距离衰减的方式还可受 PercentAtMax 影响。 | Community | ModEnc: CellSpread | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Warhead row exists. Do not apply this to Techno fallback. |
| PercentAtMax | Warhead | Valid | 设置 Warhead 在 CellSpread 最远端的伤害倍率；命中点到最大扩散距离之间的伤害会按该值线性衰减。 | Community | ModEnc: PercentAtMax | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Warhead row exists. |
| AA | Projectile | Valid | 设置该 Projectile 是否允许武器攻击空中目标。AA=yes 允许弹体朝飞行对象移动，从而让使用该弹体的武器可用于对空。 | Community | ModEnc: AA | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Projectile row exists. |
| AG | Projectile | Valid | 设置该 Projectile 是否允许武器攻击地面 / 水面移动目标。RA2/YR 中 AG=no 对强制攻击地面和主动索敌存在特殊限制，必要时还需结合 LandTargeting 等字段。 | Community | ModEnc: AG | true | Techno, Unit, Infantry, Vehicle, Aircraft, Building, Unknown, Global | Canonical Projectile row exists. |

## Rows Explicitly Not Planned

Do not apply Batch A canonical descriptions to these broad effective contexts:

```text
Damage / Techno
Damage / Unit
Damage / Infantry
Damage / Vehicle
Damage / Aircraft
Damage / Building
Damage / Unknown

ROF / Techno
ROF / Unit
ROF / Infantry
ROF / Vehicle
ROF / Aircraft
ROF / Building
ROF / Unknown

Range / Techno
Range / Unit
Range / Infantry
Range / Vehicle
Range / Aircraft
Range / Building
Range / Unknown
Range / Global

Projectile / Techno
Projectile / Unit
Projectile / Infantry
Projectile / Vehicle
Projectile / Aircraft
Projectile / Building
Projectile / Unknown

Warhead / Techno
Warhead / Unit
Warhead / Infantry
Warhead / Vehicle
Warhead / Aircraft
Warhead / Building
Warhead / Unknown

Verses / Techno
Verses / Unit
Verses / Infantry
Verses / Vehicle
Verses / Aircraft
Verses / Building
Verses / Unknown

CellSpread / Techno
CellSpread / Unit
CellSpread / Infantry
CellSpread / Vehicle
CellSpread / Aircraft
CellSpread / Building
CellSpread / Unknown

PercentAtMax / Techno
PercentAtMax / Unit
PercentAtMax / Infantry
PercentAtMax / Vehicle
PercentAtMax / Aircraft
PercentAtMax / Building
PercentAtMax / Unknown

AA / Techno
AA / Unit
AA / Infantry
AA / Vehicle
AA / Aircraft
AA / Building
AA / Unknown

AG / Techno
AG / Unit
AG / Infantry
AG / Vehicle
AG / Aircraft
AG / Building
AG / Unknown
```

Reason: the verified descriptions describe Weapon / Projectile / Warhead / Animation semantics, not generic Techno or Unknown fallback semantics.

## Implementation Boundary for Future Patch Phase

A future JSON patch phase should:

1. Patch only rows marked `ReadyToApply=true`.
2. Skip `Damage / Animation` and `Warhead / Animation` unless a later phase explicitly creates or approves those rows.
3. Preserve Project > Global > BuiltIn priority.
4. Avoid changing provider lookup behavior to make broad fallbacks appear canonical.
5. Keep Hover / Quick Peek / AI Evidence code unchanged unless a separate display hygiene phase is approved.

## No Runtime Changes

This patch plan did not modify:

- Field Registry JSON.
- Field Registry provider priority.
- Hover / Quick Peek / AI Evidence.
- Parser, diagnostics, completion, or save preflight.
- XAML / UI.
- Project or solution files.
