# ASSET-VOX-1B Stage Ledger

Date: 2026-08-26  
Package state: Completed / automated verified / supplied VXLSE structural acceptance passed

| Stage | Goal | Main evidence | State | Next entry |
|---|---|---|---|---|
| 1B-0 | Freeze canonical core contract and limits | Final contract, risk/data/reuse/verification review | Completed | 1B-1 |
| 1B-1 | Immutable single-part snapshot, palette and canonical hash | Order/copy/hash/connectivity/symmetry tests | Completed | 1B-2 |
| 1B-2 | Restricted MagicaVoxel VOX v150 exchange | Deterministic write/read/write and malformed matrix | Completed | 1B-3 |
| 1B-3 | Reviewed bounded Westwood VXL span decode | Synthetic VoxelNormalForge-compatible span and failure tests | Completed | 1B-4 |
| 1B-4 | VXLSE RGBA SliceStack and restricted PNG | Both directions, all PNG filters, CRC and exact cell round trip | Completed | 1B-5 |
| 1B-5 | Regression, package and documentation closeout | Focused/Application/IDE build+test/package | Completed | Executable VXLSE acceptance |
| 1B-A0 | Deterministic executable-acceptance fixture and verifier | `Ra2VxlseExecutableAcceptanceTests`, exported PNG/PAL/manifest | Completed | 1B-A1 |
| 1B-A1 | Supplied VXLSE import and save | First real file exposed executable axis mapping; inverse bridge implemented; corrected PNG imported into a fresh Section | Completed | 1B-A2 |
| 1B-A2 | Canonical VXL readback comparison | Five exact coordinates, dimensions, occupancy and palette indices | Completed | 1C contract/audit |

## Verification snapshot

- Final focused Stage 1A + 1B + executable acceptance tests: 30/30 passed.
- Application tests: 228/228 passed.
- IDE-only restore: passed; all projects current.
- IDE-only Debug build: passed, 0 errors / 1 pre-existing Field Registry nullable warning.
- IDE tests: full run 2778/2779 because the unchanged known WPF STA resource/Popup teardown test failed; its immediate
  isolated rerun passed 1/1.
- Clean source package: passed, 1273 files, `artifacts/RA2IniEditor.IDE.SourceClean.zip`.
- No WPF, Shell, AI Work, INI, Field Registry, diagnostics, completion, save, undo or project-file code changed.
- A real supplied-VXLSE GUI import/save and canonical readback were performed. Visual review, normals, pivot/mount,
  HVA generation and in-game smoke were not performed.
- Acceptance fixture tests: 2/2 passed; explicit export test: 1/1 passed.
- Generated fixture: `artifacts/VxlseAcceptance/vxlse-acceptance-downward.png`, 328 bytes,
  SHA-256 `6CA8AD7555D83BFFB128C8ABDD17819D74D2A8D5CE2D1CF87405B61EB569FD9F`.
- VXLSE process `vxlse_III.exe` starts and responds with the title `Voxel Section Editor III`; Computer Use identifies
  the running app but returns zero targetable windows, so no unverified UI input was attempted.
- First real VXL decoded as `5x3x4`; all five cells proved the executable mapping
  `VXL(x,y,z) = (input z, input X-1-x, input y)`. The standard VXL reader was confirmed correct; the defect was the
  missing inverse bridge in the executable-specific raster export.
- `Ra2VoxelSliceStackCodec.ExportForSuppliedVxlseDownward` now exports the inverse `Y,Z,X` import volume while keeping
  the source canonical hash. Corrected fixture is `4x15`, offset `3`, PNG SHA-256
  `CFE845E3027F10DAE4647E5CB83B2334EBC2B2C30BC76E385F1FA839C49068CC`; focused bridge/core tests pass 21/21.
- The corrected PNG was first re-imported into the non-empty previous acceptance Section. Dimensions became the expected
  `3x4x5`, but three old in-bounds cells remained, producing 8 instead of 5 cells. This confirms the source-backed empty
  target precondition; it is not accepted evidence.
- The user then imported into a new empty Section and saved over `vxlse-acceptance.vxl`. Explicit verification passed:
  Section `Body`, dimensions `3x4x5`, occupancy `5`; cells `(0,0,0)=193`, `(2,2,0)=40`, `(0,3,2)=102`,
  `(1,1,3)=145`, `(2,3,4)=168`; canonical hash
  `29A4A1150EEFB6305021B29CA37B7C3F58B0B845FEB779C63F93EA0DCF0161C2`.

## Deferred governance queue

- `Ra2VoxelSceneSnapshot` is internal and describes exactly one part. Stage 1A assembly remains the Body/Turret/Barrel
  authority.
- MagicaVoxel support is deliberately one model plus explicit RGBA. Scene graphs, transforms, materials and animation are
  rejected or ignored as bounded non-authoritative chunks.
- VXL read requires the caller's active external palette. VXL header `PaletteData` is reserved/unused by VXLSE and must
  not be mistaken for the actual theatre/unit palette.
- VXL writing remains forbidden until optional Stage 1F; 1B cannot claim a final VXL or `GameReady` artifact.
- Normal index, pivot/mount, world-axis and HVA behavior remain unresolved production facts.
- The executable structural acceptance is closed. The next phase must not reinterpret it as visual, normal, pivot,
  animation or in-game certification; GUI automation is not a production protocol.

## Source authorities

- User-authorized `C:\Users\PC\VoxelNormalForge` reader/writer source, reviewed and migrated selectively.
- Supplied VXLSE III Pascal source adjacent to the file-version `1.3.9.3281` executable.
- Official MagicaVoxel format specification:
  <https://github.com/ephtracy/voxel-model/blob/master/MagicaVoxel-file-format-vox.txt>.
