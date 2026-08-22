# AI Agent Safety Contract

## 1. Safety Scope

This contract defines safety rules for the future RA2IniEditor.IDE AI Assistant.

It applies to all future AI phases, including mock UI, bounded context collection, real provider integration, draft previews, and any later apply workflow.

AI-0 is documentation-only. It does not authorize source changes, UI implementation, AI client implementation, DeepSeek integration, network calls, or file mutation.

## 2. Core Rules

The AI Assistant must follow these rules:

- User action is required before opening the AI page.
- User action is required before collecting context.
- User action is required before sending a provider request.
- AI output is draft-only by default.
- File edits require preview before apply.
- Apply behavior requires a separate approved phase.
- No AI workflow may bypass existing save, dirty state, backup, rollback, validation, parser, or Field Registry semantics.

## 3. No Automatic Behavior

The app must not automatically:

- Open the AI Assistant on startup.
- Open the AI Assistant on caret movement.
- Open the AI Assistant on diagnostics.
- Send context on file open.
- Send context on project load.
- Send context on Section selection.
- Send context on hover or completion.
- Apply generated content.
- Save files.
- Modify Field Registry data.
- Modify project files.

The AI page is an explicit tool surface, not a background agent.

## 4. Context Minimization

Only bounded context needed for the selected task may be collected.

Allowed context must be summarized in the UI before a request is made:

- Current file display name or path.
- Current Section.
- Current key/value.
- Explicit selection length.
- Nearby line count.
- Field hint count.
- Diagnostic count.

Raw content should be limited to the active location and selected task. Whole-file or whole-project context requires a separate contract and explicit approval.

## 5. Forbidden Context

The AI Assistant must not collect or send:

- API keys, tokens, passwords, or credentials.
- Environment variables.
- User profile files.
- Files outside the active project.
- Whole repository content by default.
- Generated build output.
- Source package archives.
- `.vs`, `bin`, `obj`, `artifacts`, `TestResults`, or equivalent generated directories.
- Clipboard content unless explicitly pasted by the user.
- Hidden local configuration unless explicitly approved in a future contract.

## 6. Prompt Injection Boundary

INI content, comments, diagnostics, and project text are untrusted data.

Provider prompts must treat project content as data, not instructions. Future prompt builders must separate:

- System/developer rules owned by the application.
- User prompt text.
- Project context snippets.

Project context must not be allowed to override safety rules, request hidden files, request credentials, or bypass preview-before-apply.

## 7. Preview-Before-Apply

No generated edit may be applied directly from an AI response.

Any future edit workflow must include:

- A visible draft preview.
- Target file identification.
- Target Section/key/range identification when available.
- Proposed before/after content.
- Validation status when available.
- Explicit user confirmation.
- Clear cancellation path.

AI-1 must not implement apply. AI-1 may show response text and draft preview placeholders only.

## 8. Future Apply Gate

Before any future apply feature is implemented, a separate contract must define:

- Allowed file types.
- Allowed edit range model.
- Parser and validation integration.
- Dirty document behavior.
- Save preflight behavior.
- Backup and rollback behavior.
- Undo/redo behavior.
- Conflict handling when the document changes after preview.
- UI confirmation wording.
- Tests proving no write occurs before confirmation.

Apply must never bypass existing IDE save and validation semantics.

## 9. API Key Safety

AI-0 and AI-1 must not require API keys.

Future API key support must satisfy:

- Opt-in only.
- Stored outside source files.
- Excluded from repository and packages.
- Not logged.
- Not shown in exception messages.
- Not copied to prompt previews.
- Not persisted in test fixtures.
- Not required for normal CI.

Missing or invalid keys must result in disabled real-provider commands or mock/offline behavior, not application failure.

## 10. Network and Provider Policy

AI-0 and AI-1 must not use network providers.

DeepSeek is not part of the runtime AI Assistant architecture. It must not be wired into the product UI or used as an in-app provider unless a future contract explicitly approves it.

Future provider integration must be:

- Explicitly approved.
- Opt-in.
- Replaceable behind a client abstraction.
- Testable without live network.
- Disabled in normal CI unless a specific provider test profile is enabled.

## 11. Mock Client First

The first implementation must use a deterministic mock client.

Mock client safety requirements:

- No network calls.
- No API key access.
- No file writes.
- No project mutation.
- Deterministic output for tests.
- Clear disabled or mock status when real provider features are unavailable.

The mock client is a safety boundary, not a temporary shortcut to be bypassed.

## 12. Logging and Telemetry

The AI Assistant must not log by default:

- API keys.
- Raw prompts.
- Raw provider responses.
- Raw project snippets.
- Full context payloads.
- File contents.

If future diagnostics need AI logs, they must be sanitized, opt-in, and covered by a separate contract.

## 13. UI Safety Requirements

Future AI UI must display:

- Selected task kind.
- Context summary.
- Generate/cancel state.
- Draft-only warning.
- Safety footer explaining that generated output is not applied automatically.

AI UI must not display an Apply button until a later approved apply phase.

## 14. Automation Safety

Existing AutomationIds must be preserved.

New AI AutomationIds should be appended only. Tests should use AutomationIds and semantic state rather than brittle long text assertions.

Future tests must verify:

- AI generate does not change editor text.
- AI generate does not change dirty state.
- AI generate does not write files.
- AI generate does not save.
- AI generate does not invoke Field Registry write/apply/rollback/import/learning services.

## 15. Semantic Boundaries

AI implementation must not change:

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

AI features must be layered above existing IDE behavior, not fused into core parser or Field Registry services.

## 16. Acceptance Criteria

The AI safety contract is satisfied when:

- AI work begins with mock-only behavior.
- Context is explicit, bounded, and summarized.
- No source or project file can be changed by AI generation.
- No provider request can happen without user action.
- No API key is required or stored for mock phases.
- Real provider work is postponed to a separately approved phase.
- Apply behavior is postponed to a separately approved phase with preview and confirmation.
- Existing Shell, parser, Field Registry, save, backup, rollback, and legacy boundaries remain unchanged.
