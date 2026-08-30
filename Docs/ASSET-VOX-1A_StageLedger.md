# ASSET-VOX-1A Stage Ledger

Date: 2026-08-26  
Package state: Completed / automated verified / executable GUI and game acceptance deferred.

| Stage | Goal | Files touched | Verification | State | Next entry |
|---|---|---|---|---|---|
| 1A-0 | Code/reuse/local asset audit | Contract and investigation docs | Source and filesystem facts inspected | Completed | 1A-1 |
| 1A-1 | Detached assembly contract | `Ra2VoxelAssemblyContracts.cs` | Constructor/graph tests | Completed | 1A-2 |
| 1A-2 | Bounded VXL/HVA metadata probe | `Ra2VoxelBinaryProbe.cs` | Synthetic malformed/valid tests and real sample probe | Completed | 1A-3 |
| 1A-3 | Assembly closure probe | `Ra2VoxelAssemblyProbe.cs` | Body/Turret/Barrel, missing and mismatch tests | Completed | 1A-4 |
| 1A-4 | VXLSE asymmetric slice contract | `Ra2VxlseSliceImportContract.cs` | Source-derived 3x4x5 Downward/Rightward full coordinate round trip | Completed | 1A-5 |
| 1A-5 | Alpha/palette/import/pivot boundary freeze | Contract, tests and context docs | Alpha/empty-target/normal/palette tests; VXLSE source and executable version audit | Completed | ASSET-VOX-1B |

## Verification snapshot

- Focused `Ra2VoxelAssemblyBaselineTests`: 9/9 passed.
- Real local binary probe: `tnkd`, `tnkdtur`, `ttank`, `ttanktur` VXL/HVA metadata passed.
- Debug build: passed, 0 errors / 1 pre-existing Field Registry nullable warning.
- Application: 207/207 passed.
- Final IDE full run: 2778/2779 passed. The unrelated existing WPF STA resource test
  `VisualTokens_ResolveWithFrozenTypesAndValuesThroughStaResourceLoad` failed during Dispatcher/Popup teardown; its
  immediate isolated rerun passed 1/1. An earlier full run before the final assembly-closure-only patch passed 2779/2779.
- IdeOnly clean source package: passed, 1262 files.
- No real VXLSE import, visual review or game smoke was performed.
- Supplied VXLSE: file `1.3.9.3281`, product `1.4.0.0`, SHA-256
  `DB9A882A74E16ECB1D938C6D07EC4C97B28D51EF23975730DF2211E354916458`.
- Focused assembly + slice contract tests: 17/17 passed before final full verification.
- Actual GUI import remains deferred until Stage 1B can produce the deterministic RGBA PNG input. GUI automation is not
  a product protocol.
- Final verification: focused 17/17, Application 215/215, IDE 2779/2779, Debug build 0 errors / 1 pre-existing Field
  Registry nullable warning.
- IdeOnly clean source package passed with 1264 files:
  `artifacts/RA2IniEditor.IDE.SourceClean.zip`.

## Deferred governance queue

- Decision: detached VXL assets are modeled as an assembly, not flattened into one VXL or only multiple Sections.
- Decision: one unnamed legacy HVA Section may pair only with one unambiguous VXL Section.
- Decision: VoxelNormalForge source is user-authorized; migrate reviewed core logic rather than importing its old UI/CLI.
- Public API: none; all 1A code types remain internal.
- Technical debt: world-axis meaning, pivot/mount placement, normals, full span decode/write and animation matrices remain
  unfrozen.
- Decision: Downward/Rightward raster addressing, direct-alpha occupancy, Westwood PAL expansion and VXLSE nearest-colour
  behavior are now source-backed and frozen; world-axis interpretation, pivot/mount, normals and animation remain unfrozen.
- Decision: a real Barrel sample is optional for later visual/pivot calibration, not a Stage 1A assembly-schema gate.
