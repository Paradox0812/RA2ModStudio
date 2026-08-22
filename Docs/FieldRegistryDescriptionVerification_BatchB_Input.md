# Field Registry Description Verification Input - Batch B

Phase: FR-DQ-2C-Prep Batch B Verification Slice

This document prepares verification input for Batch B from `Docs/FieldRegistryEffectiveDescriptionAudit.md`, `Docs/FieldRegistryDescriptionBackfill_P0A_Candidates.md`, and `Docs/FieldRegistryDescriptionSourcePolicy.md`.

It does not modify Field Registry JSON, provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, AI, UI, project files, or legacy files.

## 1. Scope

Batch B covers Techno fallback and unit behavior gaps:

```text
BuildCat
Crewed
Turret
ThreatPosed
```

This phase is only a verification input slice. It does not contain final canonical descriptions and must not be used as a JSON patch plan.

## 2. Verification Rules

- Start from effective runtime audit results, not raw JSON missing rows.
- Do not backfill rows whose effective description is already valid.
- Do not copy placeholder text into any final description.
- Treat broad fallback rows as review inputs only.
- Keep `SourceTrust = Unknown` until online/source verification is completed.
- If a field has different meanings in Building, Techno, Vehicle, Infantry, Aircraft, Unit, or AI contexts, verify those contexts separately.

## 3. Batch B Verification Input

| Key | Effective SectionKind / Schema | Effective Source | Effective Description Status | Current Effective Description | Problem Type | Suggested Verification Source | Proposed Source Trust | Notes | Canonical target suggestion | DoNotApplyTo recommendation |
|---|---|---|---|---|---|---|---|---|---|---|
| BuildCat | Building | Global: User Import / User | Valid | 建筑在建造栏或 AI 建造逻辑中所属的分类。 | Effective-valid exclusion | Not needed for this phase | LocalImported | Effective audit marks `BuildCat / Building` as valid and `Needs Backfill = No`; do not re-add it as a patch target merely because raw rows may have been missing. | No new canonical target; existing Building effective description is already usable. | Do not overwrite `BuildCat / Building` during Batch B backfill prep. |
| BuildCat | Aircraft | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：BuildCat。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder broad fallback | ModEnc / RA2-YR docs | Unknown | Effective lookup exposes a Techno-style placeholder for Aircraft through fallback; this is not a proven canonical target. | Review only; prepare an explicit Aircraft target plan only if source verification confirms this context. | Do not apply Building wording or generic Techno wording directly to Aircraft. |
| BuildCat | Infantry | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：BuildCat。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder broad fallback | ModEnc / RA2-YR docs | Unknown | Effective lookup exposes a Techno-style placeholder for Infantry through fallback; this is not a proven canonical target. | Review only; prepare an explicit Infantry target plan only if source verification confirms this context. | Do not apply Building wording or generic Techno wording directly to Infantry. |
| BuildCat | Unit | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：BuildCat。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder broad fallback | ModEnc / RA2-YR docs | Unknown | Unit is an abstract/common object context; the current fallback text is not Hover-quality and is not a direct write target. | Review-only abstract target; no direct write in this phase. | Do not write Unit broad fallback text to Building, Aircraft, Infantry, Vehicle, or Techno. |
| BuildCat | Vehicle | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：BuildCat。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder broad fallback | ModEnc / RA2-YR docs | Unknown | Effective lookup exposes a Techno-style placeholder for Vehicle through fallback; this is not a proven canonical target. | Review only; prepare an explicit Vehicle target plan only if source verification confirms this context. | Do not apply Building wording or generic Techno wording directly to Vehicle. |
| BuildCat | Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：BuildCat。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder fallback row | ModEnc / RA2-YR docs | Unknown | Candidate audit says this needs official wording for Techno/Unit fallback use; it is not yet a safe patch target. | Possible `Techno` target only after source verification proves the field belongs there. | Do not apply the existing Building description to Techno without separate verification. |
| Crewed | Building | BuiltIn / effective valid through existing registry data | Valid | 建筑摧毁时是否生成乘员步兵。 | Effective-valid exclusion | Not needed for this phase | LocalImported | Candidate list explicitly excludes `Crewed / Building` because the effective description is valid. | No new canonical target; existing Building effective description is already usable. | Do not overwrite `Crewed / Building` while preparing Techno/object fallback verification. |
| Crewed | Techno | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Crewed。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local/fallback row | ModEnc / RA2-YR docs | Unknown | Keep Techno separate from Building and object-specific contexts. Do not assume the Building destruction wording is valid for Techno. | Possible `Techno` target only after source verification. | Do not apply Building, Vehicle, Infantry, Aircraft, or Unit wording to Techno without proof. |
| Crewed | Vehicle | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Crewed。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local/fallback row | ModEnc / RA2-YR docs | Unknown | Vehicle context needs separate source verification; it must not be collapsed into Building or Techno. | Possible `Vehicle` target only after source verification. | Do not apply Building or generic Techno fallback wording to Vehicle. |
| Crewed | Infantry | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Crewed。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local/fallback row | ModEnc / RA2-YR docs | Unknown | Infantry context needs separate verification; current text is only a placeholder. | Possible `Infantry` target only after source verification. | Do not apply Building, Vehicle, Aircraft, or generic Techno fallback wording to Infantry. |
| Crewed | Aircraft | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Crewed。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local/fallback row | ModEnc / RA2-YR docs | Unknown | Aircraft context needs separate verification; current text is only a placeholder. | Possible `Aircraft` target only after source verification. | Do not apply Building, Vehicle, Infantry, or generic Techno fallback wording to Aircraft. |
| Crewed | Unit | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Crewed。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local/fallback row | ModEnc / RA2-YR docs | Unknown | Unit is an abstract/common object context; verify before treating it as a canonical target. | Review-only abstract target; no direct write in this phase. | Do not write Unit broad fallback text to Building, Vehicle, Infantry, Aircraft, or Techno. |
| Turret | Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder fallback row | ModEnc / RA2-YR docs | Unknown | Keep Techno fallback separate from Vehicle and Building; current text is not Hover-quality. | Possible `Techno` target only after source verification. | Do not apply Vehicle, Building, Infantry, Aircraft, or Unit wording to Techno without source proof. |
| Turret | Vehicle | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local row | ModEnc / RA2-YR docs | Unknown | Vehicle is a likely context to verify separately, but this document does not assert final semantics. | Possible `Vehicle` target only after source verification. | Do not apply Techno fallback, Building, Infantry, Aircraft, or Unit wording to Vehicle without verification. |
| Turret | Building | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local row | ModEnc / RA2-YR docs | Unknown | Building must stay separate from Vehicle and Techno because turret behavior may differ by object type. | Possible `Building` target only after source verification. | Do not apply Vehicle or generic Techno fallback wording to Building without verification. |
| Turret | Infantry | Global: User Import / User | Placeholder | BuiltIn-style placeholder text preserved in local row: YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder local row | ModEnc / RA2-YR docs | Unknown | Infantry row is a separate local placeholder; do not infer semantics from Vehicle or Building. | Possible `Infantry` target only after source verification. | Do not apply Vehicle, Building, Aircraft, Unit, or generic Techno wording to Infantry. |
| Turret | Aircraft | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder broad fallback | ModEnc / RA2-YR docs | Unknown | Aircraft appears through BuiltIn fallback; treat as review-only until source verification confirms context meaning. | Possible `Aircraft` target only after source verification. | Do not apply Vehicle, Building, Infantry, Unit, or generic Techno wording to Aircraft. |
| Turret | Unit | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | Placeholder | YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder broad fallback | ModEnc / RA2-YR docs | Unknown | Unit is an abstract/common object context; this row should not become a broad write target before a modeling decision. | Review-only abstract target; no direct write in this phase. | Do not write Unit broad fallback text to Vehicle, Building, Infantry, Aircraft, or Techno. |
| ThreatPosed | Aircraft | Global: User Import / User | Valid | AI 和自动索敌评估中使用的威胁值；纯防空或附属对象通常应较低。 | Effective-valid exclusion | Not needed for this phase | LocalImported | Effective audit marks `ThreatPosed / Aircraft` as valid and `Needs Backfill = No`; do not re-add it as a patch target. | No new canonical target for this context. | Do not overwrite this context during Batch B unless later source verification proves the current effective description is wrong. |
| ThreatPosed | Building | Global: User Import / User | Valid | AI 和自动索敌评估中使用的威胁值；纯防空或附属对象通常应较低。 | Effective-valid exclusion | Not needed for this phase | LocalImported | Effective audit marks `ThreatPosed / Building` as valid and `Needs Backfill = No`; do not re-add it as a patch target. | No new canonical target for this context. | Do not overwrite this context during Batch B unless later source verification proves the current effective description is wrong. |
| ThreatPosed | Infantry | Global: User Import / User | Valid | AI 和自动索敌评估中使用的威胁值；纯防空或附属对象通常应较低。 | Effective-valid exclusion | Not needed for this phase | LocalImported | Effective audit marks `ThreatPosed / Infantry` as valid and `Needs Backfill = No`; do not re-add it as a patch target. | No new canonical target for this context. | Do not overwrite this context during Batch B unless later source verification proves the current effective description is wrong. |
| ThreatPosed | Techno | Global: User Import / User | Valid | AI 和自动索敌评估中使用的威胁值；纯防空或附属对象通常应较低。 | Effective-valid exclusion | Not needed for this phase | LocalImported | Effective audit marks `ThreatPosed / Techno` as valid and `Needs Backfill = No`; do not re-add it as a patch target. | No new canonical target for this context. | Do not overwrite this context during Batch B unless later source verification proves the current effective description is wrong. |
| ThreatPosed | Vehicle | Global: User Import / User | Valid | AI 和自动索敌评估中使用的威胁值；纯防空或附属对象通常应较低。 | Effective-valid exclusion | Not needed for this phase | LocalImported | Effective audit marks `ThreatPosed / Vehicle` as valid and `Needs Backfill = No`; do not re-add it as a patch target. | No new canonical target for this context. | Do not overwrite this context during Batch B unless later source verification proves the current effective description is wrong. |
| ThreatPosed | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | LowQuality | 数值型字段 | LowQuality value-type label | ModEnc / RA2-YR docs | Unknown | This is a generic type label, not field documentation. It is also tracked in Batch D AI context gaps. | Possible `AI` target only after AI-context source verification. | Do not copy the common object description to AI unless a verified source confirms the AI context has the same meaning. |

## 4. Context Classification Summary

Canonical candidates after future verification:

- `BuildCat / Techno` only if source verification proves a Techno or Unit fallback meaning beyond the already-valid `Building` row.
- `Crewed / Techno`, `Crewed / Vehicle`, `Crewed / Infantry`, `Crewed / Aircraft`, and possibly `Crewed / Unit`, each verified separately.
- `Turret / Techno`, `Turret / Vehicle`, `Turret / Building`, `Turret / Infantry`, `Turret / Aircraft`, and possibly `Turret / Unit`, each verified separately.
- `ThreatPosed / AI` only after AI-context verification.

Contexts that are explicitly not Batch B backfill targets:

- `BuildCat / Building`, because the effective description is already valid.
- `Crewed / Building`, because the effective description is already valid.
- `ThreatPosed / Aircraft`, `ThreatPosed / Building`, `ThreatPosed / Infantry`, `ThreatPosed / Techno`, and `ThreatPosed / Vehicle`, because common object contexts are already valid.
- Any broad fallback row where the only current text is a placeholder and no online/source verification has confirmed that section-kind meaning.

## 5. Next Step

Recommended next phase:

```text
FR-DQ-2C-Verify: online/source verification for Batch B rows.
```

The verification phase should collect source-backed wording and classify trust before any JSON patch plan is prepared.

## 6. Completion Checklist

- Batch B keys covered: BuildCat, Crewed, Turret, ThreatPosed.
- Combined context rows split: Yes.
- BuildCat combined row split into: Aircraft, Infantry, Unit, Vehicle.
- ThreatPosed combined row split into: Aircraft, Building, Infantry, Techno, Vehicle.
- Crewed contexts reviewed as separate rows: Building, Techno, Vehicle, Infantry, Aircraft, Unit.
- Turret contexts reviewed as separate rows: Techno, Vehicle, Building, Infantry, Aircraft, Unit.
- JSON modified: No.
- Source/runtime/UI modified: No.
- Source verification completed: No.
- Patch plan created: No.
- Ready for next phase: FR-DQ-2C-Verify.
