# ASSET-VOX-1E-UI-3D Interactive Viewport Final Contract

Status: approved by the user's explicit 2026-08-27 instruction to enter this phase.

## 1. Outcome

Replace the Voxel Style workspace's primary SliceStack preview with a native WPF interactive 3D viewport. Original,
coloured-result and geometry-region modes render the same canonical voxel snapshot through the completed Stage 1F
visible-face projection. Palette remains a 2D image. SliceStack remains an explicit diagnostic fallback.

## 2. Architecture and ownership

- `Ra2VoxelSceneSnapshot` remains the sole geometry and palette truth for both VOX and decoded VXL inputs.
- `Ra2VoxelSurfaceProjector` remains the sole visible-face extraction path.
- The IDE creates only frozen, disposable WPF presentation geometry. It is not serialized and owns no asset authority.
- The style preview coordinator exposes the already-produced coloured snapshot and geometry mask to its ViewModel; it does
  not recalculate style semantics in the UI.
- No HelixToolkit or other dependency is introduced. The viewport uses WPF `Viewport3D`.

## 3. Interaction

- Left drag: orbit around the fitted model target.
- Middle drag or Shift + left drag: pan.
- Mouse wheel: bounded zoom.
- Double-click or `Reset view`: fit the complete model.
- Camera pitch and distance are bounded; invalid or zero-size input cannot produce non-finite camera state.
- The viewport declares `X = right`, `Y = depth`, `Z = up` and states that current lighting is geometry-only review
  lighting, not VXL normal-index or game-engine lighting.

## 4. Rendering and failure behavior

- Only exposed faces are emitted. Faces are grouped by display colour/material.
- Original/result modes use the snapshot palette. Region mode uses deterministic review colours from the existing geometry
  mask. Palette mode uses the existing swatch image.
- Scene construction is cancellable and generation-guarded. Late builds cannot replace a newer source/mode.
- The UI face budget is bounded. Cancellation or resource-limit failure does not mutate source/session state and switches
  the visible surface to the existing SliceStack fallback with an actionable status.
- Scene replacement is atomic: only a completed frozen model replaces the previous valid WPF scene.

## 5. Preserved and added automation surface

All existing `VoxelStyle.*` AutomationIds are preserved. Add:

- `VoxelStyle.Preview.Viewport3D`
- `VoxelStyle.Preview.ResetCamera`
- `VoxelStyle.Preview.SliceFallback`
- `VoxelStyle.Preview.GeometryLightingNotice`

## 6. Explicit exclusions

- no Shell/menu/dock/global layout change;
- no VXL/HVA writer, normal-index mutation or GameReady claim;
- no project Apply/Save/export;
- no DeepSeek/compiler/cache change;
- no INI, Field Registry, parser, diagnostics, undo/redo or save semantic change;
- no multi-part Body/Turret/Barrel composition in this stage.

## 7. Verification

- deterministic mesh tests: exposed-face count, bounds, material grouping, region mapping and face-budget failure;
- coordinator/view-model tests: original/result/mask snapshot routing and Palette/SliceStack visibility;
- WPF contract tests: AutomationIds, native `Viewport3D`, no new dependency and real STA construction;
- affected IDE tests, Application voxel tests, solution build and clean-source package;
- physical orbit/pan/zoom/reset and 1920x1080 screenshot acceptance remain a user manual gate after automated closeout.

## 8. Risk decision

Risk is R3 because this adds a WPF rendering adapter and asynchronous view lifecycle. The current task explicitly authorizes
that boundary. No persistence, source-of-truth, public API or external-cost boundary changes, so the approved minimal action
is to implement the native viewport inside the existing dynamic workspace and stop if wider Shell or writer changes become
necessary.
