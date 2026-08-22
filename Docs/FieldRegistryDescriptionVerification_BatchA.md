# Field Registry Description Verification - Batch A

Phase: FR-DQ-2B Batch A Online / Source Verification

## 1. Scope

This document verifies Batch A candidates from `Docs/FieldRegistryDescriptionBackfill_P0A_Candidates.md`.

Batch A covers combat / weapon / warhead basics:

```text
Damage
ROF
Range
Projectile
Warhead
Verses
CellSpread
PercentAtMax
AA
AG
```

This verification uses ModEnc as a Community source. It does not authorize direct JSON patching by itself.

## 2. Important Finding

Several Batch A effective gaps are caused by broad fallback contexts such as `Techno`, `Unit`, `Vehicle`, `Infantry`, `Aircraft`, `Building`, or `Unknown`.

These should not all receive the same canonical combat description.

Example:

```text
Damage / Weapon is valid.
Damage / Techno fallback should not be backfilled with Weapon semantics unless the field registry intentionally models it as a broad fallback.
```

Therefore the recommended backfill target is the canonical section kind, not every effective fallback row.

## 3. Verified Descriptions

| Key | Recommended Target SectionKind | SuggestedDescriptionZh | SourceTrust | Source | ReadyToApply | Notes |
|---|---|---|---|---|---:|---|
| Damage | Weapon | 设置武器造成的基础伤害点数。实际应用到目标前，该数值会继续受到 Warhead 的 Verses 等伤害倍率或特殊逻辑影响。 | Community | ModEnc: Damage | true | Also applies to Particles / VoxelAnims / Animations in specific contexts, but do not write Weapon wording into generic Techno fallback. |
| Damage | Animation | 设置动画每帧或落地/碰撞时造成的伤害；具体行为取决于动画类型、Bouncer、ExpireAnim、DamageRadius 和相关 Warhead 设置。 | Community | ModEnc: Damage | true | Only if the registry has an Animation-specific row. Avoid generic ArtObject if it covers more than animations. |
| ROF | Weapon | 设置武器发射后的再装填 / 射击间隔，单位为游戏帧。该值参与 RearmDelay 计算，数值越大通常表示射击间隔越长。 | Community | ModEnc: ROF | true | Countries / Difficulty also have ROF multiplier semantics; keep separate if such rows exist. |
| Range | Weapon | 设置武器的最大射程，单位为格。目标距离在该范围内时可正常攻击；特殊值与高低差、GuardRange 等逻辑可能影响实际行为。 | Community | ModEnc: Range | true | Range is multi-meaning. Do not reuse this description for Sound or SuperWeapon rows. |
| Range | SuperWeapon | 设置超级武器目标指示环的显示半径，单位为格；它主要影响视觉范围提示，不一定等于实际效果范围。 | Community | ModEnc: Range | true | Only for SuperWeapon context. |
| Range | Sound | 设置声音传播 / 可听范围。 | Community | ModEnc: Range | review | The source confirms Sound context exists, but additional wording can be refined later. |
| Projectile | Weapon | 设置该武器使用的 Projectile section。Projectile 定义弹体的移动/表现方式，并在命中或到达目标后触发对应 Warhead 造成效果。 | Community | ModEnc: Projectile | true | Do not write this to Techno fallback directly. |
| Warhead | Weapon | 设置该武器命中后使用的 Warhead section。Warhead 决定伤害倍率、范围扩散、命中特效以及若干特殊效果。 | Community | ModEnc: Warhead | true | Also applies to VoxelAnims / Particles. |
| Warhead | Animation | 在 art(md).ini 动画上使用时，指定动画造成伤害时采用的 Warhead；通常需要与 Damage / ExpireAnim 等设置配合。 | Community | ModEnc: Warhead | true | Only for animation context; avoid broad ArtObject if ambiguous. |
| Verses | Warhead | 设置 Warhead 对各 Armor 类型的伤害倍率列表。列表顺序需对应当前游戏的 Armor 类型；RA2/YR 中常见顺序为 none、flak、plate、light、medium、heavy、wood、steel、concrete、special_1、special_2。 | Community | ModEnc: Verses | true | Includes targeting side effects for 0% / 1%; detailed behavior can go in long docs, not Hover. |
| CellSpread | Warhead | 设置 Warhead 的伤害扩散半径。没有 CellSpread 时通常只影响命中格；数值越大，影响范围越大。伤害随距离衰减的方式还受 PercentAtMax 影响。 | Community | ModEnc: CellSpread | true | Avoid applying this to Techno fallback; canonical context is Warhead. |
| PercentAtMax | Warhead | 设置 Warhead 在 CellSpread 最远端的伤害倍率；命中点到最大扩散距离之间的伤害会按该值线性衰减。 | Community | ModEnc: PercentAtMax | true | Canonical Warhead field. |
| AA | Projectile | 设置该 Projectile 是否允许武器攻击空中目标。AA=yes 允许弹体朝飞行对象移动，从而让使用该弹体的武器可用于对空。 | Community | ModEnc: AA | true | Canonical Projectile field. |
| AG | Projectile | 设置该 Projectile 是否允许武器攻击地面/水面移动目标。RA2/YR 中 AG=no 对强制攻击地面和主动索敌存在特殊限制，必要时还需结合 LandTargeting 等字段。 | Community | ModEnc: AG | true | Canonical Projectile field. |

## 4. Rows Not Recommended for Direct Backfill

Do not directly backfill these broad effective contexts with the canonical descriptions above:

```text
Damage / Aircraft, Building, Infantry, Unit, Vehicle, Techno
ROF / Aircraft, Building, Infantry, Unit, Vehicle, Techno
Projectile / Aircraft, Building, Infantry, Unit, Vehicle, Techno
Warhead / Aircraft, Building, Infantry, Unit, Vehicle, Techno
Verses / Aircraft, Building, Unit, Vehicle, Techno
CellSpread / Aircraft, Building, Infantry, Unit, Vehicle, Techno
PercentAtMax / Aircraft, Building, Infantry, Unit, Vehicle, Techno
AA / Aircraft, Building, Infantry, Unit, Vehicle, Techno
AG / Aircraft, Building, Infantry, Unit, Vehicle, Techno
```

Reason:

```text
The canonical source describes Weapon / Projectile / Warhead / Animation semantics, not generic Techno semantics.
If the effective provider returns these broad fallbacks, that is a registry modeling / fallback hygiene issue, not proof that the field belongs to those section kinds.
```

## 5. Recommended Patch Strategy

Recommended next phase should not simply write all Batch A descriptions into every listed effective row.

Instead:

```text
1. Add or update canonical section-kind rows:
   - Damage / Weapon
   - Damage / Animation, if present
   - ROF / Weapon
   - Range / Weapon
   - Projectile / Weapon
   - Warhead / Weapon
   - Warhead / Animation, if present
   - Verses / Warhead
   - CellSpread / Warhead
   - PercentAtMax / Warhead
   - AA / Projectile
   - AG / Projectile

2. Do not backfill generic Techno / Unknown fallback rows with combat-specific wording unless a later registry modeling contract approves it.

3. Keep display hygiene for remaining placeholder fallback rows so Hover / Quick Peek / AI Evidence do not leak template text.

4. Consider a later provider/display fix that suppresses non-canonical fallback descriptions when the fallback section kind is semantically wrong.
```

## 6. Trust Classification

All verified rows above are:

```text
SourceTrust = Community
Source = ModEnc
ReadyToApply = true for canonical contexts
```

`Range / Sound` is left as review because the page confirms the context but this batch focuses on weapon combat semantics.

## 7. Next Step

Recommended next phase:

```text
FR-DQ-2B-PatchPlan: Prepare canonical Batch A JSON patch plan
```

This patch plan should identify exact pack rows to update and avoid broad fallback rows.
