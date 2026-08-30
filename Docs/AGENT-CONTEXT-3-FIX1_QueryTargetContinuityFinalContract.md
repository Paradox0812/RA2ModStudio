# AGENT-CONTEXT-3-FIX1 Query Target Continuity Final Contract

## 1. Problem statement

`AGENT-CONTEXT-3` correctly resolves bounded read-only facts from the captured project pair, but two continuity gaps remain. First, the intent stage can request `target=art` while still labeling a field edit as `current-document-field-edit`, causing Shell to bind the execution tool to the current document instead of the captured rules/art pair. Second, a successful query does not currently make the resolved document target explicit enough for the second-stage structured plan. A model can therefore read `[HTNKART]` from `art` and still emit an operation for the same existing Section under `rules`. The canonical preview then correctly rejects that plan, but the UI only reports the generic `未找到目标 Section。` message.

## 2. Accepted behavior

1. An authoring package labeled `current-document-field-edit` that explicitly requests any `rules` or `art` context query is normalized to the existing bounded project capability. Other capabilities are not promoted.
2. Every successful Host-resolved Section query carries an explicit target-continuity statement into the second model prompt.
3. The project tool rules require an operation that modifies a successfully resolved existing Section to keep the same symbolic `rules` / `art` target, unless the user explicitly requested a cross-document copy or move.
4. The Host does not silently retarget, rewrite, apply, save, or retry the model plan.
5. If project preview still fails with `SectionNotFound`, the IDE inspects only the same captured project snapshot and reports:
   - the file selected by the model plan;
   - the missing Section name when uniquely identifiable;
   - the other captured file in which that Section exists, when uniquely identifiable;
   - confirmation that nothing was applied.
6. All other project preview failures keep their existing behavior.

## 3. Safety and architecture boundaries

- Keep exactly two provider calls for Work mode.
- Use only request-lifetime captured snapshots; do not read arbitrary paths or live disk state.
- Reuse the canonical INI parser for diagnostic lookup.
- Do not change Application preview semantics or public result contracts.
- Do not weaken structural/resource safety limits.
- Do not modify Field Registry, diagnostics, save, undo/redo, XAML, Shell layout, or legacy behavior.
- Do not add automatic correction. A wrong model target remains a rejected proposal with actionable evidence.

## 4. Allowed files

- `RA2IniEditor.IDE/AI/Ra2AiSharedContext.cs`
- `RA2IniEditor.IDE/AI/Ra2AiIntentAnalysisStage.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs`
- `RA2IniEditor.IDE/AI/Ra2AiAuthoringCoordinator.cs`
- focused files under `RA2IniEditor.Tests/IDE/`
- current phase / context documentation

## 5. Acceptance matrix

| Case | Expected result |
|---|---|
| Query `[HTNKART]` succeeds on `art` | Second prompt states that modifications to this resolved Section must target `art` |
| Intent says `current-document-field-edit` and requests `target=art` | Route is normalized to the existing project preview capability |
| Model correctly emits `target=art` | Existing project preview succeeds unchanged |
| Model emits `target=rules` with `replace_field_value` for `[HTNKART]`, while Section exists only in art | Proposal is rejected; message names `rulesmd.ini`, `[HTNKART]`, and `artmd.ini`; no proposal/apply payload |
| Section is absent from every captured document | Failure remains bounded and does not claim a cross-document hit |
| Cancellation, stale snapshot, or other preview failure | Existing behavior remains unchanged |
| Provider call count | Remains exactly two |

## 6. Verification

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-restore --filter "FullyQualifiedName~Ra2AiContextQueryPipelineTests|FullyQualifiedName~Ra2AiProjectAuthoringIntegrationTests"
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 7. Explicitly deferred

- automatic repair/replanning after a rejected second-stage plan;
- a third provider call;
- deterministic Host retargeting;
- broader semantic validation of model-authored RA2 content.
