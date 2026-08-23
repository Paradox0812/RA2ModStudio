# AGENT-MODE-2 — Two-Stage Intent / Execution Final Contract

Date: 2026-08-23  
Status: Implemented; local automated verification passed; real DeepSeek acceptance pending

## Goal

Replace Work mode's keyword router as the final semantic authority with two bounded DeepSeek calls:

```text
local configuration/snapshot gate
  -> required intent-analysis tool call
  -> local package validation and capability allowlist mapping
  -> required authoring tool call or advisory response
  -> canonical Preview / explicit Apply
```

Chat remains a single advisory call. No model call receives Apply, Save, file-system or Shell authority.

## Intent package

The first call must invoke `analyze_ra2_authoring_intent` exactly once and return only:

- `outcome`: `advisory | authoring | needs_clarification | unsupported`;
- `capability_id`: one local allowlisted capability;
- `domain_intent_id`: one bundled Skill routing domain;
- `request_summary`: non-empty, maximum 512 characters;
- `completion_level`: `none | field | skeleton | complete`;
- `constraints`: at most 12 non-empty strings, each at most 256 characters.

The package is request-scoped, immutable after validation, never persisted and never shown as model reasoning. Unknown, duplicate, missing, oversized or inconsistent fields fail closed and suppress the second call.

## Capability mapping

Only these authoring capabilities can reach the second-stage required tool:

- current-document field edit;
- Weapon/Projectile/Warhead skeleton;
- complete single direct-fire chain;
- complete Primary/Secondary dual armament;
- complete Arcing Projectile;
- complete Homing Projectile;
- YR core Warhead.

Unsupported complete Unit/Building/SuperWeapon/faction/AI/assets remain advisory-only. The second stage receives the original request and the locally validated package, but the existing tool catalog, adapter, Field Registry, template compiler, snapshot currency and Preview/Apply gates remain authoritative.

## Failure and lifecycle rules

- One request lifecycle and one cancellation token cover both calls.
- First-call timeout, cancellation, provider error, missing tool, multiple tools or invalid package causes no second call.
- Authoring intent without an available editable snapshot causes no second call.
- Second-stage plain text while a tool is required remains `AuthoringToolNotInvoked`.
- No automatic retry or model fallback is introduced.
- First-call content is not appended to visible chat or conversation history.
- The second call is the only streamed/displayed response.

## Compatibility

The local `Ra2AiInteractionRouter` remains as bounded admission/compatibility logic and test surface, but Work mode no longer uses its ambiguous/unsupported keyword result as the final semantic decision. No public API, persistence format, XAML, docking layout, Field Registry, parser, Apply or Save contract changes.

## Verification

```text
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
  Passed: 0 errors; one pre-existing CS8602 test warning

focused Pipeline / Route / Shell Boundary / Adapter / Template tests
  Passed: 86/86

AI / DeepSeek / ContentTemplate regression filter
  Passed: 409/409

Application full tests
  Passed: 151/151

IDE non-UI full tests
  Passed: 2610/2610

Real DeepSeek and UI smoke
  NotRun: requires user API/network acceptance
```

## Manual acceptance

In Work mode, retry:

```text
修改当前文件，为 HTNK 添加 Secondary 同轴机枪，并构建完整的 Weapon、Projectile、Warhead 链；不要使用循环或交替开火机制。
```

Expected: the request is sent through intent analysis, then the complete-chain authoring tool; a structured Preview appears, no automatic Apply/Save occurs. Network diagnostics should show two provider calls for one Work send.
