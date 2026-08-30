# ASSET-VOX-1E-UI Code Fact Audit

## 1. Goal

Expose the completed ASSET-VOX-1E natural-language voxel style pipeline as an explicit, review-first IDE workflow without adding project apply/save, VXL/HVA generation, or implicit provider calls.

## 2. Reuse facts

- `Ra2VoxelStyleSourceResolver` already resolves built-in, project, directory, and request override style sources with bounded UTF-8/path rules.
- `Ra2VoxelStyleCompiler` already owns the dedicated DeepSeek structured tool call and immutable plan cache.
- `Ra2VoxelColourizer` already performs deterministic geometry-region colouring and emits review facts.
- `Ra2VoxelColourReviewPackageBuilder` already produces path-free in-memory PNG/JSON/VOX review artifacts.
- `Ra2MagicaVoxelCodec` already reads a bounded single-model VOX v150 snapshot.
- `ShellWindow` already opens the authoring review as a dynamic central `LayoutDocument`; this is the canonical lifecycle to reuse.
- Existing IDE visual resources in `IdeVisualTokens.xaml`, `IdeControlStyles.xaml`, and `IdeWorkspaceStyles.xaml` are sufficient. No UI dependency is required.

## 3. Current missing product slice

- No Shell entry opens a voxel style workspace.
- No session owner coordinates source loading, style-source resolution, compilation, colourization, or in-memory review artifacts.
- No visual projection exists for source/result slice stacks, palette swatch, geometry mask, compiled roles/rules, assumptions, or review flags.
- No explicit cancellation or stale-result protection exists at the UI boundary.
- No honest in-session acceptance state exists; downstream export/apply remains intentionally absent.

## 4. Allowed files

- `RA2IniEditor.IDE/AssetAuthoring/*VoxelStylePreview*`
- `RA2IniEditor.IDE/ViewModels/AssetAuthoring/*`
- `RA2IniEditor.IDE/Views/AssetAuthoring/*`
- minimal `RA2IniEditor.IDE/Views/ShellWindow.xaml`
- minimal `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- targeted `RA2IniEditor.Tests/IDE/*VoxelStyle*`
- this phase's documentation and current-state documentation

## 5. Forbidden files and semantics

- no changes to Application voxel algorithms or their public/internal contracts
- no changes to DeepSeek transport, model catalogue, AI Assistant Chat/Work routing, INI authoring, Field Registry, parser, diagnostics, completion, save preflight, undo/redo, or project transaction semantics
- no changes to `ShellDockLayoutCoordinator`, compiled dock profiles, toolbar, bottom tools, right tool well, or startup visibility
- no project Apply/Save, review-package export, VXL/HVA production, asset-manifest write, or automatic provider call

## 6. UI composition fact

The workspace is a single-instance dynamic central document with `ContentId=Document.VoxelStyle`. It is removed before dock-layout serialization, matching the existing authoring-diff lifecycle. It is not a managed dock tool and therefore cannot alter persisted default tool placement.

## 7. Risk classification

R4: the slice combines WPF composition, external-provider orchestration, bounded file input, cancellation, and large in-memory image artifacts. Risk is contained by a dedicated session coordinator, immutable core results, explicit user initiation, project-contained file selection, generation tokens, and one final verification pass.

