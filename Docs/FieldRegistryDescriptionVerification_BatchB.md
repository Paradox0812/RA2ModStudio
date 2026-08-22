# Field Registry Description Verification - Batch B

Phase: FR-DQ-2C-Verify-ManualApply

This document records the manual online/source verification for Batch B fields and the limited Field Registry changes applied after verification.

This phase updates BuiltIn v3.2 Field Registry descriptions for verified Batch B rows and replaces remaining Batch B placeholder / low-quality Hover text with source-backed descriptions or explicit non-canonical guardrail text. It does not modify provider priority, lookup / fallback / enrichment behavior, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI provider, XAML / UI, legacy files, or project files.

## 1. Scope

Batch B covers:

```text
BuildCat
Crewed
Turret
ThreatPosed
```

The verification starts from `Docs/FieldRegistryDescriptionVerification_BatchB_Input.md` and the effective Hover quality concerns it identified. It does not expand to Batch C / Batch D fields.

## 2. Source Trust Policy

Source trust follows `Docs/FieldRegistryDescriptionSourcePolicy.md`:

- `Community`: ModEnc pages with explicit flag metadata and applicability.
- `LocalImported`: existing effective project/global descriptions; useful for current behavior but not authoritative for BuiltIn source-backed patching.
- `Unknown`: unresolved or broad fallback rows with no direct source support.

## 3. Verification Matrix

| Key | SectionKind / Schema | Input Status | Verified Meaning | Source | Source Trust | Verification Result | ReadyForPatchPlan | Applied Action | DoNotApplyTo | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| BuildCat | Building | Valid / LocalImported | BuildingTypes field. Sets the building's build category and affects sidebar placement; `Combat` places the structure in the defense tab, while other useful options generally keep it on the main structures tab. | ModEnc: https://modenc.renegadeprojects.com/BuildCat | Community | Verified | true | Updated Building row with source-backed wording, source URL, quality marker, and complete known allowed values. | Do not copy to non-building Techno subclasses. | ModEnc lists applicability as BuildingTypes only. |
| BuildCat | Techno | Placeholder fallback row | No direct TechnoTypes support found. Source-backed meaning is BuildingTypes-only. | ModEnc: https://modenc.renegadeprojects.com/BuildCat | Community | RejectedAsNonCanonicalContext | false | Replaced placeholder with non-canonical guardrail text explaining that this row is only a broad fallback and not a valid non-building target. | Do not apply as a general Techno field. | This removes Hover placeholder pollution without making the broad fallback a canonical target. |
| BuildCat | Aircraft | Placeholder broad fallback via Techno | No direct AircraftTypes support found. | ModEnc: https://modenc.renegadeprojects.com/BuildCat | Community | RejectedAsNonCanonicalContext | false | No exact Aircraft JSON row added; Aircraft will see the Techno guardrail if fallback lookup is used. | Do not add an Aircraft target unless a later source proves support. | Source applicability is BuildingTypes only. |
| BuildCat | Infantry | Placeholder broad fallback via Techno | No direct InfantryTypes support found. | ModEnc: https://modenc.renegadeprojects.com/BuildCat | Community | RejectedAsNonCanonicalContext | false | No exact Infantry JSON row added; Infantry will see the Techno guardrail if fallback lookup is used. | Do not add an Infantry target unless a later source proves support. | Source applicability is BuildingTypes only. |
| BuildCat | Unit | Placeholder broad fallback via Techno | No direct Unit / abstract Techno support found. | ModEnc: https://modenc.renegadeprojects.com/BuildCat | Community | RejectedAsBroadFallback | false | No Unit JSON row added. | Do not use Unit broad fallback as a write target. | Unit is an abstract lookup bucket, not a source-defined INI context. |
| BuildCat | Vehicle | Placeholder broad fallback via Techno | No direct VehicleTypes support found. | ModEnc: https://modenc.renegadeprojects.com/BuildCat | Community | RejectedAsNonCanonicalContext | false | No exact Vehicle JSON row added; Vehicle will see the Techno guardrail if fallback lookup is used. | Do not add a Vehicle target unless a later source proves support. | Source applicability is BuildingTypes only. |
| Crewed | Building | Valid / LocalImported | Boolean field for BuildingTypes; controls whether infantry crew can escape when the object is destroyed. Suicide self-destruction leaves no survivors/passengers. | ModEnc: https://modenc.renegadeprojects.com/Crewed | Community | Verified | true | Updated existing Building row with source-backed wording and source URL. | Do not copy to Infantry / Unit. | Source applicability includes BuildingTypes. |
| Crewed | Vehicle | Placeholder local/fallback row | Boolean field for VehicleTypes; controls whether infantry crew can escape when the object is destroyed. Suicide self-destruction leaves no survivors/passengers. | ModEnc: https://modenc.renegadeprojects.com/Crewed | Community | Verified | true | Added exact Vehicle row with Boolean editor metadata. | Do not infer Infantry support. | Source applicability includes VehicleTypes. |
| Crewed | Aircraft | Placeholder local/fallback row | Boolean field for AircraftTypes; controls whether infantry crew can escape when the object is destroyed. Suicide self-destruction leaves no survivors/passengers. | ModEnc: https://modenc.renegadeprojects.com/Crewed | Community | Verified | true | Added exact Aircraft row with Boolean editor metadata. | Do not infer Infantry support. | Source applicability includes AircraftTypes. |
| Crewed | Techno | Placeholder local/fallback row | Broad fallback only. Source-backed concrete contexts are BuildingTypes, VehicleTypes, and AircraftTypes. | ModEnc: https://modenc.renegadeprojects.com/Crewed | Community | PartiallyVerified | false | Replaced placeholder with broad fallback guardrail text. | Do not apply as generic Techno, Infantry, or Unit wording. | Keeps Hover useful while preventing an over-broad canonical target. |
| Crewed | Infantry | Placeholder local/fallback row | No direct InfantryTypes support found. | ModEnc: https://modenc.renegadeprojects.com/Crewed | Community | RejectedAsNonCanonicalContext | false | No exact Infantry JSON row added. | Do not add Infantry target unless a later source proves support. | Source applicability does not include InfantryTypes. |
| Crewed | Unit | Placeholder local/fallback row | No direct Unit / abstract Techno support found. | ModEnc: https://modenc.renegadeprojects.com/Crewed | Community | RejectedAsBroadFallback | false | No Unit JSON row added. | Do not use Unit broad fallback as a write target. | Unit is an abstract lookup bucket. |
| Turret | Vehicle | Placeholder local row | Boolean field for VehicleTypes; declares whether the object has a turret. Vehicle turret assets use the object image name plus `tur` VXL/HVA convention; missing files can crash the game. | ModEnc: https://modenc.renegadeprojects.com/Turret | Community | Verified | true | Added exact Vehicle row with Boolean editor metadata. | Do not copy to Aircraft / Infantry. | Source applicability includes VehicleTypes. |
| Turret | Building | Placeholder local row | Boolean field for BuildingTypes; declares whether the building has a turret. Building turrets are associated with `TurretAnim`, `TurretAnimIsVoxel`, and related offset fields. | ModEnc: https://modenc.renegadeprojects.com/Turret | Community | Verified | true | Added exact Building row with Boolean editor metadata. | Do not copy Vehicle asset wording directly to Building without building-specific note. | Source applicability includes BuildingTypes. |
| Turret | Techno | Placeholder fallback row | Broad fallback only. Source-backed concrete contexts are VehicleTypes and BuildingTypes. | ModEnc: https://modenc.renegadeprojects.com/Turret | Community | PartiallyVerified | false | Replaced placeholder with broad fallback guardrail text. | Do not apply as generic Techno, Aircraft, Infantry, or Unit wording. | Keeps Hover useful while preventing an over-broad canonical target. |
| Turret | Infantry | Placeholder local row | No direct InfantryTypes support found. | ModEnc: https://modenc.renegadeprojects.com/Turret | Community | RejectedAsNonCanonicalContext | false | No exact Infantry JSON row added. | Do not add Infantry target unless a later source proves support. | Source applicability does not include InfantryTypes. |
| Turret | Aircraft | Placeholder broad fallback | No direct AircraftTypes support found. | ModEnc: https://modenc.renegadeprojects.com/Turret | Community | RejectedAsNonCanonicalContext | false | No exact Aircraft JSON row added. | Do not add Aircraft target unless a later source proves support. | Source applicability does not include AircraftTypes. |
| Turret | Unit | Placeholder broad fallback | No direct Unit / abstract Techno support found. | ModEnc: https://modenc.renegadeprojects.com/Turret | Community | RejectedAsBroadFallback | false | No Unit JSON row added. | Do not use Unit broad fallback as a write target. | Unit is an abstract lookup bucket. |
| ThreatPosed | Techno | Valid / LocalImported | TechnoTypes field. Sets the threat level used by the threat system / auto-targeting. Unarmed targets do not become active attack targets merely by changing this value; non-building technos may still be actively targeted even with value 0. | ModEnc: https://modenc.renegadeprojects.com/ThreatPosed | Community | Verified | true | Updated Techno row with more precise source-backed wording and source URL. | Do not copy common-object wording to AI script contexts. | Source applicability is TechnoTypes: AircraftTypes, BuildingTypes, InfantryTypes, VehicleTypes. |
| ThreatPosed | Aircraft | Valid / LocalImported | Effective current object-context description remains valid. | ModEnc: https://modenc.renegadeprojects.com/ThreatPosed | Community | EffectiveValidExclusion | false | No exact Aircraft row added in this phase. | Do not overwrite current effective description unless future audit requests exact object-context rows. | Techno row remains the BuiltIn canonical fallback. |
| ThreatPosed | Building | Valid / LocalImported | Effective current object-context description remains valid. | ModEnc: https://modenc.renegadeprojects.com/ThreatPosed | Community | EffectiveValidExclusion | false | No exact Building row added in this phase. | Do not overwrite current effective description unless future audit requests exact object-context rows. | Techno row remains the BuiltIn canonical fallback. |
| ThreatPosed | Infantry | Valid / LocalImported | Effective current object-context description remains valid. | ModEnc: https://modenc.renegadeprojects.com/ThreatPosed | Community | EffectiveValidExclusion | false | No exact Infantry row added in this phase. | Do not overwrite current effective description unless future audit requests exact object-context rows. | Techno row remains the BuiltIn canonical fallback. |
| ThreatPosed | Vehicle | Valid / LocalImported | Effective current object-context description remains valid. | ModEnc: https://modenc.renegadeprojects.com/ThreatPosed | Community | EffectiveValidExclusion | false | No exact Vehicle row added in this phase. | Do not overwrite current effective description unless future audit requests exact object-context rows. | Techno row remains the BuiltIn canonical fallback. |
| ThreatPosed | AI | LowQuality value-type label | No source support found for AITrigger / TaskForce / Script / TeamType contexts. Source-backed meaning is TechnoTypes-only. | ModEnc: https://modenc.renegadeprojects.com/ThreatPosed | Community | RejectedAsNonCanonicalContext | false | Replaced `数值型字段` with explicit non-AI guardrail text. | Do not use as AI context field. | This removes low-quality Hover text while preventing incorrect AI semantics. |

## 4. Applied JSON Changes

Updated:

- `BuildCat / Building`: improved description, complete known enum values, ModEnc source, quality marker.
- `BuildCat / Techno`: placeholder replaced by non-canonical guardrail text.
- `Crewed / Building`: improved description and source.
- `Crewed / Techno`: placeholder replaced by broad fallback guardrail text.
- `Turret / Techno`: placeholder replaced by broad fallback guardrail text.
- `ThreatPosed / Techno`: improved description and source.
- `ThreatPosed / AI`: low-quality `数值型字段` replaced by non-canonical guardrail text.

Added:

- `Crewed / Vehicle`
- `Crewed / Aircraft`
- `Turret / Vehicle`
- `Turret / Building`

Not added:

- `BuildCat / Aircraft`, `BuildCat / Infantry`, `BuildCat / Unit`, `BuildCat / Vehicle`
- `Crewed / Infantry`, `Crewed / Unit`
- `Turret / Infantry`, `Turret / Aircraft`, `Turret / Unit`
- exact `ThreatPosed` object subtype rows

## 5. Hover Quality Result

Batch B BuiltIn rows no longer expose the old placeholder phrase:

```text
原始英文说明已移至复核表，不直接用于 Hover
```

Batch B also no longer exposes the low-quality AI description:

```text
数值型字段
```

Some broad fallback rows intentionally remain as guardrails instead of canonical rows. This is deliberate: it prevents Hover from showing meaningless placeholders while still warning that a field is not verified for the current context.

## 6. Test Coverage Added

`RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs` now verifies:

- Batch B source-backed descriptions do not contain placeholder/TODO/TBD text.
- concrete Batch B rows use expected editor kinds:
  - `BuildCat / Building`: Enum
  - `Crewed / Vehicle`: Boolean
  - `Crewed / Aircraft`: Boolean
  - `Turret / Vehicle`: Boolean
  - `Turret / Building`: Boolean
  - `ThreatPosed / Techno`: Integer

## 7. Validation Notes

Static validation completed:

- BuiltIn v3.2 JSON parses successfully.
- Field count changed from 4634 to 4638 because four exact rows were added.
- Batch B direct BuiltIn placeholder count changed from 3 to 0.
- Batch B direct BuiltIn low-quality count changed from 1 to 0.

`dotnet test` was not run in this environment because the current container does not have the `dotnet` CLI installed.

## 8. Next Step

Recommended next phase:

```text
FR-DQ-2F：Continue effective Hover quality audit for remaining non-Batch-B placeholders and low-quality descriptions.
```

Do not treat this patch as full Field Registry cleanup. The full BuiltIn v3.2 registry still contains many placeholder / low-quality rows outside Batch B and should continue to be handled in small source-verified batches.
