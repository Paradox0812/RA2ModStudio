# ASSET-VOX-UI-R1 Stage Ledger

> Package: Workspace Recomposition and Camera Stability
>
> Status: Implementation complete; automated verification complete; physical DPI review pending
>
> Risk: R3, localized WPF layout and viewport lifecycle

## Stage results

| Stage | Result | Evidence |
|---|---|---|
| UI-R1-0 Characterization | Completed | Existing AutomationIds/provider boundaries retained; camera state gained isolated finite-value and bounds-mapping tests. |
| UI-R1-1 Camera Stability | Completed | Scene swaps preserve normalized target, yaw, pitch and distance ratio within one source group; reset/new-source rules remain explicit. |
| UI-R1-2 Workspace Recomposition | Completed | Replaced the long form with inspector / dominant viewport / resizable evidence composition and two splitters. |
| UI-R1-3 Navigation | Completed | Added Generation/Geometry/Style/Output and Quality/Structure/Colour/Review tabs while preserving bindings and actions. |
| UI-R1-4 Responsive Polish | Completed | 312/280/260 DIP inspector defaults, 240/160 DIP evidence defaults, local scroll fallback and no root scaling transforms. |
| UI-R1-5 Verification | Automated complete | Build and affected suites pass. Full IDE suite recorded one unrelated intermittent ContextMenu open-state failure; its isolated rerun passed. Manual DPI review remains. |

## Modified runtime surface

- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewport3D.xaml.cs`
- `RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelViewportCameraState.cs`

No Shell, ViewModel, SceneBuilder, provider, model algorithm, Apply/Save, VOX writer, VXL/HVA, INI, Field Registry,
public API or persistence file was changed by this package.

## Automated evidence

- Debug solution build: passed, 0 errors; one pre-existing nullable warning in
  `BuiltInFieldRegistryPackLoaderTests.cs:1983`.
- Camera / viewport / workspace contract tests: 14/14 passed.
- Affected IDE voxel plus visual-boundary tests: 88/88 passed.
- Affected Application voxel tests: 87/87 passed.
- AssetHost tests: 50/50 passed.
- Application full suite: 285/285 passed.
- IDE full suite: 2849/2850 passed on the first full run. The only failure was
  `IdeVisualSystemBoundaryTests.VisualTokens_ResolveWithFrozenTypesAndValuesThroughStaResourceLoad` at the unrelated
  temporary `ContextMenu.IsOpen` assertion; immediate isolated rerun passed 1/1. Per the minimum-sufficient-evidence rule,
  the whole suite was not repeatedly rerun merely to obtain a green line.
- IdeOnly clean source package: passed, 1394 files; build/cache/test/output directories and archive patterns excluded.

## Manual acceptance still required

1. 1920×1080 at 100% and 125% Windows scaling.
2. Resize both splitters to their limits and switch all workflow/evidence tabs.
3. Rotate/pan/zoom, switch Original/Direct/Refined/Difference/Structure/Colour modes, then temporarily switch documents and
   return. The view must not auto-fit or jump.
4. Select a genuinely different model; its first valid scene must auto-fit once.
5. Confirm no overlap, clipping, whole-page zoom or uncontrolled horizontal scrolling.

## Remaining risk

- WPF/AvalonDock physical DPI and input feel cannot be certified by source tests alone.
- Camera state is intentionally session-only and disappears when the document instance closes.
- Scene-builder performance and model/style semantics were deliberately not changed.

## 2026-08-29 runtime binding correction

- First manual launch exposed a WPF binding exception in the header summary: `Run.Text` binds two-way by default and tried
  to write into the read-only `SourceName` property.
- Every dynamic `Run.Text` in the workspace now declares `Mode=OneWay`, including header facts, semantic-region rows and
  colour-rule rows. The UI contract rejects future bare property bindings on `Run.Text`.
- The fix changes no ViewModel setter, business state or public API. Debug build and the targeted XAML/STA construction
  tests pass.
