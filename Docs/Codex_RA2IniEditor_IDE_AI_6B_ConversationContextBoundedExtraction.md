# Codex Task: RA2IniEditor.IDE AI-6B Chat History Context Model / Bounded Extraction

## 0. Current Baseline

AI-6A has been completed.

Reported state:

```text
Docs/AiAssistantConversationContextContract.md created.
Conversation Context / Current Subject contract completed.
Tests: 1402 passed.
IdeOnly package: passed, packaged file count 746.
No source / XAML / code-behind / ViewModel / tests / scripts / project files changed.
No PromptBuilder / send-flow / DeepSeek adapter changes.
No Apply / Insert / file modification.
Legacy not restored.
```

Next phase:

```text
AI-6B: Chat history context model and bounded extraction
```

This is a limited source implementation phase.

Do not implement current subject extraction from drafts yet. That belongs to AI-6C.

Do not integrate PromptBuilder yet. That belongs to AI-6D.

## 1. Goal

Implement a small, bounded Conversation Context model for the current AI Assistant session.

The system should be able to extract recent visible chat turns into a safe context package for future PromptBuilder use.

Required result:

```text
1. Add model for recent chat turns.
2. Extract only current-session AI chat messages.
3. Bound by turn count and character count.
4. Exclude API keys / provider metadata / hidden context.
5. Mark assistant responses as conversation draft text, not applied file state.
6. Do not change current DeepSeek / Mock provider behavior yet.
```

## 2. Hard Boundaries

Do not implement:

```text
PromptBuilder integration
Current Subject extraction
Draft section ID extraction
Apply / Insert
file modification
Field Registry writes
whole-project context
unbounded chat history
cross-session memory
hidden memory
settings persistence
DeepSeek adapter changes
provider selection changes
auto-send context
diagnostic auto-fix
```

Do not modify:

```text
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn Field Registry JSON
legacy files
solution / project files
```

This phase only creates bounded conversation context extraction.

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/AI/Ra2AiConversationTurn.cs
RA2IniEditor.IDE/AI/Ra2AiConversationContext.cs
RA2IniEditor.IDE/AI/Ra2AiConversationContextRequest.cs
RA2IniEditor.IDE/AI/IRa2AiConversationContextProvider.cs
RA2IniEditor.IDE/AI/Ra2AiConversationContextProvider.cs
RA2IniEditor.IDE/AI/Ra2AiChatMessage.cs, only if existing chat model needs minimal read-only metadata
RA2IniEditor.Tests/IDE/Ra2AiConversationContextProviderTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if needed for minimal wiring / future visibility:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
```

Prefer pure provider/model tests first.

Do not modify Shell UI in this phase unless absolutely required.

## 4. Conversation Context Model

Suggested future model:

```csharp
internal enum Ra2AiConversationRole
{
    User,
    Assistant
}

internal sealed class Ra2AiConversationTurn
{
    public Ra2AiConversationRole Role { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsDraftResponse { get; init; }
}

internal sealed class Ra2AiConversationContext
{
    public IReadOnlyList<Ra2AiConversationTurn> Turns { get; init; } = [];
    public int TotalCharacterCount { get; init; }
    public bool WasTruncated { get; init; }
}
```

Adjust names/style to project conventions.

## 5. Extraction Rules

### 5.1 Source

Conversation context may only come from:

```text
current AI Assistant chat messages in the current session
```

Do not use:

```text
old persisted chats
cross-session memory
hidden model memory
provider raw payloads
logs
clipboard
whole project files
```

### 5.2 Bounds

Recommended defaults:

```text
LastTurns = 6
MaxCharacters = 6000
MaxSingleTurnCharacters = 2000
```

The exact values may be constants.

### 5.3 Truncation

If a message is too long:

```text
truncate safely
mark context WasTruncated = true
```

Do not throw for long assistant responses.

### 5.4 Sensitive content exclusion

Exclude or sanitize:

```text
API key
Authorization header
provider internal metadata
raw request payload
raw response payload beyond displayed assistant text
environment variables
absolute paths if not already visible to user
```

Since visible chat messages should not contain API keys, this is mostly a guardrail.

### 5.5 Draft state

Assistant messages must be marked as draft/advisory context:

```text
Assistant draft responses are not applied file state.
```

Future PromptBuilder must know this later.

## 6. Non-goals

Do not implement in AI-6B:

```text
current subject inference
section ID extraction from code blocks
PromptBuilder Conversation Context section
Context Summary UI display
DeepSeek call changes
```

Those belong to:

```text
AI-6C: Current Subject / Draft Subject Extraction
AI-6D: PromptBuilder integration
AI-6E: Context Summary UI polish
```

## 7. Tests

Add focused tests:

```text
1. Extracts recent user/assistant turns.
2. Keeps only last N turns.
3. Enforces max total character count.
4. Truncates oversized assistant response safely.
5. Marks assistant turns as draft responses.
6. Does not include hidden provider metadata.
7. Does not include API key-like text if sanitizer is implemented.
8. Empty chat returns empty context safely.
9. Extraction does not modify chat messages.
10. Extraction does not modify editor text or dirty state.
```

Avoid UI pixel tests.

Do not require real DeepSeek or API key.

## 8. Validation Commands

Run full validation because source/tests may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 9. Manual Smoke Checklist

AI-6B may have no visible UI changes.

Optional smoke:

```text
1. Open AI Assistant.
2. Send several messages.
3. Confirm chat behavior remains unchanged.
4. Confirm provider behavior remains unchanged.
5. Confirm no file changes and no dirty state.
```

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-6B.
2. Files changed.
3. Conversation context model summary.
4. Bounded extraction rules.
5. Tests added.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no PromptBuilder integration yet.
11. Confirmation no current subject extraction yet.
12. Confirmation no Apply/Insert/file modification behavior added.
13. Recommended next phase.
```
