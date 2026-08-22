# AI Assistant Architecture

## 1. Product Goal

The AI Assistant is a DeepSeek-powered RA2 Modding Assistant for Red Alert 2 / Yuri's Revenge / Ares / Phobos INI authoring.

It is not a Codex-like autonomous file-editing agent. It helps users understand, search, and draft INI content by combining bounded IDE context with Field Registry evidence and a DeepSeek text generation backend.

Primary jobs:

- Explain fields.
- Find fields for a gameplay requirement.
- Generate unit prototype drafts.
- Generate weapon chain drafts.
- Review selected or pasted INI snippets.
- Explain diagnostics with Field Registry evidence.

AI outputs are explanations, suggestions, or drafts. They are not authoritative edits and must not be treated as trusted configuration.

## 2. Supported AI Task Kinds

Initial task kinds:

- `ExplainField`
- `FindFieldsByRequirement`
- `GenerateUnitPrototype`
- `GenerateWeaponChainDraft`
- `ReviewIniSnippet`
- `ExplainDiagnostics`

No task kind may write files, save documents, update Field Registry data, run tools, or apply changes directly.

## 3. High-Level Data Flow

The intended flow is:

```text
explicit user request
  -> bounded current IDE context
  -> local Field Registry retrieval
  -> prompt builder
  -> AI client abstraction
  -> Mock client first, DeepSeek adapter later
  -> explanation / field suggestion / INI draft / unit prototype
  -> user copies result or uses a future preview/confirm insertion flow
```

The IDE remains the authority for editing, validation, save, backup, rollback, and Field Registry operations. DeepSeek is only a text generation backend and has no file-system authority.

## 4. Right Tool Well Relationship

Placement is governed by `Docs/AiAgentPanelPlacementContract.md`.

The AI Assistant belongs in the existing right-side Section area after that area is conservatively evolved into a Right Tool Well:

- Default page: Section Tree / Navigator.
- Second page: AI Assistant.
- Section Tree remains the default view.
- AI Assistant opens only through explicit user command.
- No non-modal AI window fallback.
- No second independent right sidebar.
- No bottom AI tab, modal AI surface, caret popup, editor overlay, or new docking framework.

Future implementation must preserve existing Section Tree behavior and AutomationIds.

## 5. AI Panel Shape

The future AI page should contain:

- Header.
- Context Summary.
- Task Kind Selector.
- Prompt Input.
- Actions.
- Response Area.
- Field Evidence / References.
- Draft Preview.
- Safety Footer.

First implementation phases must not include an Apply button.

The Safety Footer must make clear that generated output is draft/suggestion text and does not modify files automatically.

## 6. Field Registry Retrieval Strategy

The assistant must not send the entire Field Registry by default.

Use lightweight local retrieval:

```text
task kind + user instruction + current key/section
  -> local Field Registry search
  -> top relevant field definitions
  -> compact evidence block
  -> prompt context
```

Field Registry matches are advisory evidence. They help ground the model response, but they remain fallback/reference data and do not become a hard legality gate.

Retrieval should prefer:

- Current key under caret.
- Current Section type when available.
- Direct field-name matches.
- Aliases or known related fields when available.
- Diagnostics-linked field names.
- User-selected INI snippet fields.

The prompt should include only the top relevant matches needed for the task.

## 7. Context Provider

The context provider builds a bounded, explainable context package from existing IDE state.

Allowed initial context:

- Current file display name.
- Current Section name.
- Current key/value under caret.
- Explicit user selection or pasted snippet.
- Nearby lines around the caret, with a small bound.
- Diagnostic summaries relevant to the current file, caret, or selection.
- Field Registry match summaries.

Avoid by default:

- Whole project content.
- Whole repository content.
- Entire Field Registry.
- Absolute local paths unless explicitly needed.
- Hidden files.
- API keys, tokens, credentials, environment variables, or user-local secrets.
- Generated directories such as `.vs`, `bin`, `obj`, `artifacts`, and `TestResults`.

Context collection must be triggered by explicit user action. It must not run as a background upload mechanism.

## 8. Prompt Builder

The prompt builder converts task kind, bounded context, retrieved Field Registry evidence, and user instruction into a provider request.

Prompt builder responsibilities:

- Keep application rules separate from project content.
- Treat INI content and comments as data, not instructions.
- Include only bounded context.
- Mark Field Registry evidence as advisory reference data.
- Ask for uncertainty notes when relevant.
- Request INI output as draft text, not as an edit instruction.

The prompt builder must not ask the provider to edit files, save files, update registries, run tools, or infer authority it does not have.

## 9. AI Client Abstraction

Future code should use a small provider abstraction such as:

```csharp
internal interface IRa2AiClient
{
    Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken);
}
```

This is an architecture suggestion only. AI-0 does not authorize adding this interface.

The client abstraction should support:

- Mock client.
- Future DeepSeek adapter.
- Cancellation.
- Error reporting without crashing the IDE.
- Testability without live network.

## 10. Mock Client First Policy

The first implementation phase must use a deterministic mock client.

Mock-first requirements:

- No network access.
- No API key.
- No DeepSeek call.
- No file writes.
- Deterministic responses for tests.
- Clear UI state for busy, cancel, response, copy, and clear.

The mock client validates the Right Tool Well integration, panel state, task kind selector, context summary, command enablement, and response display before provider risk is introduced.

## 11. DeepSeek Adapter Future Phase

DeepSeek integration belongs to a later phase after the mock UI and context pipeline are stable.

The DeepSeek adapter must:

- Sit behind the AI client abstraction.
- Use bounded prompt requests produced by the prompt builder.
- Treat DeepSeek output as text only.
- Never grant file-system authority to DeepSeek.
- Handle cancellation and network errors gracefully.
- Avoid requiring real network or API keys in normal tests.

DeepSeek must not execute tools, inspect files directly, or modify project state.

## 12. Draft / Copy Workflow

Early phases should support copy-oriented use:

```text
generate response
  -> show explanation / suggestion / draft
  -> user reviews
  -> user copies result manually
```

The IDE may provide copy commands for response text or draft blocks. Copying is explicit and does not mutate the active document.

Generated INI blocks must be visibly marked as draft output.

## 13. Future Confirmed Insert Workflow

A future insert workflow may be considered only after a separate contract.

Required boundaries for that future phase:

- Show exact insertion target.
- Show draft content before insertion.
- Require explicit user confirmation.
- Re-check document state before insertion.
- Preserve existing dirty-state, save preflight, backup, rollback, parser, diagnostics, and undo/redo semantics.
- Never insert automatically from a DeepSeek response.

Confirmed insert is not part of AI-0, AI-1, AI-2, AI-3, AI-4, or AI-5 unless separately approved.

## 14. Test Strategy

Future implementation tests should cover:

- Section Tree remains the default Right Tool Well page.
- AI page opens only by explicit command.
- Existing Section Tree AutomationIds remain.
- AI panel AutomationIds exist after implementation.
- No Apply button exists in early phases.
- Mock generation does not modify editor text.
- Mock generation does not change document dirty state.
- Mock generation does not write files.
- Context summary shows bounded categories and counts.
- Field evidence is shown as advisory reference data.
- Cancel returns the panel to a stable state.
- Copy copies response text without document mutation.
- Network/provider errors are represented as UI state and do not crash the IDE.

Normal CI must not require real DeepSeek credentials or live network.

## 15. Phase Roadmap

Recommended AI phase order:

- AI-0P: Placement contract. Completed as `Docs/AiAgentPanelPlacementContract.md`.
- AI-0: Architecture and safety contracts. Creates `Docs/AiAssistantArchitecture.md` and `Docs/AiAssistantSafetyContract.md`.
- AI-1: Right Tool Well AI tab with mock client only. No DeepSeek, no network, no apply, no file modification.
- AI-2: Context provider and local Field Registry retrieval. Still no required DeepSeek.
- AI-3: Prompt builder for the initial task kinds.
- AI-4: DeepSeek client adapter behind the AI client abstraction.
- AI-5: Draft output and explicit copy workflow.
- AI-6: Optional confirmed insert preview, only after a separate contract and approval.

Each implementation phase requires an approved implementation contract before source changes.

## 16. Acceptance Criteria

The architecture is acceptable when:

- The assistant is framed as a DeepSeek-powered RA2 Modding Assistant.
- It is not framed as a Codex-like file-editing agent.
- DeepSeek is only a text generation backend.
- Field Registry retrieval is local, bounded, and advisory.
- Context collection is explicit, bounded, and explainable.
- Mock client comes before DeepSeek.
- Early phases produce explanation, suggestions, drafts, and copyable output only.
- Any future insertion requires preview and explicit confirmation.
- Existing IDE editing, parser, diagnostics, save, backup, rollback, Field Registry, and legacy boundaries remain unchanged.
