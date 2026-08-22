# AGENT-AUTHORING-1-R1 A4-R1 — Reliable Structured Edit Contract

Status: Proposed final contract; awaiting explicit user confirmation.

Date: 2026-08-20

Risk: R4 — provider request shape, editing authority routing, asynchronous lifecycle,
and Shell integration are changed together. Implementation must not start before this
contract is confirmed.

## 1. Relationship to the completed A4 package

`AGENT-AUTHORING-1-R1 A4` remains a completed historical implementation. A4-R1 does
not erase its verification record. It selectively supersedes these A4 behaviors:

- endpoint identity is no longer inferred from whether `DEEPSEEK_BASE_URL` is set;
- explicit editing requests no longer depend on `tool_choice=auto`;
- authoring and advisory requests no longer share one undifferentiated prompt shape;
- a required-tool authoring response may not silently fall back to ordinary Markdown;
- proposal preparation and Shell attachment gain an explicit asynchronous ownership
  boundary;
- authoring refusals and clarification requests become typed local outcomes.

All A1-A3 snapshot, preview, apply, transaction, undo, dirty-state, and save authority
remain authoritative.

## 2. Problem statement

The current code can complete a DeepSeek request successfully while returning only an
INI code example instead of an actionable proposal card. The confirmed causes are:

1. `UsesCustomEndpoint` is currently true whenever `DEEPSEEK_BASE_URL` is non-empty,
   including when it explicitly equals the official endpoint.
2. Shell enables edit-preview capability from configuration/document state rather than
   from a deterministic request-intent route.
3. Authoring currently uses `tool_choice=auto`, so the provider may return ordinary
   assistant text.
4. stable draft output rules are appended unconditionally and encourage Markdown/INI
   draft output even for editing requests.
5. fixed application rules and untrusted current INI context are currently serialized
   at the same user-message privilege.
6. Shell finalizes streaming before asynchronous local preview preparation and can lose
   terminal ownership when cancellation or supersession occurs between those steps.
7. source-string Shell tests do not prove stale-result rejection or exactly-once terminal
   behavior.

## 3. Functional goal

For a clear, supported request such as:

```text
把当前文件 [E1] 下的 Strength 修改为 150
```

the system must deterministically follow:

```text
UI-thread immutable capture
  -> local intent route: EditExplicit
  -> official DeepSeek authoring request
  -> required preview_ini_edit_plan call
  -> strict local outcome/operation validation
  -> background-safe A3 Preview
  -> UI-thread active-generation check
  -> inline proposal card
  -> explicit user Apply
  -> A3 final currency check
  -> one existing editor transaction
```

At no point may the model apply, save, select a file, supply revisions, or mutate editor
text directly.

## 4. Non-goals

A4-R1 does not add:

- automatic retry;
- automatic apply or save;
- multi-file editing;
- raw text patching;
- section deletion, key deletion, rename, move, or reorder operations;
- custom-endpoint tools;
- cross-session proposal persistence;
- proposal rebasing;
- a second parser, diagnostic engine, field lookup path, editor transaction, or undo
  implementation;
- an API-key UI, new dependency, project-file change, Dock redesign, global theme change,
  or general AI panel redesign.

## 5. Authority and data ownership

The following facts are created locally and must never be accepted from provider JSON:

```text
DocumentId
source path
EditRevision
FieldRegistryRevision
PlanId
PreviewId
Origin
confirmation state
apply/save flags
candidate text
diagnostic comparison
```

The provider may provide only a bounded outcome payload and, for a proposal outcome,
bounded `upsert_field` or `replace_field_value` operations.

The captured `Ra2AiAuthoringRequestContext.Snapshot` is the sole request authority. A
fresh current snapshot is compared against it before preview. A3 performs the final
currency check immediately before apply. No automatic rebase is permitted.

## 6. Endpoint identity contract

### 6.1 Canonical classification

Configuration must expose a typed internal endpoint classification:

```csharp
internal enum DeepSeekRa2AiEndpointKind
{
    Official = 0,
    Custom,
    Invalid
}
```

`UsesCustomEndpoint` may remain temporarily as a compatibility projection, but Shell
authoring decisions must use `EndpointKind`.

### 6.2 Official endpoint rule

Classification is performed only after the existing base URL validation and
chat-completions normalization. The endpoint is `Official` only when the final request
URI is semantically equal to:

```text
https://api.deepseek.com/chat/completions
```

Equality requires HTTPS, host `api.deepseek.com`, default HTTPS port, normalized path,
and no userinfo, query, or fragment. Host and scheme comparisons are case-insensitive;
the normalized path comparison is ordinal-ignore-case. Explicitly setting the official
base URL therefore remains official. Every other valid HTTPS or allowed loopback endpoint
is `Custom`; invalid input is `Invalid`.

Normalization logic must have one implementation. Factory code must not duplicate URI
path construction.

## 7. Deterministic interaction routing

### 7.1 Route kinds

```csharp
internal enum Ra2AiInteractionRouteKind
{
    Advisory = 0,
    EditExplicit,
    EditAmbiguous,
    EditUnavailable
}
```

An internal pure resolver receives the visible user prompt plus an immutable capability
fact. It performs no network, file, WPF, provider, parser, or field-registry access.

### 7.2 Priority and examples

Routing priority is:

```text
explicit negation/advisory wording
  > explicit current-document edit wording
  > ambiguous edit-like wording
  > advisory fallback
```

Examples:

| Prompt | Route |
|---|---|
| `解释 Strength` | Advisory |
| `不要修改，只给代码示例` | Advisory |
| `把当前文件 [E1] 下的 Strength 修改为 150` | EditExplicit |
| `将当前文档 Primary 设置为 M60` | EditExplicit |
| `Strength 150` | EditAmbiguous |
| `优化一下这个单位` | EditAmbiguous |

The first implementation is intentionally conservative. Unknown phrasing must not gain
edit authority by default.

### 7.3 Route behavior

- `Advisory`: send the existing advisory request without tools.
- `EditExplicit`: send an authoring request only when the official endpoint is ready and
  an editable current-document snapshot was captured.
- `EditAmbiguous`: do not call the provider; preserve the prompt and ask the user to state
  the exact section/key/value or explicitly request advisory output.
- `EditUnavailable`: do not call the provider; preserve the prompt and state the safe
  reason, such as no editable document, missing configuration, unsupported/custom
  endpoint, or unavailable snapshot.

No local route may consume paid provider capacity when it has already determined that a
safe edit proposal cannot be produced.

## 8. Provider tool contract

The provider still sees exactly one function tool:

```text
preview_ini_edit_plan
```

Its parameters use one provider-compatible flat JSON Schema object with required
`outcome`, optional branch fields, and `additionalProperties=false` on every object.
The local adapter, rather than an optional provider-side `oneOf` implementation, enforces
the strict discriminated union. This avoids making request acceptance depend on an
undocumented provider JSON Schema subset.

### 8.1 Proposal variant

```json
{
  "outcome": "proposal",
  "summary": "string, 1..512 characters",
  "operations": [
    {
      "kind": "upsert_field | replace_field_value",
      "section": "string, 1..256 characters",
      "key": "string, 1..256 characters",
      "value": "string, 0..8192 characters"
    }
  ]
}
```

`operations` contains 1..128 items.

### 8.2 Clarification variant

```json
{
  "outcome": "needs_clarification",
  "message": "string, 1..512 characters"
}
```

The two semantic variants are mutually exclusive. For `proposal`, `summary` and
`operations` are required and `message` is forbidden. For `needs_clarification`,
`message` is required and `summary`/`operations` are forbidden. A clarification result
never creates a plan, preview, active proposal, or Apply affordance.

### 8.3 Strict local validation

The adapter continues to reject:

- duplicate or unknown properties;
- comments, trailing commas, excessive JSON depth, oversize arguments, invalid strings,
  and NUL characters;
- missing or mixed discriminator fields;
- unknown tools and more than one tool call;
- unknown operation kinds and out-of-range operation counts;
- model-supplied identity, revision, origin, apply, save, or confirmation properties.

The existing domain constructors remain the final structural validators.

## 9. Request construction and message roles

### 9.1 Advisory compatibility

The advisory path keeps its current request serialization shape: no `tools`, no
`tool_choice`, and no forced message-role migration in A4-R1. This protects established
chat, streaming, cancellation, and request-shape tests.

### 9.2 Authoring request

The authoring path must use:

```text
tools = [preview_ini_edit_plan]
tool_choice = required
```

The fixed application/authority rules are serialized as a `system` message. The visible
user request and all bounded current-document, conversation, field-evidence, and
diagnostic context are serialized as `user` content and explicitly marked untrusted.

Authoring must omit ordinary Markdown response requirements and
`AppendStableDraftOutputRules`. It must instruct the provider to return exactly one tool
call using one of the two declared outcomes.

Only the authoring request path changes message roles in this stage. The transport must
not synthesize tool-result messages because the IDE performs preview locally and does not
run a second provider turn.

## 10. Provider response rules

### 10.1 Required tool not invoked

If an authoring request completes with ordinary assistant text and no complete tool call,
the result is a typed local failure:

```text
AuthoringToolNotInvoked
```

It is not successful advisory output. The prompt is restored, no Preview is created, and
there is no automatic retry. Any returned text may be shown only as visibly incomplete,
non-authoritative provider output and must not enter conversation context as an applied
or accepted answer.

### 10.2 Mixed text and tool call

When one valid tool call and free text are both present, the tool call is the sole edit
authority. Free text cannot alter operations, summary, candidate text, policy, or apply
state. Conversation history stores a local bounded proposal summary, not raw provider
arguments, candidate text, or provider free text as applied state.

### 10.3 Clarification

`needs_clarification` is shown as a normal local assistant clarification message. The
original prompt is restored for editing. No proposal card is added and no preview is
retained. It is a successful adapter outcome with no edit plan, not a failure.

## 11. Proposal preparation and lifecycle

### 11.1 Thread-affinity finding

Read-only audit confirmed that the current `Ra2IniAuthoringWorkspace.Preview` path uses
immutable snapshots and local parser/semantic/diagnostic services. It has no WPF,
Dispatcher, network, or file-I/O dependency. Provider snapshots are stable and their lazy
caches are lock-protected. Preview may therefore run on a background thread in the current
implementation.

Cancellation is cooperative between analysis phases; A4-R1 does not promise immediate
preemption inside a single analysis call.

### 11.2 Runner boundary

Introduce one internal proposal-preparation runner that owns the background invocation
and always returns a typed terminal result. Shell must not pass the request token directly
to `Task.Run` scheduling in a way that can prevent the delegate from starting without a
terminal result.

The runner guarantees:

- exactly one terminal result for each preparation attempt;
- cancellation before start, during preview, and after preview are distinguishable only
  through safe typed outcomes, not leaked exceptions;
- unexpected exceptions become `UnexpectedFailure` with sanitized text;
- it does not access WPF controls or mutate Shell state.

### 11.3 UI attachment authority

Shell captures a monotonically increasing request/proposal generation. After awaiting the
runner, it attaches a card only when all are still true:

```text
the request is the current active request
the generation still matches
the captured document identity/revisions/text still match
the coordinator still owns the returned proposal
the AI panel/chat lifecycle has not invalidated the request
```

Otherwise the preview is discarded and the stale result is not rendered as ready.

Streaming completion and proposal preparation together form one request lifecycle. Busy,
cancel, prompt restoration, and terminal message state must each settle exactly once.

Starting a new send, clearing chat, switching/reloading/mutating the document, changing
field-registry revision, or closing Shell invalidates active preparation and proposal.
Hiding the AI Dock continues not to invalidate an already ready proposal.

Chat trimming must not remove the sole visible representation of a still-active proposal.
The implementation may pin the active card or invalidate/discard it before trimming; it
may not leave a hidden active authority object.

## 12. Failure taxonomy additions

Add only the minimum failure state required by this contract:

```text
AuthoringToolNotInvoked
```

Existing `RequestContextUnavailable`, `RequestContextStale`, `PreviewCancelled`,
`PreviewRejected`, `UnexpectedFailure`, and transport failure kinds remain authoritative.
Clarification is represented by a distinct adapter outcome kind rather than a failure
kind.
Safe UI text must never include raw prompt, source text, provider body, tool arguments,
API key, Authorization header, environment values, or absolute paths.

## 13. UI contract

A4-R1 keeps the existing inline proposal card. It does not introduce a modal, a new Dock
pane, or general visual redesign.

The following AutomationIds must remain unchanged:

```text
AiAssistant.PromptBox
AiAssistant.GenerateButton
AiAssistant.ConfigurationStatus
AiAssistant.RestorePromptButton
AiAssistant.RestorePromptStatus
AiAssistant.EditProposalCard
AiAssistant.EditProposalCard.Status
AiAssistant.EditProposalCard.Summary
AiAssistant.EditProposalCard.OperationList
AiAssistant.EditProposalCard.DiagnosticSummary
AiAssistant.EditProposalCard.ApplyButton
AiAssistant.EditProposalCard.DismissButton
AiAssistant.EditProposalCard.ResultMessage
```

The proposal card must show the locally validated summary, operation list, diagnostic
delta, field-trust risk, and explicit Apply/Dismiss actions. It must never present provider
free text as proof that a change is ready or applied.

## 14. Internal API ledger

No public API is added or changed. Candidate internal changes are:

| API | Change | Stability |
|---|---|---|
| `DeepSeekRa2AiEndpointKind` | New typed endpoint identity | Experimental internal |
| `DeepSeekRa2AiConfigurationSnapshot.EndpointKind` | New immutable fact | Experimental internal |
| `UsesCustomEndpoint` | Compatibility projection; no longer routing authority | Deprecated internal projection |
| `Ra2AiInteractionRouteKind` / route result | New pure local routing result | Experimental internal |
| `Ra2AiToolChoiceMode.Required` | New request serialization mode | Experimental internal |
| `Ra2AiRequest` authoring system/user content | Extend without changing advisory shape | Experimental internal |
| tool outcome discriminator | Replace proposal-only root payload | Provider contract v2 |
| `Ra2AiProposalPreparationRunner` | New UI-independent async boundary | Experimental internal |
| adapter outcome kind | Distinguish Proposal / NeedsClarification / Failed | Experimental internal |
| `AuthoringToolNotInvoked` | Typed required-tool contract failure | Experimental internal |

If implementation reveals that any public API is required, work stops and returns to a
new contract revision.

### 14.1 Reuse Scan

Search terms used:

```text
Router / Route / IntentResolver / InteractionRoute
Capability / CurrentDocumentEditPreview
PreparationRunner / Task.Run
Ra2AiToolChoiceMode
Ra2AiEditProposalFailureKind
Ra2AiAuthoringRequestContext
```

Existing canonical pieces to extend and reuse:

- `Ra2AiCapabilityMode` remains the provider capability fact; no second capability enum
  is added.
- `Ra2AiIntent` remains the prompt task intent; interaction routing is separate because
  it governs edit authority rather than content category.
- `Ra2AiAssistantPipeline`, `Ra2AiPromptBuilder`, `Ra2AiRequest`,
  `Ra2AiAuthoringToolCatalog`, `Ra2AiAuthoringToolAdapter`, and
  `Ra2AiAuthoringCoordinator` are extended in place.
- `Ra2AiAuthoringRequestContext`, `Ra2AuthoringSnapshot`, A3 Preview/Apply, and the
  existing editor transaction are reused unchanged as authority.
- .NET `Uri`/`UriBuilder`, `CancellationToken`, and `Task` remain the platform
  primitives; no helper dependency is introduced.

No existing deterministic interaction router or UI-independent proposal scheduling
boundary was found. Therefore exactly two intentional production abstractions are
created across separate cards: one pure interaction-route unit and one proposal runner.
No parallel parser, endpoint normalizer, workspace, or lifecycle coordinator is allowed.

### 14.2 Data Model Check

| Concept | Primary owner | Lifetime/mutation | Identity/query | Serialization |
|---|---|---|---|---|
| Endpoint kind | configuration snapshot | one environment capture; immutable | queried by Shell route setup/status | never serialized to provider |
| Interaction route | one send attempt | computed once; immutable | consumed by Shell/Pipeline by route kind | never persisted |
| Provider tool outcome | untrusted transport response until adapted | one response; immutable after parsing | discriminator `outcome` | provider JSON only |
| Authoring request context | local authoring capture | one request; immutable | document/edit/registry revisions | never provider-owned |
| Preparation result | runner/coordinator | one preparation attempt; terminal | request/proposal generation and proposal id | never persisted |
| Active proposal | coordinator authority | until apply/dismiss/invalidate/supersede | proposal id + preview id | never conversation state |

Required invariants:

- every new fact has exactly one owner;
- no provider payload can become local identity or confirmation authority;
- route and endpoint facts are immutable per request;
- clarification and failure results cannot carry an active proposal;
- a successful preparation result must correspond to the coordinator's active proposal;
- non-serialized derived state is discarded at lifecycle invalidation points;
- all failure paths are typed when callers need different UI behavior.

### 14.3 Exact internal signatures and responsibilities

The implementation may refine parameter names, but it must preserve these shapes and
ownership boundaries:

```csharp
internal enum Ra2AiEditAvailabilityKind
{
    Available = 0,
    MissingConfiguration,
    UnsupportedEndpoint,
    NoEditableDocument,
    SnapshotUnavailable
}

internal readonly record struct Ra2AiInteractionRoute(
    Ra2AiInteractionRouteKind Kind,
    Ra2AiCapabilityMode CapabilityMode,
    Ra2AiEditAvailabilityKind EditAvailability);

internal static class Ra2AiInteractionRouter
{
    internal static Ra2AiInteractionRoute Resolve(
        string userPrompt,
        Ra2AiEditAvailabilityKind editAvailability);
}
```

`Resolve` only normalizes bounded visible prompt text and applies table-driven lexical
rules. An explicit edit requires an edit action plus a concrete current-document target
and assignment/value expression; a bare key/value pair remains ambiguous. The resolver
does not manufacture user-facing error text.

```csharp
internal static bool TryResolveChatCompletionsEndpoint(
    string value,
    out Uri? endpoint);

internal static DeepSeekRa2AiEndpointKind ClassifyEndpoint(Uri endpoint);
```

`TryValidate` and the factory both reuse this normalization entry point. `ClassifyEndpoint`
accepts only an already validated, final chat-completions URI.

```csharp
internal enum Ra2AiToolAdaptationOutcomeKind
{
    Proposal = 0,
    NeedsClarification,
    Failed
}
```

The existing `Ra2AiEditPlanCreationResult` is extended with the outcome kind and preserves
its existing `Succeeded` compatibility projection (`true` only for Proposal). It carries
exactly one of Plan, clarification Message, or FailureKind/Message.

```csharp
internal sealed class Ra2AiProposalPreparationRunner
{
    internal Task<Ra2AiEditProposalResult> PrepareAsync(
        Ra2AiAuthoringRequestContext requestContext,
        Ra2AuthoringSnapshot currentSnapshot,
        Ra2AiResponse response,
        CancellationToken cancellationToken);
}
```

The runner schedules its delegate without supplying the request token as the scheduling
token, then observes that token inside the delegate/coordinator. It catches and converts
terminal cancellation/unexpected exceptions. It never captures controls, Dispatcher, or
mutable Shell collections.

### 14.4 Exact API Inventory for any later delegation

No DeepSeek implementation delegation is authorized by this contract, so no worker task
package is generated. If bounded test boilerplate is later delegated, the package must
include verbatim current signatures from:

```text
Ra2AiCapabilityMode
Ra2AiIntent
Ra2AiPromptBuildRequest
Ra2AiRequest
Ra2AiResponse / Ra2AiResponseKind
Ra2AiToolChoiceMode / Ra2AiToolDefinition / Ra2AiToolCall
Ra2AiEditPlanCreationResult / Ra2AiEditProposalResult
Ra2AiAuthoringRequestContext
Ra2AuthoringSnapshot
Ra2AiAuthoringCoordinator.PrepareProposal
the exact existing test fake/client constructors used by the target test file
```

The worker may not guess convenience APIs, create replacement fakes, or alter any
signature.

## 15. Allowed implementation files by Task Card

Every card remains within the repository budget of at most five modified files.

### A4-R1-1 Endpoint identity

```text
RA2IniEditor.IDE/AI/DeepSeekRa2AiClientOptions.cs
RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs
RA2IniEditor.IDE/AI/DeepSeekRa2AiConfigurationSnapshot.cs
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientFactoryTests.cs
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs
```

### A4-R1-2 Intent routing

```text
RA2IniEditor.IDE/AI/Ra2AiInteractionRoute.cs (new; enum, readonly route fact, resolver)
RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs
RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs
```

### A4-R1-3A Tool schema and adapter outcomes

```text
RA2IniEditor.IDE/AI/Ra2AiAuthoringToolCatalog.cs
RA2IniEditor.IDE/AI/Ra2AiAuthoringToolAdapter.cs
RA2IniEditor.IDE/AI/Ra2AiEditProposalContracts.cs
RA2IniEditor.Tests/IDE/Ra2AiToolContractTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAuthoringToolAdapterTests.cs
```

### A4-R1-3B Required-tool request

```text
RA2IniEditor.IDE/AI/Ra2AiToolContracts.cs
RA2IniEditor.IDE/AI/Ra2AiRequest.cs
RA2IniEditor.IDE/AI/DeepSeekRa2AiClient.cs
RA2IniEditor.Tests/IDE/Ra2AiClientTests.cs
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs
```

### A4-R1-4 Authoring prompt privilege split

```text
RA2IniEditor.IDE/AI/Ra2AiPromptBuildRequest.cs
RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs
RA2IniEditor.IDE/AI/Ra2AiRequest.cs
RA2IniEditor.Tests/IDE/Ra2AiPromptBuilderTests.cs
RA2IniEditor.Tests/IDE/DeepSeekRa2AiClientTests.cs
```

### A4-R1-5 Response enforcement

```text
RA2IniEditor.IDE/AI/Ra2AiResponse.cs
RA2IniEditor.IDE/AI/Ra2AiResponseKind.cs
RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs
RA2IniEditor.Tests/IDE/Ra2AiClientTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAssistantPipelineTests.cs
```

### A4-R1-6A Proposal runner

```text
RA2IniEditor.IDE/AI/Ra2AiProposalPreparationRunner.cs (new)
RA2IniEditor.IDE/AI/Ra2AiAuthoringCoordinator.cs
RA2IniEditor.IDE/AI/Ra2AiEditProposalContracts.cs
RA2IniEditor.Tests/IDE/Ra2AiProposalPreparationRunnerTests.cs (new)
RA2IniEditor.Tests/IDE/Ra2AiAuthoringCoordinatorTests.cs
```

### A4-R1-6B Shell lifecycle wiring

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/AI/Ra2AiProposalPreparationRunner.cs
RA2IniEditor.Tests/IDE/Ra2AiAuthoringShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2AiRequestLifecycleTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
```

### A4-R1-7 Proposal presentation and history ownership

```text
RA2IniEditor.IDE/ViewModels/AI/Ra2AiEditProposalViewModel.cs
RA2IniEditor.IDE/Views/AI/Ra2AiEditProposalView.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/Ra2AiEditProposalViewModelTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAuthoringShellBoundaryTests.cs
```

### A4-R1-8A Integration verification

```text
RA2IniEditor.Tests/IDE/DeepSeekRa2AiLoopbackIntegrationTests.cs
RA2IniEditor.Tests/IDE/Ra2AiAuthoringShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/Ra2AiRequestLifecycleTests.cs
```

### A4-R1-8B Documentation and package closure

```text
Docs/AGENT-AUTHORING-1-R1_A4_R1_StageLedger.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/UserGuide.md
Docs/ReleaseChecklist.md
```

File names may be corrected to an already-existing equivalent before a card begins, but
scope may not expand beyond five files without splitting the card and recording why.

## 16. Forbidden files and semantic boundaries

Unless a later separately confirmed contract says otherwise, do not modify:

```text
ShellWindow.xaml
AvalonDock layout or persistence
global themes, menus, toolbar, Project Explorer, bottom tabs, status bar
Core or IDE parser semantics
diagnostic rules or ordering
Field Registry data, priority, lookup, Hover, Quick Peek, Completion
Save Preflight, writer, backup, rollback, Undo/Redo
Search/Replace
solution/project files or dependencies
legacy projects
```

`ShellWindow.xaml.cs` is approved only for the bounded A4-R1 request/proposal lifecycle
wiring described above.

## 17. DeepSeek delegation decision

No A4-R1 architecture, routing, authority, network request, cancellation, or Shell lifecycle
implementation is delegated to DeepSeek. These are R3/R4 integration responsibilities.
Bounded test boilerplate may be reconsidered only after exact APIs exist, with a separate
task package and Exact API Inventory; it is not part of the current plan.

## 18. Verification contract

### 18.1 Required automated cases

1. Empty/default/explicit official endpoint all classify as Official after normalization.
2. Official full completion URL remains Official; custom HTTPS and loopback HTTP classify
   as Custom; malformed endpoint classifies as Invalid.
3. advisory, explicit-edit, ambiguous-edit, negated-edit, and unavailable routes are
   deterministic, including Chinese punctuation and whitespace variants.
4. Advisory serialized request remains byte-for-byte structurally equivalent to its
   pre-R1 JSON shape.
5. Authoring request contains separate system/user messages, exactly one tool, and
   `tool_choice=required`; stable draft rules are absent.
6. Both tool union variants pass; mixed, duplicate, unknown, missing, oversize, and invalid
   variants fail safely.
7. Required-tool plain text becomes `AuthoringToolNotInvoked`, restores prompt, and creates
   no preview/card.
8. Mixed text/tool accepts only the validated tool authority.
9. Cancellation before runner start, during preview, and after preview settles exactly
   once without unobserved `OperationCanceledException`.
10. stale/superseded preparation cannot attach a ready card and discards its preview.
11. chat clear, new send, document mutation/switch/reload, registry revision change, and
    Shell close invalidate preparation/proposal correctly.
12. chat trimming cannot hide an active proposal authority object.
13. Apply remains explicit, in-memory, one transaction, dirty, undoable, and unsaved.
14. Existing AutomationIds remain present and unique.

### 18.2 Verification commands

Use the smallest credible filter after each card. At package closure run once:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Do not repeat a successful full build/test/package without relevant input changes.

### 18.3 Optional live-provider gate

Live testing is not required to prove local authority. If explicitly enabled after all
deterministic tests pass:

- use the configured DeepSeek V4 Flash model only;
- use the official endpoint and an environment-provided API key;
- perform at most two bounded calls: one advisory and one explicit edit;
- disable retry;
- use a disposable unsaved test document;
- verify no auto-apply and no auto-save;
- never log credentials, source text, request bodies, response bodies, or tool arguments.

## 19. Manual acceptance

After automated verification, the user should verify:

1. open one editable INI document and request an exact section/key/value edit;
2. confirm a proposal card appears instead of a code sample;
3. inspect before/after operation and diagnostics information;
4. dismiss once and verify text is unchanged;
5. repeat, Apply once, verify dirty state and Undo, and verify the file is not saved;
6. mutate the document during a request and verify no stale card becomes ready;
7. issue an ambiguous edit phrase and verify no provider call is made;
8. issue `不要修改，只解释 Strength` and verify ordinary advisory behavior;
9. explicitly set `DEEPSEEK_BASE_URL=https://api.deepseek.com` and verify it remains an
   official endpoint;
10. choose a custom endpoint and verify editing is locally unavailable.

## 20. Stop conditions and rollback

Stop immediately and return to contract review if implementation requires:

- a public API or new dependency;
- modification of a forbidden semantic surface;
- WPF access from the preview runner;
- weakening snapshot/revision checks;
- allowing custom endpoints to edit;
- accepting plain text as edit authority;
- more than one editor transaction or any automatic save;
- more than five files in one Task Card without a documented split.

Rollback is card-local. The safe feature rollback is to route all prompts to the existing
advisory path and disable authoring capability; it must not remove A1-A3 or historical A4
types merely to recover the UI.

## 21. Approval gate

This document is the final proposed A4-R1 contract. Implementation begins only after the
user explicitly confirms:

```text
确认 AGENT-AUTHORING-1-R1 A4-R1 最终契约
```
