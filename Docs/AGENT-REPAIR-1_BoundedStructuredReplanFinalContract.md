# AGENT-REPAIR-1 Bounded Structured Replan Final Contract

Status: Approved / implemented / automated verified

Date: 2026-08-25

Risk: R3 — architecture-sensitive internal orchestration and conditional provider cost

Governance: Completed through R1-1 → R1-5

Code-fact audit: `AGENT-REPAIR-1_BoundedStructuredReplanCodeFactAudit.md`

## 1. Goal

Allow explicit Work mode to correct one model-authored structured-plan failure without
asking the user to resend the whole request, while preserving all current Host safety,
preview, explicit Apply, Undo/Redo and Save boundaries.

This stage is not a general retry mechanism. It is one conditional structured replan
inside the same user request.

## 2. User-visible contract

1. Chat remains one provider call and never enters structured repair.
2. A normal successful Work request remains two provider calls.
3. Only an allowlisted, model-correctable failure after the execution stage may trigger
   one additional DeepSeek call.
4. The repair call receives the original intent and bounded evidence explaining why the
   first structured result failed. It must return a fresh complete structured tool call,
   not a patch to invalid JSON.
5. A repaired result still passes the same adapter, template compiler and canonical
   preview. No output is auto-applied or auto-saved.
6. If repair succeeds, the UI shows only the final proposal and a concise
   `已自动修正 1 次` status. The failed intermediate draft does not become conversation
   truth.
7. If repair returns a valid clarification, the UI displays it and creates no proposal.
8. If repair fails, the request stops after that attempt, restores the user's prompt and
   reports the final typed reason plus `已自动修正 1 次，未能生成可预览修改`.
9. Cancel, new request, Shell close or context invalidation stops the flow immediately.

## 3. Provider-call invariant

```text
Chat
  call 1 only

Work success
  call 1 intent/Skill/query package
  local bounded HLI queries
  call 2 structured execution
  canonical preview

Work eligible failure
  call 1 intent/Skill/query package
  local bounded HLI queries
  call 2 structured execution
  typed adaptation/preview failure
  pre-repair currency check
  call 3 bounded structured repair, non-streaming
  canonical preview again
  stop
```

Hard limits:

- maximum repair attempts per user request: `1`;
- maximum provider calls per Work request: `3`;
- intent-analysis calls: exactly `1`;
- Host context-query executions: at most the original `0..8`, exactly once;
- Skill resolution: exactly once;
- model/provider fallback: none;
- automatic Apply/Save: none.

## 4. Architecture

### 4.1 New internal orchestration owner

Introduce one focused internal coordinator under `RA2IniEditor.IDE/AI/`, named for
example `Ra2AiBoundedStructuredReplanCoordinator`. It composes rather than replaces:

- `Ra2AiAssistantPipeline`;
- `Ra2AiProposalPreparationRunner`;
- `IRa2AiPromptBuilder` and `IRa2AiClient` through the pipeline;
- a narrow Host-owned current-context recapture port;
- the repair eligibility policy.

It may return an internal result containing the final response, final proposal result,
initial/repair request diagnostics, repair decision and attempt count. It must not know
about WPF controls, message panels, file writes or Apply commands.

### 4.2 Host context recapture port

Add one internal interface or equivalent focused contract whose only responsibility is
to recapture the current document/project authoring context corresponding to the
original request context. The Shell adapter must marshal capture to the UI thread and
reuse existing capture methods.

The orchestrator performs a currency check before spending the third call and obtains a
fresh current context again before final proposal preparation. The original request
snapshot remains the plan target; a mismatch produces `RequestContextStale` and no
repair.

### 4.3 Reusable execution seed

Capture one immutable internal `Ra2AiWorkExecutionSeed` when building call 2. It carries
only the values already used by the canonical prompt builder:

- original user prompt;
- sanitized current context, bounded conversation and current subject;
- resolved capability/domain route;
- validated intent package;
- final Skill resolution;
- request-lifetime project projection;
- ordered bounded context-query results.

The seed is request-local, non-serialized and never logged as a full prompt. Repair must
rebuild through `Ra2AiPromptBuilder`; it may not concatenate or replay a raw prior HTTP
body.

## 5. Typed failure evidence

Add one internal immutable evidence carrier and propagate it through
`Ra2AiEditPlanCreationResult` and `Ra2AiEditProposalResult`. It may reference existing
typed Application enums but must not create a second semantic taxonomy.

Minimum evidence:

- high-level `Ra2AiEditProposalFailureKind`;
- source: response, adapter, template, document preview or project preview;
- optional existing template/document/project leaf failure kind;
- failed tool name when available;
- bounded, redacted model argument fragment when needed;
- safe bounded failure detail;
- project failed-document symbolic target/file name when already known.

Repair eligibility must never parse localized UI messages.

## 6. Eligibility policy

The policy is a pure internal function over typed response/proposal evidence.

### 6.1 Eligible response failure

- `Ra2AiResponseKind.AuthoringToolNotInvoked`.

### 6.2 Eligible adapter failures

- `UnsupportedTool`;
- `MultipleToolCalls`;
- `MissingArguments`;
- `InvalidArgumentsJson`;
- `UnknownArgumentProperty`;
- `DuplicateArgumentProperty`;
- `InvalidOperation`.

### 6.3 Eligible template leaf failures

Only model-correctable argument/selection failures:

- template not found or version mismatch;
- invalid, missing, unknown or duplicate arguments;
- required Section missing or wrong kind;
- project document target missing or ambiguous.

`TemplateExpansionRejected` without a preserved leaf kind is not eligible.

### 6.4 Eligible document preview leaf failures

- `InvalidPlan`;
- `UnsupportedOperation`;
- `InvalidSection`;
- `SectionNotFound`;
- `AmbiguousSection`;
- `FieldNotFound`;
- `AmbiguousField`;
- `ConflictingOperations`;
- `OverlappingChanges`;
- `NoChanges`;
- `SectionAlreadyExists`;
- `ConflictingSectionCreations`;
- `SectionClassificationMismatch`.

### 6.5 Eligible project preview failures

- `InvalidProjectPlan`;
- `DocumentNotFound`;
- `DuplicateDocumentTarget`;
- `DocumentPreviewFailed` only when its preserved document leaf kind is in section 6.4.

### 6.6 Never eligible

- provider incomplete, cancellation, timeout, network/protocol/auth/rate-limit error;
- missing/invalid configuration or unsupported endpoint/model;
- first-call intent package validation failure;
- valid `needs_clarification`;
- local route/edit unavailability;
- request context unavailable or stale;
- read-only state;
- preview/apply cancellation;
- Apply blocked or any post-proposal Apply failure;
- document/project resource limit, document-too-large or result-limit failure;
- current/candidate analysis failure;
- blocked field trust or other minimum structural safety rejection;
- unexpected failure;
- a failure after the one repair attempt.

Unknown future enum values default to not eligible.

## 7. Repair prompt contract

The canonical prompt builder receives a new optional internal repair context. For call 3
it must state:

1. this is repair attempt `1/1`;
2. original user intent, route, Skills and Host facts remain authoritative;
3. the prior structured attempt failed for the supplied typed reason;
4. return one complete valid tool call for the same capability;
5. do not broaden scope, invent another task, request Apply/Save or change authority;
6. if essential user information is truly missing, return the existing valid
   clarification outcome rather than guessing.

Provider-visible repair evidence is untrusted data. Bounds:

- safe failure detail: at most 1,024 characters;
- failed model argument fragment/provider prose: at most 4,096 characters;
- one failed tool only;
- no absolute paths, project root, credentials, document IDs or raw revisions;
- existing total prompt maximum remains 65,536 characters.

When trimming is required, remove low-priority conversation/context before repair
evidence. Existing redaction and request-preparation flags remain authoritative.

## 8. Streaming, conversation and diagnostics

- Call 2 keeps existing streaming behavior.
- Call 3 uses `IRa2AiClient.SendAsync`; no repair deltas are rendered live.
- Initial failed tool/prose content is not added as a completed assistant turn.
- Initial and repair request diagnostics are retained separately in the internal result.
- Existing diagnostics UI may append only bounded metadata: attempt count, model,
  request ID, latency and prompt character count. It must not log API keys, full prompts,
  raw document text or unredacted arguments.

## 9. Allowed implementation files

- `RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuildRequest.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs`
- `RA2IniEditor.IDE/AI/Ra2AiEditProposalContracts.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAuthoringToolAdapter.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAuthoringCoordinator.cs`
- `RA2IniEditor.IDE/AI/Ra2AiProposalPreparationRunner.cs` only if result propagation is required
- new focused internal repair/orchestration files under `RA2IniEditor.IDE/AI/`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs` only for narrow construction, context-recapture adapter and rendering wiring
- focused IDE tests, loopback tests and current documentation

## 10. Forbidden changes

- no XAML, resource dictionary, layout, AutomationId, Dock or visual redesign;
- no DeepSeek configuration/default model/fallback change;
- no retry in `DeepSeekRa2AiClient` or another transport wrapper;
- no public Application API, public DTO or persistence change;
- no parser, Field Registry data/provider priority, Completion, Hover, diagnostics,
  save preflight or semantic compiler weakening;
- no deterministic Host retargeting or rewriting of the model plan;
- no additional Skill lookup, HLI query, filesystem read, network source or shell command;
- no auto Apply, auto Save, silent file creation or asset generation;
- no legacy restoration.

## 11. Stage plan

### R1-0 — Audit and approval packet

- code-fact audit;
- final contract;
- Proposed decision and current-phase update;
- stop for user approval.

### R1-1 — Typed failure evidence and eligibility

- add internal evidence carrier;
- preserve template/document/project leaf failures;
- implement pure allowlist policy with deny-by-default behavior;
- no provider call change yet.

Review gate: all prior failure messages and proposal outcomes remain compatible; no
string matching; public API diff zero.

### R1-2 — Execution seed and repair prompt

- add immutable request-local execution seed;
- add optional repair prompt context and budgets;
- prove same route/Skills/query facts and no path leakage.

Review gate: normal call-2 prompt remains unchanged when repair context is absent.

### R1-3 — Bounded orchestrator

- add internal orchestrator and Host context-recapture port;
- perform pre-cost currency check;
- execute at most one non-streaming repair call;
- run the same proposal preparation and preview again;
- enforce cancellation and terminal stop rules.

Review gate: exact call-count tests and no transport retry.

### R1-4 — Narrow Shell integration and observability

- replace scattered Work proposal branching with one orchestrator result path;
- reuse existing proposal/message views;
- show bounded repaired/final status;
- no XAML or AutomationId change.

Review gate: Shell contains no eligibility switch, retry counter or repair prompt text.

### R1-5 — Verification and handoff

- focused unit, boundary and loopback tests;
- full Application/IDE non-UI regression;
- clean package;
- documentation/decision/status closeout;
- manual real-DeepSeek cases remain user-run unless separately authorized.

Each stage must be reviewed before continuing. A failed gate may be repaired within the
same approved boundary, but cannot be reported as passed.

## 12. Automated acceptance matrix

1. Chat performs exactly one provider call.
2. normal Work success performs exactly two calls.
3. eligible invalid call-2 tool JSON performs exactly three calls and can yield one
   proposal.
4. `AuthoringToolNotInvoked` is repaired once.
5. wrong document/Section typed preview failure is repaired using the original query
   target facts; Host does not retarget it.
6. valid clarification performs no repair.
7. timeout/network/provider/configuration failure performs no repair.
8. stale/read-only/resource/safety failure performs no repair.
9. invalid repaired response stops at three calls.
10. cancellation before or during repair yields no proposal and no later call.
11. intent analysis, Skill resolution and HLI queries execute once only.
12. repair uses the same selected model/configuration and does not stream deltas.
13. prompt limits/redaction remove paths and bound failed arguments.
14. a repaired proposal still requires explicit Apply and does not Save.
15. Shell boundary test proves no XAML change and no repair policy in presentation code.

## 13. Manual acceptance cases

After automated gates pass, the user may run with the real provider:

1. request a rules/art binding and deliberately induce an unknown argument; expect one
   automatic correction and a Project Diff;
2. request an existing art Section field edit that call 2 targets to rules; expect one
   correction to the model-authored plan, never Host retargeting;
3. disconnect network during call 2; expect immediate provider error and no third call;
4. make both execution and repair structurally invalid; expect a final failure after one
   correction only;
5. cancel while repair is pending; expect no proposal or apply state.

## 14. Verification commands

Planned minimum gates after implementation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Ra2AiBoundedStructuredReplan|FullyQualifiedName~Ra2AiAssistantPipeline|FullyQualifiedName~Ra2AiProjectAuthoringIntegration|FullyQualifiedName~Ra2AiAuthoringShellBoundary|FullyQualifiedName~DeepSeekRa2AiLoopbackIntegration"
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Real DeepSeek and computer-control testing are not part of automatic execution without
separate authorization.

## 15. Stop rules

Stop and request a new decision if implementation requires:

- more than one repair attempt or more than three Work provider calls;
- rerunning intent analysis, Skill selection or HLI queries;
- provider/model fallback or transport retry;
- public API, persistence or provider settings changes;
- XAML/UI redesign;
- weakening canonical preview/resource/safety checks;
- auto Apply/Save or arbitrary filesystem access.

## 16. Approval and completion record

The user approved this contract and authorized, in sequence, R1-1 through R1-5 within
the listed files and stop rules. That approval explicitly accepted:

- a conditional third DeepSeek call on the allowlisted path;
- its added latency and token cost;
- narrow `ShellWindow.xaml.cs` wiring;
- internal result/failure contract changes with zero planned public API diff.

R1-1 through R1-5 are implemented and automated-verified. Real DeepSeek and physical UI
acceptance remain user-run and were not part of this execution.
