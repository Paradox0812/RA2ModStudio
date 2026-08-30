# ASSET-VOX-1E Stage Result Ledger

Date: 2026-08-27  
Package: Natural-Language Style Profile and Palette Review  
State: Completed / automated verified / user visual review pending

## Stage status

| Stage | Delivered result | Verification | State |
|---|---|---|---|
| 1E-0 | Code-fact audit, R4 contract and self-review | audit/contract/decision gate reviewed | Completed |
| 1E-1 | bounded `VOXEL_STYLE.md` source pack, exact ancestor-chain resolution, scope/hash facts and bundled default | resolver focused 4/4 | Completed / verified |
| 1E-2 | one-call dedicated structured compiler, local typed plan compiler and bounded exact-key cache | Application plan 4/4; IDE style total 8/8 | Completed / verified |
| 1E-3 | deterministic geometry masks, fixed paint ordering, explicit-mask/remap rules and immutable recolour result | colourizer/codecs focused 4/4, then 6/6 with review package | Completed / verified |
| 1E-4 | path-free in-memory review package with safe JSON, palette/region/remap images, VOX and SliceStack | artifact/hash/failure matrices included in 6/6 | Completed / verified |
| 1E-5 | existing 1D Body candidate replay, golden review artifacts, full regressions and clean package | real candidate 1/1; full results below | Completed / verified |

## Delivered boundaries

- IDE owns contained style-source loading, dedicated AI orchestration and disposable `%LocalAppData%` cache.
- Application owns the immutable plan, palette resolution, geometry mask, colourization and in-memory review package.
- Existing 1B snapshot/palette/VOX/SliceStack code remains the sole voxel truth and codec authority.
- A successful colour operation changes palette indices only. Dimensions, part descriptor, coordinates and occupancy remain unchanged.
- Semantic glass/rubber/accent/remap painting requires an explicit reviewed mask. Text-only intent remains unresolved and visible.
- The package does not write a mod project, Apply/Save, generate VXL/HVA, produce normals or claim game readiness.

## Existing Body candidate acceptance

Input fixture: excluded `artifacts/asset-vox-1d-acceptance/p2-body-64/body-candidate.vox`  
Output directory: excluded `artifacts/asset-vox-1e-acceptance/p2-body-64/`

| Fact | Result |
|---|---|
| Source snapshot | `6741135AABF752C0A7DA53A6BE28FAF4F15445F0F1C325932A80C97BA6431DE8` |
| Result snapshot | `1693CB306125C1701B368DCCF8F2280534C96BE73F887DA792F162B3F876DA4A` |
| Style plan | `815ACD394C136F838EEE3C36F033DF87112D94C1DEA65CABF57960E3579736AB` |
| Occupancy | 20,261 before and after; geometry/occupancy equality passed |
| Replay | two local colourizations and two review packages produced identical hashes |
| Colour result | non-uniform; five applied geometry roles; maximum squared palette error 576 |
| Unresolved | `glass.unresolved` lacks an explicit mask and was not painted |
| Live DeepSeek | `NotRun` — explicitly outside this approval |
| User visual review | `NotRun` — generated artifacts are ready for separate review |

Generated review artifacts are exactly:

```text
style-source-pack.json
compiled-style-plan.json
colour-review-report.json
palette-swatch.png
region-mask.png
body-coloured.vox
body-coloured-slicestack.png
```

No `remap-mask.png` exists in the real acceptance case because the approved profile forbids remap. The focused explicit-mask
matrix separately proves the conditional remap artifact and remap-index boundary.

## Verification closeout

| Gate | Result |
|---|---|
| `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed; 0 errors, 1 pre-existing nullable warning in `BuiltInFieldRegistryPackLoaderTests.cs:1983` |
| Full Application tests | Passed 249/249 |
| Full IDE tests | Passed 2787/2787 |
| Full AssetHost tests | Passed 47/47 |
| Real-candidate acceptance | Passed 1/1; no remote call |
| Application public allowlist | unchanged at 77 |
| AssetHost exported public types | unchanged at 0 |
| IdeOnly clean package | recorded after final documentation flush |

## Deferred governance queue — flushed

| Kind | Entry | State / next gate |
|---|---|---|
| Public API | No candidate; all new types stay assembly-internal | Closed / zero change |
| Persistence | `VOXEL_STYLE.md` is authoring truth; cache v1 is disposable and fully keyed | Accepted / implemented |
| UI | style selector/editor, plan diff, swatches and mask/error overlays | Deferred to separately approved `ASSET-VOX-1E-UI` |
| Semantic masks | donor/image/material segmentation and mask authoring | Deferred; current text-only output cannot infer these regions |
| Final game assets | pivot/mount, normals, VXL/HVA, project Apply/Save and game smoke | Deferred; separate approval required |
| Product orchestration | invoking the dedicated compiler/colourizer from Work or asset UI | Deferred; no current UI entry point was added |

## Technical-debt register

| Debt ID | Area | Accepted limitation | Risk | Repayment trigger | State |
|---|---|---|---|---|---|
| VOX-1E-D01 | Semantic masks | text alone cannot locate glass, tyres, insignia or remap cells | coarse output requires human review | reliable donor/source-material facts or a reviewed mask editor exists | Open / intentional |
| VOX-1E-D02 | Colour metric | v1 keeps deterministic squared RGB for compatibility | nearest game-palette colour may be perceptually suboptimal | visual acceptance demonstrates repeatable mismatch | Open / intentional |
| VOX-1E-D03 | Product composition | core is headless and has no UI/Work entry point | capability is currently test/API-level only | approve `ASSET-VOX-1E-UI` or a separate asset orchestration slice | Open / intentional |

## Final stop point

The approved 1E-1 through 1E-5 package is complete. Automated correctness is verified; aesthetic acceptance remains a
human decision. Do not proceed into UI, a real DeepSeek style compile, project Apply/Save, VXL/HVA, normals or game
validation without the separate approval named by the user.
