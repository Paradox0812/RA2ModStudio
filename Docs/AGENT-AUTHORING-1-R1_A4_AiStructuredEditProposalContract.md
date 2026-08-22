# AGENT-AUTHORING-1-R1 A4 — AI Structured Edit Proposal Contract

Status: Completed and verified on 2026-07-28.

## 1. Goal

A4 adds one DeepSeek tool, `preview_ini_edit_plan`, which may propose a bounded
single-document `Ra2IniEditPlan`. The IDE remains the sole authority for
identity, revisions, preview generation, confirmation, editor mutation, undo
and save state.

The AI may propose. It may not apply, save, undo, redo, select a file, or write
raw editor text.

## 2. Canonical path

```text
Shell capture
  -> Ra2AiAssistantPipeline
  -> DeepSeek tool-call transport
  -> Ra2AiAuthoringToolAdapter
  -> Ra2IniAuthoringWorkspace.Preview
  -> proposal card
  -> explicit user confirmation
  -> Ra2IniAuthoringWorkspace.Apply
  -> IRa2EditorTransactionPort
```

No parallel editor, parser, diagnostic, field-registry, save or undo path may be
introduced.

## 3. Provider tool

Only the following function tool is exposed:

```text
preview_ini_edit_plan
```

Arguments:

```json
{
  "summary": "string, 1..512 characters",
  "operations": [
    {
      "kind": "upsert_field | replace_field_value",
      "section": "string, 1..256 characters",
      "key": "string, 1..256 characters",
      "value": "string, 0..8192 characters"
    }
  ]
}
```

Rules:

- `operations` contains 1..128 items.
- `arguments` is limited to 65536 UTF-16 characters.
- Tool call ids are limited to 256 characters; names to 128 characters.
- Unknown and duplicate JSON properties, comments and trailing commas are
  rejected.
- Document id, path, edit revision, registry revision, plan id, preview id,
  origin, confirmation, save and apply flags are not accepted from the model.
- `Ra2IniEditPlan` and `Ra2IniEditOperation` constructors remain the final
  structural validators.
- `PlanId` is generated locally and `Origin` is the fixed trusted value
  `DeepSeekToolCall`.

Transport validates only stream structure, tool fragments, indexes and size.
The authoring adapter owns JSON and semantic validation. Invalid argument JSON
is not classified as a network protocol failure.

## 4. Capability policy

- Existing overloads are advisory-only and serialize the existing request
  shape without `tools` or `tool_choice`.
- The official DeepSeek endpoint may use
  `CurrentDocumentEditPreview` with `tool_choice=auto`.
- Custom endpoints remain advisory-only in A4.
- The prompt instructs the model to call the tool only when the user explicitly
  requests a current-document modification.
- An unexpected tool call can only create a review card; it can never apply.

## 5. Snapshot and concurrency

- Shell captures the immutable authoring snapshot on the UI thread before
  sending.
- The same UI-thread turn builds the provider context.
- The request lifecycle remains active through streaming, tool parsing and
  local preview generation.
- At tool completion, Shell captures a second snapshot.
- Document id, edit revision, exact text and field-registry revision must match.
  No automatic rebase is allowed.
- Preview preparation observes the request cancellation token.
- Apply is initiated on the UI thread and A3 performs a final currency check.
- Only one proposal may be active. Starting a new proposal preview supersedes
  the previous card even when the new preview later fails.

## 6. Failure taxonomy

```text
None
UnsupportedTool
MultipleToolCalls
MissingArguments
InvalidArgumentsJson
UnknownArgumentProperty
DuplicateArgumentProperty
InvalidOperation
RequestContextUnavailable
RequestContextStale
PreviewRejected
PreviewCancelled
ApplyBlocked
UnexpectedFailure
```

Provider text that was already streamed remains visible when proposal creation
fails. Raw provider payloads and arguments are never shown in safe error
messages.

## 7. Apply authority

Presentation state is not authority. The coordinator creates an immutable
proposal and enforces its apply policy before calling A3.

Apply is rejected when a proposal is blocked, stale, superseded, dismissed,
already applied, or no longer matches the active preview.

Policy:

- New errors: Blocked.
- New warnings: Caution.
- Unknown fields: Caution.
- `VerifiedGuardrail`, `Inferred`, `AutoExtracted`, `Obsolete`,
  `NonExistent`, `PseudoField` and `Unknown`: Caution.
- `Verified` and `ManualCurated`, with no new diagnostics: Normal.

Only the user-facing Apply button grants explicit confirmation. Apply changes
the in-memory document once, does not save, and remains undoable through the
existing editor transaction.

## 8. Proposal lifecycle

```text
Preparing -> Ready -> Applying -> Applied
          -> Blocked
          -> Stale
          -> Superseded
          -> Dismissed
          -> Failed
```

- New proposal preparation supersedes the prior proposal.
- Document mutation, document switch/reload, field-registry revision change and
  chat clearing invalidate both workspace preview and card state.
- Hiding the AI dock does not invalidate a ready proposal.
- Window close and chat clearing release event handlers and active preview.
- `Applied` records that the proposal was applied once; A4 does not track later
  undo/redo state changes in the card.

## 9. Conversation history and privacy

- Tool JSON, candidate text and preview objects are not stored as conversation
  turns.
- Provider assistant text is retained when present.
- When it is empty, the local assistant turn is:
  `已生成当前文件的结构化修改建议：{summary}。`
- Unapplied proposal refinement by reference to “the previous card” is outside
  A4; the user must restate the desired adjustment.
- Prompt, source text, tool arguments, candidate text, API keys and file
  contents are not logged.
- Diagnostics may record model id, counts, timing, failure classification and
  HTTP status only.

## 10. UI

The proposal is an inline UserControl inserted before the existing response
action row. It is not a modal dialog or a Dock pane.

- Use a locally styled virtualizing ListBox, not DataGrid.
- Full operation values remain inspectable with wrapping and internal scrolling.
- Apply disables synchronously when clicked.
- Buttons support keyboard navigation and automation names.
- Required AutomationIds:

```text
AiAssistant.EditProposalCard
AiAssistant.EditProposalCard.Status
AiAssistant.EditProposalCard.Summary
AiAssistant.EditProposalCard.OperationList
AiAssistant.EditProposalCard.DiagnosticSummary
AiAssistant.EditProposalCard.ApplyButton
AiAssistant.EditProposalCard.DismissButton
AiAssistant.EditProposalCard.ResultMessage
```

Visual checks cover 320, 360 and 520 DIP panel widths, 1920x1080 at 100% DPI,
150% DPI, long Chinese text and 128 operations.

## 11. Dark launch and rollback

A4 transport, adapter, coordinator and view remain dormant until Shell wiring
is completed. If activation fails, the Shell capability selection returns to
AdvisoryOnly and ordinary chat continues unchanged. No persistence migration,
dependency or project-file change is required.

## 12. Forbidden scope

Do not modify ShellWindow.xaml, docking layout, global theme, menus, toolbar,
parser semantics, diagnostics semantics, Field Registry priority/data,
completion, Hover, Quick Peek, Save Preflight, backup/rollback, search/replace,
project dependencies or legacy projects.

## 13. Verification

Required gates:

1. Tool DTO and response contract tests.
2. SSE fragmentation, mixed content/tool, cancellation and malformed protocol
   tests.
3. Exact ordinary-chat request-shape regression.
4. Adapter duplicate/unknown property, limits and semantic validation tests.
5. Snapshot staleness, single-active proposal and apply-policy tests.
6. ViewModel state, automation and Shell boundary tests.
7. IDE-only restore, build, full test suite and clean source package.
