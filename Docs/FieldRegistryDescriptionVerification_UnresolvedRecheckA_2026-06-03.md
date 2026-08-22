# Field Registry Description Verification - Unresolved Recheck A

Phase: FR-DQ-3C-UnresolvedRecheck-A-ManualApply

This pass rechecked a targeted subset of rows previously classified as `NeedsMoreEvidence` guardrails. It only promotes rows when a reliable ModEnc, Ares, or Phobos source explicitly supports the key and context; otherwise rows remain in `Docs/FieldRegistryUnresolvedRows_2026-06-03.md`.

## 1. Scope

- Aircraft / Phobos extended aircraft mission rows.
- Ares IvanBomb weapon customization rows and IvanBomb Warhead canonical row.
- Phobos weapon visual / electric bolt customization rows.
- Phobos vehicle retain-target / turret recoil / sinking rows.
- ModEnc TeamTypes rows that had been parked under the legacy `AI` schema.

## 2. Source-backed rows promoted

| Key | SectionKind | Result | Source family |
|---|---|---|---|
| `CurleyShuffle` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `ExtendedAircraftMissions` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `ExtendedAircraftMissions.SmoothMoving` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `ExtendedAircraftMissions.EarlyDescend` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `ExtendedAircraftMissions.RearApproach` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `ExtendedAircraftMissions.FastScramble` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `ExtendedAircraftMissions.UnlandDamage` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `SpawnDistanceFromTarget` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `SpawnHeight` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `LandingDir` | `Aircraft` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.Delay` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.Warhead` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.Damage` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.AttachSound` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.Detachable` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.DestroysBridges` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.Image` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb.FlickerRate` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `IvanBomb` | `Warhead` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `FireOnce.ResetSequence` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Bolt.Duration` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Bolt.FollowFLH` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `EBoltZAdjust` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `EBoltZAdjust.ClampInitialDepthForBuilding` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `LaserZAdjust` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Anim.Update` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `KeepTargetOnMove` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `KeepTargetOnMove.Weapon` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `KeepTargetOnMove.NoMorePursuit` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `TurretRecoil` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `TurretTravel` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `TurretCompressFrames` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `TurretHoldFrames` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `TurretRecoverFrames` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `BarrelTravel` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `BarrelCompressFrames` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `BarrelHoldFrames` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `BarrelRecoverFrames` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `TurretRecoil.Suppress` | `Weapon` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `NoTurret.TrackTarget` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `SinkSpeed` | `Vehicle` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Autocreate` | `AI` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Aggressive` | `AI` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `AreTeamMembersRecruitable` | `AI` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Recruiter` | `AI` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `LooseRecruit` | `AI` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Script` | `AI` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |
| `Side` | `AI` | Source-backed or source-backed legacy-schema guardrail | ModEnc / Ares / Phobos |

## 3. Non-canonical guardrails updated

| Key | SectionKind | Reason |
|---|---|---|
| `IvanBomb` | `Weapon` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `IvanBomb` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `CurleyShuffle` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `TurretRecoil` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `TurretTravel` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `TurretCompressFrames` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `TurretHoldFrames` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `TurretRecoverFrames` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `BarrelTravel` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `BarrelCompressFrames` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `BarrelHoldFrames` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `BarrelRecoverFrames` | `Techno` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |
| `NoTurret.TrackTarget` | `Weapon` | Reliable source points to a different canonical context; row retained only to prevent old fallback pollution. |

## 4. Summary

```text
Rows changed: 61
Existing unresolved rows resolved or reclassified with source-backed descriptions: 53
New canonical row added: IvanBomb / Warhead
Total BuiltIn rows: 5070
NeedsMoreEvidence unresolved rows remaining: 1815
Direct Hover-risk rows remaining: 0
```

## 5. Next Step

Continue with source-family unresolved passes. Recommended next slice: TeamTypes / AITriggerTypes schema cleanup, then Phobos vehicle/building extension leftovers.
