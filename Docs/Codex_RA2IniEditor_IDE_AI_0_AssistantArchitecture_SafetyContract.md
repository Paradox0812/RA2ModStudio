# Codex Task: RA2IniEditor.IDE AI-0 DeepSeek Modding Assistant Architecture / Safety Contract

## 0. Context

The user clarified that the planned AI feature is **not a Codex-like file modifying agent**.

It is a DeepSeek-powered RA2 Modding Assistant:

```text
DeepSeek reads a bounded field registry / current context prompt
and returns explanations, field suggestions, INI drafts, or unit prototypes.
```

DeepSeek itself does not modify files and does not execute tools.

This task is AI-0 only.

Do not implement source code.

Do not implement UI.

Do not connect DeepSeek.

---

## 1. Goal

Create architecture and safety contract documentation for the RA2 Modding Assistant.

Output documents:

```text
Docs/AiAssistantArchitecture.md
Docs/AiAssistantSafetyContract.md
```

These documents replace any wording that incorrectly frames the feature as a Codex-like autonomous file-editing agent.

---

## 2. Required Documents to Read

Read first:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/AiAgentPanelPlacementContract.md
Docs/Codex_RA2IniEditor_IDE_AI_Assistant_Roadmap.md
```

If the roadmap file does not exist yet, create it from the supplied current task document or report that it is missing.

---

## 3. Required Architecture Content

`Docs/AiAssistantArchitecture.md` must define:

```text
1. Product goal.
2. Supported AI task kinds.
3. High-level data flow.
4. Right Tool Well AI panel relationship.
5. Field Registry retrieval strategy.
6. Context provider.
7. Prompt builder.
8. AI client abstraction.
9. Mock client first policy.
10. DeepSeek adapter future phase.
11. Draft/copy workflow.
12. Future confirmed insert workflow.
13. Test strategy.
14. Phase roadmap.
```

It must state that AI outputs are drafts/suggestions, not authoritative edits.

---

## 4. Required Safety Content

`Docs/AiAssistantSafetyContract.md` must define:

```text
1. DeepSeek is text generation backend, not file-modifying agent.
2. No automatic file writes in early phases.
3. No auto-save.
4. No auto-apply.
5. No field registry writes.
6. No whole-project upload by default.
7. No API key in repository.
8. Context must be bounded and explainable.
9. Field Registry matches are advisory evidence.
10. Generated INI drafts must be clearly marked as drafts.
11. User copy/use is explicit.
12. Future insertion must require preview and confirmation.
13. Network errors and cancellation must not crash IDE.
14. Logging/redaction policy.
```

---

## 5. Supported Initial Task Kinds

Document these first-phase task kinds:

```text
ExplainField
FindFieldsByRequirement
GenerateUnitPrototype
GenerateWeaponChainDraft
ReviewIniSnippet
ExplainDiagnostics
```

No task kind may directly write files.

---

## 6. Hard Boundaries

Do not modify:

```text
XAML
code-behind
ViewModels
tests
scripts
field registry JSON
solution/project files
source code
```

This is documentation only.

Do not implement:

```text
AI panel UI
MockRa2AiClient
DeepSeek client
context provider
prompt builder
apply/insert flow
```

---

## 7. Wording Correction

Avoid misleading wording such as:

```text
DeepSeek must not directly modify files
```

Prefer precise wording:

```text
DeepSeek is only a text generation backend and has no file-system authority.
The IDE must not treat DeepSeek output as trusted configuration.
Codex or the application may only use DeepSeek output as draft/suggestion text unless a later explicit preview/confirm insertion workflow is implemented.
```

---

## 8. Validation Commands

For this documentation-only phase:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing, run full validation.

---

## 9. Final Report Format

Report:

```text
1. Phase completed: AI-0.
2. Files changed.
3. Architecture decisions.
4. Safety rules.
5. Commands run.
6. Test result.
7. Package result.
8. Confirmation no source code changed.
9. Recommended next phase.
```
