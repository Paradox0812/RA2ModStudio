# AGENT-REPAIR-1 Bounded Structured Replan Code-Fact Audit

Status: Audited / contract input only

Date: 2026-08-25

Risk: R3 — internal orchestration and provider-call-count change

Implementation authorization: Not granted

## 1. Audit question

Determine whether Work mode can safely make one bounded correction attempt after the
second DeepSeek call returns a structurally invalid or locally unpreviewable edit plan,
without introducing an open retry loop, duplicating semantic authority, weakening the
canonical preview gate, or moving orchestration into WPF presentation code.

## 2. Current request path

The production path is:

```text
Shell captures authoring context and starts one request lifecycle
  -> Ra2AiAssistantPipeline
     -> provider call 1: intent / capability / Skill / context-query package
     -> bounded Host HLI queries
     -> provider call 2: structured execution, streaming
  -> Shell selects document or project request context
  -> Ra2AiProposalPreparationRunner
  -> Ra2AiAuthoringCoordinator
     -> tool adaptation / template expansion
     -> canonical document or project preview
  -> Shell renders clarification, failure, or explicit proposal
  -> user alone may Apply; Apply does not Save
```

`Ra2AiAssistantPipeline.SendWorkStreamingAsync` owns the two provider calls. It returns
the validated intent package, resolved route, Skill resolution, project projection and
bounded context-query results. It does not currently own proposal preparation and does
not retain an immutable reusable execution seed.

`ShellWindow.PrepareAndAttachAiEditProposalAsync` currently owns live context recapture,
proposal preparation and result rendering. It has no repair branch.

## 3. Existing reusable authorities

The following existing components must remain the only authorities for their domains:

| Concern | Existing authority | Reuse requirement |
|---|---|---|
| Provider transport, timeout and cancellation | `IRa2AiClient` / `DeepSeekRa2AiClient` | Reuse; do not add semantic retry here |
| Prompt construction and redaction | `IRa2AiPromptBuilder` / `Ra2AiPromptBuilder` | Reuse for repair request |
| Intent, route and selected Skills | first-call package plus Host resolution | Freeze and reuse; do not analyze again |
| Project facts | request-lifetime project projection and HLI query results | Freeze and reuse; do not query again |
| Tool JSON adaptation | `Ra2AiAuthoringToolAdapter` | Reuse unchanged as parser/admission authority |
| Template expansion | existing Application template service/compiler | Reuse; no repair-side compiler |
| Document/project preview | existing workspace and Application preview engines | Remains the only semantic acceptance gate |
| Apply/Undo/Redo/Save | existing authoring transaction path | Repair grants no new authority |
| Request cancellation | `Ra2AiRequestLifecycle` session token | All attempts share one session |

## 4. Failure facts

### 4.1 Provider response classification

`Ra2AiResponseKind` already distinguishes success, incomplete, cancellation, timeout,
provider/configuration failures, tool calls, missing required tool invocation and local
rejection. A transport failure therefore does not need a new retry taxonomy.

`AuthoringToolNotInvoked` is the only response-level failure that is potentially
repairable: transport succeeded, Work required a tool, but the model returned prose.

### 4.2 Proposal classification

`Ra2AiEditProposalFailureKind` distinguishes adapter, context, preview, apply, template
and unexpected failures. Adapter failures such as invalid JSON, unknown arguments or
multiple tool calls are model-correctable.

However, `PreviewRejected` currently collapses the canonical leaf failure enum into a
localized message. `TemplateExpansionRejected` likewise collapses most template leaf
failures. Any implementation that decides repair eligibility by matching Chinese error
text would be brittle and is rejected by this audit.

### 4.3 Canonical leaf failures already exist

The Application layer already exposes typed document, project and template failure
enums. No parallel semantic classifier is needed. The IDE needs only an internal,
non-serialized evidence carrier that propagates those existing values through
`Ra2AiEditPlanCreationResult` and `Ra2AiEditProposalResult`.

## 5. Architecture gap

Neither of the current owners is an acceptable place for a full repair loop:

- putting the loop in `DeepSeekRa2AiClient` would misclassify semantic output failure as
  transport retry and would hide extra cost;
- putting the loop directly in `ShellWindow` would spread provider orchestration,
  eligibility policy and prompt construction across WPF presentation code;
- putting proposal preview inside the existing pipeline without a narrow context-
  recapture boundary would mix stale snapshots or access Shell state off the UI thread.

The missing boundary is one internal Work-authoring orchestrator that composes the
existing pipeline, proposal runner, a narrow Host context-recapture port and one bounded
repair policy. It returns typed final state; it never renders UI or applies changes.

## 6. Existing decision conflict

Two accepted decisions explicitly hold Work to two provider calls and reject a third
provider loop:

- `AGENT-SKILL-ROUTING-2` keeps one analysis plus one execution call;
- `AGENT-CONTEXT-3` rejects a third provider query loop because of cost and failure
  surface.

Therefore `AGENT-REPAIR-1` cannot be treated as routine implementation. It requires an
explicit Proposed decision that introduces only a conditional, single repair exception.
It does not reopen Skill reading or context-query loops.

## 7. Public API and persistence impact

The required types and coordinator can remain `internal` to `RA2IniEditor.IDE`. Existing
Application Experimental failure enums are consumed but not changed. Planned public API
diff is zero. There is no persistence, settings, document format or serialized provider
history change.

If implementation requires changing an Application public contract, provider settings,
persistence schema or save/apply authority, it exceeds this audit and must stop for a
new contract.

## 8. UI and Shell impact

No XAML, layout, visual template, AutomationId or docking change is necessary. A narrow
`ShellWindow.xaml.cs` wiring change is necessary because Shell owns live document/project
capture and the existing request lifecycle. That wiring must only:

1. construct/invoke the internal orchestrator;
2. provide a UI-thread-safe recapture port over existing capture methods;
3. render the orchestrator's final typed result through existing message/proposal views.

It must not contain an attempt counter, eligibility switch, prompt composition or
provider call.

## 9. Cost and reliability finding

The safe upper bound is:

```text
Chat: 1 provider call
Work normal path: 2 provider calls
Work eligible repair path: 3 provider calls maximum
```

The repair call must use the already selected provider/model/configuration. It must be
non-streaming to avoid publishing a failed intermediate draft into conversation UI. It
must not silently upgrade to another model, re-run intent analysis, re-select Skills,
repeat HLI queries or refresh semantic authority.

## 10. Audit conclusion

A bounded structured replan is feasible with moderate implementation difficulty, but
only after typed failure evidence is preserved and a dedicated internal orchestration
boundary is introduced. A generic retry, error-text matcher, Shell-local loop or
unbounded third-call design is not reliable enough.

The companion final contract is Proposed and must be approved before runtime code is
changed.
