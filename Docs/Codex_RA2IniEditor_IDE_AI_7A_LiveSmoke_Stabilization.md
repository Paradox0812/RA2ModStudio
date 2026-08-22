# Codex Task: RA2IniEditor.IDE AI-7A Live Smoke / AI Module Stabilization Checklist

## 0. Current Baseline

AI-6F has been completed.

Reported state:

```text
AI-6B: bounded conversation context extraction completed.
AI-6C: current subject extraction completed.
AI-6D: PromptBuilder receives ConversationContext and CurrentSubject.
AI-6F: subject-aware Field Registry evidence expansion completed.
Evidence retrieval now uses CurrentSubject / ConversationContext / previous draft field keys / follow-up intent profiles.
Tests: 1432 passed.
IdeOnly package: passed, packaged file count 763.
No DeepSeek adapter behavior change.
No provider selection behavior change.
No API key loading change.
No PromptBuilder safety rule weakening.
No Apply / Insert / file modification.
No Field Registry writes.
Legacy not restored.
```

Next phase:

```text
AI-7A: Live smoke / stabilization checklist
```

This is a verification and issue-capture phase.

Do not implement new features in this task.

## 1. Goal

Run a focused live smoke pass for the AI Assistant module and document remaining issues.

The goal is to verify that the current AI module is stable enough to call it an MVP / Alpha feature.

## 2. Hard Boundaries

Do not implement source changes unless a clear bug is found and the user approves a separate fix task.

Do not add:

```text
Apply / Insert
file modification
Field Registry writes
new provider behavior
new API key UI
settings persistence
streaming
retry loops
whole-project context
cross-session memory
```

## 3. Required Smoke Scenarios

### 3.1 Mock mode baseline

```text
1. Open AI Assistant.
2. Keep model/provider as Mock.
3. Send a simple prompt.
4. Confirm fake response appears.
5. Confirm no network/API key is required.
6. Confirm no editor text changes and no dirty state.
```

### 3.2 DeepSeek missing API key

```text
1. Clear DEEPSEEK_API_KEY for the launched process.
2. Select DeepSeek.
3. Send a simple prompt.
4. Confirm MissingConfiguration message appears in chat.
5. Confirm no crash.
6. Confirm no API key input UI appears.
```

### 3.3 DeepSeek live response

```text
1. Set DEEPSEEK_API_KEY in environment.
2. Launch IDE from a process that can see the environment variable.
3. Select DeepSeek.
4. Send: 解释 Strength 字段。
5. Confirm response appears.
6. Confirm no editor text changes and no dirty state.
7. Confirm no Apply / Insert button exists.
```

### 3.4 Stable draft generation

Prompt:

```text
帮我设计一个轻型防空车
```

Verify:

```text
1. Output is structured Markdown.
2. Clean INI block has no explanatory inline comments.
3. Missing faction uses TODO owner placeholder if user did not specify faction.
4. Generated IDs are listed under follow-up definitions.
5. Field rationale is outside code blocks.
6. Unverified fields go to optional / verify-before-use.
```

### 3.5 Conversation continuity

After scenario 3.4, prompt:

```text
在这个单位基础上，把它改成苏军单位。
```

Verify:

```text
1. AI understands "这个单位" as the previous draft subject.
2. It does not claim the subject already exists in project files.
3. It keeps output as draft/advisory.
4. It uses faction/owner-related field evidence when available.
```

### 3.6 Evidence expansion

Prompt variants:

```text
把这个单位改成盟军背景。
给这个单位加上对空武器。
让这个单位可以部署成防空炮。
让这个单位可以运输步兵。
让这个单位隐形侦察。
```

Verify:

```text
1. Evidence is no longer obviously too narrow for common fields.
2. Unconfirmed seed keys are not treated as evidence.
3. Output does not hallucinate unsupported field keys into clean draft.
```

### 3.7 Markdown rendering and copy

Use response containing:

```text
headings
bullet list
pipe table
inline code
fenced ini block
```

Verify:

```text
1. Headings render.
2. Lists render.
3. Pipe tables render.
4. Inline code renders without raw backticks.
5. Fenced code blocks render as code cards.
6. Copy full message copies original Markdown.
7. Copy code block copies code without fence markers.
```

### 3.8 Enter-to-send

```text
1. Type a normal prompt.
2. Press Enter.
3. Confirm it sends.
4. Type multi-line prompt with Shift+Enter.
5. Confirm newline is inserted and message is not sent.
6. Empty prompt + Enter is no-op.
```

## 4. Issue Classification

If issues are found, classify them as:

```text
Blocking:
  crash, file mutation, dirty-state mutation, API key leak, broken save/diagnostics, DeepSeek cannot work despite direct API success.

High:
  context continuity broken, evidence too narrow for common follow-up, Markdown rendering breaks code copy.

Medium:
  UI layout/spacing, minor wording, non-critical error messages.

Low:
  cosmetic polish.
```

## 5. Output Required

Create or update:

```text
Docs/AiAssistantLiveSmokeReport.md
```

Suggested structure:

```markdown
# AI Assistant Live Smoke Report

## 1. Environment

## 2. Scenarios Run

## 3. Results

## 4. Issues Found

## 5. Fix Recommendations

## 6. MVP Readiness Judgment
```

## 6. Validation Commands

After smoke, if no source changes are made:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If any source fix is approved later, run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 7. Acceptance Criteria

AI Assistant MVP / Alpha can be considered ready when:

```text
1. Mock mode works.
2. DeepSeek mode works when DEEPSEEK_API_KEY is set.
3. Missing API key is handled cleanly.
4. No file modification / dirty state occurs.
5. Stable draft output is acceptable.
6. Follow-up references like “这个单位” work.
7. Evidence retrieval is sufficient for common unit/weapon follow-ups.
8. Markdown rendering/copy behavior is usable.
9. No API key or sensitive data is exposed.
```
