# AGENT-CONTEXT-3 Shared Conversation and Bounded Project Query Final Contract

Status: Approved for continuous execution by the user on 2026-08-25  
Risk: R4 — provider-visible structured contract and host orchestration  
Governance: staged; final public/debt/decision/status ledgers flush in C3-5

## 1. Goal

Make Work mode reason from one coherent request-lifetime context without increasing the
number of model calls:

1. the first intent/Skill-routing call and the second execution call receive the same
   bounded recent conversation and current subject;
2. both calls receive the same immutable projection of the captured current/rules/art
   document identities and revisions;
3. the first call may request bounded read-only Section or reference facts;
4. the Host resolves those requests locally through the existing HLI query gateway and
   supplies the results to the second call;
5. Work remains exactly two provider calls and Chat remains one provider call.

## 2. Code-fact baseline

- `ShellWindow` already captures an immutable current-document authoring snapshot and,
  when a unique pair exists, a `Ra2AutomationProjectSnapshot` for rules/art preview.
- Those snapshots currently remain outside `Ra2AiAssistantPipeline` until proposal
  preparation, so neither model call can inspect project facts.
- `Ra2AiIntentAnalysisStage` currently receives the current caret context and current
  subject, but not the bounded conversation used by the execution call.
- The HLI gateway already owns `GetSection` and `ResolveReference`; adding another parser,
  project index, filesystem reader, or Field Registry lookup path would duplicate
  authority.
- The Work pipeline already has the correct two-call shape: one non-streaming analysis
  call followed by one streaming execution call.

## 3. Allowed files

- `RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs`
- `RA2IniEditor.IDE/AI/Ra2AiIntentAnalysisStage.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuildRequest.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs`
- new focused internal files under `RA2IniEditor.IDE/AI/`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`, only for passing already-captured
  contexts and injecting the existing HLI gateway
- focused IDE tests and project documentation

## 4. Forbidden files and semantics

- no XAML, AutomationId, Shell layout, Dock, menu, toolbar, or visual changes;
- no parser, Field Registry data/provider priority, Hover, Quick Peek, Diagnostics,
  Completion, save preflight, preview compiler, apply, save, undo, or redo changes;
- no public Application HLI API expansion in this phase;
- no arbitrary path, directory enumeration, raw disk read, network query, shell command,
  or third provider call;
- no automatic application or persistence of any model output.

## 5. Data contract

### 5.1 Context source set

The Shell passes a request-local source set containing the current-document authoring
context and the optional rules/art project authoring context. It is not persisted.

### 5.2 Provider-visible project projection

The Host derives one immutable projection containing only:

- stable symbolic target: `current`, `rules`, or `art`;
- display file name;
- document version;
- Field Registry revision;
- project session/revision metadata when project-scoped.

Absolute paths, project root, document text, credentials, and mutable session objects are
never serialized into prompts.

### 5.3 Model-requested read-only queries

The first call may return `context_queries` with at most eight entries. Supported kinds:

- `get_section`: symbolic target + Section name + optional occurrence;
- `resolve_reference`: symbolic target + Section/key + optional occurrences and bounded
  reference index.

Unused optional values are represented by empty key and `-1` occurrence sentinels. The
Host validates identifiers, counts, indices, and target aliases. Unknown or unavailable
targets produce bounded evidence failures; they never fall back to paths.

### 5.4 Query results

Results are immutable, ordered like the accepted requests, bounded by item/text budgets,
and explicitly marked as untrusted INI data. Section results contain only the canonical
HLI Section fact, with a bounded field list. Reference results contain only the canonical
HLI resolution fact. Failures remain typed evidence and do not silently abort the second
provider call.

## 6. Orchestration invariants

```text
Work:
  capture source set once
  -> derive shared projection once
  -> provider call 1 (conversation + subject + projection + Skill manifest)
  -> validate intent/query package
  -> local HLI read-only queries (0..8)
  -> provider call 2 (same shared context + Skill bodies + query results)
  -> existing preview/admission/apply flow

Chat:
  existing single provider call; no model-requested context-query loop
```

Cancellation is checked before and during local queries. The captured snapshot is never
refreshed between provider calls, preventing mixed revisions. Query resolution grants no
edit/apply/save authority.

## 7. Compatibility

- Existing pipeline overloads remain source-compatible and behave as before when no
  context source set or gateway is supplied.
- Older valid intent packages without `context_queries` are accepted as an empty list;
  the advertised tool schema requires the field for new provider calls.
- No public API or persistence format changes.

## 8. Verification gates

Focused tests must prove:

1. both Work requests contain the same bounded conversation and project projection;
2. a valid Section query is resolved through the HLI gateway and appears only in the
   second request;
3. arbitrary targets/paths and malformed query shapes are rejected or returned as safe
   local evidence;
4. query/result budgets and cancellation are enforced;
5. Work performs exactly two provider calls; Chat performs one;
6. missing project context preserves current behavior;
7. Shell wiring captures once and does not alter proposal/apply semantics.

Final gates:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 9. Stop rules

Stop successful completion if implementation requires a third provider call, arbitrary
filesystem access, a new project snapshot owner, a public HLI API change, XAML/UI edits,
or changes to preview/apply/save/undo semantics. A failing mandatory gate may be diagnosed
and repaired within this contract, but may not be reported as passed.

