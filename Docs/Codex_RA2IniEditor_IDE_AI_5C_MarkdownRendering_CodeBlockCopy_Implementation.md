# Codex Task: RA2IniEditor.IDE AI-5C Markdown Response Rendering / Code Block Copy Implementation

## 0. Current Baseline

AI-5B has been completed.

Expected state:

```text
Ra2AiPromptBuilder now enforces stable draft output rules.
DeepSeek output should be Markdown-structured.
Clean INI draft blocks should be copy-friendly.
Unverified field keys should not enter clean draft by default.
New object IDs / values must be listed under follow-up definitions.
AI output remains draft/advisory.
No Apply / Insert / file modification behavior exists.
```

Next phase:

```text
AI-5C: Markdown response rendering / code block copy
```

This is a limited UI/interaction implementation phase.

---

## 1. Goal

Improve AI Assistant response usability by treating AI responses as Markdown chat content and making fenced INI code blocks easy to copy.

The AI Assistant should:

```text
1. Keep showing AI responses in chat history.
2. Preserve Markdown structure in a readable way.
3. Detect fenced code blocks such as ```ini.
4. Provide copy action for each assistant message.
5. Provide copy action for each detected code block.
6. Never apply / insert / save generated content automatically.
```

---

## 2. Hard Boundaries

Do not implement:

```text
Apply / Insert
automatic file modification
Field Registry writes
Markdown-to-file conversion
automatic code block insertion
draft validation against Field Registry
new DeepSeek adapter behavior
new provider switching behavior
API key UI
settings persistence
whole-project context
auto-send context
diagnostic auto-fix
streaming output
retry loops
```

Do not modify:

```text
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn field registry JSON
legacy files
solution / project files
```

This phase is display/copy UX only.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/AI/Ra2AiChatMessage.cs, if such message model exists or needs a minimal extension
RA2IniEditor.IDE/AI/Ra2AiMarkdownBlock.cs
RA2IniEditor.IDE/AI/Ra2AiMarkdownResponseParser.cs
RA2IniEditor.Tests/IDE/Ra2AiMarkdownResponseParserTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project conventions. If the existing chat message model differs, adapt minimally.

Do not create a broad Markdown rendering framework.

---

## 4. Required Behavior

### 4.1 Markdown response handling

AI response text should remain the source of truth.

Minimum acceptable rendering:

```text
raw Markdown displayed clearly in assistant message cards
```

Preferred, if small and safe:

```text
split response into text blocks and fenced code blocks
show code blocks as compact code cards
```

Do not attempt full Markdown engine integration unless already available in the project.

### 4.2 Fenced code block detection

Detect code fences:

```markdown
```ini
[LAAV]
Strength=200
```
```

Also accept:

```markdown
```rules
...
```

```art
...
```

```
...
```
```

Each detected code block should expose:

```text
language label, if present
code text
copy code action
```

### 4.3 Per-message copy

Every assistant message should have a copy action.

Copy message:

```text
copies full assistant response text
```

Do not copy:

```text
hidden context
raw prompt
API key
provider metadata
editor text
```

### 4.4 Per-code-block copy

Every fenced code block should have its own copy action.

Copy code block:

```text
copies only the code content inside the fence
without Markdown fence markers
```

Do not modify the editor.

Do not mark document dirty.

### 4.5 User messages

User messages do not need code-block copy actions in this phase.

---

## 5. UI Requirements

Assistant message card should support:

```text
message body
copy message action
zero or more code block cards
copy code action per code block
```

Suggested AutomationIds:

```text
AiAssistant.AssistantMessageCopyButton
AiAssistant.CodeBlock
AiAssistant.CodeBlockCopyButton
AiAssistant.CodeBlockLanguage
```

Existing AutomationIds must remain where applicable:

```text
AiAssistant.ChatHistory
AiAssistant.UserMessageList
AiAssistant.AssistantMessageList
AiAssistant.LatestAssistantMessage
```

Forbidden:

```text
AiAssistant.ApplyButton
AiAssistant.InsertButton
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
```

---

## 6. Markdown Parser Requirements

Implement a small deterministic parser if needed.

Suggested output model:

```csharp
internal sealed class Ra2AiMarkdownBlock
{
    public bool IsCodeBlock { get; init; }
    public string? Language { get; init; }
    public string Text { get; init; } = string.Empty;
}
```

Parser rules:

```text
1. Split fenced code blocks from surrounding text.
2. Preserve code block content exactly except removing fence markers.
3. Preserve language token if present.
4. Handle multiple code blocks.
5. Handle unterminated fence gracefully as plain text or safe fallback.
6. Do not execute or interpret code.
```

---

## 7. Tests

### 7.1 Parser tests

Required:

```text
1. Parses a single ini fenced block.
2. Parses multiple fenced blocks.
3. Preserves language token.
4. Copy content excludes fence markers.
5. Handles text before and after code block.
6. Handles unterminated fence safely.
```

### 7.2 UI / behavior tests

Required:

```text
1. Assistant message copy action exists.
2. Code block copy action exists when response contains fenced code block.
3. Copy message copies full assistant response text, if testable.
4. Copy code block copies only code content, if testable.
5. Copy does not modify source editor text.
6. Copy does not mark document dirty, if observable.
7. No Apply / Insert button exists.
8. Existing provider behavior remains unchanged.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 8. Validation Commands

Run full validation because UI/source/tests may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Select Mock or DeepSeek.
3. Generate a response containing a fenced ini code block.
4. Confirm assistant message is readable.
5. Confirm each assistant message has copy action.
6. Confirm each code block has copy-code action.
7. Copy full message and verify it includes Markdown.
8. Copy code block and verify it contains only INI text.
9. Confirm no editor text changes.
10. Confirm no dirty state.
11. Confirm no Apply / Insert button exists.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-5C.
2. Files changed.
3. Markdown parsing/rendering strategy.
4. Per-message copy implementation.
5. Per-code-block copy implementation.
6. AutomationIds added/preserved.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation no Apply/Insert/file modification behavior added.
12. Confirmation provider behavior unchanged.
13. Manual smoke steps or result.
14. Remaining risks.
15. Recommended next phase.
```
