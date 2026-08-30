# AGENT-SKILL-ROUTING-2 Model-Selected Skill Manifest Continuous Final Contract

Status: Approved for continuous execution after self-review  
Date: 2026-08-25  
Risk: R4 (network-facing provider tool schema and two-stage orchestration); governance deferred to 2F  
Scope: IDE-internal Work-mode Skill discovery and prompt composition only

## 1. Goal

Make the first DeepSeek Work call see a compact, immutable manifest of the bundled RA2 Skills and return an
ordered Skill selection plus bounded knowledge gaps. The Host validates that selection against the exact same
catalog snapshot, merges capability-required Skills, enforces mode and character budgets, and injects only the
resolved full Skill bodies into the second DeepSeek call.

This package does not add a third model call, a model-visible file-listing tool, filesystem authority, Apply/Save
authority, or a second Skill registry.

## 2. Approved files

- `RA2IniEditor.IDE/AI/IRa2AiPromptBuilder.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuildRequest.cs`
- `RA2IniEditor.IDE/AI/Ra2AgentSkillCatalog.cs`
- `RA2IniEditor.IDE/AI/Ra2AiIntentAnalysisStage.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs`
- directly related IDE tests
- this contract, stage ledger, API/decision/debt/status/context documentation

## 3. Forbidden changes

- DeepSeek transport, endpoint, timeout, retry, pricing, or model selection
- Shell/XAML/AutomationIds and all visual behavior
- Field Registry packs, provider priority, Completion, Hover, Diagnostics, parser, save preflight
- Project Preview/Apply/Undo/Redo/Save semantics
- Application/Core/Infrastructure public API, project files, dependencies, or legacy editor
- arbitrary local/external Skill roots, scripts, plugins, or real provider calls

## 4. Data contract

### 4.1 Manifest

The existing `Ra2AgentSkillCatalog` is the sole owner. Each manifest entry contains only:

- immutable Skill ID (`name`)
- version
- bounded description
- supported domains
- supported modes
- instruction character count
- content hash

The manifest contains no Skill body. It is deterministically ordered by Skill ID and derived from the same
in-memory catalog instance later used for resolution and prompt injection.

### 4.2 First-call result

`analyze_ra2_authoring_intent` adds two required properties:

- `selected_skill_ids`: ordered unique intent-stage recommendations, maximum 6
- `knowledge_gaps`: bounded facts the execution stage should clarify or treat as uncertain, maximum 6

Each item is a non-empty string with a 256-character bound. Unknown Skill IDs do not invalidate the entire
intent package; the local resolver records and omits them. Duplicate IDs are stable-deduplicated.

### 4.3 Local resolution

Resolution produces immutable facts:

- requested IDs
- capability-required IDs
- active Skill descriptors
- unavailable/invalid IDs
- omitted-by-budget IDs
- knowledge gaps

Order is capability-required first, then first-call order. Mode-incompatible and unknown entries are unavailable.
Required Skill bodies must fit the existing 14 KiB budget; that invariant is tested. Optional Skills that exceed
the remaining budget are omitted and reported, never silently substituted.

Capability requirements:

| Capability | Required Skill |
|---|---|
| `techno-rules-art-binding` | `ra2-rules-art-binding` |
| weapon skeleton/complete/dual armament | `ra2-weapon-chain` |
| arcing/homing projectile | `ra2-projectile-trajectory` |
| YR core warhead | `ra2-warhead-damage` |

`ra2-field-schema-trust` remains a cross-cutting Work requirement when available. An explicit Ares/Phobos marker
keeps the existing extension fallback. If the first call selects no usable Skill, the existing domain selector is
the compatibility fallback.

## 5. Pipeline and prompt contract

- Chat remains one call and keeps local catalog selection.
- Work remains exactly two provider calls: one non-streaming analysis call, one streaming execution call.
- The first request receives the compact manifest in user content and a dynamically generated schema whose Skill
  ID enum comes from that catalog snapshot.
- After validated intent parsing, the pipeline resolves the selection before the second call.
- `Ra2AiPromptBuildRequest` receives the resolution explicitly. Work PromptBuilder must use its `ActiveSkills`
  and must not independently reselect them.
- The second request includes full active Skill bodies plus a compact resolution report: requested, required,
  active, unavailable, omitted-by-budget, and knowledge gaps.
- Skill content is advisory domain workflow. It grants no tool, path, network, apply, save, or shell authority.

## 6. Continuous stages

### 2A — Catalog Manifest

Implement immutable manifest projection and deterministic lookup on the existing catalog. Verify ordering,
metadata bounds, hash identity, and absence of instruction bodies.

### 2B — Intent Tool Schema

Make the first-stage tool definition catalog-aware; add selected IDs and gaps to schema, parser, exact-property
validation, package JSON, and prompt manifest. Verify valid, duplicate, unknown, over-limit, and malformed data.

### 2C — Resolver

Implement stable required/requested merge, mode validation, fallback selection, explicit unavailable/omitted
facts, and budget enforcement. Verify capability mappings and determinism.

### 2D — End-to-End Wiring

Expose the prompt builder's canonical catalog snapshot through its internal interface, pass the resolution through
the pipeline/build request, and inject actual selected bodies and resolution facts into call two. Verify exactly
two Work calls, one Chat call, same-snapshot behavior, and project capability protection.

### 2E — Boundary and Regression Matrix

Add focused tests for prompt size, no body leakage into call one, no unknown body injection, no third call,
analysis failure short circuit, no change to authoring tools/authority, and legacy local selection fallback.

### 2F — Release Gate and Documentation

Run restore/build, focused tests, full Application and IDE tests, package hygiene, and scoped diff audit. Update
the stage ledger, decision log, API ledger, technical debt/status/context docs. No real DeepSeek call is permitted.

## 7. Stop rules

Stop success and report if any of these cannot be preserved:

- exact two-call Work flow or one-call Chat flow
- same catalog snapshot for manifest, resolution, and injection
- 14 KiB selected Skill body budget and 65,536-character prompt bound
- existing provider/tool authority and Preview/Apply/Save boundaries
- focused or full regression gates
- approved file boundary

## 8. Self-review result

Approved. The design reuses the existing catalog and selector, makes the model recommendation non-authoritative,
keeps capability Skills mandatory, records rather than hides unknown/budget omissions, and preserves provider call
count. The package introduces no public .NET API or persistence format. The JSON shape sent to the first provider
is Experimental and must be recorded in the API ledger at 2F.
