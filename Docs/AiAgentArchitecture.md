# AI Agent Architecture

## 1. Scope

AI-0 defines the architecture contract for adding an AI Assistant to RA2IniEditor.IDE.

This document is design-only. It does not authorize source changes, UI implementation, AI client implementation, network calls, DeepSeek integration, or automatic file edits.

The AI Assistant must be integrated into the existing right-side Section area by evolving it into a Right Tool Well:

- Section Tree remains the default page.
- AI Assistant is the second page.
- No non-modal AI window is introduced.
- No second right sidebar is introduced.
- No bottom AI tab, modal AI surface, editor overlay, or new docking framework is introduced.

## 2. Non-Goals

AI-0 must not implement:

- XAML or code-behind changes.
- AI client code.
- OpenAI, DeepSeek, or other real provider integration.
- API key loading.
- Prompt execution.
- Apply-to-file behavior.
- Project-wide indexing.
- Background context collection.
- Automatic opening of AI UI.
- Automatic sending of editor content.

## 3. Placement Baseline

The accepted placement contract is `Docs/AiAgentPanelPlacementContract.md`.

The future AI panel belongs in the right-side tool area where the Section Tree currently lives. The Right Tool Well owns page selection between:

- Section Tree / Navigator page.
- AI Assistant page.

The Section Tree is the default visible page. The AI page is shown only after an explicit user command. Closing the AI page returns to the Section Tree or the previously active right-side page.

## 4. Future Component Model

The future implementation should keep the AI system separated into small, testable components.

Recommended component responsibilities:

- `RightToolWell`: Owns right-side page selection and preserves the Section Tree as the default page.
- `AiAssistantView`: Displays the AI assistant panel inside the Right Tool Well.
- `AiAssistantViewModel`: Owns display state, task kind selection, prompt text, generated response text, draft preview state, busy state, and command enablement.
- `AiContextSnapshot`: Immutable display model describing the bounded context that may be sent to the AI provider.
- `IAiContextCollector`: Collects bounded editor context only when the user explicitly asks.
- `IAiPromptBuilder`: Converts task kind, user prompt, and approved context snapshot into a provider request.
- `IAiAssistantClient`: Provider abstraction. AI-1 must use a mock implementation first.
- `MockRa2AiClient`: Deterministic local client for UI and workflow testing.
- `AiResponseModel`: Stores provider response text and optional draft suggestion metadata.
- `AiDraftPreviewModel`: Represents draft changes for preview only.

These names are architectural suggestions, not authorization to add files in AI-0.

## 5. Context Collection Flow

Context collection must be explicit, bounded, and visible.

Required flow for future implementation:

1. User explicitly opens the AI Assistant page.
2. User selects a task kind.
3. User optionally enters a prompt.
4. User explicitly requests generation.
5. The app collects a bounded `AiContextSnapshot`.
6. The UI displays a context summary before or as part of the request.
7. The AI client receives only the bounded context needed for the selected task.
8. The response is displayed as draft text.
9. Any edit-like output stays in a preview area until a future approved apply phase.

The app must not collect or send context automatically on caret movement, file load, project load, diagnostics update, Section selection, hover, completion, or save.

## 6. Allowed Context Sources

Future context collection may include only bounded, user-visible information:

- Current document path or display name.
- Current Section name.
- Current key and value at caret.
- Explicit text selection, if any.
- A small bounded range of nearby lines.
- Field Registry hint count and short summary.
- Diagnostic count and short summary for the active document or active location.
- Current mod target hint, if already available in the existing IDE state.

The context summary must show what categories and approximate counts will be sent before the provider request is made.

## 7. Forbidden Context Sources

Future implementation must not collect or send by default:

- Whole project content.
- Whole repository content.
- Files outside the active project.
- Environment variables.
- API keys, tokens, credentials, or machine-local secrets.
- Build output directories.
- Package archives.
- `.vs`, `bin`, `obj`, `artifacts`, `TestResults`, or other generated folders.
- Hidden user files.
- Clipboard content unless the user explicitly pasted it into the prompt.

Project-wide context, if ever needed, requires a separate contract and explicit user approval.

## 8. Task Kinds

Initial task kinds should be constrained to low-risk assistant behavior:

- Explain field.
- Explain Section.
- Explain reference.
- Explain diagnostic.
- Suggest Value.
- Generate draft.

AI-1 must not include apply behavior. AI output is advisory and draft-only.

## 9. Preview-Before-Apply Architecture

Any future AI output that proposes file edits must go through a preview model before it can be applied.

Required architecture boundary:

- AI client returns text or a structured draft.
- Draft preview displays target file, target location, proposed replacement, and rationale.
- No write operation is allowed during generation.
- No write operation is allowed from the response area.
- Apply requires a separate approved phase, explicit user confirmation, and integration with existing dirty/save/backup rules.

AI-0 and AI-1 must stop at response and preview-only surfaces.

## 10. API Key Strategy

AI-0 and AI-1 must not require or load API keys.

Future real provider support must follow these rules:

- API keys are opt-in.
- API keys are never stored in source files.
- API keys are never committed to the repository.
- API keys are never included in source packages.
- API keys are never logged.
- API keys are never shown in diagnostics, test output, or crash output.
- Recommended sources are user configuration outside the repository, environment variables, OS credential storage, or another explicitly approved secure store.
- Missing API key must keep the UI usable with mock/offline behavior or a clear disabled state.

## 11. Mock Client First

The first implementation phase must use `MockRa2AiClient` or an equivalent deterministic local mock.

Mock-first requirements:

- No network access.
- No DeepSeek access.
- No real AI provider.
- Deterministic responses for tests.
- Explicit fake latency only if useful for UI state testing.
- Clear visual distinction that output is draft assistant text.

Mock-first allows Right Tool Well integration, UI state, context summary, command enablement, and preview-only behavior to be tested before real provider risk is introduced.

## 12. AutomationId Plan

Future Right Tool Well IDs should preserve any existing Section Tree IDs and append only new IDs.

Suggested new IDs:

- `RightToolWell.Root`
- `RightToolWell.SectionTab`
- `RightToolWell.AiTab`
- `RightToolWell.ActiveView`
- `AiAssistant.Panel`
- `AiAssistant.Header`
- `AiAssistant.CloseButton`
- `AiAssistant.ContextSummary`
- `AiAssistant.TaskKindSelector`
- `AiAssistant.PromptBox`
- `AiAssistant.GenerateButton`
- `AiAssistant.CancelButton`
- `AiAssistant.CopyButton`
- `AiAssistant.ClearButton`
- `AiAssistant.ResponseArea`
- `AiAssistant.DraftPreview`
- `AiAssistant.SafetyFooter`

Existing AutomationIds must not be renamed or removed.

## 13. Future Phase Roadmap

Recommended future phases:

- AI-0: Architecture and safety contract only.
- AI-1: Right Tool Well page integration and AI panel UI with mock client only.
- AI-2: Bounded context collector and prompt builder, still mock-first.
- AI-3: Optional real provider client behind explicit API key configuration and opt-in network behavior.
- AI-4: Structured draft preview and diff rendering, still no automatic apply.
- AI-5: Explicit apply workflow only after a separate contract covering confirmation, dirty state, backup, rollback, undo/redo, and save preflight.

Each phase requires its own implementation contract before code changes.

## 14. Test Strategy

Future tests should cover:

- Section Tree remains the default right-side page.
- AI page opens only by explicit command.
- AI page closes back to the Section Tree or previous right-side page.
- Existing Section Tree AutomationIds remain available.
- AI Assistant AutomationIds exist after implementation.
- Generate command is disabled when required input is missing.
- Mock client response is displayed without file mutation.
- Context summary contains bounded counts and does not include raw whole-project content.
- Cancel clears busy state.
- Copy and clear commands affect only AI panel state.
- No source file changes occur during mock generation.

Real provider tests must not require live network or real API keys in normal CI.

## 15. Acceptance Criteria

AI architecture is acceptable when:

- AI is scoped to the Right Tool Well.
- Section Tree remains the default page.
- Mock client is the first implementation strategy.
- Context collection is explicit, bounded, and visible.
- API key policy excludes keys from source, logs, packages, and tests.
- AI output is draft-only until a later approved apply phase.
- DeepSeek and real network providers are excluded from AI-0 and AI-1.
- Future phases are split so apply behavior cannot enter by accident.
