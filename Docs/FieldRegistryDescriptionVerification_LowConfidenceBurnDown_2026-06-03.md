# Field Registry Description Verification - Low Confidence Burn Down - 2026-06-03

## Scope

- 修复 `schema.type=Text`，使 BuiltIn JSON 只使用 `Ra2FieldValueKind` 支持的 schema type。
- 对 `RA2IniEditor_FR_DQ_3E_SourceBackedCandidates.csv` 中的 Phobos Techno 高收益簇进行核验。
- 仅晋级官方文档直接列出且上下文匹配的字段；上下文不匹配的字段改为 source-verified guardrail。
- 未处理所有外部源 residual 行；它们保留在 backlog，避免未经逐项确认就伪晋级。

## Stats

| Metric | Before | After | Delta |
|---|---:|---:|---:|
| BuiltIn fields | 5109 | 3519 | -1590 |
| needs-more-evidence* | 1771 | 0 | -1771 |
| source-verified* | 1870 | 2051 | 181 |
| schema.type=Text | 103 | 0 | -103 |

## Processed Rows

| group | action | appliesTo | key |
|---|---|---|---|
| AttackMove | promote | Techno | `AttackMove.Aggressive` |
| AttackMove | promote | Techno | `AttackMove.Follow` |
| AttackMove | promote | Techno | `AttackMove.Follow.IfMindControlIsFull` |
| AttackMove | promote | Techno | `AttackMove.Follow.IncludeAir` |
| AttackMove | promote | Techno | `AttackMove.PursuitTarget` |
| AttackMove | promote | Techno | `AttackMove.StopWhenTargetAcquired` |
| AttackMove | promote | Techno | `AttackMove.UpdateTarget` |
| AttackMove | guardrail_wrong_context | Techno | `AttackMove.IgnoreWeaponCheck` |
| AutoDeath | promote | Techno | `AutoDeath.AfterDelay` |
| AutoDeath | promote | Techno | `AutoDeath.Behavior` |
| AutoDeath | promote | Techno | `AutoDeath.OnAmmoDepletion` |
| AutoDeath | promote | Techno | `AutoDeath.OnOwnerChange` |
| AutoDeath | promote | Techno | `AutoDeath.OnOwnerChange.ComputerToHuman` |
| AutoDeath | promote | Techno | `AutoDeath.OnOwnerChange.HumanToComputer` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosDontExist` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosDontExist.AllowLimboed` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosDontExist.Any` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosDontExist.Houses` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosExist` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosExist.AllowLimboed` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosExist.Any` |
| AutoDeath | promote | Techno | `AutoDeath.TechnosExist.Houses` |
| AutoDeath | promote | Techno | `AutoDeath.VanishAnimation` |
| Interceptor | promote | Techno | `Interceptor` |
| Interceptor | promote | Techno | `Interceptor.ApplyFirepowerMult` |
| Interceptor | promote | Techno | `Interceptor.CanTargetHouses` |
| Interceptor | promote | Techno | `Interceptor.DeleteOnIntercept` |
| Interceptor | promote | Techno | `Interceptor.EliteGuardRange` |
| Interceptor | promote | Techno | `Interceptor.EliteMinimumGuardRange` |
| Interceptor | promote | Techno | `Interceptor.GuardRange` |
| Interceptor | promote | Techno | `Interceptor.GuardRange.IsCylindrical` |
| Interceptor | promote | Techno | `Interceptor.KeepIntact` |
| Interceptor | promote | Techno | `Interceptor.MinimumGuardRange` |
| Interceptor | promote | Techno | `Interceptor.TargetingDelay` |
| Interceptor | promote | Techno | `Interceptor.VeteranGuardRange` |
| Interceptor | promote | Techno | `Interceptor.VeteranMinimumGuardRange` |
| Interceptor | promote | Techno | `Interceptor.Weapon` |
| Interceptor | promote | Techno | `Interceptor.WeaponCumulativeDamage` |
| Interceptor | promote | Techno | `Interceptor.WeaponReplaceProjectile` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.AllowedHouses` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.Anim` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.CostMultiplier` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.CostRateCap` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.DisplaySoylent` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.DisplaySoylentOffset` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.DisplaySoylentToHouses` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.DontScore` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.Rate` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.Rate.SizeMultiply` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.Soylent` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.SoylentAllowedHouses` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.SoylentMultiplier` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.UnderEMP` |
| PassengerDeletion | promote | Techno | `PassengerDeletion.UseCostAsRate` |
| ForceWeapon | promote | Techno | `ForceAAWeapon.Aircraft` |
| ForceWeapon | promote | Techno | `ForceAAWeapon.Infantry` |
| ForceWeapon | promote | Techno | `ForceAAWeapon.InRange.ApplyRangeModifiers` |
| ForceWeapon | promote | Techno | `ForceAAWeapon.InRange.Overrides` |
| ForceWeapon | promote | Techno | `ForceAAWeapon.Units` |
| ForceWeapon | promote | Techno | `ForceWeapon.Aircraft` |
| ForceWeapon | promote | Techno | `ForceWeapon.Buildings` |
| ForceWeapon | promote | Techno | `ForceWeapon.Cloaked` |
| ForceWeapon | promote | Techno | `ForceWeapon.Defenses` |
| ForceWeapon | promote | Techno | `ForceWeapon.Disguised` |
| ForceWeapon | promote | Techno | `ForceWeapon.Infantry` |
| ForceWeapon | promote | Techno | `ForceWeapon.InRange.ApplyRangeModifiers` |
| ForceWeapon | promote | Techno | `ForceWeapon.InRange.Overrides` |
| ForceWeapon | promote | Techno | `ForceWeapon.InRange.TechnoOnly` |
| ForceWeapon | promote | Techno | `ForceWeapon.Naval.Decloaked` |
| ForceWeapon | promote | Techno | `ForceWeapon.Naval.Units` |
| ForceWeapon | promote | Techno | `ForceWeapon.UnderEMP` |
| ForceWeapon | promote | Techno | `ForceWeapon.Units` |
| TiberiumEater | promote | Techno | `TiberiumEater.AmountPerCell` |
| TiberiumEater | promote | Techno | `TiberiumEater.AnimMove` |
| TiberiumEater | promote | Techno | `TiberiumEater.Anims` |
| TiberiumEater | promote | Techno | `TiberiumEater.Anims.Tiberium0` |
| TiberiumEater | promote | Techno | `TiberiumEater.Anims.Tiberium1` |
| TiberiumEater | promote | Techno | `TiberiumEater.Anims.Tiberium2` |
| TiberiumEater | promote | Techno | `TiberiumEater.Anims.Tiberium3` |
| TiberiumEater | promote | Techno | `TiberiumEater.CashMultiplier` |
| TiberiumEater | promote | Techno | `TiberiumEater.CellN` |
| TiberiumEater | promote | Techno | `TiberiumEater.Display` |
| TiberiumEater | promote | Techno | `TiberiumEater.Display.Houses` |
| TiberiumEater | promote | Techno | `TiberiumEater.TransDelay` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.AllowFiringIfAttackedByLocomotor` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.AllowFiringIfDeactivated` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.DamageMultiplier` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.DecloakToFire` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.IgnoreRangefinding` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.RangeBonus` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.ShareTransportTarget` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.WarpDistance` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTransport.DamageMultiplier` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTransport.RangeBonus` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.CheckTransportDisableWeapons` |
| OpenTopped/OpenTransport | promote | Techno | `OpenTopped.UseTransportRangeModifiers` |
| Spawner | promote | Techno | `Spawner.AttackImmediately` |
| Spawner | promote | Techno | `Spawner.DelayFrames` |
| Spawner | promote | Techno | `Spawner.ExtraLimitRange` |
| Spawner | promote | Techno | `Spawner.LimitRange` |
| Spawner | promote | Techno | `Spawner.RecycleAnim` |
| Spawner | promote | Techno | `Spawner.RecycleCoord` |
| Spawner | promote | Techno | `Spawner.RecycleOnTurret` |
| Spawner | promote | Techno | `Spawner.RecycleRange` |
| Spawner | promote | Techno | `Spawner.UseTurretFacing` |
| BuildLimitGroup | promote | Techno | `BuildLimitGroup.ContentIfAnyMatch` |
| BuildLimitGroup | promote | Techno | `BuildLimitGroup.ExtraLimit.MaxNum` |
| BuildLimitGroup | promote | Techno | `BuildLimitGroup.ExtraLimit.Types` |
| BuildLimitGroup | promote | Techno | `BuildLimitGroup.Factor` |
| BuildLimitGroup | promote | Techno | `BuildLimitGroup.NotBuildableIfQueueMatch` |
| BuildLimitGroup | promote | Techno | `BuildLimitGroup.Nums` |
| BuildLimitGroup | promote | Techno | `BuildLimitGroup.Types` |
| AttachEffect | promote | Techno | `AttachEffect.AttachTypes` |
| AttachEffect | promote | Techno | `AttachEffect.Delays` |
| AttachEffect | promote | Techno | `AttachEffect.DurationOverrides` |
| AttachEffect | promote | Techno | `AttachEffect.InitialDelays` |
| AttachEffect | promote | Techno | `AttachEffect.RecreationDelays` |
| DrainMoneyDisplay | promote | Techno | `DrainMoneyDisplay` |
| DrainMoneyDisplay | promote | Techno | `DrainMoneyDisplay.Houses` |
| DrainMoneyDisplay | promote | Techno | `DrainMoneyDisplay.Offset` |
| DrainMoneyDisplay | promote | Techno | `DrainMoneyDisplay.OnTarget` |
| DrainMoneyDisplay | promote | Techno | `DrainMoneyDisplay.OnTarget.UseDisplayIncome` |
| Tint | promote | Techno | `Tint.Color` |
| Tint | promote | Techno | `Tint.Intensity` |
| Tint | promote | Techno | `Tint.VisibleToHouses` |
| DigitalDisplay | promote | Techno | `DigitalDisplay.Disable` |
| DigitalDisplay | promote | Techno | `DigitalDisplay.Health.FakeAtDisguise` |
| DigitalDisplay | promote | Techno | `DigitalDisplayTypes` |
| DigitalDisplay | guardrail_wrong_context | Techno | `DigitalDisplay.Enable` |
| Insignia | promote | Techno | `Insignia` |
| Insignia | promote | Techno | `Insignia.Elite` |
| Insignia | promote | Techno | `Insignia.PassengersN` |
| Insignia | promote | Techno | `Insignia.PassengersN.Elite` |
| Insignia | promote | Techno | `Insignia.PassengersN.Rookie` |
| Insignia | promote | Techno | `Insignia.PassengersN.Veteran` |
| Insignia | promote | Techno | `Insignia.Rookie` |
| Insignia | promote | Techno | `Insignia.ShowEnemy` |
| Insignia | promote | Techno | `Insignia.Veteran` |
| Insignia | promote | Techno | `Insignia.WeaponN` |
| Insignia | promote | Techno | `Insignia.WeaponN.Elite` |
| Insignia | promote | Techno | `Insignia.WeaponN.Rookie` |
| Insignia | promote | Techno | `Insignia.WeaponN.Veteran` |
| Insignia | promote | Techno | `InsigniaFrame` |
| Insignia | promote | Techno | `InsigniaFrame.Elite` |
| Insignia | promote | Techno | `InsigniaFrame.PassengersN` |
| Insignia | promote | Techno | `InsigniaFrame.PassengersN.Elite` |
| Insignia | promote | Techno | `InsigniaFrame.PassengersN.Rookie` |
| Insignia | promote | Techno | `InsigniaFrame.PassengersN.Veteran` |
| Insignia | promote | Techno | `InsigniaFrame.Rookie` |
| Insignia | promote | Techno | `InsigniaFrame.Veteran` |
| Insignia | promote | Techno | `InsigniaFrame.WeaponN` |
| Insignia | promote | Techno | `InsigniaFrame.WeaponN.Elite` |
| Insignia | promote | Techno | `InsigniaFrame.WeaponN.Rookie` |
| Insignia | promote | Techno | `InsigniaFrame.WeaponN.Veteran` |
| Insignia | promote | Techno | `InsigniaFrames` |
| Insignia | promote | Techno | `InsigniaFrames.PassengersN` |
| Insignia | promote | Techno | `InsigniaFrames.WeaponN` |
| Insignia | promote | Techno | `InsigniaType` |
| Insignia | promote | Techno | `InsigniaType.PassengersN` |
| Insignia | promote | Techno | `InsigniaType.WeaponN` |
| DrawInsignia | guardrail_wrong_context | Techno | `DrawInsignia.AdjustPos.Buildings` |
| DrawInsignia | guardrail_wrong_context | Techno | `DrawInsignia.AdjustPos.BuildingsAnchor` |
| DrawInsignia | guardrail_wrong_context | Techno | `DrawInsignia.AdjustPos.Infantry` |
| DrawInsignia | guardrail_wrong_context | Techno | `DrawInsignia.AdjustPos.Units` |
| DrawInsignia | guardrail_wrong_context | Techno | `DrawInsignia.OnlyOnSelected` |
| DrawInsignia | guardrail_wrong_context | Techno | `DrawInsignia.UsePixelSelectionBracketDelta` |
| CombatAlert | promote | Techno | `CombatAlert` |
| CombatAlert | promote | Techno | `CombatAlert.NotBuilding` |
| CombatAlert | promote | Techno | `CombatAlert.UseAttackVoice` |
| CombatAlert | promote | Techno | `CombatAlert.UseEVA` |
| CombatAlert | promote | Techno | `CombatAlert.UseFeedbackVoice` |
| CombatAlert | guardrail_wrong_context | Techno | `CombatAlert.Default` |
| CombatAlert | guardrail_wrong_context | Techno | `CombatAlert.IgnoreBuilding` |
| CombatAlert | guardrail_wrong_context | Techno | `CombatAlert.Interval` |
| CombatAlert | guardrail_wrong_context | Techno | `CombatAlert.MakeAVoice` |
| CombatAlert | guardrail_wrong_context | Techno | `CombatAlert.SuppressIfAllyDamage` |
| CombatAlert | guardrail_wrong_context | Techno | `CombatAlert.SuppressIfInScreen` |
| AutoTargetOwnPosition | promote | Techno | `AutoTargetOwnPosition` |
| AutoTargetOwnPosition | promote | Techno | `AutoTargetOwnPosition.Self` |
| Passengers.SyncOwner | promote | Techno | `Passengers.SyncOwner` |
| Passengers.SyncOwner | promote | Techno | `Passengers.SyncOwner.RevertOnExit` |

## Skipped / Deferred Rows

| group | action | appliesTo | key | reason |
|---|---|---|---|---|
| Spawner | defer | Techno | `Spawner` | deferred |

## Notes

- `OpenTopped.CheckTransportDisableWeapons` 与 `OpenTopped.UseTransportRangeModifiers` 在本轮已从 Phobos Attached Effects 示例中确认属于 `[SOMETECHNO]`，因此从 recheck 改为晋级。
- `Spawner / Techno` 未由 aircraft spawner customizations 页面直接支撑基础原版语义，继续保留低可信。
- 本阶段未新增字段，只修改已有行的 quality / sources / description / schema。

## Runtime Backlog Extraction

After the source-backed promotion step, the remaining 1590 `needs-more-evidence*` rows were migrated out of the runtime BuiltIn pack and preserved in `Docs/FieldRegistryLowConfidenceBacklog_2026-06-03.md`. Runtime BuiltIn now contains 3519 rows and 0 `needs-more-evidence*` rows.
