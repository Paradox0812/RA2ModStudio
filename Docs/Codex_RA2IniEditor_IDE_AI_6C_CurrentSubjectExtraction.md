# Codex Task: RA2IniEditor.IDE AI-6C Current Subject / Draft Subject Extraction

## 0. Current Baseline

AI-6B has been completed.

Reported state:

```text
Ra2AiConversationTurn / Ra2AiConversationContext / Ra2AiConversationContextRequest added.
IRa2AiConversationContextProvider / Ra2AiConversationContextProvider added.
Current-session visible chat turns can be extracted with LastTurns / MaxCharacters / MaxSingleTurnCharacters bounds.
Assistant turns are marked as draft/advisory, not applied file state.
Sensitive provider/API-key-like metadata is redacted.
Tests: 1413 passed.
IdeOnly package: passed, packaged file count 753.
No PromptBuilder integration.
No current subject extraction.
No draft section ID extraction.
No Shell UI wiring.
Legacy not restored.
```

Next phase:

```text
AI-6C: Current Subject / Draft Subject Extraction
```

This is a limited source implementation phase.

Do not integrate PromptBuilder yet. That belongs to AI-6D.

Do not modify AI send flow yet.

---

## 1. Goal

Implement a small current-subject extraction layer for AI Assistant conversation context.

The system should be able to infer the current discussed subject from recent visible chat turns, especially from AI-generated INI drafts.

Example:

```text
Assistant generated:
[LAAV]
Primary=LAAVMissile

[LAAVMissile]
Warhead=LAAVMissileWH
```

Future prompts like:

```text
在这个单位基础上，把它改成苏军单位
```

should eventually have a current subject:

```text
SubjectKind = Unit
SubjectId = LAAV
Source = LastAssistantDraft
Summary = 轻型防空车草稿
```

AI-6C only extracts the subject. It does not yet inject it into PromptBuilder.

---

## 2. Hard Boundaries

Do not implement:

```text
PromptBuilder integration
Conversation Context section in prompt
Current Subject UI display
DeepSeek adapter changes
provider selection changes
AI send-flow changes
Apply / Insert
file modification
Field Registry writes
whole-project context
unbounded chat history
cross-session memory
hidden memory
settings persistence
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

This phase only extracts subject metadata from current-session chat context / assistant drafts.

---

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/AI/Ra2AiCurrentSubject.cs
RA2IniEditor.IDE/AI/Ra2AiSubjectKind.cs
RA2IniEditor.IDE/AI/Ra2AiSubjectSource.cs
RA2IniEditor.IDE/AI/IRa2AiCurrentSubjectExtractor.cs
RA2IniEditor.IDE/AI/Ra2AiCurrentSubjectExtractor.cs
RA2IniEditor.IDE/AI/Ra2AiConversationContext.cs, only if a small optional CurrentSubject property is needed
RA2IniEditor.IDE/AI/Ra2AiMarkdownResponseParser.cs, only if section-id extraction can reuse existing parser safely
RA2IniEditor.Tests/IDE/Ra2AiCurrentSubjectExtractorTests.cs
RA2IniEditor.Tests/IDE/Ra2AiMarkdownResponseParserTests.cs, only if parser behavior is extended
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Allowed only if needed for minimal future wiring:

```text
RA2IniEditor.IDE/AI/Ra2AiConversationContextProvider.cs
RA2IniEditor.Tests/IDE/Ra2AiConversationContextProviderTests.cs
```

Prefer pure model/extractor tests first.

Do not modify Shell UI in this phase.

---

## 4. Subject Model

Suggested model:

```csharp
internal enum Ra2AiSubjectKind
{
    Unknown,
    Unit,
    Weapon,
    Warhead,
    Projectile,
    Art,
    Section
}

internal enum Ra2AiSubjectSource
{
    Unknown,
    CurrentCaretSection,
    LastAssistantDraft,
    UserMention
}

internal sealed class Ra2AiCurrentSubject
{
    public Ra2AiSubjectKind Kind { get; init; }
    public string? SubjectId { get; init; }
    public Ra2AiSubjectSource Source { get; init; }
    public string Summary { get; init; } = string.Empty;
    public double Confidence { get; init; }
}
```

Adjust naming/style to project conventions.

---

## 5. Extraction Inputs

The extractor should consume only:

```text
Ra2AiConversationContext
optional current IDE SectionName if supplied by future request object
```

In AI-6C, prefer extracting from conversation context only unless a small optional request object already exists.

Do not read:

```text
project files
current editor text directly
Field Registry provider
diagnostics service
environment variables
clipboard
cross-session memory
```

---

## 6. Draft Section ID Extraction

Extract candidate section IDs from assistant draft text.

Relevant patterns:

```ini
[LAAV]
[LAAVMissile]
[LAAVMissileWH]
[LAAVMissileP]
```

Only parse visible assistant draft text.

Do not parse hidden payloads.

Do not assume extracted IDs exist in project files.

---

## 7. Subject Kind Heuristics

Use lightweight heuristics only.

Suggested heuristics:

### 7.1 Unit

If assistant draft contains a section with fields commonly associated with units:

```text
Strength
Armor
Primary
Speed
TechLevel
Owner
Cost
Prerequisite
```

Then that section may be `Unit`.

### 7.2 Weapon

If a section has fields:

```text
Damage
ROF
Range
Projectile
Warhead
```

Then it may be `Weapon`.

### 7.3 Warhead

If a section has fields:

```text
Verses
CellSpread
PercentAtMax
InfDeath
Wall
Wood
```

Then it may be `Warhead`.

### 7.4 Projectile

If a section has fields:

```text
AA
AG
Arm
Shadow
Proximity
Ranged
Rotates
SubjectToCliffs
```

Then it may be `Projectile`.

### 7.5 Art

If context is under `artmd.ini 草稿` or contains:

```text
Voxel
Remapable
Cameo
TurretOffset
PrimaryFireFLH
```

Then it may be `Art`.

Keep these heuristics conservative.

If uncertain:

```text
Kind = Section
or Unknown
```

---

## 8. Current Subject Selection Rules

If multiple subjects exist, prioritize:

```text
1. last assistant draft's main unit section if present
2. user-mentioned section ID in recent user turn
3. last assistant draft's first non-weapon support section
4. highest-confidence extracted subject
```

For generated unit prototypes, the main unit is usually the first unit-like section.

Weapon / Warhead / Projectile sections should be treated as follow-up definitions unless the user specifically asks about "这个武器", "这个弹头", or mentions the ID.

AI-6C does not need full pronoun resolution yet; it only extracts candidate current subject.

---

## 9. Draft State Rules

Extracted subject must be marked as draft/advisory context:

```text
source = LastAssistantDraft
summary includes "上一轮 AI 草稿"
```

Do not present it as applied file state.

Do not modify project files.

Do not update Field Registry.

---

## 10. Tests

Add focused tests:

```text
1. Extracts unit subject from assistant draft containing [LAAV] with Strength / Armor / Primary.
2. Extracts weapon subject from [LAAVMissile] with Damage / ROF / Warhead.
3. Extracts warhead subject from [LAAVMissileWH] with Verses / CellSpread.
4. Extracts projectile subject from [LAAVMissileP] with AA / AG / Image.
5. Prioritizes main unit over weapon follow-up definitions for unit prototype draft.
6. User-mentioned section ID can influence selection if implemented.
7. Unknown / malformed draft returns Unknown safely.
8. Extracted subject is marked LastAssistantDraft and draft/advisory.
9. Does not claim subject exists in project files.
10. Does not read files / providers / diagnostics / environment variables.
```

Avoid UI pixel tests.

Do not require real DeepSeek or API key.

---

## 11. Validation Commands

Run full validation because source/tests may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 12. Manual Smoke Checklist

AI-6C may have no visible UI changes.

Optional smoke:

```text
1. Open AI Assistant.
2. Generate a unit draft.
3. Confirm chat behavior remains unchanged.
4. Confirm provider behavior remains unchanged.
5. Confirm no file changes and no dirty state.
```

---

## 13. Final Report Format

Report:

```text
1. Phase completed: AI-6C.
2. Files changed.
3. Current subject model summary.
4. Draft subject extraction heuristics.
5. Tests added.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation no PromptBuilder integration yet.
11. Confirmation no send-flow/UI change.
12. Confirmation no Apply/Insert/file modification behavior added.
13. Recommended next phase.
```
