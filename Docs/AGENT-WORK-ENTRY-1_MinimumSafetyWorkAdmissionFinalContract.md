# AGENT-WORK-ENTRY-1 Minimum-Safety Work Admission Final Contract

Status: Approved by explicit user implementation request / self-reviewed

Date: 2026-08-25

Risk: R4 — provider-visible tool contract and Work orchestration boundary

## 1. Problem statement

The production Work entry currently treats descriptive model metadata as execution
authority. A single unknown root property, domain string, completion value, oversized
recommendation list, omitted query placeholder, or malformed optional query rejects the
entire first-call package. The pipeline then discards the parser diagnostic and shows a
generic local rejection. Existing green tests mostly use hand-authored ideal JSON and do
not establish compatibility with realistic DeepSeek tool arguments.

This contradicts the accepted model-owned authoring boundary: DeepSeek owns INI content
decisions; the Host owns captured targets, preview authority, explicit Apply, Save
separation and resource limits.

## 2. Authoritative path

There remains one production path and one parser per provider stage:

```text
Work UI
  -> Ra2AiAssistantPipeline
  -> Ra2AiIntentAnalysisStage (one tolerant admission parser)
  -> bounded read-only Host queries
  -> Ra2AiSemanticRetrievalStage (one tolerant refinement parser)
  -> existing execution tool / adapter / canonical preview
  -> explicit user Apply; never automatic Save
```

No regex authoring classifier, Field Registry enum, template profile, diagnostic result,
or UI code may veto a successfully parsed model intent before the canonical preview
boundary.

For production AI Work, Field Registry trust and diagnostics remain visible review
evidence after Preview but do not block the user's explicit Apply. Canonical parsing,
safe identifiers, resource limits, immutable snapshot identity, atomicity, stale
rejection and explicit Apply/no-auto-Save remain authoritative.

## 3. Minimum fatal boundary

Only the following first-stage conditions reject the package:

1. response is not exactly one required tool call;
2. tool name is not `analyze_ra2_authoring_intent`;
3. arguments exceed the bounded payload size;
4. arguments are not a bounded JSON object;
5. a root property is duplicated.

Unknown additive properties are ignored. Missing, unknown or inconsistent descriptive
metadata is normalized or routed through the existing generic preview capability. It
does not grant Apply, Save, arbitrary path, shell, network or asset-write authority.

## 4. Context-query admission

Each query is admitted independently. A bad query must not destroy the intent package.

- only `current`, `rules`, and `art` symbolic targets are executable;
- only the existing read-only query kinds are executable;
- invalid/unknown targets or structurally unusable items are dropped;
- optional occurrence/index/search fields receive bounded defaults;
- arrays and strings are truncated to existing resource ceilings;
- duplicate query properties make that query unusable, not the whole package;
- no file path from provider JSON is ever resolved or opened.

The semantic-refinement parser follows the same query rules. Additive fields and
outcome/query mismatches are normalized; only the tool envelope, payload size, JSON root
and duplicate root properties remain fatal.

## 5. Routing

- recognized capabilities keep their existing reviewed routes;
- non-authoring outcomes are resolved before capability routing and cannot enter an edit
  tool;
- unknown authoring capabilities use the existing generic bounded preview route:
  project preview when a captured rules/art project is available, otherwise current-
  document preview when available;
- known SuperWeapon and rules/art routes keep their existing project admission checks;
- every production current-document capability exposes `preview_ini_edit_plan`, and every production rules/art capability
  exposes `preview_ini_project_edit_plan`; capability IDs retain Skill/retrieval identity only;
- fixed current-document and project typed Profiles remain explicit headless compatibility helpers only and cannot veto
  DeepSeek's production Work content;
- unknown domain IDs remain descriptive metadata and may select no optional Skill, but
  cannot block preview admission.

## 6. Diagnostics data model

`Ra2AiIntentAnalysisParseResult` is internal, request-scoped, immutable and not
serialized. It owns:

- typed fatal failure kind;
- locally generated safe diagnostic text;
- bounded recovery notes;
- the normalized intent package on success.

The pipeline preserves this result. UI text may expose only the local diagnostic, never
raw provider arguments. No settings, project file, history or persistence shape changes.

## 7. Test contract

Required tests cover:

1. realistic additive/missing/variant first-call arguments are accepted;
2. invalid query target is dropped while a valid package continues;
3. unknown domain/capability uses generic preview rather than rejection;
4. advisory/clarification outcome cannot be converted to authoring by capability text;
5. list overflow is bounded, not fatal;
6. malformed JSON, wrong tool and duplicate root properties still reject with typed
   diagnostics;
7. semantic retrieval accepts optional query shapes and ignores additive metadata;
8. natural-language SuperWeapon Work reaches execution with realistic non-ideal JSON;
9. existing preview/apply/save/undo and project-target tests remain green.
10. every non-advisory production capability exposes a generic model-owned preview tool, never an `expand_*_template` tool;
11. a current-document generic plan that upserts absent Sections creates those Sections in Preview without auto-Apply/Save.
12. document/project proposals ignore missing, blank, null or non-string `message`; it is presentation-only and never
    participates in execution admission;
13. missing, blank, null or non-string proposal `summary` uses a local default without changing operations;
14. clarification still requires one readable bounded `message`; any echoed proposal/documents remain inert;
15. the bounded structured replan path is parsed by the same adapter and accepts the same presentation drift.

Hand-authored ideal JSON tests are retained only as compatibility checks, not used as the
sole reliability evidence.

## 8. Forbidden changes

- no Shell XAML/layout or AutomationId change;
- no parser, Field Registry, completion, Hover, diagnostics or save-preflight change;
- no automatic Apply or Save;
- no arbitrary filesystem path, raw full-file replacement, shell or network tool;
- no dependency or provider/model policy change;
- no legacy solution restoration.

## 9. Self-review result

Approved for implementation. The design removes semantic overreach without weakening
the actual authority boundary. It reuses the canonical pipeline, query executor,
adapter and preview engines, introduces no parallel editor, and makes accepted recovery
and fatal rejection independently testable.

## 10. Presentation metadata correction

For the generic production document/project operation tools, `operations` / `documents`
are executable payload and `summary` / `message` are display metadata. Proposal validity
must never depend on the latter. A valid string summary may be displayed; otherwise the
Host supplies a fixed local summary. Proposal message is ignored regardless of its JSON
shape. The provider schema keeps these fields as optional string guidance but does not
require non-empty content.

This relaxation does not apply to clarification: a `needs_clarification` result must
contain a non-empty bounded string message, and any echoed executable payload is ignored.
Malformed JSON, duplicate properties, unsafe identifiers, invalid targets/operations,
resource overflow, stale snapshots and explicit Apply/no-auto-Save boundaries are
unchanged.
