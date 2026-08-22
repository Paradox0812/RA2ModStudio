# RA2IniEditor.IDE Handoff Archive Index

## Purpose

This file indexes historical handoff documents so they are not mistaken for current product requirements.

The current active product direction is **RA2IniEditor.IDE-only**. The legacy table-style editor has been intentionally separated from this package and must not be restored as part of IDE-only work.

## Current Active Baseline

- Active package: `RA2IniEditor.IDE-only`
- Current build entry: `RA2IniEditor.IDE.sln`
- Current clean package profile: `IdeOnly`
- Current source package target: `RA2IniEditor.IDE.SourceClean.zip`

Use these commands for the current IDE-only package:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## Archive Boundary

Historical handoff documents may mention behavior, project shape, or UI flows that are no longer active in the IDE-only package.

Legacy mentions are preserved as history only. They are not active product requirements unless a newer IDE-only task explicitly re-authorizes them.

Do not use historical handoff references to restore:

- legacy root `RA2IniEditor.sln`
- legacy root `RA2IniEditor.csproj`
- legacy `MainWindow`
- legacy table-style editor
- legacy object workbench
- old Country / Side management windows
- old object copy or weapon-chain copy workflows

## Active Product Documentation

Start from `Docs/README.md`. Current active documents are:

- `Docs/ProductVisionAndRequirements.md`
- `Docs/CurrentCapabilities.md`
- `Docs/DevelopmentRoadmap.md`
- `Docs/DecisionLog.md`
- `Docs/Codex_CurrentPhase.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `Docs/FeatureOverview.md`
- `Docs/UserGuide.md`
- `Docs/ReleaseChecklist.md`
- `Docs/DeveloperNotes.md`
- `IDE_ONLY_PACKAGE_NOTE.md`

Use this archive index only to find historical implementation context.

## Historical Handoff Groups

### Field Registry Foundation And Import Flow

These documents are useful for understanding field registry provenance, import preview, apply / rollback contracts, and BuiltIn fallback history.

- `Docs/RA2IniEditor_IDE_Handoff_v0.4.19.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.20.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.21.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.22A.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.22B.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.23A.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.23B.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.24A.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.24B.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.24C.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.24C-hotfix.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.24D.md`

### Field Registry Stabilization And Early IDE Integration

These documents describe later field registry, import, test, and IDE integration stabilization work.

- `Docs/RA2IniEditor_IDE_Handoff_v0.4.25A.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.25B.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.25C.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.26.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.27.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.28.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.29.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.30.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.31.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.32.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.32.1.md`

### Source Editor, Completion, Hover, And Shell Boundary Work

These documents are useful for understanding the source-first IDE shell direction, completion, hover, controller boundaries, and editor-session behavior.

- `Docs/RA2IniEditor_IDE_Handoff_v0.4.33.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.34.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.35.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.36.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.37.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.38.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.39.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.40.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.41.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.42.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.42.1.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.42.2.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.43.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.44.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.44.1.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.44.2.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.45.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.47.2.md`

### Navigation, Reference UX, Save, And Dirty-State Work

These documents describe navigator behavior, reference information surfaces, save smoke work, dirty navigation, and UI automation history.

- `Docs/RA2IniEditor_IDE_Handoff_v0.4.49.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.50_51.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.52.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.53.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.55.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.56.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.57.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.58.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.59.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.65.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.66.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.68.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.69.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.71.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.71_AlwaysEditable.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.72.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.72_DirtyNavigation.md`
- `Docs/RA2IniEditor_IDE_Handoff_v0.4.73_DirtyNavigationUIA.md`

## How To Use This Archive

When using an archived handoff document:

1. Check current product docs first.
2. Treat old handoff text as implementation history.
3. Prefer current IDE-only solution and package commands.
4. Do not infer that legacy table-editor features are active.
5. If old text conflicts with current IDE-only docs, the current IDE-only docs win.

## Superseded Accumulated Status Snapshots

The following large append-only status files were replaced by concise current-state
documents on 2026-08-22. They remain evidence only:

- `Docs/Archive/Codex_CurrentPhase_Accumulated_Through_2026-08-22.md`
- `Docs/Archive/RA2IniEditor_IDE_Full_Codex_Context_Accumulated_Through_2026-08-22.md`

## Next Phase

The current safe entry is HLI-0B confirmation, followed by
`AUTOMATION-HLI-1A0 Dependency Cone Characterization Contract`. See
`Docs/Codex_CurrentPhase.md`; historical UI “next phase” entries are superseded.
