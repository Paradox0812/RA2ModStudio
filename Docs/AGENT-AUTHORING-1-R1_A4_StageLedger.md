# AGENT-AUTHORING-1-R1 A4 Stage Ledger

Last updated: 2026-07-28

## Final package result

| Stage | Goal | Verification | State |
|---|---|---|---|
| A4-0 | Approved contract persisted | Document review | Completed |
| A4-1 | Tool DTO, request, response and stream-event contracts | Build; 19 targeted tests | Completed |
| A4-2 | DeepSeek request serialization and SSE tool-call assembly | Build; 103 targeted tests | Completed |
| A4-3 | Advisory/authoring capability gate and prompt rules | Build; 43 targeted tests | Completed |
| A4-4A | Strict tool argument adapter | Build; 22 targeted tests | Completed |
| A4-4B | Coordinator authority and conditional preview discard | Build; 24 targeted tests | Completed |
| A4-5 | Proposal UserControl and ViewModel | Build; 23 targeted tests | Completed |
| A4-6 | Shell activation and lifecycle wiring | Build; 40 Shell/UI boundary tests | Completed |
| A4-7 | Full verification and documentation flush | Restore/build; 2486 full tests; clean package | Completed |

The implementation is active only for a ready official endpoint plus a successfully
captured editable current-document snapshot. Existing Pipeline overloads, custom
endpoints, read-only states and ordinary chat remain AdvisoryOnly.

## Verification matrix

| Gate | Result |
|---|---|
| DTO/response/request invariants | Passed |
| SSE fragmentation, mixed content/tool and malformed protocol | Passed |
| Ordinary-chat request-shape regression | Passed |
| Strict adapter and bounded operation validation | Passed |
| Snapshot currency, one-active proposal and apply policy | Passed |
| ViewModel, AutomationId and frozen Shell XAML boundaries | Passed |
| IDE-only restore | Passed; projects already current |
| IDE-only Debug build | Passed; 0 errors, one pre-existing CS8602 warning in BuiltIn test source |
| Full non-UI tests | Passed 2486/2486 |
| Clean source package | Passed |

No UI automation was launched. The user requested manual product acceptance after
implementation; the release checklist now contains the bounded official-endpoint
proposal/apply/stale/custom-endpoint scenarios.

## Deferred Governance Queue

### PublicApiLedger finalized entries

| API | Kind | Stability | Reason | Tests |
|---|---|---|---|---|
| `preview_ini_edit_plan` | Provider tool schema | Stable | Single bounded proposal tool | Client/adapter contract tests |
| `Ra2AiResponseKind.ToolCalls` | Internal response state | Experimental | Preserve tool calls outside text streaming | Response/client tests |
| `Ra2AiToolCall` | Internal transport DTO | Experimental | Complete unparsed provider call | Tool/client tests |
| `Ra2AiEditProposal` and failure taxonomy | Internal authoring result | Experimental | Explicit safe proposal lifecycle | Adapter/coordinator tests |
| `IRa2IniAuthoringWorkspace.TryDiscardActivePreview` | Internal interface method | Stable | Prevent stale UI cards from clearing newer previews | Workspace tests |

### DecisionLog entries

| Decision | Status | Reason |
|---|---|---|
| Expose one preview-only tool; never expose Apply or Save | Accepted for A4 | Preserve IDE authority |
| Keep custom endpoints advisory-only | Accepted for A4 | Tool compatibility is not guaranteed |
| Keep existing overloads advisory-only until Shell activation | Accepted | Dark launch and rollback |

### Technical debt

None introduced by A4. The pre-existing nullable warning in
`BuiltInFieldRegistryPackLoaderTests.cs` remains outside this package.
