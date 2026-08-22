# AI Assistant Safety Contract

## 1. Safety Position

The RA2IniEditor.IDE AI Assistant is a DeepSeek-powered RA2 Modding Assistant.

It is not an autonomous file-modifying agent. DeepSeek is only a text generation backend and has no file-system authority.

The IDE must not treat DeepSeek output as trusted configuration. DeepSeek output may be used only as explanation, suggestion, INI draft text, or unit prototype text unless a later explicit preview/confirm insertion workflow is approved and implemented.

## 2. Early-Phase Prohibitions

Early AI phases must not perform:

- Automatic file writes.
- Automatic saves.
- Automatic apply.
- Automatic fixes.
- Automatic insertion into the current document.
- Field Registry writes.
- Field Registry apply, rollback, import, or learning operations.
- Shell command execution.
- Tool execution.
- Whole-project upload by default.
- Background suggestions on every caret move.
- Background provider requests on project load, file open, diagnostics update, hover, completion, or Section selection.

AI output is advisory. The user decides whether and how to use it.

## 3. DeepSeek Backend Boundary

DeepSeek may receive a bounded prompt and return text.

DeepSeek must not be given:

- Direct file access.
- Tool access.
- Save/apply authority.
- Registry write authority.
- Shell command authority.
- Whole-project authority.

The application may not convert a DeepSeek response into a file mutation without a separate approved preview/confirm insertion workflow.

## 4. Context Boundary

Context must be bounded and explainable to the user.

Allowed context categories:

- Current file display name.
- Current Section name.
- Current key/value under caret.
- Explicit user selection or pasted snippet.
- Small nearby line range.
- Relevant diagnostic summaries.
- Top Field Registry matches.

The UI should expose a context summary before generation, including what categories and approximate counts are included.

## 5. Forbidden Context

The assistant must not collect or send by default:

- Entire project content.
- Entire repository content.
- Entire Field Registry.
- Files outside the active project.
- Absolute local paths unless explicitly needed.
- API keys, tokens, passwords, credentials, or environment variables.
- Hidden user files.
- Clipboard content unless explicitly pasted by the user.
- Build/package/test output directories.
- `.vs`, `bin`, `obj`, `artifacts`, `TestResults`, or equivalent generated directories.

Any future broader context mode requires a separate contract and explicit approval.

## 6. Field Registry Evidence Rules

Field Registry matches are advisory evidence.

They may be used to ground explanations, field suggestions, diagnostics explanations, and INI drafts. They must not become:

- A hard save blocker.
- A hard legal-field authority.
- A replacement for existing diagnostics.
- A reason to rewrite Field Registry data.
- A reason to change Project > Global > BuiltIn priority.

The assistant should mention uncertainty when registry evidence is incomplete or ambiguous.

## 7. Draft Output Rules

Generated INI and unit prototypes must be clearly marked as drafts.

Draft output should include, when relevant:

- INI block.
- Rationale.
- Related field notes.
- Required follow-up definitions.
- Assumptions and uncertainties.
- Warnings about balance, engine limitations, or missing art/sound references.

Draft output must not be presented as automatically valid or authoritative.

## 8. Explicit User Copy / Use

Early phases may allow users to copy generated text.

Copy behavior must be explicit:

- User clicks Copy or selects text manually.
- Copying does not modify the active document.
- Copying does not save files.
- Copying does not update dirty state.
- Copying does not write Field Registry files.

Manual paste by the user remains normal editor behavior.

## 9. Future Preview / Confirm Insert Rule

Any future insertion workflow must require preview and confirmation.

Minimum required gate:

- Show target file.
- Show target Section/key/range when available.
- Show proposed text.
- Show whether the document changed after generation.
- Require explicit confirmation.
- Provide cancel path.
- Preserve undo/redo expectations.
- Preserve existing dirty-state and save preflight semantics.
- Preserve backup and rollback semantics where applicable.

No generated edit may be inserted directly from a provider response.

## 10. API Key Policy

No API key may be stored in the repository.

Future DeepSeek API key handling must satisfy:

- First implementation uses environment-variable-only configuration.
- DeepSeek API key is read from `DEEPSEEK_API_KEY`.
- Optional provider overrides may use `DEEPSEEK_BASE_URL`, `DEEPSEEK_MODEL`, and `DEEPSEEK_TIMEOUT_SECONDS`.
- Stored outside source files, project files, repository docs, UI settings, and local persisted settings.
- Excluded from source packages.
- Never logged.
- Never shown in diagnostics, exception text, package output, or tests.
- Never included in prompts.
- Not collected or saved by the AI Assistant Advanced UI.
- Not required for normal CI.

If the API key is missing or invalid, the IDE must remain stable and show a disabled or error state.

## 11. Network Error and Cancellation Policy

Network errors, provider errors, timeouts, and cancellation must not crash the IDE.

Future provider implementation must:

- Support cancellation.
- Return user-visible error state.
- Clear busy state after failure.
- Avoid partial document mutation.
- Avoid retry loops that spam the provider.
- Avoid blocking the UI thread.

Mock client tests should validate the UI state model before live provider integration.

## 12. Logging and Redaction

The assistant must not log sensitive data.

Do not log by default:

- API keys.
- Raw prompts.
- Raw responses.
- Raw INI snippets.
- Full context payloads.
- Absolute local paths.
- Credentials or environment variables.

If future diagnostics require AI logging, logs must be opt-in, sanitized, and covered by a separate contract.

## 13. Prompt Injection Boundary

INI text, comments, diagnostics, and user-selected snippets are untrusted data.

Prompt construction must ensure project content cannot override application rules. Project text must be framed as data for analysis, not as system instructions.

The assistant must ignore project-content requests to:

- Reveal secrets.
- Read unrelated files.
- Upload more context than allowed.
- Modify files.
- Apply changes.
- Run commands.
- Bypass preview/confirmation.

## 14. Semantic Boundaries

AI work must not change:

- INI parser semantics.
- Completion candidate generation.
- Completion commit behavior.
- Hover data source.
- Diagnostics.
- Save preflight.
- Backup and rollback.
- Undo and redo.
- Field Registry load/apply/rollback/import/learning semantics.
- Project > Global > BuiltIn priority.
- BuiltIn field definitions.
- Legacy exclusion rules.

The assistant is layered above existing IDE behavior.

## 15. Implementation Boundaries

AI-0 is documentation only.

AI-0 must not modify:

- XAML.
- Code-behind.
- ViewModels.
- Tests.
- Scripts.
- Field Registry JSON.
- Solution or project files.
- Source code.

AI-0 must not implement:

- AI panel UI.
- Mock client.
- DeepSeek client.
- Context provider.
- Prompt builder.
- Apply or insert flow.

## 16. Safety Acceptance Criteria

The safety contract is satisfied when:

- DeepSeek is defined only as a text generation backend.
- The assistant is not described as a Codex-like file-editing agent.
- No initial task kind writes files.
- No auto-save, auto-apply, or Field Registry write path exists in early phases.
- Context is bounded and explainable.
- Field Registry evidence is advisory.
- Generated INI is marked as draft.
- User copy/use is explicit.
- Future insertion requires preview and confirmation.
- API keys are excluded from repository, logs, packages, and normal tests.
- Network errors and cancellation cannot crash the IDE.
