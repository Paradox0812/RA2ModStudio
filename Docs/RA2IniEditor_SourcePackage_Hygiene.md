# RA2IniEditor Source Package Hygiene

## Scope

This document defines what may enter a RA2IniEditor source package and how to
clean or package the repository without changing runtime behavior.

The current primary application is `RA2IniEditor.IDE`.

Two source package profiles are supported:

- `Full`: keeps the full repository shape, including the legacy root
  `RA2IniEditor` project when it exists.
- `IdeOnly`: keeps only `RA2IniEditor.IDE` and its supporting Core,
  Infrastructure, test, UI automation, docs, tools, and BuiltIn v3.2 field
  registry files. It must not reintroduce the legacy root
  `RA2IniEditor.csproj` or old table-style editor sources.

## Excluded From Source Packages

Source packages must not contain build outputs, IDE caches, logs, temporary
files, or local-only runtime data.

Excluded directories:

- `.vs/`
- `bin/`
- `obj/`
- `TestResults/`
- `artifacts/`
- `Logs/`
- `publish/`

Excluded files:

- `*.suo`
- `*.user`
- `*.vsidx`
- `*.DotSettings.user`
- `*.log`
- `*.tmp`
- `*.cache`
- `*.bak`
- `*.orig`
- `*_wpftmp*`
- `*.nupkg`
- `*.snupkg`

Local IDE metadata under `.ra2ide/local/` is also excluded.

## Cleaning

Preview cleanup:

```powershell
.\tools\clean.ps1 -WhatIf
```

Apply cleanup:

```powershell
.\tools\clean.ps1
```

The cleanup script is intentionally limited to paths under the repository root.
It deletes build outputs and local caches only; it does not delete `.git`, source
files, docs, or test data.

After cleanup, `dotnet build` or `dotnet test` will recreate `bin/` and `obj/`.
Run cleanup again immediately before preparing a clean source package if needed.

## Packaging

Create an IDE-only clean source package:

```powershell
.\tools\package-source-clean.ps1 -Profile IdeOnly
```

Create a full clean source package from a full repository checkout:

```powershell
.\tools\package-source-clean.ps1 -Profile Full
```

Create a package without deleting the current working build cache:

```powershell
.\tools\package-source.ps1
```

Create a package after first cleaning the working tree outputs:

```powershell
.\tools\package-source.ps1 -CleanFirst
```

The default package path is:

```text
artifacts/RA2IniEditor-source.zip
```

The package script uses a temporary staging directory and removes excluded
content from the staging copy before compression.

For the `IdeOnly` profile, the package must include `RA2IniEditor.IDE.sln` and
must not include the legacy root `RA2IniEditor.csproj`.

## UIA Tests

UIA tests live in `RA2IniEditor.UiAutomationTests` and are intentionally opt-in.
Normal solution tests do not launch UI windows.

Default compile/skip check:

```powershell
dotnet test .\RA2IniEditor.UiAutomationTests\RA2IniEditor.UiAutomationTests.csproj -c Release
```

Interactive desktop run:

```powershell
$env:RA2INIEDITOR_RUN_UI_AUTOMATION='1'
dotnet test RA2IniEditor.UiAutomationTests -c Release
```

Do not include UIA runtime screenshots, logs, or temporary project directories
in source packages.

## Annotation Sidecars And INI Save

Field annotation sidecars and field registry packs under `.ra2ide/` are IDE
support data. They are separate from INI source saving.

The current IDE edit path is still text-first and in-memory unless a specific
feature explicitly writes a sidecar or field registry pack. Source package
cleanup must not be treated as an INI save operation.

## Backup Directories

Runtime backup directories and generated rollback manifests are local output and
must not be included in source packages. Test fixtures may include explicit
sample data only when stored under a test-data directory and documented as test
input.

## Historical Test Zip

`RA2IniEditor_PrerequisiteIllegalBuilding_TestCases.zip` is historical/manual
test material for Prerequisite illegal building diagnostics. It is stored at:

```text
RA2IniEditor.Tests/TestData/PrerequisiteIllegalBuilding/RA2IniEditor_PrerequisiteIllegalBuilding_TestCases.zip
```

No automated test currently reads this zip directly. It is kept intact for
manual reference and has not been unpacked or transformed.

## Verification

Recommended verification after script changes:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
.\tools\package-source-clean.ps1 -Profile IdeOnly
```

To inspect the package:

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead("artifacts/RA2IniEditor-source.zip")
$zip.Entries.FullName | Where-Object {
    $_ -match '(^|/)(\.vs|bin|obj|TestResults|artifacts|Logs|publish)/' -or
    $_ -match '\.(suo|user|vsidx|log|tmp|cache|bak|orig|nupkg|snupkg)$' -or
    $_ -match '_wpftmp'
}
$zip.Dispose()
```

The inspection command should produce no output.

## Product Documentation Review

The following product-facing documents were not rewritten as part of the
IdeOnly infrastructure fix. They should be manually reviewed before product
release because they may still describe older table-style editor behavior:

- `Docs/UserGuide.md`: still mentions a Key-Value table workflow.
- `Docs/ReleaseChecklist.md`: still contains broad DataGrid-oriented UI review
  wording that may need IDE-specific review.
- `Docs/FeatureOverview.md`: no rewrite was performed in this infrastructure
  pass.
- `Docs/DeveloperNotes.md`: no rewrite was performed in this infrastructure
  pass.
