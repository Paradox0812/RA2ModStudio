# ASSET-VOX-1E-UI Natural-Language Style Workspace Final Contract

Status: self-reviewed and approved for continuous execution by the user's 2026-08-27 instruction.

## 1. Product outcome

The IDE provides a central **Voxel Style** document that lets the user select a canonical `.vox` file inside the active project, describe a style in natural language, explicitly compile it through the existing dedicated style compiler, and review the deterministic colour result before any downstream materialization.

Opening the workspace or selecting a source never calls DeepSeek. Only the explicit compile/recompile command may create a provider request.

## 2. Entry and lifecycle

- Menu: `Tools -> Voxel Style Preview`.
- AutomationId: `Shell.Menu.VoxelStyleWorkspace`.
- Document ContentId: `Document.VoxelStyle`.
- Exactly one document instance may exist. Reopening activates it.
- Closing cancels the active request, releases image/session state, and removes event subscriptions.
- Shell shutdown closes this dynamic document before persisting the AvalonDock layout.
- The document is not a default dock profile and cannot appear during startup layout restoration.

## 3. Source admission

- An active project is required.
- The user explicitly chooses one `.vox` file.
- The normalized file must be inside the active project, must not be a reparse point, and must pass the existing bounded MagicaVoxel codec.
- Loading is read-only and creates an immutable canonical snapshot.
- Initial preview is generated locally from the source snapshot; no provider is called.
- Unsupported VOX scene graphs, missing palettes, oversized files, or malformed inputs produce an actionable failure state without replacing the last valid session.

## 4. Style source semantics

- Source priority remains: built-in -> project root -> contained directory chain -> per-request override.
- The text editor contains only the per-request natural-language override; inherited files remain read-only facts.
- The resolved source list displays scope and relative display path without exposing secrets.
- Existing source bounds and hashes remain authoritative.

## 5. Compile and review transaction

One explicit compile action performs:

1. resolve the current source pack;
2. capture the selected DeepSeek model identity and configuration;
3. call the existing `Ra2VoxelStyleCompiler` once (or consume its immutable cache hit);
4. colour the source through `Ra2VoxelColourizer`;
5. build the path-free review package;
6. atomically replace the visible preview only if the session generation still matches.

No retry is automatic. Clarification, unsupported intent, missing configuration, timeout, malformed plan, palette failure, colourization failure, and review-package failure remain distinct user-visible states.

## 6. Concurrency

- At most one compile is active per workspace.
- A new source load, document close, or explicit cancel invalidates the active generation.
- Late responses cannot replace a newer source or style state.
- Commands are disabled while their preconditions are false.

## 7. Visual contract

The document uses existing IDE tokens and has no native `DataGrid`/form layout.

- Compact command header: source name, dimensions/cell count, model, status, choose-source, compile/recompile, cancel.
- Left authoring column: source path, inherited style sources, natural-language override, session acceptance status.
- Right review column: mode selector and a large image viewport for original slice stack, coloured result, geometry-region mask, and palette swatch.
- Lower review area: compiled roles, compiled rules, unresolved assumptions, review flags, palette error, and geometry/occupancy invariants.
- At widths below the normal 1920x1080 workspace allocation, both columns retain bounded minimum widths and the outer surface scrolls instead of clipping commands.
- There is no maximize button, detached floating host, or new global dock chrome.

## 8. Acceptance semantics

`Accept Preview` means **accepted in the current in-memory workspace session only**. The UI must state that it does not write a file, apply to the project, export review artifacts, generate VXL/HVA, or save anything. Any later downstream handoff requires a separate contract.

Changing source or style text clears acceptance.

## 9. Automation IDs

- `VoxelStyle.Document`
- `VoxelStyle.SourcePicker`
- `VoxelStyle.SourcePath`
- `VoxelStyle.SourceFacts`
- `VoxelStyle.StyleSources`
- `VoxelStyle.StyleOverride`
- `VoxelStyle.Model`
- `VoxelStyle.Compile`
- `VoxelStyle.Cancel`
- `VoxelStyle.AcceptSession`
- `VoxelStyle.Preview.Original`
- `VoxelStyle.Preview.Result`
- `VoxelStyle.Preview.RegionMask`
- `VoxelStyle.Preview.Palette`
- `VoxelStyle.Preview.Image`
- `VoxelStyle.Plan.Roles`
- `VoxelStyle.Plan.Rules`
- `VoxelStyle.Review.Issues`
- `VoxelStyle.Status`

## 10. Verification contract

- Coordinator tests use a deterministic fake `IRa2AiClient`; no real provider call.
- Path and codec boundary tests cover outside-project, invalid source, and valid source admission.
- Transaction tests cover success, cancellation/stale replacement, clarification/failure mapping, and no output file creation.
- XAML/Shell contract tests cover all AutomationIds, single-instance document lifecycle, explicit-only provider initiation, and absence from dock profiles/startup layout.
- Run one final IDE solution build, full test suite, and clean source package verification.
- Manual screenshot and real DeepSeek compile remain `NotRun` until separately performed by the user or explicitly authorized.

## 11. Self-review result

Approved. No parallel architecture is introduced; all semantic work is delegated to the completed 1E core. The Shell change is a bounded composition-root entry only. The contract closes the main failure modes: hidden calls, stale async results, unsafe paths, layout persistence pollution, misleading acceptance, and accidental writes.

