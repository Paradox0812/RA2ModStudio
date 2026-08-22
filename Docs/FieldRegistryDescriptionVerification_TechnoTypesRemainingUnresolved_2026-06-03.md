# Field Registry Description Verification - TechnoTypes Remaining Unresolved Guardrail MegaBatch

Phase: FR-DQ-2W-TechnoTypesRemaining-UnresolvedGuardrail-MegaBatch-ManualApply

## 1. Scope

This phase accelerates the Field Registry Hover quality cleanup by clearing remaining direct Hover-risk rows whose `appliesTo` is exactly `Techno` and whose description was still a placeholder, generic short label, or migrated-review marker.

This phase deliberately does **not** invent final field semantics. Rows without a reliable field page are converted to `NeedsMoreEvidence` guardrails and recorded in the unresolved list for later focused verification.

## 2. Source Policy

- Source-backed canonical descriptions still require a concrete ModEnc, Ares, or Phobos field page.
- This batch did not find or assert row-level authoritative semantics for the affected rows.
- Each affected row is therefore marked as `NeedsMoreEvidence` rather than `source-verified`.
- The row description is changed only to remove misleading placeholder text from Hover and to point users to the unresolved list.

## 3. Batch Result

```text
BuiltIn v3.2 field count: 5069 -> 5069
Rows affected: 1413
New exact/context rows: 0
Rows converted to NeedsMoreEvidence guardrail: 1413
Source-verified rows: 1312 -> 1312
Direct placeholder rows: 2073 -> 802
Exact integer generic rows: 78 -> 62
Exact numeric generic rows: 0 -> 0
Direct Hover-risk rows: 2151 -> 864
```

## 4. Representative Rows

| Key | SectionKind | Result | Notes |
|---|---|---|---|
| AARate | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Accelerates | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Action | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AddPlanningModeCommandSound | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Adjacent | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Agent | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Aggressive | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIAllToHunt | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIAlternateProductionCreditCutoff | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIAttackMoveTargetingDelay | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIAutoDeployFrameDelay | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIBasePlanningSide | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIBuildsWalls | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIBuildThis | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AICaptureLowMoneyMark | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIDifficulty | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIFireSale | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIFireSaleDelay | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIGuardAreaTargetingDelay | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIIonCannonPlugValue | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIIonCannonTempleValue | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AINavalYardAdjacency | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AINormalTargetingDelay | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIParadropMission | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIPickWallDefensePercent | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIPlayers | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Aircraft.DefaultDigitalDisplayTypes | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AircraftCostBonus | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AircraftFogReveal | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIRestrictReplaceTime | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AirRangeBonus | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AirShadowBaseScale | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Airspeed | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AISuperDefenseDistance | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AITriggerFailureWeightDelta | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AITriggerSuccessWeightDelta | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AITriggerTrackRecordCoefficient | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AIUseTurbineUpgradeProbability | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlliedCrew | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlliedDisguise | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlliedSurvivorDivisor | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlliedWallTransparency | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlliesAllowed | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AllowAirstrike | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AllowBurrowing | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AllowShroudedSubteranneanMoves | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AllowWeaponSelectAgainstWalls | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AllyParaDropInf | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AllyParaDropNum | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AllyReveal | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlphaImage | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlternateArcticArt | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlternateFLH.ApplyVehicle | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlternateFLH.OnTurret | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlternateFlightLevel | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AltImage | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AlwaysConsideredThreat | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmbientChangeRate | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmbientChangeStep | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmbientDamage | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmbientSound | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmerParaDropInf | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmerParaDropNum | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Ammo.AutoConvertMaximumAmount | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Ammo.AutoConvertMinimumAmount | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Ammo.AutoConvertType | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmmoCrateDamage | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmmoPipFrame | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmmoPipOffset | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmmoPipSize | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AmmoPipWrapStartFrame | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AnimationProbability | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AnimationRate | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AntiXXXValue | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| ApproachTargetResetMultiplier | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AreaGuardRange | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| ArmorDefensesMult | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| Artillary | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AssaultAnim | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| AtmosphereEntry | Techno | NeedsMoreEvidence | 保留为来源不足防污染说明 |
| ... | ... | ... | 仅展示前 80 行；完整清单见 `FieldRegistryUnresolvedRows_2026-06-03.md`。本轮共 1413 行。 |

## 5. Why This Is Not A Canonical Patch

These rows remain unresolved because the batch goal is risk burn-down, not semantic invention. The old text either said `原始英文说明已移至复核表` / `内置参考字段` or was a generic type label. The new text is a defensive guardrail that prevents Hover from presenting placeholder text as real documentation.

## 6. Full Unresolved List

The complete list is maintained in:

```text
Docs/FieldRegistryUnresolvedRows_2026-06-03.md
```

## 7. Next Step

Recommended next phase:

```text
FR-DQ-2X-SuperWeaponSideCountryUIMegaBatch-ManualApply
```

If the goal is maximum speed, continue with 250-400 risk-row mega batches. If the goal is semantic depth, return to unresolved entries by source family after the direct Hover-risk cleanup is complete.
