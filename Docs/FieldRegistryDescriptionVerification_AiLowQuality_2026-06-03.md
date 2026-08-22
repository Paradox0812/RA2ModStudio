# Field Registry Description Verification - AI LowQuality Batch

Phase: FR-DQ-2F-AI-LowQuality-ManualApply

This document records the source verification and BuiltIn v3.2 patch for the remaining direct `数值型字段` Hover descriptions found after Batch B manual apply.

## 1. Scope

Target file:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
```

Target rows:

- 14 `[AI]` base-composition Ratio / Limit fields that previously showed only `数值型字段`.
- 5 `Dumb*Coefficient` rows that previously appeared under `[AI]` with only `数值型字段`, but were verified as `[General]` threat-system coefficients.

This phase did not modify Field Registry provider priority, lookup/fallback/enrichment, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML, UI, project files, or legacy files.

## 2. Source Summary

Primary source family: ModEnc RA2/YR field pages.

Key findings:

- ModEnc `[AI]` lists AI settings in `Rules(md).ini`, and its applicable flag table places `RefineryRatio`, `RefineryLimit`, `BarracksRatio`, `BarracksLimit`, `WarRatio`, `WarLimit`, `DefenseRatio`, `DefenseLimit`, `AARatio`, `AALimit`, `TeslaRatio`, `TeslaLimit`, `HelipadRatio`, and `HelipadLimit` under `[AI]`.
- Individual Ratio / Limit pages describe them as old base-composition controls and mark the related logic obsolete / parsed but no-op in TS / RA2 / YR.
- ModEnc The Threat System places `DumbMyEffectivenessCoefficient` and related `DumbTarget*Coefficient` flags in `[General]`, not `[AI]` or TechnoType.

## 3. Verification Matrix

| Key | Canonical Context | Verified Meaning | Source | Source Trust | JSON Action | DoNotApplyTo |
|---|---|---|---|---|---|---|
| AARatio | AI | Old AI base-composition ratio for anti-air defenses; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/AARatio | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| AALimit | AI | Old AI base-composition limit for anti-air defenses; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/AALimit | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| BarracksRatio | AI | Old AI base-composition ratio for barracks structures; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/BarracksRatio | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| BarracksLimit | AI | Old AI base-composition limit for barracks structures; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/BarracksLimit | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| DefenseRatio | AI | Old AI base-composition ratio for basic ground defenses; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/DefenseRatio | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| DefenseLimit | AI | Old AI base-composition limit for basic ground defenses; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/DefenseLimit | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| HelipadRatio | AI | Old AI base-composition ratio for helipads; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/HelipadRatio | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| HelipadLimit | AI | Old AI base-composition limit for helipads; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/HelipadLimit | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| RefineryRatio | AI | Old AI base-composition ratio for ore refineries; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/RefineryRatio | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| RefineryLimit | AI | Old AI base-composition limit for ore refineries; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/RefineryLimit | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| TeslaRatio | AI | Old AI base-composition ratio for Tesla Coil defenses; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/TeslaRatio | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| TeslaLimit | AI | Old AI base-composition limit for Tesla Coil defenses; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/TeslaLimit | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| WarRatio | AI | Old AI base-composition ratio for war factories; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/WarRatio | Community | Updated AI row to Float source-backed description. | Global / Techno rows are guardrails only. |
| WarLimit | AI | Old AI base-composition limit for war factories; obsolete / parsed no-op in TS/RA2/YR. | https://modenc.renegadeprojects.com/WarLimit | Community | Updated AI row to Integer source-backed description. | Global / Techno rows are guardrails only. |
| DumbMyEffectivenessCoefficient | Global | Threat-system coefficient used when evaluating a target with lower ThreatPosed than the evaluator. | https://modenc.renegadeprojects.com/DumbMyEffectivenessCoefficient | Community | Added Global Float canonical row; AI / Techno rows changed to guardrails. | AI / Techno rows are non-canonical. |
| DumbTargetEffectivenessCoefficient | Global | Threat-system target-effectiveness coefficient used without a Threat Rating Node. | https://modenc.renegadeprojects.com/The_Threat_System | Community | Added Global Float canonical row; AI / Techno rows changed to guardrails. | AI / Techno rows are non-canonical. |
| DumbTargetSpecialThreatCoefficient | Global | Threat-system coefficient applied to targets with `SpecialThreatValue=1`. | https://modenc.renegadeprojects.com/DumbTargetSpecialThreatCoefficient | Community | Added Global Float canonical row; AI / Techno rows changed to guardrails. | AI / Techno rows are non-canonical. |
| DumbTargetStrengthCoefficient | Global | Threat-system coefficient applied when the target has higher strength than the evaluator. | https://modenc.renegadeprojects.com/DumbTargetStrengthCoefficient | Community | Added Global Float canonical row; AI / Techno rows changed to guardrails. | AI / Techno rows are non-canonical. |
| DumbTargetDistanceCoefficient | Global | Threat-system coefficient applied when the target is outside weapon range. | https://modenc.renegadeprojects.com/DumbTargetDistanceCoefficient | Community | Added Global Float canonical row; AI / Techno rows changed to guardrails. | AI / Techno rows are non-canonical. |

## 4. JSON Patch Summary

```text
Total fields after this phase: 4643
Exact low-quality `数值型字段` descriptions after this phase: 0
Placeholder rows after this phase: 2459
Short generic low-quality labels after this phase: 609
```

Changes:

- 14 `[AI]` Ratio / Limit rows now use explicit source-backed Chinese descriptions and proper `Float` / `Integer` editor kinds.
- 14 existing `Global` rough rows for those Ratio / Limit keys were changed to non-canonical guardrails, because sources place those flags in `[AI]`.
- 14 `Techno` placeholder rows for those Ratio / Limit keys were changed to non-canonical guardrails.
- 5 new `[Global]` canonical rows were added for `Dumb*Coefficient` threat-system fields.
- 5 `[AI]` `Dumb*Coefficient` rows and 5 `Techno` `Dumb*Coefficient` rows were changed to non-canonical guardrails.

## 5. Safety Notes

- This phase intentionally did not touch unrelated placeholder rows such as `Owner / AI`, `Prerequisite / AI`, or `Sight / AI`.
- `AirstripRatio` and `AirstripLimit` were also left for a future AI-base-composition continuation batch because they were not part of the original direct `数值型字段` set.
- The existing provider priority and runtime lookup semantics remain unchanged.

## 6. Next Step

Recommended next phase:

```text
FR-DQ-2F-AI-LowQuality-Continue or FR-DQ-2G-HighFrequency-Techno-Placeholder-Candidates
```

Continue with another small, source-verified batch. Do not mass-replace remaining placeholders with guessed text.
