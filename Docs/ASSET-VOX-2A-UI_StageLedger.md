# ASSET-VOX-2A-UI Stage Result Ledger

Date: 2026-08-27

State: completed / automated verified / manual visual gate pending

## Contract gate

- Code fact audit: completed.
- Final UI contract: self-reviewed.
- Risk: `R3 / StopForReview`.
- Implementation: UI-1 through UI-5 completed continuously after explicit approval.
- XAML: approved Voxel Style workspace changed. Shell/layout unchanged.

## Planned stages

| Stage | Scope | State |
|---|---|---|
| `UI-1` | candidate transaction, GLB admission and provenance | completed; coordinator tests 9/9 |
| `UI-2` | ViewModel candidate lifecycle and working geometry | completed; combined tests 11/11 |
| `UI-3` | style/contrast candidate composition | completed; combined tests 13/13 |
| `UI-4` | XAML review candidate surface and AutomationIds | completed; focused/visual tests 33/33 |
| `UI-5` | full verification, docs and manual gate handoff | completed; physical 1920x1080 review pending |

## Delivered behavior

- Explicit project-contained GLB quality source with Verified/UserPaired/Mismatch provenance.
- Local Direct, Refined and optional Symmetry candidates in the existing interactive 3D viewport.
- Explicit `用于本会话` geometry selection; baseline remains unchanged.
- Existing explicit style compile consumes the selected session geometry.
- Ordinary styled result remains authoritative; an optional deterministic contrast candidate is published separately.
- Quality metrics, normal comparison, semantic-region provenance and palette-contrast facts are visible without DataGrid.

## Verification ledger

| Gate | Result |
|---|---|
| UI-1 focused | 9/9 passed |
| UI-2 focused | 11/11 passed |
| UI-3 focused | 13/13 passed |
| UI-4 + visual boundary | 33/33 passed |
| Application full | 264/264 passed |
| IDE full | 2814/2814 passed |
| AssetHost full | 47/47 passed |
| IDE-only solution build | passed; 0 errors, 1 pre-existing nullable warning |
| IdeOnly clean source package | passed; 1366 files |
| Real DeepSeek/Tencent | NotRun by contract |
| Physical 1920x1080 interaction | pending user verification |

## Deferred governance queue

- Accepted session result materialization, Apply/Save/export and VXL/HVA remain separately approved work.
- Authoritative tyre/glass/weapon masks and multi-part Body/Turret/Barrel composition remain deferred.
- The UserPaired provenance state requires human judgment because no source hash can prove the pairing.

## Frozen gates

- real DeepSeek/Tencent calls;
- Shell/docking/layout;
- project Apply/Save/export;
- VXL/HVA writer;
- public API or persistence;
- source mesh editing and multipart generation.

## 2026-08-27 component-dominance correction

- The physical `body-candidate.vox + mesh.glb` review exposed that an absolute six-neighbour single-component gate
  classified one detached voxel as complete model fragmentation.
- Candidate admission now reuses canonical snapshot connectivity facts: one component passes, and a multi-component
  candidate is rejected unless one component contains at least 95% of occupied cells.
- The default downsample coverage was corrected from 50% to 40% using the same real product path. The resulting refined
  candidate is one component with 17,397 occupied cells; occupied-volume change is 4.94% and the maximum fixed-view
  silhouette change is 2.63%, so the existing 5%/3% safety gates were not relaxed.
- The UI quality projection now includes component count and dominant-body share. No XAML, Shell, provider, Apply/Save,
  VXL/HVA, public API or persistence boundary changed.
