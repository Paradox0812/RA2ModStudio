# AI Assistant Conversation Context / Current Subject Contract

## 1. Scope and Baseline

This contract defines the future conversation-context and current-subject boundary for the RA2IniEditor.IDE AI Assistant.

Baseline:

- The AI Assistant is an RA2 / Yuri's Revenge / Ares / Phobos INI modding assistant.
- AI output is explanation, suggestion, or draft text only.
- Current AI context already separates bounded current IDE context, Field Registry evidence, and diagnostics summary.
- AI-5B stable draft rules require generated INI to stay draft/advisory, avoid hallucinated field keys, and avoid claiming Apply / Insert / Save / file write.
- AI-6A is documentation only.

AI-6A does not authorize source changes, XAML changes, code-behind changes, ViewModel changes, tests, scripts, project files, Field Registry JSON changes, legacy changes, DeepSeek adapter changes, provider selection changes, PromptBuilder source changes, send-flow changes, Apply / Insert, editor text mutation, dirty-state mutation, file writes, or Field Registry writes.

## 2. Problem Statement

The current live send flow can include:

- the current user prompt,
- bounded current IDE context,
- local Field Registry evidence,
- bounded diagnostics summary.

That is not enough for follow-up prompts such as:

- "这个单位"
- "刚才那个武器"
- "在这个基础上"
- "继续修改"
- "把它改成苏军单位"

The missing concept is a bounded, explainable carryover from recent chat and recent draft output. When the previous assistant turn generated a draft such as `[LAAV]`, `[LAAVMissile]`, `[LAAVMissileP]`, and `[LAAVMissileWH]`, a later prompt like "把这个单位改成苏军单位" should be able to resolve "这个单位" to the current subject. The model must still understand that the draft is only conversation text, not applied project file state.

## 3. Context Types

Future prompts must keep three context types separate.

### 3.1 Current IDE Context

Current IDE Context is derived from the active editor and current IDE state.

Allowed categories:

- Current file display name.
- Caret line / offset summary.
- Current Section name and Section kind.
- Current Key / Value under caret.
- Bounded nearby text around the caret.
- Explicit selected text when the user selected it.
- Field Registry evidence already retrieved from the active provider.
- Diagnostics summary already bounded by the diagnostics context provider.

Rules:

- It represents current visible IDE state.
- It must remain bounded.
- It must not include whole project content.
- It must not include entire current document text unless a later contract explicitly permits a bounded broader mode.
- It must not include absolute local paths by default.
- It must not read hidden files, generated directories, environment variables, API keys, or provider-internal metadata.

### 3.2 Conversation Context

Conversation Context is recent chat history used to resolve follow-up references.

Allowed categories:

- Last N user / assistant turns from the current AI Assistant session.
- Bounded character-limited user text.
- Bounded character-limited assistant text.
- Summaries of large assistant responses.
- Summaries of fenced code block section IDs and draft object IDs.

Forbidden categories:

- Complete infinite chat history.
- Cross-session hidden memory.
- Hidden provider metadata.
- API keys, Authorization headers, credentials, tokens, environment variables, or settings values.
- Raw DeepSeek request or response payloads.
- Full context payload logs.
- Whole project files.
- Full Field Registry dump.

Conversation Context is advisory. It helps resolve references, but it does not override Current IDE Context, application safety rules, Field Registry evidence boundaries, diagnostics semantics, or stable draft-output rules.

### 3.3 Current Subject

Current Subject is a compact state describing the primary object being discussed.

Examples:

- `LAAV` unit draft from the previous assistant response.
- `LAAVMissile` weapon draft from the previous assistant response.
- Current caret Section `[HTNK]`.
- A section ID explicitly mentioned by the user.

Current Subject is not proof that the object exists in the project. If the source is a prior assistant draft, it is only conversation draft state unless the user explicitly says it was applied, pasted, or saved.

## 4. Current Subject Model

Future implementation should use a small model similar to this contract anchor:

```csharp
internal enum Ra2AiSubjectKind
{
    Unit,
    Weapon,
    Warhead,
    Projectile,
    Section,
    Unknown
}
```

```csharp
internal enum Ra2AiSubjectSource
{
    CurrentCaretSection,
    LastAssistantDraft,
    UserMention
}
```

```csharp
internal sealed class Ra2AiCurrentSubject
{
    public Ra2AiSubjectKind SubjectKind { get; init; }
    public string? SubjectId { get; init; }
    public Ra2AiSubjectSource Source { get; init; }
    public string Summary { get; init; } = string.Empty;
    public double Confidence { get; init; }
}
```

Field meanings:

- `SubjectKind`: coarse object type. Values are `Unit`, `Weapon`, `Warhead`, `Projectile`, `Section`, and `Unknown`.
- `SubjectId`: object or section ID, such as `LAAV`, `HTNK`, `LAAVMissile`, `LAAVMissileP`, or `LAAVMissileWH`.
- `Source`: where the subject came from: current caret section, last assistant draft, or explicit user mention.
- `Summary`: short human-readable explanation, such as "上一轮 AI 草稿中的轻型防空车".
- `Confidence`: deterministic confidence score for UI/prompt wording. Low confidence must be expressed as uncertainty, not as a fact.

Selection priority recommendation:

1. Explicit user mention in the current prompt.
2. Current caret Section when the prompt points at current editor context.
3. Last assistant draft when the prompt uses follow-up language such as "这个单位", "刚才那个武器", "在这个基础上", or "继续修改".
4. Unknown when there is no reliable subject.

If the prompt is ambiguous and multiple recent subjects exist, the prompt should include the ambiguity and ask the model to state uncertainty instead of silently choosing one.

## 5. Chat History Bounds

Conversation Context must be bounded.

Recommended defaults for future implementation:

- Last turns: 3 to 5 user / assistant turns.
- Maximum total conversation context: 4000 to 8000 characters.
- Maximum single prior assistant message included verbatim: 1500 to 2500 characters.
- Maximum code block summary items: 8 to 12 section IDs.
- Maximum current-subject candidates: 3.

Large assistant response handling:

1. Keep the first concise prose summary when available.
2. Extract fenced code block section IDs.
3. Extract obvious heading/object IDs when deterministic.
4. Summarize code blocks as IDs and target file hints, not full code, when the response exceeds the character cap.
5. Preserve enough text to explain that the prior content is an AI draft.

Do not include the entire chat history forever. Do not include hidden memory, cross-session memory, raw provider payloads, or full unbounded assistant responses.

## 6. Draft Memory Rules

Assistant-generated INI drafts are conversation drafts only.

Future draft extraction may derive lightweight metadata from recent assistant messages:

- Section IDs inside fenced INI code blocks.
- Target file headings such as `rulesmd.ini` and `artmd.ini`.
- Object IDs referenced as values, such as `Primary=LAAVMissile`.
- TODO definitions listed by the assistant.
- Short assistant-provided design summary.

Example draft section IDs:

```text
[LAAV]
[LAAVMissile]
[LAAVMissileP]
[LAAVMissileWH]
```

Possible extracted subject candidates:

```text
SubjectKind=Unit
SubjectId=LAAV
Source=LastAssistantDraft
Summary=上一轮 AI 草稿中的单位主体
```

```text
SubjectKind=Weapon
SubjectId=LAAVMissile
Source=LastAssistantDraft
Summary=上一轮 AI 草稿中的主武器
```

Rules:

- A prior assistant draft is not project file state.
- A prior assistant draft is not evidence that the object exists in `rulesmd.ini` or `artmd.ini`.
- Do not assume generated IDs are valid or already defined.
- Do not use prior draft fields to bypass Field Registry evidence rules.
- Do not write the draft into the editor.
- Do not mark the document dirty.
- Do not write Field Registry data.
- Do not implement Apply / Insert.
- If the user explicitly says they pasted/applied/saved the draft, future logic may treat that as a user claim, but must still verify via Current IDE Context or explicit user-provided text before calling it project state.

## 7. PromptBuilder Integration Rules

Future PromptBuilder integration should add two separate prompt sections:

```text
Conversation Context
Current Subject
```

These sections must remain separate from:

```text
Current IDE Context
Field Registry Evidence
Diagnostics Summary
Output Requirements
```

Required prompt wording:

```text
Conversation Context is bounded recent chat from this AI Assistant session. It is advisory context only.
Prior assistant drafts are conversation draft text, not applied project file state.
Do not assume a prior generated draft exists in rulesmd.ini or artmd.ini unless the user explicitly says it was applied/pasted/saved and the current IDE context supports that.
Current Subject helps resolve phrases like "这个单位" or "刚才那个武器", but low-confidence subjects must be treated as uncertain.
```

PromptBuilder must continue to obey existing boundaries:

- It must not read files.
- It must not inspect editor controls directly.
- It must not query providers.
- It must not rerun diagnostics.
- It must not read environment variables or API keys.
- It must not call DeepSeek or network code.
- It must not collect extra context beyond the already-built bounded context objects.
- It must not request Apply, Insert, Save, file write, Field Registry write, tool execution, or shell command execution.
- It must preserve no-hallucinated-fields rules for clean draft blocks.

## 8. UI Context Summary

Future UI may display a compact current-subject summary in the AI panel.

Examples:

```text
当前主题：LAAV（上一轮 AI 草稿）
```

```text
当前主题：当前光标 Section [HTNK]
```

```text
当前主题：未确定
```

UI rules:

- The summary is display-only.
- It must not trigger provider calls.
- It must not trigger file IO.
- It must not reload Field Registry data.
- It must not rerun diagnostics.
- It must not mutate editor text.
- It must not mark dirty.
- It must not imply a draft was applied.

The UI may expose enough information for the user to understand what "这个单位" will refer to before sending.

## 9. Privacy and Safety Boundaries

Conversation context and current subject must preserve all existing safety boundaries.

Forbidden:

- Uploading whole project content.
- Uploading entire repository content.
- Uploading full unlimited chat history.
- Reading hidden memory.
- Reading cross-session memory.
- Reading or sending API keys.
- Reading or sending environment variables.
- Sending Authorization headers.
- Sending hidden provider metadata.
- Sending raw provider payloads.
- Sending absolute paths by default.
- Logging raw prompts, raw responses, full context payloads, nearby text, selected INI text, or assistant drafts by default.
- Modifying editor text.
- Marking dirty state.
- Writing files.
- Writing Field Registry data.
- Implementing Apply / Insert.
- Changing DeepSeek adapter behavior.
- Changing provider selection behavior.
- Changing PromptBuilder source in AI-6A.
- Changing send flow in AI-6A.

Conversation Context and Current Subject are advisory context only. They do not create any new editing authority.

## 10. Tests to Add / Update

Future implementation tests should cover:

1. Last N user / assistant turns are included.
2. Conversation context is bounded by character count.
3. Complete unlimited chat history is not included.
4. Current subject can be extracted from a prior assistant draft code block containing `[LAAV]`.
5. Weapon / projectile / warhead subject candidates can be extracted from `[LAAVMissile]`, `[LAAVMissileP]`, and `[LAAVMissileWH]`.
6. A prompt containing "这个单位" includes the current subject when one exists.
7. A prompt containing "刚才那个武器" prefers the prior weapon subject when one exists.
8. Prior assistant draft context is labeled as draft, not applied file state.
9. If a prior response is large, it is truncated or summarized.
10. Code block summaries list section IDs without including the full unbounded code block when over limit.
11. API keys, Authorization headers, environment variable names/values, provider metadata, raw request payload, and raw response body are excluded.
12. Current IDE Context remains separate from Conversation Context.
13. Field Registry Evidence remains separate and advisory.
14. Diagnostics Summary remains separate and advisory.
15. Conversation context extraction does not modify editor text.
16. Conversation context extraction does not mark document dirty.
17. Conversation context extraction does not write files.
18. Conversation context extraction does not write Field Registry data.
19. PromptBuilder tests verify that prior drafts are not described as project file state.
20. UI boundary tests, if AI-6E is approved later, verify the current-subject summary is display-only.

Suggested future test files:

```text
RA2IniEditor.Tests/IDE/Ra2AiConversationContextTests.cs
RA2IniEditor.Tests/IDE/Ra2AiCurrentSubjectExtractorTests.cs
RA2IniEditor.Tests/IDE/Ra2AiPromptBuilderConversationContextTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
```

AI-6A itself is documentation only and does not add these tests.

## 11. Recommended Implementation Split

Recommended phases:

- AI-6A: Conversation context / current subject contract.
- AI-6B: Chat history context model and bounded extraction.
- AI-6C: Draft subject extraction from assistant messages / code blocks.
- AI-6D: PromptBuilder integration.
- AI-6E: Context Summary UI polish.

AI-6B should stay model/extraction-only if possible. AI-6C should focus on deterministic parsing of recent assistant messages and fenced code blocks. AI-6D should only integrate already-built conversation context and current subject into PromptBuilder. AI-6E should be a separate UI contract/implementation phase with explicit approval.

No phase in this split should implement Apply / Insert or automatic file modification without a later dedicated preview/confirm insertion contract.

## 12. Acceptance Criteria

AI-6A is accepted when this contract defines:

1. The difference between Current IDE Context, Conversation Context, and Current Subject.
2. A future Current Subject model with `SubjectKind`, `SubjectId`, `Source`, `Summary`, and `Confidence`.
3. Bounded chat history rules.
4. Draft memory rules for assistant-generated INI drafts.
5. Explicit wording that prior AI drafts are not applied project state.
6. PromptBuilder integration rules that keep Conversation Context and Current Subject separate from Current IDE Context, Field Registry Evidence, and Diagnostics Summary.
7. Future UI context summary rules.
8. Privacy and safety boundaries.
9. Future tests to add or update.
10. A staged implementation split for AI-6B through AI-6E.

AI-6A must not modify source code, XAML, code-behind, ViewModels, tests, scripts, project files, Field Registry JSON, or legacy files.
