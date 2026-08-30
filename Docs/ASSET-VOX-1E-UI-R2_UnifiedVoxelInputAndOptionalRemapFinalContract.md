# ASSET-VOX-1E-UI-R2 Unified Voxel Input and Optional Remap Final Contract

Date: 2026-08-27  
State: approved by the user's instruction to execute the proposed correction  
Risk: R3 / Immediate governance

## Outcome

The existing Voxel Style workspace accepts either a project-contained MagicaVoxel `.vox` source or a
project-contained Westwood `.vxl` source with an explicitly selected Westwood `.pal`. Both inputs reuse the canonical
Stage 1B readers and become one immutable `Ra2VoxelSceneSnapshot` before the unchanged Stage 1E style pipeline runs.

Ordinary colour styling never requires an RA2 remap range. When the active palette has no remap indices, the dedicated
model request must omit remap roles and preserve any textual team-colour intent only as an unresolved assumption. Local
validation remains authoritative and reports its precise bounded reason instead of the former generic palette message.
If the model nevertheless returns a remap role under `remap_policy=none` and every referencing rule is text-only,
the IDE adapter deterministically demotes those non-executable roles/rules into one unresolved assumption. Any explicit
mask, executable remap rule, remap interior role or active remap policy remains a validation failure.

Each model role selects exactly one colour source: a palette index or an RGB target. A redundant pair is normalized only
when the existing palette resolver proves that both values select the same eligible palette entry. Missing, conflicting,
duplicate or invalid roles remain failures and identify the exact reason instead of sharing one generic message.

## Input contract

- The current project remains the read boundary. Source and PAL files must be contained regular files with no reparse
  traversal.
- `.vox` continues through `Ra2MagicaVoxelCodec`; it needs no PAL and does not invent remap metadata.
- `.vxl` requires an explicit 768-byte Westwood PAL and uses `Ra2VxlseSliceImportContract.DecodeWestwoodPalette` plus
  `Ra2WestwoodVxlReader`.
- The workspace accepts exactly one decoded VXL Section. Multi-Section assets are rejected with an actionable message;
  no Section is chosen implicitly.
- VXL HVA animation is not needed for static colour review and is neither loaded nor modified.

## UI contract

- Keep the existing layout and `VoxelStyle.SourcePicker` AutomationId.
- Rename the command to `选择模型…` and allow `.vox;*.vxl`.
- Selecting VXL opens a second explicit `.pal` picker. Cancelling either picker changes no active source.
- Source/status copy says `体素模型`, not `VOX`, where both formats are supported.
- No new control, layout row/column, Shell entry, dock behavior or AutomationId is added.

## Semantic boundaries

- No VXL writer, HVA writer, normals, pivot inference, project Apply/Save, provider generation or game-ready claim.
- No changes to the Stage 1B codecs or canonical snapshot format.
- No public API, persistence schema, cache schema, INI, Field Registry, Shell or layout change.
- Failed input or style compilation never replaces the last valid source/preview.

## Verification

- Palette-empty request contract and specific local failure projection.
- VOX admission regression, VXL/PAL validation and single-Section admission.
- UI text/AutomationId contract and STA view construction.
- Focused Application/IDE tests, complete Application/IDE tests, solution build and IdeOnly clean package.
- Real DeepSeek, VXLSE GUI, HVA and in-game validation remain manual/NotRun.
