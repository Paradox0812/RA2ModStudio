# Field Registry Placeholder Retirement Audit - 2026-07-20

Phase: `FR-DQ-4 PlaceholderRetirementAndTrustCleanup`, updated through `FR-DQ-4D governance checkpoint 2`

## 1. Scope

Read-only source:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
```

Candidate manifest:

```text
Docs/FieldRegistryPlaceholderRetirementCandidates_2026-07-20.csv
SHA-256: FDBCDB40F62B22715B9AD4A4EFA0B3F044B46B2DC165B9E5FF94EC0A5B341168
```

The audit does not modify runtime JSON or infer deletion from array indexes. Candidate identity uses case-insensitive key, sorted exact appliesTo, current quality and SHA-256 of the trimmed current description.

## 2. Runtime baseline

| Metric | Count |
|---|---:|
| Runtime BuiltIn rows | 4866 |
| Exact key + sorted appliesTo duplicates | 0 |
| Candidate manifest rows | 2947 |
| DiagnosticOnlyKeep | 349 |
| SupersededRemove | 12 |
| PromoteAndRewrite | 30 |
| PendingManualReview | 2556 |
| All candidates with a higher-quality same-key row elsewhere | 574 |
| Narrow inferred/auto/empty-quality same-key candidates | 330 |

## 3. Candidate trust buckets

| Trust bucket | Count | Initial disposition |
|---|---:|---|
| Inferred | 1590 | PendingManualReview |
| AutoExtracted | 810 | PendingManualReview |
| VerifiedGuardrail | 329 | DiagnosticOnlyKeep |
| Unknown | 193 | PendingManualReview |
| Obsolete | 20 | DiagnosticOnlyKeep |
| Empty quality currently classified as Verified | 5 | PendingManualReview |

No Inferred, AutoExtracted, Unknown or empty-quality row was automatically approved for deletion or promotion.

## 4. Description shapes

| Shape | Count |
|---|---:|
| UniformInferredTemplate | 1524 |
| SpecificOrNeedsReview | 879 |
| GenericTypeOnly | 267 |
| KeyPlusField | 125 |
| ShortDescription | 117 |
| RawLegacyFragment | 35 |

Shape sets are mutually exclusive in the manifest because precedence is fixed as template -> exact key+field -> generic type -> raw legacy fragment -> short -> specific/review.

The earlier `1583` template estimate was incorrect. Current code classification treats 1590 rows as Inferred, but 59 source-assisted rows do not use the uniform template and 7 `name-inferred-ui-fallback` rows have individual descriptions; the exact uniform-template count is 1524.

## 5. Largest context groups

| appliesTo | Candidate count |
|---|---:|
| Techno | 1401 |
| Global | 509 |
| ArtObject | 221 |
| Weapon | 161 |
| Warhead | 151 |
| AI | 118 |
| Building | 113 |
| Vehicle | 52 |

The Techno group is high-risk because `LocalRa2FieldDefinitionProvider` uses Techno as an abstract fallback for Infantry, Vehicle, Aircraft, Building and Unit. A wrong Techno row can therefore leak into several concrete unit contexts.

## 6. Initial deterministic decisions

Only existing diagnostic trust states receive an automatic disposition:

```text
VerifiedGuardrail: 329 -> DiagnosticOnlyKeep
Obsolete: 20 -> DiagnosticOnlyKeep
```

These rows remain available to lookup, Hover and Diagnostics, but FR-DQ-4B will exclude them from field-name Completion. `AA/AG` in Techno and Weapon are representative rows.

All other candidates remain `PendingManualReview`. Same-key higher-quality presence is only a review signal because the same key can have legitimate context-specific meanings.

## 7. Manifest columns

- `Key`, `AppliesTo`: stable semantic identity components.
- `EditorKind`, `SourceKind`, `CurrentQuality`, `CurrentTrustBucket`: current runtime facts.
- `DescriptionShape`, `DescriptionSha256`: description audit identity without relying on JSON index.
- `SourceCount`: source-file evidence count; not a runtime DTO fact.
- `SameKeyHigherQualityElsewhere`, `HigherQualityContexts`: review signal only.
- `ProposedDisposition`, `ReviewReason`: current decision state.

## 8. Guardrails

- Do not sum overlapping historical ad-hoc shape counts; use the mutually exclusive manifest shape.
- Do not delete by quality, description length or same-key presence alone.
- Do not treat `sources` as runtime trust state.
- Do not restore low-quality rows merely to suppress Unknown Key counts.
- Do not change Project > Global > BuiltIn or exact -> Unit -> Techno -> Global -> Unknown lookup order.

## 9. FR-DQ-4C confirmed retirements

The first confirmed retirement batch removed 12 broad `Techno` inferred fallbacks. Each key has a retained source-verified `Global` definition backed by the ModEnc `[General]` page; eight also retain an explicit `[AI]` wrong-context guardrail. This is stronger evidence than same-key presence alone.

```text
AIAlternateProductionCreditCutoff
AIAutoDeployFrameDelay
AISuperDefenseDistance
BaseDefenseDelay
DisabledDisguiseDetectionPercent
DissolveUnfilledTeamDelay
FillEarliestTeamProbability
GameSpeedBias
MaximumBuildingPlacementFailures
SuspendDelay
ThreatPerOccupant
UseMinDefenseRule
```

`BuildupTime / Techno` was deliberately excluded because its verified Global description states that Ares permits a BuildingType override. It requires context-specific review in 4D and cannot be retired under the 4C replacement rule.

Validation after removal: runtime rows `4866`; exact identity duplicates `0`; targeted BuiltIn/Completion/trust tests `627/627` passed. Runtime JSON SHA-256 is `4674013332486FA04C291C978FDE74B6425759CF01F652676BD29D47B3D74FE6`.

## 10. FR-DQ-4D governance checkpoint 1

Thirty uniform inferred-template rows have been source-verified and rewritten rather than removed:

- 7 BannerType rows from the official Phobos Display Banner documentation.
- 8 TerrainType rows and 1 Tiberium row from the official Phobos TerrainTypes/Tiberiums documentation.
- 11 Country UI rows from the official Ares Country User Interface documentation.
- 3 Phobos UI settings corrected from broad `Side` to `Global`; `ConditionYellow.Terrain` was likewise corrected from `Terrain` to the `[AudioVisual]`/Global context and now uses the existing `Float` schema token.

Current runtime rows remain `4866`; exact identity duplicates remain `0`; uniform `推断型字段：` descriptions are `1482`. BuiltIn/Completion/trust targeted tests passed `657/657`. Runtime JSON SHA-256 is `50D219CA95586A8F9B37FDD84D5778700B9A7E28AD834BFA6387A20918329A7E`.

The package full suite and clean package are intentionally deferred to 4H; each completed data card has passed the scoped loader gate.

## 11. FR-DQ-4D governance checkpoint 2

Checkpoint 2 completed three further context-family cards after checkpoint 1:

- EVA Types: promoted `Allied` / `Russian` / `Yuri`; quarantined fixed EVA rows `Priority` / `Text` / `Type` because no direct EVA-event evidence was found.
- Small official families: promoted damaged Aircraft images, Ares generic prerequisites, and Building/Vehicle sell feedback; corrected the four prerequisite rows from `Unit` to `Techno`.
- Infantry families: promoted engineer repair, auto-deploy, prone speed, building-upgrade and slave-owner rows; corrected `PowersUp.*` to `Building`, `ProneSpeed.Crawls/NoCrawls` to `Global`, and retired the inferred `DefaultDisguise / Infantry` fallback in favor of the retained official `Side` row.

Current measured state:

| Metric | Count |
|---|---:|
| Runtime BuiltIn rows | 4862 |
| Exact key + sorted appliesTo duplicates | 0 |
| Uniform `推断型字段：` descriptions | 1458 |
| All inferred-quality rows | 1465 |
| DiagnosticOnlyKeep | 349 |
| SupersededRemove | 13 |
| PromoteAndRewrite | 50 |
| Quarantine | 3 |
| PendingManualReview | 2532 |

The first replay attempt at this checkpoint exposed a Windows PowerShell code-page failure: a mechanical rewrite read/wrote UTF-8 Chinese through an ANSI console path, corrupting JSON strings. Execution stopped, the v3.2 runtime file was restored from the 4-0 rollback JSON with exact SHA-256 `39256F8EEA11C45A05FB87863E532F28AF3F9C01A3ED0A153044D6276E10BDE5`, and all reviewed changes were replayed with explicit UTF-8, unique identity assertions, round-trip JSON validation and atomic replacement. The transient repair helper was deleted after use.

Recovered runtime JSON SHA-256: `69B229F4854A20547A3DD0A5C95DF2C8726E61042D0881EB4A8B7AD17AD87A21`. Reviewed descriptions containing replacement question marks: `0`. BuiltIn/Completion/trust scoped tests passed `681/681` after recovery.

Do not pipe non-ASCII Field Registry rewrite source through Windows PowerShell's default console encoding. Future bulk mechanical rewrites must use an encoding-stable file or ASCII-only command input, explicit UTF-8 decode/encode, identity cardinality assertions, JSON round-trip validation and an atomic final replace.

## 12. FR-DQ-4D governance checkpoint 3

Four further context cards completed after checkpoint 2:

- Small contexts: 10 directly documented Phobos/ModEnc rows promoted; `Gas.MaxDriftSpeed` corrected to `Particle`, `Bolt.Arcs` corrected to Integer, and `tempValue / AI`, `Threat / AI`, `Limit / Sound` quarantined because their fixed-context evidence remained insufficient or contradictory.
- ArtObject + Building: 168 uniform templates quarantined.
- Warhead + Weapon: 72 uniform templates quarantined.
- Global + Vehicle: 58 uniform templates quarantined.

Every context quarantine used the exact candidate identity and required the runtime removal set to equal the manifest transition set. Non-template and already verified definitions in the same contexts were retained. The scoped BuiltIn/Completion/trust gate passed after every card; the latest result is `700/700`.

Current measured state:

| Metric | Count |
|---|---:|
| Runtime BuiltIn rows | 4561 |
| Exact key + sorted appliesTo duplicates | 0 |
| Uniform `推断型字段：` descriptions | 1147 |
| Remaining uniform-template contexts | Techno only |
| DiagnosticOnlyKeep | 349 |
| SupersededRemove | 13 |
| PromoteAndRewrite | 60 |
| Quarantine | 304 |
| PendingManualReview | 2221 |

Runtime JSON SHA-256: `53426A6EFFF3122615ABB4A7783D69CB48FBB45BE8DF0454D0551690AB1D47B5`. Manifest SHA-256: `2BE861C5DC4185060895081047482B18318AA60ED1BC75C3C06D521AEED9131E`.

## 13. FR-DQ-4D completed

The remaining 1147 broad Techno templates were split into five stable, case-insensitive key ranges of `230 / 230 / 230 / 230 / 227` rows. Every card required:

- exact `Techno + UniformInferredTemplate + PendingManualReview` matching;
- runtime removal set equal to manifest transition set;
- no duplicate key + sorted appliesTo identity after rewrite;
- explicit UTF-8 JSON round trip and atomic replacement;
- a scoped BuiltIn/Completion/trust test run before continuing.

The five reviewed ranges were:

```text
AARate .. Culling
CustomGS .. GUITabSound
Gunner .. NoRearm.UnderEMP
NoReload.Temporal .. SmallFire
SmallVisceroid .. ZVelocityRange
```

One historical test still required three generic Techno fallbacks (`AttachedParticleSystem`, `Duration`, `Type`) to exist. Those assertions conflicted with the confirmed retirement contract and were removed; the retained source-backed `Agent` / `Aggressive` diagnostic guards remain covered.

FR-DQ-4D final state:

| Metric | Count |
|---|---:|
| Runtime BuiltIn rows | 3414 |
| Exact key + sorted appliesTo duplicates | 0 |
| Uniform `推断型字段：` descriptions | 0 |
| Remaining inferred-quality rows with specific non-template text | 7 |
| AutoExtracted rows | 810 |
| DiagnosticOnlyKeep | 349 |
| SupersededRemove | 13 |
| PromoteAndRewrite | 60 |
| Quarantine | 1451 |
| PendingManualReview | 1074 |

Final 4D scoped gate: `703/703` passed. Runtime JSON SHA-256: `898A8A37CF41835DEA870F351D03977478F85295E771E9F4C5AE04EC26AA9409`. Manifest SHA-256: `FDBCDB40F62B22715B9AD4A4EFA0B3F044B46B2DC165B9E5FF94EC0A5B341168`.

Quarantine remains an evidence state, not a claim that the underlying INI key is invalid. The 4-0 rollback JSON preserves every retired row for later evidence-driven recovery.

## 14. Next entry

Enter `FR-DQ-4E AutoExtractedDisposition`. Process the 810 `auto-extracted` rows in bounded context/source cards. A row must be promoted from direct evidence, quarantined when evidence is insufficient, or classified diagnostic-only when the wrong-context/obsolete fact is verified; `auto-extracted` is not an accepted final runtime quality.

## 15. FR-DQ-4E completed

The 810 remaining `auto-extracted` rows were split by exact source kind and stable case-insensitive key range. Each card required an exact runtime/manifest identity-set match, duplicate check, UTF-8 JSON round trip, atomic replacement, and scoped regression run.

| Source | Key range | Rows |
|---|---|---:|
| Phobos | `AbsorbOverDamage .. IsHideable` | 175 |
| Phobos | `IsHouseColor .. ZShapePointMove.OnBuildup` | 175 |
| Yuri | `ActivateSound .. DeployedFire` | 154 |
| Yuri | `DeployedIdle .. PowersUpBuilding` | 154 |
| Yuri | `PowerUp1Anim .. ZShapePointMove` | 152 |

All 810 rows were quarantined because source metadata alone did not prove an extracted context, and the extraction population contained provable context errors. Quarantine is an evidence-state decision, not a declaration that the underlying INI key is invalid.

Two historical loader tests were corrected during the cards: `AllowWeaponSelectAgainstWalls` and `ActiveAnim / ArtObject` had implicitly required unverified auto-extracted rows to remain. The ArtObject mapping test now uses source-verified `AltPalette`, and the pack-size smoke threshold remains high enough to detect a missing resource without freezing the pre-retirement row count.

| Metric | Count |
|---|---:|
| Runtime BuiltIn rows | 2604 |
| Exact key + sorted appliesTo duplicates | 0 |
| Uniform inferred templates | 0 |
| AutoExtracted rows | 0 |
| DiagnosticOnlyKeep | 349 |
| SupersededRemove | 13 |
| PromoteAndRewrite | 60 |
| Quarantine | 2261 |
| PendingManualReview | 264 |

Final 4E scoped gate: `709/709` passed. Runtime JSON SHA-256: `0A5C469863726EA080EA9B674CC98D62D772F7FC19C4ED3961D5A802E79E6880`. Manifest SHA-256: `DB81C661B1FC4486FD0E0E08727089A33288AE4AFCCEE5A34A55839F4DA64190`.

## 16. Next entry

Enter `FR-DQ-4F QualityNormalization`. The remaining 264 pending rows classify as Unknown 193, Inferred 66, and legacy empty-quality/Verified 5. Do not relabel rows merely to satisfy counters; preserve only evidence-consistent recognized trust states and quarantine the rest.

## 17. FR-DQ-4F completed

Three bounded quality-family cards completed:

1. All 208 runtime `community-reviewed-*` labels were prefixed with `manual-curated-`. This normalized 193 pending community-reviewed rows to the existing ManualCurated trust family while preserving 15 diagnostic guardrails because their quality still contains the higher-precedence `guardrail` token.
2. The 66 `source-assisted-*` / `name-inferred-*` rows were retained unchanged and recorded as `RetainReviewed`. Their descriptions are specific, non-empty and not uniform inferred templates; no source-verified promotion was claimed.
3. Five required identity/garrison rows (`EliteOccupyWeapon`, `Occupier`, `OccupyWeapon`, `OpenTransportWeapon`, and Country `UIName`) retained their compatibility identities, had mojibake Chinese descriptions repaired, and received `manual-curated-identity-garrison-patch-20260720` instead of empty quality.

Final state:

| Metric | Count |
|---|---:|
| Runtime BuiltIn rows | 2604 |
| Empty quality rows | 0 |
| Unrecognized runtime trust rows | 0 |
| Uniform inferred templates | 0 |
| AutoExtracted rows | 0 |
| PendingManualReview | 0 |
| DiagnosticOnlyKeep | 349 |
| SupersededRemove | 13 |
| PromoteAndRewrite | 65 |
| RetainReviewed | 259 |
| Quarantine | 2261 |

Final 4F scoped gate: `712/712` passed. Runtime JSON SHA-256: `0A4024853D78CA57A13796E5C405ECDF86D4BED7124758891A0E491524B81A3C`. Manifest SHA-256: `696384F56045B18047B24AC25C28FB54433400C36B90B9A5F8EF682D13404EB1`.

## 18. Next entry

Enter `FR-DQ-4G RuntimeSurfaceAndUnknownKeyRegression`. Measure Completion, Hover, Quick Peek, Diagnostics and representative Unknown Key changes. Do not restore quarantined placeholders merely to improve the metric.

## 19. FR-DQ-4G completed

Surface-focused regression used the real v3.2 pack rather than the Core minimal provider:

- Completion: canonical Projectile AA/AG remain available; Vehicle/Techno diagnostic guardrails remain excluded from key candidates.
- Hover: Vehicle AA guardrail still resolves and shows the lightweight wrong-context footnote.
- Quick Peek: Vehicle AA remains available with `上下文保护` trust details.
- Diagnostics: Vehicle AA produces `FIELD_WRONG_CONTEXT`.
- Unknown Key: a fixed Vehicle sample containing `AllowWeaponSelectAgainstWalls`, `AARate`, and `AirstrikeTeam` produces exactly three `FIELD_UNKNOWN_KEY` warnings. The 4-0 rollback JSON contains compatible identities for all three keys; the current runtime contains none.
- Highlighting and related surface regressions were included in the same focused gate.

Surface-focused test result: `900/900` passed. The only build warning remains the pre-existing nullable CS8602 in `BuiltInFieldRegistryPackLoaderTests.cs`.

The manifest contains 2261 quarantined exact rows across 1992 unique keys. The largest exact-context groups are Techno 1258, Global 238, ArtObject 205, Warhead 150, Building 112, Weapon 106, and Vehicle 52. These counts describe compatibility exposure, not observed user-file occurrences.

No `.ini` corpus exists in the repository after excluding generated/artifact directories, so real-project Unknown Key occurrence counts could not be measured. The reproducible representative result is therefore `+3` warnings for the fixed three-row sample; this limitation must remain visible in final delivery.

## 20. Next entry

Enter `FR-DQ-4H FullVerificationDocumentationAndPackage`: run the IDE-only restore/build/full test/package commands, complete diff and package hygiene review, and close governance documents.

## 21. FR-DQ-4H completed

Final verification:

| Gate | Result |
|---|---|
| `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed; 0 warnings / 0 errors |
| `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | Passed; 2274/2274 |
| `powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly` | Passed |
| Package excluded directories / archive-temporary patterns | 0 violations |
| Legacy root solution/project/MainWindow entries | 0 |
| Critical package/workspace hash mismatches | 0 |

Final runtime JSON SHA-256: `0A4024853D78CA57A13796E5C405ECDF86D4BED7124758891A0E491524B81A3C`.

Final manifest SHA-256: `696384F56045B18047B24AC25C28FB54433400C36B90B9A5F8EF682D13404EB1`.

No public API, provider priority, lookup hierarchy, Completion commit/value candidate, Hover source, Quick Peek core, Diagnostics core, parser, save, XAML/Shell, project file, or legacy behavior was changed. The approved behavior changes are limited to BuiltIn data quality and diagnostic-only field-name Completion visibility.

FR-DQ-4 is closed. Quarantined rows remain recoverable from the 4-0 rollback anchors and stable manifest identities, but any recovery requires direct evidence and a new bounded contract.
