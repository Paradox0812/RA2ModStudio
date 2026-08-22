# Field Registry Description Verification - Art Animation Core

Phase: FR-DQ-2V-ArtAnimationCore-BigBatch-ManualApply

## 1. Scope

This batch verifies Art / Animation core Hover descriptions and same-domain Phobos Animation extensions in BuiltIn v3.2.

Processed families:

- art(md).ini Image / Theater resource selection.
- Animation playback and looping: Start, End, LoopStart, LoopEnd, LoopCount, Rate, RandomRate, Normalized, Next.
- Trailer and spawn chains: Trailer, TrailerAnim, TrailerSeperation, Spawns, SpawnCount.
- Animation damage / warhead / report / shadow harmonization.
- Visual flags: Translucent, UseNormalLight, AltPalette, AnimPalette.
- Phobos Animation rows: VisibleTo, RestrictVisibilityIfCloaked, DetachOnCloak, AttachedAnimPosition, AttachedSystem, Layer.UseObjectLayer, HideIfNoOre.Threshold, CreateUnit.*, Damage.*, fire animation spawning rows, SplashAnims / WakeAnim / Warhead.Detonate.

This phase does not modify provider priority, provider lookup, Hover code, Quick Peek, AI Evidence, parser, diagnostics, completion, save preflight, XAML/UI, project files, user Global active pack, or legacy files.

## 2. Sources

- ModEnc Image page.
- ModEnc Theater page.
- ModEnc Animation Looping page.
- ModEnc Normalized page.
- ModEnc TrailerAnim and TrailerSeperation pages.
- ModEnc SpawnCount page.
- ModEnc Translucent, UseNormalLight, AltPalette, AnimPalette, and Shadow pages.
- Phobos New / Enhanced Logics: attached particle system and customizable animation visibility settings.
- Phobos Fixed / Improved Logics: attached animation position, fire animations spawned by Scorch & Flamer, layer on attached animations, ore threshold, debris / meteor impact and animation damage settings.

## 3. Result Summary

```text
BuiltIn v3.2 field count: 5055 -> 5069
Rows affected: 107
New exact/context rows: 14
Updated / guarded existing rows: 93
Source-verified rows: 1212 -> 1312
Strict non-source-verified rows: 3843 -> 3757
Direct placeholder rows: 2115 -> 2073
Exact integer generic rows: 80 -> 78
Exact numeric generic rows: 0 -> 0
Direct Hover-risk placeholder/generic rows: 2195 -> 2151
```

## 4. New Exact / Context Rows

```text
Image / Animation
Image / ArtObject
Theater / ArtObject
Theater / Animation
Start / Animation
End / Animation
End / ArtObject
RandomRate / Animation
Trailer / Animation
TrailerAnim / Animation
TrailerSeperation / Animation
SpawnCount / Animation
Spawns / Animation
AnimPalette / Projectile
```

## 5. Updated Source-backed Rows

Representative rows:

```text
Normalized / Animation, ArtObject
LoopStart / Animation, ArtObject
LoopEnd / Animation, ArtObject
LoopCount / Animation, ArtObject
Rate / Animation, ArtObject
TrailerAnim / ArtObject
TrailerSeperation / ArtObject
SpawnCount / ArtObject
Translucent / Animation, ArtObject
UseNormalLight / Animation, ArtObject
AltPalette / Animation, ArtObject
Next / Animation, ArtObject
```

Phobos Animation extension rows were also updated for:

```text
AttachedAnimPosition
AttachedSystem
CreateUnit.*
Damage.*
VisibleTo*
RestrictVisibilityIfCloaked
DetachOnCloak
AttachFireAnimsToParent
ConstrainFireAnimsToCellSpots
FireAnimDisallowedLandTypes
SmallFire* / LargeFire*
SplashAnims*
WakeAnim
Warhead.Detonate
```

## 6. Guardrail Rows

Wrong-context rows were not deleted. They were converted to conservative guardrail descriptions where appropriate, including:

```text
Normalized / Global
Normalized / Techno
Rate / Techno
TrailerAnim / Techno
SpawnCount / Techno
Translucent / Techno
AIAutoDeployMCV / Animation
AIBiasSpawnCell / Animation
AICleanWallNode / Animation
AIForbidConYard / Animation
AINodeWallsOnly / Animation
AISetBaseCenter / Animation
```

The AI base construction rows are intentionally guarded because their Phobos source category is AI base construction, not AnimationType behavior.

## 7. Validation

Static validation completed:

```text
JSON parse: passed
Exact key/appliesTo duplicate check: passed
Target row validation: passed
Target bad placeholder rows: 0
Expected verification doc: present
Clean package validation: passed
```

`dotnet restore`, `dotnet build`, and `dotnet test` were not run in the patch environment because dotnet CLI is unavailable.

## 8. Next Step

Recommended next phase:

```text
FR-DQ-2W-ArtAnimationPhobosCreateAndVisuals-BigBatch-ManualApply
```

Candidate focus:

```text
Remaining Phobos Animation / art rows that are not covered by this core batch, especially fire / debris / visibility / create-unit edge rows and any remaining Animation placeholders surfaced by the hover quality scan.
```
