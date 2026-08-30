# DIFF-REVIEW-1 Stage Ledger

Date: 2026-08-26

Status: Completed / automated verified / manual visual acceptance pending

Contract: `Docs/DIFF-REVIEW-1_CanonicalResultAndObjectContextFinalContract.md`

## Stage result ledger

| Stage | Goal | Files touched | Verification | State |
|---|---|---|---|---|
| DR1-A | Exact candidate review projection, shared source-to-candidate mapping and changed outline | `Ra2AuthoringDiffProjection.cs`, `Ra2AuthoringReviewProjection.cs`, tests | focused projection tests + Debug build | Completed |
| DR1-B | Readonly highlighted AvalonEdit Result, change renderer and navigation | Result renderer, ViewModel, View XAML/code-behind, tests | XAML compile + UI contract tests | Completed |
| DR1-C | Preserve the existing unified Changes projection and independently degradable Result | ViewModel/View, existing diff tests | diff statistics/limit regressions | Completed |
| DR1-D | Depth-one same/cross-document related Section context, missing/ambiguous/budget/cancel states | review projection + tests | exact/cross/missing/ambiguous/64-cap/cancel tests | Completed |
| DR1-E | Responsive outline, automation/keyboard contract, docs and package gate | View XAML/code-behind, docs | full verification matrix below | Completed |

## Invariants verified

- Result text is the exact successful Preview `CandidateText`; it is not rebuilt from the plan.
- Unified Diff remains the sole removed-line representation and keeps its prior limits.
- Object Context reads only request-captured candidate/source snapshots through existing semantic/query services.
- Review state is IDE-internal, transient and non-serialized.
- Apply/Dismiss delegate to the existing proposal ViewModel; there is no partial Apply, Save or edit path.
- Shell, layout persistence, parser, Field Registry, diagnostics, transactions and legacy were not modified by this package.

## Verification matrix

| Step | Status | Evidence |
|---|---|---|
| Restore | Passed | `dotnet restore .\RA2IniEditor.IDE.sln` |
| Debug build | Passed | 0 warnings, 0 errors |
| Focused review/diff tests | Passed | 19/19 |
| Application tests | Passed | 198/198 |
| IDE tests | Passed | 2779/2779 |
| IdeOnly clean package | Passed | 1255 files; generated output/caches excluded |
| Physical WPF visual acceptance | NotRun | requires user 1920x1080 and narrow-width screenshots |

## Deferred governance queue

- Public API: none.
- Persistence/schema: none.
- Technical debt: no accepted runtime shortcut; physical mixed-DPI/visual acceptance remains manual evidence.
- Decision: canonical Result is review truth; diff/context are independently degradable evidence layers. Recorded in `Docs/DecisionLog.md`.
- Next safe entry: manual visual acceptance, then `ASSET-VOX-1` architecture/feasibility contract; do not add asset writes before that contract.
