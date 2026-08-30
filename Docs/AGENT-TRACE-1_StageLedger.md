# AGENT-TRACE-1 Stage Ledger

Date: 2026-08-25

| Stage | Goal | Files touched | Verification | State after stage | Next entry satisfied |
|---|---|---|---|---|---|
| TRACE-1A | Map backend retrieval facts to compact UI text | `ShellWindow.xaml.cs`, AI Shell boundary tests | formatter and disclosure-boundary tests | Completed | Yes |
| TRACE-1B | Insert one metadata line before the proposal card | `ShellWindow.xaml.cs`, AI Shell boundary tests | dynamic-only AutomationId/style contract | Completed | Yes |
| SEARCH-FIX1 | Avoid startup creation of Search and normalize restored Search to hidden/Floating Home | `ShellWindow.xaml.cs`, `ShellDockLayoutCoordinator.cs`, Dock layout tests | STA lifecycle and restore-order tests | Completed after Fix2 | Yes |

No XAML, public API, persistence format, query behavior, Preview/Apply/Undo/Save authority or legacy path changed.
Physical startup observation and final visual spacing remain manual acceptance items.

## Verification

- Focused TRACE + Dock tests: 38/38 passed after Search restore-state Fix2.
- IDE full tests: 2745/2745 passed.
- Application tests: 198/198 passed.
- Release solution build: 0 warnings, 0 errors.
- IdeOnly clean package: 1247 files.
