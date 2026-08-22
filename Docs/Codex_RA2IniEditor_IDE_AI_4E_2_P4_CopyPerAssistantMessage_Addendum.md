# Codex Task Addendum: RA2IniEditor.IDE AI-4E-2-P4 Copy Button Per Assistant Message

## 0. Context

User refined the AI Assistant chat action placement decision:

```text
复制应该出现在每一条 AI 回复旁边。
```

This addendum supersedes the earlier "copy latest response" placement.

The copy action is still valid, but it must be attached to each assistant message card, not placed globally inside Advanced or only as a single latest-response button.

---

## 1. Goal

Move / expose copy action per assistant message.

Target behavior:

```text
1. Every assistant message has its own copy action.
2. Copy copies that specific assistant message content.
3. User messages do not need copy buttons in this phase.
4. Advanced area still only contains model selection.
5. No global copy button inside Advanced.
```

---

## 2. UI Placement

For each assistant message card:

```text
助手回复内容                                      [复制]
```

or compact icon-style:

```text
助手回复内容                                      ⧉
```

Preferred:

```text
small copy button/icon in the top-right or bottom-right of each assistant message card
```

Avoid:

```text
copy button in Advanced
copy button inside composer
one global copy button that only copies latest response
large button row for each message
```

---

## 3. Behavior

When clicking copy on an assistant message:

```text
copy exactly that assistant message text
```

Rules:

```text
1. Do not copy user messages.
2. Do not copy hidden context.
3. Do not copy raw prompt.
4. Do not copy API key or provider metadata.
5. Do not modify editor text.
6. Do not mark document dirty.
```

If current message rendering is not data-template-based yet, implement the smallest safe approach.

---

## 4. AutomationId Guidance

Existing global `AiAssistant.CopyButton` may remain only if it is already used in tests, but it should no longer be the primary visible copy action in Advanced.

Recommended new IDs:

```text
AiAssistant.AssistantMessageCopyButton
```

If each message needs unique automation identification, use stable message container structure rather than dynamic IDs.

Allowed:

```text
AiAssistant.AssistantMessageList
AiAssistant.LatestAssistantMessage
AiAssistant.AssistantMessageCopyButton
```

Forbidden:

```text
AiAssistant.CopyButton inside Advanced
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
AiAssistant.ApplyButton
```

---

## 5. Tests

Update boundary/behavior tests:

```text
1. Advanced area does not contain visible Copy button.
2. Assistant message card exposes copy action.
3. Copy action copies the corresponding assistant message text.
4. Multiple assistant messages can each be copied independently, if current chat model supports multiple messages.
5. Copy does not modify source editor text.
6. Copy does not mark document dirty, if observable.
7. No Apply button exists.
```

If UI automation for multiple message copy is too heavy, add source-boundary and unit tests around the chat message copy handler / model.

Avoid pixel-perfect tests.

---

## 6. Manual Smoke Checklist

After implementation:

```text
1. Send one mock message.
2. Confirm the AI reply has a copy action beside it.
3. Click copy and confirm copied text matches that reply.
4. Send a second message.
5. Confirm both AI replies have copy actions.
6. Copy the first reply and confirm it does not copy the latest reply by mistake.
7. Confirm Advanced still only shows model selector.
8. Confirm no editor text changes and no dirty state.
```

---

## 7. Final Report Addition

In the final report for AI-4E-2-P4, include:

```text
Copy placement:
  Per assistant message / global latest response / unchanged

If implemented per message:
  confirm each assistant message can be copied independently.
```
