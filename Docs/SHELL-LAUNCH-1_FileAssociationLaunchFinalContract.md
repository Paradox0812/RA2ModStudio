# SHELL-LAUNCH-1 File Association Launch Final Contract

Status: Completed / automated verified / physical Explorer smoke pending

Date: 2026-08-25

Risk: R3 — Shell startup lifecycle integration; no visual/layout change

## 1. Goal

When Windows starts `RA2IniEditor.IDE.exe` with one existing `.ini` path, treat the
file's direct parent directory as the project root, open that project through the
canonical IDE project pipeline, and load the exact requested file into an editable
source session.

## 2. Accepted startup inputs

| Input | Result |
|---|---|
| no arguments | normal empty IDE startup |
| one existing `.ini` path | open direct parent as project, then load the exact file |
| `--automation-open-folder <existing folder>` | preserve the existing UI automation path |
| missing path, non-INI target, unknown option, or multiple targets | keep the IDE open and report a typed startup error in Output/status |

The parser normalizes full paths and accepts `.ini` case-insensitively. It does not
search ancestors, recursively discover project files, or invent a project manifest.

## 3. Lifecycle and authority

1. Parse arguments before constructing the Shell.
2. Show the Shell and wait until the initial AvalonDock layout lifecycle finishes.
3. Enter the same dirty-boundary and `ShellViewModel.OpenProjectFolderAsync` path used
   by the menu command.
4. Initialize the canonical project document session store and source highlighting.
5. Resolve the exact file from the successful top-level project file list.
6. Load it through `LoadProjectExplorerFileAsync`, start the existing editable session,
   select the matching Project Explorer node, and focus the source editor.

No startup component reads INI content directly. Save, backup, diagnostics, Field
Registry, completion, Undo/Redo and project transaction semantics remain owned by the
existing services.

## 4. Explicit exclusions

- no registry/file-association writer or installer change;
- no single-instance mutex or IPC forwarding (candidate `SHELL-LAUNCH-2`);
- no recursive project discovery or ancestor-root heuristics;
- no XAML, docking topology, AutomationId or visual change;
- no automatic save, apply, AI request, or project mutation;
- no new public .NET API or persistent format.

## 5. Verification contract

- parser unit tests for empty/raw/quoted/mixed-case/automation/invalid inputs;
- integration test proving the parsed target can be opened and loaded exactly;
- source boundary test proving App -> Shell ready gate -> canonical project/session path;
- IDE build, Application tests, IDE tests and clean package gate;
- physical Windows file-association launch remains a manual release smoke because the
  stage does not modify the user's registry.
