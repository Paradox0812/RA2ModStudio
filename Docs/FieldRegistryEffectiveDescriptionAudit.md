# Field Registry Effective Description Audit

Phase: FR-DQ-0B Effective Field Description Audit

This is a read-only audit of P0 field descriptions after the runtime-style provider lookup path is applied. It does not modify Field Registry JSON, provider priority, Hover, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML, or runtime code.

## Scope

The FR-DQ-0A raw list reports JSON rows whose `description` is missing or placeholder-like. That raw list can contain false positives because the IDE does not display raw rows directly. Hover, Quick Peek, and AI Evidence use effective definitions after provider lookup, section-kind fallback, and built-in enrichment.

This audit re-checks the P0 fields through the effective provider/display path:

- `FieldRegistryRuntimeService`: composes Project > Global > BuiltIn providers.
- `CompositeRa2FieldDefinitionProvider`: selects the best section-kind match and can enrich weak local definitions from BuiltIn fallback definitions.
- `LocalRa2FieldDefinitionProvider`: resolves exact section kind, abstract section kind, Global, then Unknown.
- `Ra2FieldDisplayResolver`: passes the effective definition `Description` to Hover display.
- `Ra2HoverProvider`: displays `displayInfo.Note ?? displayInfo.Description`.
- `Ra2FieldQuickPeekService`: uses provenance first, then provider fallback; provenance also applies built-in enrichment.
- `Ra2FieldRegistryAiEvidenceProvider`: includes the effective definition description in advisory evidence.

## Data Sources

| Scope | Path | Status |
|---|---|---|
| BuiltIn | `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json` | Scanned |
| Global active | `C:\Users\PC\AppData\Roaming\RA2IniEditor\FieldRegistry\active\user-import.fields.json` | Scanned |
| Project active | `C:\Users\PC\Desktop\RA2Ini_IDE\.ra2inieditor\field-registry\active` | Directory missing |

Because no project active pack exists in this workspace, the effective audit is based on Global > BuiltIn for local overrides. If a user's project active pack exists, that pack can change effective descriptions because Project has higher priority than Global and BuiltIn.

## P0 Fields Audited

`Name`, `UIName`, `Image`, `Prerequisite`, `Primary`, `Secondary`, `Strength`, `Armor`, `Speed`, `Sight`, `Cost`, `TechLevel`, `Owner`, `RequiredHouses`, `ForbiddenHouses`, `Category`, `BuildCat`, `Trainable`, `Turret`, `Crusher`, `Crewed`, `Locomotor`, `MovementZone`, `ThreatPosed`, `Damage`, `ROF`, `Range`, `Projectile`, `Warhead`, `Verses`, `CellSpread`, `PercentAtMax`, `AA`, `AG`.

Audited section kinds included common effective lookup contexts: `Infantry`, `Vehicle`, `Aircraft`, `Building`, `Unit`, `Techno`, `Weapon`, `Projectile`, `Warhead`, `ArtObject`, `Animation`, `VoxelAnimation`, `SuperWeapon`, `Country`, `Side`, `AI`, `Global`, and `Unknown`.

## Effective Status Summary

| Effective Description Status | Effective lookup rows |
|---|---:|
| Valid | 249 |
| Missing | 111 |
| Placeholder | 75 |
| LowQuality | 12 |
| Total | 447 |

Interpretation:

- `Valid`: the effective description is non-empty and does not match the placeholder / low-quality patterns used in this audit.
- `Missing`: the effective definition exists but has no effective description.
- `Placeholder`: the effective text still contains placeholder wording such as `原始英文说明`, `不直接用于 Hover`, or equivalent markers.
- `LowQuality`: the effective text is non-empty but is too vague or clearly non-reference prose, for example `Great Britain 说明文字,随便添,没什么意义`, numeric-only text, or generic `数值型字段`.

## Effective Rows Needing Backfill

Rows below are grouped when the effective source, description, status, raw-missing state, and backfill decision are the same. The `SectionKind / Schema` column therefore may contain multiple section kinds.

| Key | SectionKind / Schema | Raw Source Pack(s) | Effective Source | Effective Description | Effective Description Status | Was In Raw Missing List | Needs Backfill | Notes |
|---|---|---|---|---|---|---|---|---|
| AA | Aircraft, Building, Infantry, Unit, Vehicle | (lookup fallback only) | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：AA。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | No | Yes | Effective Techno fallback still contains placeholder text; verify against ModEnc / official docs before backfill. |
| AA | Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：AA。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Direct Techno row is still placeholder. |
| AA | AI, Animation, ArtObject, Country, Global, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import where exact Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Unknown/global fallback is empty; avoid treating as reliable Hover text. |
| AG | Aircraft, Building, Infantry, Unit, Vehicle | (lookup fallback only) | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：AG。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | No | Yes | Effective Techno fallback still contains placeholder text. |
| AG | Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：AG。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Direct Techno row is still placeholder. |
| AG | AI, Animation, ArtObject, Country, Global, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import where exact Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Unknown/global fallback is empty. |
| BuildCat | Aircraft, Infantry, Unit, Vehicle | (lookup fallback only) | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：BuildCat。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | No | Yes | Building effective row is valid through Global; Techno fallback remains placeholder. |
| BuildCat | Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：BuildCat。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Needs official wording for Techno/Unit fallback use. |
| CellSpread | Aircraft, Building, Infantry, Unit, Vehicle | (lookup fallback only) | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：CellSpread。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | No | Yes | Warhead-specific meaning is not captured by this effective Techno fallback. |
| CellSpread | Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：CellSpread。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Placeholder. |
| CellSpread | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import where exact Animation/Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Missing effective text for non-Techno fallback contexts. |
| Crewed | Aircraft, Infantry, Techno, Unit, Vehicle | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2; Global: User Import where exact local rows exist | Global: User Import / User | YR 内置参考字段：Crewed。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Global effective description is still inherited placeholder text. |
| Damage | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 where exact Techno exists; otherwise lookup fallback only | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Damage。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Weapon context was not the only effective context; Techno fallback still unsafe. |
| Damage | ArtObject | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Damage。适用于 Art 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Placeholder in ArtObject context. |
| Locomotor | AI, Animation, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import where exact Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Vehicle/Infantry/Aircraft effective rows are valid, but Unknown fallback is empty. |
| Name | Animation, ArtObject, Country, Projectile, Side, Warhead, Weapon, Global, SuperWeapon, Unknown, VoxelAnimation | Global: User Import where exact Global/Unknown and BuiltIn raw rows exist; otherwise lookup fallback only | Global: User Import / User | Great Britain 说明文字,随便添,没什么意义 | LowQuality | Mixed | Yes | This is the explicit low-quality example; do not use it as a Hover-quality field description. |
| Owner | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Owner。适用于 AI 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Unit/Techno rows are valid through Global; AI context remains placeholder. |
| PercentAtMax | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn exact Techno or lookup fallback | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：PercentAtMax。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Warhead semantics need official verification; effective Techno fallback is placeholder. |
| PercentAtMax | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import where exact Animation/Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Missing effective text for non-Techno fallback contexts. |
| Prerequisite | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Prerequisite。适用于 AI 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Techno/unit rows are valid through Global; AI context remains placeholder. |
| Projectile | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn exact Techno or lookup fallback | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Projectile。适用于 Techno 类型配置，值类型为 引用。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Weapon field meaning needs direct Weapon-context wording. |
| Projectile | AI, Animation, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Warhead, Unknown | Global: User Import where exact Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Missing effective Unknown fallback text. |
| Range | AI, Aircraft, Animation, ArtObject, Building, Country, Global, Infantry, Projectile, Side, Techno, Unit, Vehicle, VoxelAnimation, Warhead, SuperWeapon, Unknown | Global: User Import where exact SuperWeapon/Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Important P0 weapon field still lacks effective description in this environment. |
| ROF | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn exact Techno or lookup fallback | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：ROF。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Unknown row has a non-empty local value-like text, but canonical effective Techno fallback remains placeholder. |
| Sight | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Sight。适用于 AI 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Unit rows are valid through Global; AI context remains placeholder. |
| Sight | Animation, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Warhead, Weapon, Unknown | Global: User Import where exact Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Unknown fallback empty. |
| Strength | AI, Animation, ArtObject, Country, Global, Side, SuperWeapon, VoxelAnimation, Warhead, Weapon, Unknown | Global: User Import where exact Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Unit rows are valid through Global; Unknown fallback empty. |
| ThreatPosed | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | 数值型字段 | LowQuality | No | Yes | Generic field-type label, not enough for Hover-quality reference text. |
| Turret | Aircraft, Unit, Techno | BuiltIn exact Techno or lookup fallback | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Building/Infantry/Vehicle effective rows also use placeholder via Global. |
| Turret | Building, Infantry, Vehicle | Global: User Import | Global: User Import / User | YR 内置参考字段：Turret。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Local row currently preserves placeholder text. |
| Verses | Aircraft, Building, Unit, Vehicle, Techno | BuiltIn exact Techno or lookup fallback | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Verses。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Warhead semantics should be verified; current effective fallback is placeholder. |
| Verses | AI, ArtObject, Country, Global, Projectile, Side, SuperWeapon, VoxelAnimation, Animation, Unknown | Global: User Import where exact Animation/Unknown exists; otherwise lookup fallback only | Global: User Import / User | (empty) | Missing | Mixed | Yes | Missing effective text outside canonical contexts. |
| Verses | Infantry | Global: User Import | Global: User Import / User | YR 内置参考字段：Verses。适用于 Techno 类型配置，值类型为 文本。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Local Infantry row is placeholder. |
| Warhead | Aircraft, Building, Infantry, Unit, Vehicle, Techno | BuiltIn exact Techno or lookup fallback | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Warhead。适用于 Techno 类型配置，值类型为 引用。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Mixed | Yes | Weapon field meaning needs direct Weapon-context wording. |
| Warhead | ArtObject | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | YR 内置参考字段：Warhead。适用于 Art 类型配置，值类型为 引用。原始英文说明已移至复核表，不直接用于 Hover | Placeholder | Yes | Yes | Placeholder in ArtObject context. |

## Raw Missing False Positives

These rows appeared in the raw JSON missing/placeholder export, but the effective description used by the IDE is valid after provider composition, section-kind fallback, or enrichment. They should not be backfilled merely because FR-DQ-0A listed a raw row.

| Key | SectionKind / Schema | Raw Source Pack(s) | Effective Source | Effective Description | Effective Description Status | Was In Raw Missing List | Needs Backfill | Notes |
|---|---|---|---|---|---|---|---|---|
| Armor | Aircraft, Building, Infantry, Vehicle | Global/BuiltIn raw rows include missing entries | Global: User Import / User | 对象使用的装甲类型；Warhead 的 Verses 会按该类型计算伤害倍率。 | Valid | Yes | No | Raw missing false positive. |
| Armor | Unknown | Global raw row was missing | Global: User Import / User | 装甲类型；决定弹头 Verses 对该对象造成的伤害倍率 | Valid | Yes | No | Effective Unknown description is valid. |
| BuildCat | Building | Global raw row was missing | Global: User Import / User | 建筑在建造栏或 AI 建造逻辑中所属的分类。 | Valid | Yes | No | Building effective row is valid. |
| Category | Aircraft, Infantry, Unit, Vehicle | Global raw rows were missing | Global: User Import / User | 对象的 AI/威胁分类，例如 Infantry、AFV、Support、LRFS 等。 | Valid | Yes | No | Raw missing false positive. |
| Cost | Aircraft, Building, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象的建造价格。 | Valid | Yes | No | Raw missing false positive. |
| Crusher | Vehicle | Global raw row was missing | Global: User Import / User | 该对象是否可以碾压 Crushable=yes 的对象。 | Valid | Yes | No | Raw missing false positive. |
| ForbiddenHouses | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 列出的国家/阵营不能使用该对象。 | Valid | Yes | No | Effective P0 faction gating text is usable. |
| Image | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 指定该对象使用的图像、SHP、VXL 或 Art 资源条目。 | Valid | Yes | No | Effective main object contexts are valid. |
| Locomotor | Aircraft, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象使用的 locomotor GUID，决定移动行为。 | Valid | Yes | No | Raw missing false positive. |
| MovementZone | Aircraft, Infantry, Unit, Vehicle | Global raw rows were missing | Global: User Import / User | 对象可移动的地形区域类型。 | Valid | Yes | No | Raw missing false positive. |
| Name | Aircraft, Building, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象在编辑器或游戏中显示的名称文本；通常配合 UIName 或内部 ID 使用。 | Valid | Yes | No | This confirms the user-observed `Name / Infantry` Hover description is effective and should not be backfilled from raw-missing status. |
| Owner | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 允许拥有或建造该对象的国家/阵营列表。 | Valid | Yes | No | Effective common object contexts are valid. |
| Prerequisite | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 建造该对象所需的前置建筑或特殊前置条件，通常为逗号分隔列表。 | Valid | Yes | No | Raw missing false positive. |
| Primary | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 对象使用的主武器，指向一个 Weapon section。 | Valid | Yes | No | Effective common object contexts are valid. |
| RequiredHouses | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 只有列出的国家/阵营可以使用该对象。 | Valid | Yes | No | Effective P0 faction gating text is usable. |
| Secondary | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 对象使用的副武器，指向一个 Weapon section。 | Valid | Yes | No | Effective common object contexts are valid. |
| Sight | Aircraft, Building, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象可揭示地图和发现目标的视野范围。 | Valid | Yes | No | Raw missing false positive. |
| Speed | Aircraft, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象的移动速度。 | Valid | Yes | No | Raw missing false positive. |
| Strength | Aircraft, Building, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象的生命值或耐久度。 | Valid | Yes | No | Raw missing false positive. |
| TechLevel | Aircraft, Building, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象在建造栏中出现所需的科技等级；负值通常表示不可正常建造。 | Valid | Yes | No | Raw missing false positive. |
| ThreatPosed | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | AI 和自动索敌评估中使用的威胁值；纯防空或附属对象通常应较低。 | Valid | Yes | No | Common object contexts are valid; AI context is low quality separately. |
| Trainable | Aircraft, Building, Infantry, Techno, Vehicle | Global raw rows were missing | Global: User Import / User | 对象是否能通过经验获得等级和能力提升。 | Valid | Yes | No | Raw missing false positive. |
| UIName | Aircraft, Building, Infantry, Vehicle | Global raw rows were missing | Global: User Import / User | 对象的本地化名称标签，通常指向 CSF 文本，例如 Name:E1。 | Valid | Yes | No | Common object contexts are valid. |

## Low-Quality Effective Descriptions

These are not empty and do not necessarily contain the explicit placeholder markers, but they are not suitable as reliable Hover / Quick Peek / AI Evidence text.

| Key | SectionKind / Schema | Effective Source | Effective Description | Reason |
|---|---|---|---|---|
| Name | Animation, ArtObject, Country, Projectile, Side, Warhead, Weapon, Global, SuperWeapon, Unknown, VoxelAnimation | Global: User Import / User | Great Britain 说明文字,随便添,没什么意义 | Explicit low-quality sample text. |
| ThreatPosed | AI | BuiltIn: RA2/YR/Ares/Phobos BuiltIn fallback v3.2 / Yuri | 数值型字段 | Generic value-type label, not field documentation. |

## Effective Missing / Placeholder Themes

The P0 fields that still need later verification or backfill in at least one effective context are:

- Weapon / combat context: `Damage`, `ROF`, `Range`, `Projectile`, `Warhead`, `Verses`, `CellSpread`, `PercentAtMax`, `AA`, `AG`.
- Techno fallback context: `BuildCat`, `Crewed`, `Turret`.
- Non-canonical or Unknown fallback context: `Strength`, `Sight`, `Locomotor`, `Projectile`, `Verses`, `PercentAtMax`.
- AI context: `Owner`, `Prerequisite`, `Sight`, `ThreatPosed`.
- Explicit low-quality Global fallback: `Name`.

These are the P0 fields that should be prioritized for future online verification against ModEnc, Ares docs, Phobos docs, or an accepted source pack before any JSON backfill is attempted.

## Audit Conclusions

1. FR-DQ-0A is useful as a raw JSON hygiene list, but it is not safe as a direct backfill queue.
2. `Name / Infantry` is a confirmed raw-list false positive in this environment: the effective description is valid through `Global: User Import / User`.
3. Hover directly displays effective descriptions, so effective placeholder or low-quality text can surface in Hover when the lookup resolves to those definitions.
4. Quick Peek can also surface the same effective placeholder or low-quality text because provenance lookup applies the same enrichment pattern and then builds field details from the effective definition.
5. AI Evidence can surface the same effective description because evidence items include `definition.Description`.
6. Do not modify Field Registry priority or runtime lookup behavior to fix data-quality issues. The next phase should backfill verified descriptions into the appropriate pack rows, with Project > Global > BuiltIn priority preserved.

## No Runtime Changes

This audit did not modify:

- Field Registry JSON.
- Field Registry provider priority.
- Hover / Quick Peek / AI Evidence code.
- Parser, diagnostics, completion, or save preflight.
- XAML or UI behavior.
