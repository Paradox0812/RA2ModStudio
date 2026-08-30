# ASSET-VOX-3D Center-Seam Bridge Proposal — Stage Ledger

Date: 2026-08-29  
State: Completed / automated verified / live-provider and physical sample pending

| Stage | Goal | Files touched | Verification | State |
|---|---|---|---|---|
| 3D-1 | Bind one/two-cell center seams into immutable evidence | Application symmetry evidence | Build + Application focused | Completed |
| 3D-2 | Add Agent-only `bridge_center_gap` operation | Application proposal + IDE compiler | Compiler focused | Completed |
| 3D-3 | Materialize exact selected gaps through existing safety gates | Application executor/refiner reuse | Executor focused | Completed |
| 3D-4 | Prove eligible/ineligible geometry and target compatibility | Application/IDE tests | 16/16 + 9/9 | Completed |
| 3D-5 | Full regression, docs and package | Solution/docs/package | Recorded below | Completed |

## Verification evidence

- `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`: Passed, 0 warnings / 0 errors.
- Application focused `Ra2VoxelSemanticSymmetryTests`: Passed 16/16.
- IDE focused `Ra2VoxelSemanticSymmetryCompilerTests`: Passed 9/9.
- Application full: Passed 293/293.
- IDE full: Passed 2856/2856.
- AssetHost full: Passed 50/50.
- IdeOnly clean package: Passed, 1400 source files; output `artifacts/RA2IniEditor.IDE.SourceClean.zip`.
- Real DeepSeek/Tencent: NotRun; no call was authorized or needed.
- Physical `body-candidate.vox` review: Pending user restart and explicit Agent recognition.

## Boundary confirmation

- Legacy was not restored.
- Shell/XAML/AutomationIds were not changed.
- Apply/Save, VOX writer, VXL/HVA and persistence were not changed.
- Public API and canonical snapshot schema were not changed.
- The only semantic change is the approved internal Agent center-seam bridge proposal.

## Next safe entry

Restart the rebuilt IDE, rerun local candidates and `AI 识别结构`, inspect whether `seam-gap-*` targets are selected,
then compare the Agent candidate/difference before using `用于本会话`. If the desired gap is longer than two cells or
not on the selected center plane, treat it as a separate geometry-repair contract rather than widening this rule.
