# UI-MODERN-PROGRAM-R1 — M6 Stage Result Ledger

Status: M6-B completed; M6-C pending  
Last updated: 2026-07-23  
Authority: `Docs/UI-MODERN-PROGRAM-R1_ProjectVisualModernizationContract.md`

## Stage checkpoints

| Stage | Goal | Files touched | Verification | State after stage | Next entry satisfied |
|---|---|---|---|---|---|
| M6-B0 | Exact resource/reference audit and rollback anchor | Audit only; rollback artifact | 397-key baseline, dynamic/implicit/test-contract review | Completed | Yes |
| M6-B1 | Remove safe Shell zero-reference history | `ShellTheme.xaml` | XML parse; Shell/visual tests 47/47 | Completed with conservative scope reduction from 15 to 14 keys | Yes |
| M6-B2 | Retire Field Editor/Learning aliases | 2 views, 2 tests | Contract-token comparison; targeted tests 17/17 | Completed | Yes |
| M6-B3 | Retire Allowed Values/Remote Preset aliases | 2 views, 3 tests | Contract-token comparison; targeted tests 17/17 | Completed | Yes |
| M6-B4 | Retire Harvest/application compatibility dictionary | Harvest, App, deleted dictionary, 2 tests | Static audit; targeted tests 33/33 | Completed | Yes |
| M6-BV | Package-level verification | Whole accepted M6-B source state | Restore/build; affected 64/64; full 2332/2332; startup smoke | Completed | Yes |

## Rollback anchor

```text
Path: artifacts/RA2IniEditor.IDE.SourceClean.UI-MODERN-PROGRAM-R1-M6B.PreChange.Rollback.zip
Profile: IdeOnly
Entries: 963
Bytes: 10,459,342
SHA-256: 8C3BC4C8BA43810B2734EF5D792A878D8E4735A47399A5AC5B6EFC67394057A6
```

The archive excludes `.git`, `.vs`, `bin`, `obj`, `artifacts`, `TestResults`, caches, logs and old archives.

## Result

- Removed 14 safe Shell resource keys.
- Retired 14 `IdeSecondary*` compatibility definitions and migrated their 56 production references.
- Removed `Resources/Styles/IdeSecondaryWindowStyles.xaml` and its application merge entry.
- Final application resource inventory: 379 explicit keys, zero duplicates.
- Final production `IdeSecondary` occurrences: zero.
- Five migrated windows retained identical AutomationId, binding and Click-handler sets.
- No public API, dependency, project file, C# runtime path, Dock topology or product semantic change.

## Verification Matrix

| Step | Status | Evidence |
|---|---|---|
| Pre-change rollback package | Passed | 963 entries; SHA-256 recorded above |
| Static resource/XAML audit | Passed | 7 modified XAML files parsed; 379 unique keys; no `IdeSecondary` production occurrence |
| Per-card targeted tests | Passed | 47/47, 17/17, 17/17 and 33/33 |
| Restore | Passed | All projects up to date |
| Debug build | Passed | 0 warnings, 0 errors |
| Affected boundary tests | Passed | 64/64 |
| Full non-UI suite | Passed | 2332/2332 |
| Real startup smoke | Passed | Hidden Debug process produced a real main-window handle and stayed alive |
| New screenshot set | NotRun | M6-B is visual-neutral cleanup; M6-C owns the final screenshot index |
| Final clean package | NotRun | Explicitly reserved for M6-C |

## Diff intent

| Area | Intent |
|---|---|
| `Themes/ShellTheme.xaml` | Remove only audited safe historical keys |
| Five Field Registry subwindow XAML files | Replace compatibility aliases with accepted canonical styles |
| `App.xaml` and deleted secondary dictionary | Retire the zero-reference compatibility authority |
| Four boundary-test files | Replace compatibility-presence assertions with canonical-style/absence assertions |
| M6 governance documents | Record evidence and set M6-C as the next safe entry |

## Deferred Governance Queue

### PublicApiLedger Pending Entries

None.

### TechnicalDebt Pending Entries

- `UI-MODERN-M1-A11Y-001` remains unchanged.
- `UI-MODERN-M6A-UIA-001` remains unchanged.
- Physical 150% DPI and mixed-monitor hardware evidence remains NotRun.

### DecisionLog Candidate Entries

- Preserve zero-reference resources when they retain an accepted inheritance/test contract; `IdeToolResultListStyle` is the M6-B concrete example.

### CurrentStatus Pending Updates

Flushed in this package: M6-B completed; M6-C is the next safe entry.

## Stop rule

M6-B stops here. Do not begin M6-C automatically.

