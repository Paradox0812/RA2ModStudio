# AGENT-TRACE-1 Compact Retrieval Summary UI Contract

Status: approved and implemented on 2026-08-25; automated verification recorded in the current phase ledger.

## Goal

Expose enough retrieval state to explain why Work proceeded or stopped without adding another form-like panel, raw diagnostics dump, expander, or model-control surface.

## Exact visual behavior

- Chat mode: no retrieval summary.
- Work mode with no retrieval activity: no new line.
- Work mode after semantic retrieval: one muted line below the assistant status and above the proposal card.
- Text shape: `项目检索：1 轮 · 3 个实体 · 7 项事实 · 已就绪`.
- A round is one executed Host query batch: the initial intent-selected batch plus each compact refinement batch.
- An entity is one canonical Host binding. A fact is one successful bounded Host query result; field values are not inflated into separate UI counts.
- Stop labels:
  - `EvidenceReady` / `NoRefinementRequired`: `已就绪`
  - `NoProgress`: `无新证据，使用现有事实`
  - `RoundLimit`: `达到补查上限`
  - `NeedsClarification`: existing error card owns the explanation; summary remains hidden
  - `ProviderFailure`: existing error card owns the explanation; summary remains hidden
- No border, background card, icon-only command, disclosure arrow, tooltip containing raw prompt, or clickable action.
- Font, foreground and spacing reuse the current AI secondary-text token; maximum one line with ellipsis.

## Automation and accessibility

- Add `AutomationId=AiAssistant.RetrievalSummary` to the generated summary `TextBlock`.
- Accessible name equals the fully expanded Chinese summary.
- The element is not focusable and has no keyboard action.

## Allowed files

- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs` only, if the existing dynamic message builder can host the line without XAML changes.
- `RA2IniEditor.Tests/IDE/Ra2AiAuthoringShellBoundaryTests.cs` and related UI contract tests.

## Forbidden changes

- `ShellWindow.xaml`, Dock layout, composer geometry, mode selector, proposal card, toolbar, menus and global theme.
- Raw request/response, hidden prompt, Skill body, absolute path or provider metadata display.
- Any change to query, preview, apply, save or retry behavior.

## Acceptance

- Existing Chat and Work layout screenshots remain unchanged except for the one compact line when retrieval actually ran.
- The line reports backend facts exactly and hides on provider/clarification error cards.
- Existing AutomationIds and keyboard order remain unchanged.
