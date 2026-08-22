# Codex Task: RA2IniEditor.IDE Icon-S2A Resource Dictionary Scaffold and Brush Token Mapping

## 0. Current Baseline

Icon-S1 has been completed.

Reported state:

```text
Docs/IconVectorResourceContract.md created.
Preferred future location: RA2IniEditor.IDE/Themes/IconResources.xaml.
Resource strategy: WPF vector resources, Path / Geometry / DrawingImage.
Compatibility strategy: preserve existing Icon* keys for main toolbar migration.
No source / XAML / runtime icon resources changed in Icon-S1.
Tests: 1433 passed.
IdeOnly package: passed, packaged file count 777.
```

Next phase:

```text
Icon-S2A: Resource dictionary scaffold and brush token mapping
```

This is a limited implementation phase.

The goal is to add the icon resource infrastructure without replacing visible toolbar icons yet.

---

## 1. Goal

Create the WPF icon resource scaffold so later phases can replace placeholder letter icons safely.

Required result:

```text
1. Add IconResources.xaml in the approved location.
2. Define icon brush token aliases / placeholder theme mapping.
3. Define a minimal vector icon resource pattern.
4. Merge IconResources.xaml into the existing application/theme resource chain.
5. Add a very small non-invasive sample or test-only icon resource if needed.
6. Do not replace existing toolbar Icon* placeholders yet unless explicitly approved.
```

This phase is infrastructure only.

---

## 2. Hard Boundaries

Do not:

```text
replace toolbar placeholder letters
replace IconOpenFolder / IconSave / IconSearch / IconFieldRegistry values
change toolbar button content
change command handlers
change menu entries
change toolbar layout
add PNG runtime assets
add SVG runtime assets
add image2 output as runtime assets
restore legacy AutomationIds
change AI Assistant behavior
change Field Registry behavior
change parser / diagnostics / completion / hover / quick peek / save preflight behavior
```

Do not modify:

```text
solution files
project files, unless IconResources.xaml is not automatically included by current SDK-style project behavior and build requires explicit inclusion
Field Registry JSON
legacy files
```

If project file modification is required only to include XAML resources, stop and report before editing unless the existing project convention clearly requires it.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Themes/IconResources.xaml
RA2IniEditor.IDE/App.xaml or existing theme merge location, only if required to merge IconResources.xaml
RA2IniEditor.IDE/Themes/ShellTheme.xaml, only if the existing theme merge pattern requires it and no placeholder Icon* value is replaced
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if needed:

```text
RA2IniEditor.Tests/IDE/IconResourceBoundaryTests.cs
```

Do not modify ShellWindow.xaml in this phase unless required for resource resolution smoke, and stop before doing so.

---

## 4. Resource Dictionary Requirements

Create:

```text
RA2IniEditor.IDE/Themes/IconResources.xaml
```

The dictionary should be small and infrastructure-focused.

It may define:

```text
IconBrush.Normal
IconBrush.Muted
IconBrush.Disabled
IconBrush.Warning
IconBrush.Error
IconBrush.Success
IconBrush.Accent
IconBrush.Project
IconBrush.Global
IconBrush.BuiltIn
```

If actual shell brush resources exist, map to them with DynamicResource where safe.

If exact brush names are not available, use conservative aliases while preserving future replaceability.

Do not hard-code final visual icon resources in this phase beyond minimal scaffold resources.

---

## 5. Sample Resource Rules

If a sample icon resource is needed for tests, use a harmless internal sample key such as:

```text
IconSampleCheck
```

or a non-consumed resource.

Do not replace production toolbar keys yet:

```text
IconOpenFolder
IconSave
IconUndo
IconRedo
IconRevert
IconEditMode
IconSearch
IconFieldRegistry
IconIssues
IconProjectExplorer
```

unless the user explicitly moves to Icon-S2B.

---

## 6. Merge Strategy

Use the existing resource merge style in the project.

Preferred:

```text
IconResources.xaml is merged near ShellTheme.xaml or App.xaml resource dictionaries.
```

Requirements:

```text
1. Existing windows still load.
2. Existing ShellTheme resources still resolve.
3. Existing Icon* placeholder resources still resolve.
4. No visible icon replacement occurs.
```

---

## 7. Tests

Add/update boundary tests to verify:

```text
1. IconResources.xaml exists.
2. IconResources.xaml is referenced / merged in the resource chain, if testable.
3. Icon brush tokens are defined.
4. Existing Icon* toolbar placeholder resources still resolve.
5. Existing approved toolbar AutomationIds remain.
6. No legacy Shell.FieldRegistryButton is restored.
7. No PNG / SVG runtime asset dependency is introduced.
8. No AiAssistant.ApplyButton / Insert behavior appears.
```

Avoid pixel-perfect tests.

---

## 8. Validation Commands

Run full validation because resource dictionaries may affect WPF load:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Launch IDE.
2. Confirm main window loads.
3. Confirm toolbar still shows existing placeholder letters.
4. Confirm no command behavior changed.
5. Confirm AI Assistant still opens/sends as before.
6. Confirm Field Registry opens as before.
7. Confirm no missing resource exceptions.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: Icon-S2A.
2. Files changed.
3. IconResources.xaml scaffold summary.
4. Brush token mapping summary.
5. Resource merge strategy.
6. Tests added/updated.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation no toolbar icon replacement occurred.
12. Confirmation no command behavior changed.
13. Recommended next phase.
```
