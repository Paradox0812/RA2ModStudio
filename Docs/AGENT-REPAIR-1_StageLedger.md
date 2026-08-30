# AGENT-REPAIR-1 Stage Ledger

Status: Completed / automated verified

Date: 2026-08-25

Contract: `AGENT-REPAIR-1_BoundedStructuredReplanFinalContract.md`

## 1. Delivered stages

| Stage | Result | Review evidence |
|---|---|---|
| R1-1 Typed failure evidence | Completed | Internal leaf failure evidence reaches a deny-by-default pure eligibility policy; unknown/infrastructure/safety failures remain ineligible. |
| R1-2 Execution seed and repair prompt | Completed | Request-local immutable seed reuses route, intent, Skill, project projection and HLI results; failed content is bounded, sanitized and absolute-path redacted. |
| R1-3 Bounded orchestrator | Completed | One coordinator owns initial proposal preparation, pre-cost currency check, one non-streaming repair and canonical proposal preparation; no recursive repair path exists. |
| R1-4 Shell integration | Completed | Shell adds only a UI-thread recapture adapter, coordinator construction and final rendering. No XAML, AutomationId, layout, policy switch or prompt composition was added. |
| R1-5 Verification and handoff | Completed | Focused, Application and full IDE test gates passed; documentation and decision records were updated; clean package gate passed. |

## 2. Runtime invariants

- Chat: one provider call.
- normal Work: one intent call plus one streaming execution call.
- eligible structured failure: at most one additional non-streaming repair; Work hard maximum is three calls.
- intent analysis, Skill resolution and HLI query execution are not repeated.
- timeout, network, cancellation, configuration, stale context, resource, safety and unknown failures do not repair.
- repair uses the original request snapshot as target and recaptures current context before cost and before final Preview.
- repaired output passes the same adapter/template/canonical document or project Preview.
- no provider response can Apply or Save; user confirmation remains mandatory.

## 3. Changed implementation surface

New internal files:

- `RA2IniEditor.IDE/AI/Ra2AiStructuredRepairContracts.cs`
- `RA2IniEditor.IDE/AI/Ra2AiWorkExecutionSeed.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAuthoringContextRecapture.cs`
- `RA2IniEditor.IDE/AI/Ra2AiBoundedStructuredReplanCoordinator.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiStructuredRepairPolicyTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2AiBoundedStructuredReplanCoordinatorTests.cs`

Existing files changed within the approved boundary:

- AI pipeline, prompt request/builder, proposal result, tool adapter and authoring coordinator.
- `ShellWindow.xaml.cs` narrow construction/recapture/rendering wiring only.
- focused prompt, Shell and existing architecture boundary tests.
- current project documentation and decision log.

No public Application API or serialized shape changed. `ShellWindow.xaml`, resource dictionaries,
AutomationIds, Dock layout, parser, Field Registry data/provider priority, Completion, Hover,
Diagnostics, Save Preflight and provider configuration were not changed by this stage.

## 4. Automated verification

```text
dotnet restore .\RA2IniEditor.IDE.sln
  Passed / up-to-date

dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
  Passed / 0 warnings / 0 errors

focused repair/pipeline/project/Shell/prompt/loopback tests
  Passed 125/125

dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
  Passed 188/188

dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
  Passed 2706/2706

powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
  Passed; 1227 files; forbidden build/cache/archive entries excluded
```

One existing nullable warning in `BuiltInFieldRegistryPackLoaderTests.cs` appeared only when a
test command rebuilt the test project. The final solution build itself completed with zero warnings.

## 5. Manual verification status

- Real DeepSeek: NotRun by this stage.
- Computer control / physical WPF smoke: NotRun.
- Recommended user cases are retained in the final contract and `ReleaseChecklist.md`.

## 6. Governance closeout

- Public API ledger: no entry required; exported Application surface is unchanged.
- Persistence: unchanged.
- Decision: accepted and recorded in `DecisionLog.md`.
- Deferred governance: real-provider behavior and physical UI status remain explicit manual checks,
  not silent success claims.
- Technical debt: no new compatibility adapter, fallback, retry loop or TODO was introduced.

## 7. Remaining risks

- A real model may produce a failure outside the allowlist; it will correctly stop instead of repairing.
- Repair adds one provider call, latency and token usage only on the eligible path.
- Physical status placement and prompt restoration still require user-run WPF acceptance.

## 8. Recommended next phase

First run the manual real-DeepSeek acceptance cases for one successful repair, one failed repair and
one network failure. After that, choose a separately contracted content expansion such as complete
Techno/SuperWeapon/Faction semantics; do not broaden this bounded repair into a generic retry system.
