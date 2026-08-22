# Codex Task: RA2IniEditor.IDE FR-DQ-1 Description Source Policy and FR-DQ-2A Backfill Candidate Preparation

## 0. Current Baseline

FR-DQ-0B has been completed.

Reported state:

```text
Docs/FieldRegistryEffectiveDescriptionAudit.md created.
FR-DQ-0A raw missing list is confirmed to contain false positives.
Effective audit statistics:
- Valid: 249
- Missing: 111
- Placeholder: 75
- LowQuality: 12
- Total: 447
Name / Infantry is confirmed as a raw-list false positive with valid effective Global description.
Project active pack is absent in the audited workspace.
No Field Registry JSON / source / UI / provider behavior changed.
```

Next phase:

```text
FR-DQ-1: Description Source / Trust Policy
FR-DQ-2A: Effective P0 Backfill Candidate Preparation
```

This is still a documentation / candidate preparation phase.

Do not write Field Registry JSON yet.

---

## 1. Goal

Prepare a verified and reviewable backfill candidate list based on effective missing / placeholder / low-quality P0 fields only.

Do not use the raw missing list as the patch source.

Use `Docs/FieldRegistryEffectiveDescriptionAudit.md` as the source of truth.

---

## 2. Hard Boundaries

Do not modify:

```text
Field Registry JSON
Field Registry provider priority
Field Registry loader/writer/apply/rollback/cleanup behavior
Hover / Quick Peek / AI Evidence code
Parser / diagnostics / completion / save preflight
XAML / UI
AI provider behavior
project / solution files
legacy files
```

Do not fabricate official descriptions.

Do not write back to BuiltIn v3.2 in this phase.

---

## 3. Required Inputs

Read:

```text
Docs/FieldRegistryEffectiveDescriptionAudit.md
Docs/FieldRegistryMissingDescriptionList.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
```

Use effective audit status, not raw audit status.

---

## 4. Required Output Files

Create:

```text
Docs/FieldRegistryDescriptionSourcePolicy.md
Docs/FieldRegistryDescriptionBackfill_P0A_Candidates.md
```

---

## 5. Source Policy Requirements

`Docs/FieldRegistryDescriptionSourcePolicy.md` must define trust levels:

| Trust | Meaning |
|---|---|
| Official | Official Westwood / Ares / Phobos documentation or confirmed project docs. |
| Community | ModEnc or reputable community documentation. |
| Derived | Inferred from field usage / examples, must be marked as derived. |
| LocalImported | Existing user-import/global active field description. |
| Unknown | Not verified; must not be treated as authoritative. |

Rules:

```text
1. Official / Community descriptions may be used as normal Hover text after review.
2. Derived descriptions must include a caution note or lower trust marker.
3. Unknown fields must remain missing / verify-before-use.
4. LowQuality existing descriptions should not be copied forward unchanged.
5. If source text is unclear, keep the field in review status.
```

---

## 6. Candidate List Requirements

`Docs/FieldRegistryDescriptionBackfill_P0A_Candidates.md` should include only rows from `FieldRegistryEffectiveDescriptionAudit.md` where:

```text
Needs Backfill = Yes
and Effective Description Status is Missing / Placeholder / LowQuality
and Priority is P0 or listed under P0 effective themes.
```

Do not include rows where:

```text
Effective Description Status = Valid
Needs Backfill = No
```

Example false positives that must not be re-added as backfill candidates:

```text
Name / Infantry
Armor / common object contexts
Cost / common object contexts
Owner / common object contexts
Primary / common object contexts
UIName / common object contexts
```

---

## 7. Candidate Table Columns

For each candidate row include:

```text
Key
SectionKind / Schema
Effective Source
Effective Description Status
Current Effective Description
Problem Type: Missing / Placeholder / LowQuality
Suggested Verification Source: ModEnc / Ares Docs / Phobos Docs / Unknown
Proposed Source Trust: Official / Community / Derived / Unknown
SuggestedDescriptionZh
NeedsOnlineVerification: true / false
ReadyToApply: false
Notes
```

At this phase, `SuggestedDescriptionZh` may be blank or contain a rough placeholder such as:

```text
待联网核验后填写
```

Do not generate final descriptions unless a source has already been confirmed.

---

## 8. Prioritization

Group candidates into batches:

### Batch A: Combat / Weapon / Warhead basics

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

### Batch B: Techno fallback and unit behavior gaps

```text
BuildCat
Crewed
Turret
ThreatPosed
```

### Batch C: Non-canonical / Unknown fallback gaps

```text
Name low-quality fallback
Strength unknown fallback
Sight unknown fallback
Locomotor unknown fallback
Projectile unknown fallback
Verses unknown fallback
PercentAtMax unknown fallback
```

### Batch D: AI context gaps

```text
Owner / AI
Prerequisite / AI
Sight / AI
ThreatPosed / AI
```

---

## 9. Tests / Validation

Documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing, run full validation.

---

## 10. Acceptance Criteria

This phase is accepted when:

```text
1. Source trust policy is documented.
2. Backfill candidate list is based on effective audit, not raw audit.
3. Raw-list false positives are excluded.
4. Candidates are grouped into reviewable batches.
5. No JSON/runtime behavior is changed.
6. Candidate descriptions are not fabricated.
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed.
2. Files changed.
3. Source trust policy summary.
4. Candidate batches created.
5. Number of candidates by status / batch.
6. Explicit false positives excluded.
7. Commands run.
8. Test/package result.
9. Confirmation no Field Registry JSON/runtime code changed.
10. Recommended next phase: online verification for Batch A.
```
