# AGENT-AUTHORING-1-R1 A4-R1 Stage Ledger

Last updated: 2026-08-20

Package state: Completed. Contract, implementation, targeted verification, full
IDE-only verification, documentation and clean source package are closed.

## 1. Stage status

| Stage | Goal | Current evidence | State |
|---|---|---|---|
| A4-R1-0A | Persist reliability contract and bounded plan | Contract/static review | Completed; confirmed |
| A4-R1-0B | Audit preview thread affinity and current failure chain | Read-only code audit; 25 targeted tests | Completed |
| A4-R1-1 | Canonical endpoint identity | 87 targeted tests | Completed |
| A4-R1-2 | Deterministic advisory/edit routing | 18 targeted tests | Completed |
| A4-R1-3A | Discriminated tool outcome and strict adapter | 26 targeted tests | Completed |
| A4-R1-3B | Required-tool authoring request | 76 targeted tests | Completed |
| A4-R1-4 | Authoring system/user privilege split | 90 targeted tests | Completed |
| A4-R1-5 | Plain-text/mixed response enforcement | 31 targeted tests | Completed |
| A4-R1-6A | UI-independent proposal preparation runner | 11 targeted tests | Completed |
| A4-R1-6B | Exactly-once Shell lifecycle wiring | 39 targeted tests | Completed |
| A4-R1-7 | Proposal/history presentation ownership | 41 targeted tests | Completed |
| A4-R1-8A | Loopback integration and lifecycle verification | 19 targeted tests | Completed |
| A4-R1-8B | Full verification, docs and clean package | Build 0/0; tests 2519/2519; IdeOnly package | Completed |

## 2. Implemented reliability result

The previously confirmed failure chain is closed as follows:

```text
configured endpoint
  -> normalize the final chat-completions URI
  -> classify Official / Custom / Invalid
  -> resolve Advisory / EditExplicit / EditAmbiguous / EditUnavailable locally
  -> explicit official current-document edit uses required tool mode
  -> provider tool arguments remain untrusted
  -> local adapter + A3 Preview produce the only proposal authority
  -> explicit Apply is still required and never saves
```

- Advisory requests retain their existing single-user-message transport and have no tools.
- Authoring requests use separated system/user messages and `tool_choice=required`.
- The tool result is a strict `proposal` / `needs_clarification` discriminated outcome.
- Plain provider text cannot become a proposal when the required tool is not invoked.
- Preview preparation runs behind a UI-independent runner; Shell rechecks cancellation,
  generation, active handle, document/registry currency, and coordinator ownership before attach.
- The active proposal card is pinned against history trimming, and conversation context stores
  only the locally validated bounded proposal summary.

### Preview thread-affinity result

The current Preview path is background-safe under its present implementation:

```text
Ra2IniAuthoringWorkspace.Preview
  -> immutable Ra2AuthoringSnapshot
  -> Ra2IniEditPreviewService
  -> Ra2IniLanguageAnalysisService
  -> local parser / semantic model / diagnostics
```

The audited path contains no WPF/Dispatcher, network, environment, file, or process access.
Field Registry provider snapshots are stable and lazy caches are lock-protected.

This result is scoped to the current implementation. Any later addition of thread-affine
or I/O dependencies reopens the audit.

## 3. Verification evidence

Targeted implementation evidence was run after each Task Card. The final integration set was:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --filter "FullyQualifiedName~DeepSeekRa2AiLoopbackIntegrationTests|FullyQualifiedName~Ra2AiAuthoringShellBoundaryTests|FullyQualifiedName~Ra2AiRequestLifecycleTests"
```

Result:

```text
Passed: 19
Failed: 0
Skipped: 0
Duration: 953 ms
```

The loopback set covers HTTP/SSE tool assembly, required-tool enforcement, local adapter,
and A3 Preview without a live provider. Computer-control/UI automation and live-provider
calls were intentionally not run.

Final A4-R1-8B commands:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Final result:

```text
Restore: passed
Build: passed, 0 warnings, 0 errors
Tests: passed 2519, failed 0, skipped 0
Clean package: artifacts/RA2IniEditor.IDE.SourceClean.zip
Package profile: IdeOnly
Packaged file count: 1049
```

## 4. Deferred Governance Queue

Flush only at package completion, R4 failure stop, architecture conflict, handoff/review,
or context-size risk.

### Decision records to finalize

| Decision | Provisional state | Reason |
|---|---|---|
| Classify endpoint after canonical completion-URI normalization | Implemented | Prevent explicit official URL from becoming custom |
| Route edit authority locally before provider send | Implemented | Avoid ambiguous or unavailable paid requests |
| Require the one authoring tool | Implemented | Prevent Markdown fallback from masquerading as editing |
| Use a locally enforced discriminated clarification outcome over a flat provider schema | Implemented | Required tool needs a safe no-operation response without depending on provider `oneOf` support |
| Split system rules from untrusted user/context data for authoring only | Implemented | Preserve authority hierarchy without breaking advisory shape |
| Keep custom endpoints advisory-only | Preserved | Tool compatibility is not trusted |
| Run current Preview behind a UI-independent runner | Implemented | Current preview path is thread-safe; Shell retains final ownership checks |
| Never retry automatically | Preserved | User explicitly deferred retry |

### Internal API ledger candidates

| API | Kind | Intended stability |
|---|---|---|
| `DeepSeekRa2AiEndpointKind` | Internal configuration fact | Experimental |
| `Ra2AiInteractionRouteKind` and route result | Internal routing fact | Experimental |
| `Ra2AiToolChoiceMode.Required` | Internal transport contract | Experimental |
| `preview_ini_edit_plan` outcome union | Provider tool schema v2 | Stable after verification |
| `Ra2AiProposalPreparationRunner` | Internal lifecycle boundary | Experimental |
| `Ra2AiToolAdaptationOutcomeKind.NeedsClarification` | Internal non-failure result | Experimental |
| `AuthoringToolNotInvoked` | Internal required-tool failure | Experimental |

### Technical-debt watch list

- `UsesCustomEndpoint` may remain temporarily as a compatibility projection. Remove only
  after all consumers migrate; do not leave it as routing authority.
- Preview cancellation is cooperative between analysis phases. Do not claim immediate
  cancellation unless later profiling justifies deeper cancellation checks.
- Shell behavioral coverage must move beyond source-string assertions for lifecycle
  invariants, but A4-R1 must not introduce a second Shell architecture merely for tests.

## 5. Current boundaries

- Legacy was not restored or touched.
- `ShellWindow.xaml.cs` received only the approved private routing/lifecycle wiring.
- `ShellWindow.xaml`, Dock topology, menu, toolbar and AutomationIds were unchanged.
- Parser, diagnostics, Field Registry, Completion, Hover, Quick Peek, Save Preflight,
  backup/rollback, Undo/Redo, Search/Replace, and editor transaction semantics were not
  changed.
- No dependency, project file, API key, live network call, computer control, or DeepSeek
  delegation was used.

## 6. Next action

After A4-R1 closes, the recommended next phase is `AGENT-AUTHORING-1-R1 HLI-0A`:
contract the higher-level
Agent interface above this reliable current-document proposal boundary; do not broaden
to multi-file writes, automatic Apply/Save, retry, or custom-endpoint tools implicitly.
