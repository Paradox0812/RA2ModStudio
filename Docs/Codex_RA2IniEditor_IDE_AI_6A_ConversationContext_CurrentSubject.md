# Codex Task: RA2IniEditor.IDE AI-6A Conversation Context / Current Subject Carryover Contract

## 0. Context

Manual live DeepSeek smoke shows that the AI Assistant can answer, but it does not reliably preserve conversational context.

Observed issue:

```text
User: 在这个单位基础上，我希望它是苏军的单位
Assistant: 用户没有明确指定这个单位是哪一个……
```

This means the current live prompt likely contains:

```text
current user prompt
current IDE caret context
field evidence
diagnostics summary
```

but does not include enough bounded chat history / previous draft context / current subject state.

This task is a contract / planning phase.

Do not implement source changes in this task.

---

## 1. Goal

Define how AI Assistant should carry short bounded conversation context between turns.

The assistant must understand references such as:

```text
这个单位
刚才那个武器
在这个基础上
继续修改
把它改成苏军单位
```

without uploading unbounded chat history or project files.

---

## 2. Required Context Types

The AI prompt should eventually include three separate context blocks:

```text
1. Current IDE Context
   - file / caret / section / key / nearby text

2. Current Chat Context
   - last N user/assistant turns
   - bounded character limit
   - assistant draft summaries / code block IDs

3. Current Subject
   - the main object being discussed, if known
   - e.g. LAAV unit draft, LAAVMissile weapon, current selected section
```

---

## 3. Current Subject Contract

Track a small current subject state.

Suggested fields:

```text
SubjectKind: Unit / Weapon / Warhead / Projectile / Section / Unknown
SubjectId: e.g. LAAV, HTNK, LAAVMissile
Source: CurrentCaretSection / LastAssistantDraft / UserMention
Summary: short text
```

Examples:

```text
SubjectKind=Unit
SubjectId=LAAV
Source=LastAssistantDraft
Summary=轻型防空车草稿
```

When user says:

```text
这个单位
这个武器
在这个基础上
```

PromptBuilder should include Current Subject so the model can resolve reference.

---

## 4. Chat History Rules

Include only bounded recent context.

Recommended:

```text
Last 3 to 5 turns
Max characters: 4000 to 8000
Assistant code blocks may be summarized if too long
```

Do not include:

```text
entire conversation
raw full chat history forever
API keys
hidden context
full project files
```

---

## 5. Draft Memory Rules

If assistant generated an INI draft, extract lightweight draft metadata:

```text
object IDs mentioned in headings/code blocks
section IDs from code blocks
field blocks generated
follow-up definitions
```

Example from generated draft:

```text
[LAAV]
[LAAVMissile]
[LAAVMissileP]
[LAAVMissileWH]
```

Store or derive this from recent assistant messages.

Do not apply it to files.

Do not treat it as verified project state.

---

## 6. PromptBuilder Requirement

PromptBuilder should include:

```text
Conversation Context
Current Subject
```

separately from:

```text
Current IDE Context
Field Registry Evidence
Diagnostics Summary
```

Prompt must state:

```text
Conversation context is prior assistant/user chat, not applied file state.
If a prior draft is referenced, treat it as draft text unless the user says it was applied.
Do not assume generated draft exists in project files.
```

---

## 7. UI Requirement

Optional future UI:

```text
Context summary line:
当前主题：LAAV（上一轮 AI 草稿）
```

or:

```text
当前主题：当前光标 Section [HTNK]
```

This helps user know what "这个单位" will refer to.

---

## 8. Hard Boundaries

Do not implement:

```text
automatic file modification
Apply / Insert
Field Registry writes
whole-project context
unbounded chat upload
hidden memory outside this session
settings persistence
DeepSeek adapter changes
API key handling changes
```

---

## 9. Recommended Implementation Split

```text
AI-6A: Conversation context / current subject contract
AI-6B: Chat history context model and bounded extraction
AI-6C: Draft subject extraction from assistant messages/code blocks
AI-6D: PromptBuilder integration
AI-6E: Context summary UI polish
```

---

## 10. Tests to Plan

Future tests should cover:

```text
1. Last N turns are included and bounded.
2. Current subject can be derived from last assistant draft.
3. "这个单位" prompt includes current subject.
4. Prior draft is marked as draft, not applied file state.
5. Chat history does not include API key or hidden provider metadata.
6. Large previous response is summarized/truncated.
7. Current IDE context still remains bounded.
8. No file modification / dirty state occurs.
```

---

## 11. Acceptance Criteria

This contract is accepted when it defines:

```text
1. How short chat history is included.
2. How current subject is tracked.
3. How prior AI drafts are treated as draft context.
4. How PromptBuilder separates chat context from IDE context.
5. How bounds/privacy are preserved.
```
