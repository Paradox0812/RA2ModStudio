# Field Registry Description Verification - AI Cross-Context Batch

Phase: FR-DQ-2F-AI-CrossContext-ManualApply

This document records the source verification and BuiltIn v3.2 patch for the next focused AI hover-quality batch after `FR-DQ-2F-AI-LowQuality-ManualApply`.

## 1. Scope

Target file:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
```

Target rows:

- `Owner / AI`
- `Prerequisite / AI`
- `Sight / AI`
- `AirstripRatio / AI`
- `AirstripLimit / AI`
- `AirstripRatio / Global`
- `AirstripLimit / Global`
- `AirstripRatio / Techno`
- `AirstripLimit / Techno`

This phase did not modify Field Registry provider priority, lookup/fallback/enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML, UI, project files, or legacy files.

## 2. Source Summary

Primary source family: ModEnc RA2/YR field pages.

Key findings:

- `Owner` is a `TechnoTypes` field for `AircraftTypes`, `BuildingTypes`, `InfantryTypes`, and `VehicleTypes`. It controls which countries/houses can construct or otherwise receive the object. ModEnc also notes that RA2/YR AI production has special behavior: AI does not check `Owner` when producing infantry, vehicles, and aircraft in taskforces, but AI building construction does check that the building's `Owner` includes the AI country.
- `Prerequisite` is a `TechnoTypes` field listing required structures or prerequisite keywords for building an object. ModEnc's prerequisite-system page notes that AI players in RA2/YR ignore normal `Prerequisite=` checks for building queues except for other constraints such as `TechLevel`, `AIBuildThis`, `Owner`, `RequiredHouses`, and `ForbiddenHouses`.
- `Sight` is a `TechnoTypes` integer field describing how far the object can reveal shroud. ModEnc notes that AI target selection is generally not based on target visibility / shroud coverage.
- `AirstripRatio` and `AirstripLimit` are `[AI]` fields listed in the ModEnc `[AI]` applicable-flag table. Their individual pages state that the logic is obsolete in TS/RA2/YR and that the flags are parsed but do nothing.

## 3. Verification Matrix

| Key | Canonical Context | Verified Meaning | Source | Source Trust | JSON Action | DoNotApplyTo |
|---|---|---|---|---|---|---|
| Owner | AI | Non-canonical guardrail. `Owner` belongs to `TechnoTypes`; the `[AI]` row only prevents imported placeholder text from appearing in Hover. | https://modenc.renegadeprojects.com/Owner and https://modenc.renegadeprojects.com/AI | Community | Updated AI row to source-backed non-canonical guardrail. | Do not treat `Owner` as an `[AI]` section flag. |
| Prerequisite | AI | Non-canonical guardrail. `Prerequisite` belongs to `TechnoTypes`; RA2/YR AI building queues do not use normal prerequisite logic in the same way as human construction. | https://modenc.renegadeprojects.com/Prerequisite and https://modenc.renegadeprojects.com/The_Prerequisite_System | Community | Updated AI row to source-backed non-canonical guardrail. | Do not treat `Prerequisite` as an `[AI]` section flag. |
| Sight | AI | Non-canonical guardrail. `Sight` belongs to `TechnoTypes`; AI target selection is not generally based on whether the target is visible through shroud. | https://modenc.renegadeprojects.com/Sight and https://modenc.renegadeprojects.com/AI | Community | Updated AI row to source-backed non-canonical guardrail. | Do not treat `Sight` as an `[AI]` section flag. |
| AirstripRatio | AI | Old AI base-composition ratio for Airstrip/AFLD-style airfield buildings; parsed but no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/AirstripRatio and https://modenc.renegadeprojects.com/AI | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| AirstripLimit | AI | Old AI base-composition limit for Airstrip/AFLD-style airfield buildings; parsed but no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/AirstripLimit and https://modenc.renegadeprojects.com/AI | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| AirstripRatio | Global | Non-canonical guardrail. Source places this flag in `[AI]`, not `[General]` / Global. | https://modenc.renegadeprojects.com/AirstripRatio and https://modenc.renegadeprojects.com/AI | Community | Updated Global row to explicit non-canonical guardrail. | Do not use as Global field. |
| AirstripLimit | Global | Non-canonical guardrail. Source places this flag in `[AI]`, not `[General]` / Global. | https://modenc.renegadeprojects.com/AirstripLimit and https://modenc.renegadeprojects.com/AI | Community | Updated Global row to explicit non-canonical guardrail. | Do not use as Global field. |
| AirstripRatio | Techno | Non-canonical guardrail. Source places this flag in `[AI]`, not `TechnoTypes`. | https://modenc.renegadeprojects.com/AirstripRatio and https://modenc.renegadeprojects.com/AI | Community | Updated Techno row to explicit non-canonical guardrail. | Do not use as TechnoType field. |
| AirstripLimit | Techno | Non-canonical guardrail. Source places this flag in `[AI]`, not `TechnoTypes`. | https://modenc.renegadeprojects.com/AirstripLimit and https://modenc.renegadeprojects.com/AI | Community | Updated Techno row to explicit non-canonical guardrail. | Do not use as TechnoType field. |

## 4. Applied Description Summary

Source-backed `[AI]` rows:

- `AirstripRatio / AI`: Float description for old AI base-composition airstrip ratio; includes obsolete / parsed no-op caveat.
- `AirstripLimit / AI`: Integer description for old AI base-composition airstrip limit; includes obsolete / parsed no-op caveat.

Non-canonical guardrails:

- `Owner / AI`
- `Prerequisite / AI`
- `Sight / AI`
- `AirstripRatio / Global`
- `AirstripLimit / Global`
- `AirstripRatio / Techno`
- `AirstripLimit / Techno`

## 5. Result Summary

```text
Target rows updated: 9
Source-backed canonical AI rows: 2
Source-backed non-canonical guardrail rows: 7
Field count change: 4643 -> 4643
Exact `数值型字段` rows: 0
Placeholder rows after this patch: 2452
```

## 6. Next Step

Recommended next phase:

```text
FR-DQ-2F-BaseDefense-Placeholder-ManualApply
```

Suggested scope:

- `AlliedBaseDefenses / AI`
- `SovietBaseDefenses / AI`
- `ThirdBaseDefenses / AI`
- `BuildDefense / AI`
- `BuildAA / AI`

Do not expand into all remaining placeholders in one pass.
