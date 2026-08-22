# RA2IniEditor.IDE DeepSeek Modding Assistant Roadmap

## 0. Revised Direction

This project will not build a Codex-style file-modifying agent.

The AI feature is a **DeepSeek-powered RA2 Modding Assistant**:

```text
Field Registry / current INI context
        ↓
local retrieval / context builder
        ↓
prompt builder
        ↓
DeepSeek text generation
        ↓
explanation / field suggestions / INI draft / unit prototype
        ↓
user copies result or later uses explicit preview/confirm insert flow
```

DeepSeek is treated as a text generation backend. It does not modify files and does not execute tools.

---

## 1. Product Goal

The assistant helps RA2 / YR / Ares / Phobos mod authors understand and draft INI content.

Primary jobs:

```text
1. Explain fields.
2. Find relevant fields for a gameplay requirement.
3. Generate unit / weapon / building prototype drafts.
4. Review pasted INI snippets.
5. Explain diagnostics using field registry context.
6. Provide grounded suggestions with field evidence.
```

The assistant should make the IDE smarter without replacing the source editor or the user's judgment.

---

## 2. Non-goals

The assistant must not be a file-modifying autonomous agent.

Do not implement in early phases:

```text
auto-apply
auto-save
auto-fix
auto-edit current document
auto-update field registry
auto-run shell commands
whole-project upload by default
background suggestions on every caret move
```

Future insert/apply features, if ever added, must require explicit preview and user confirmation.

---

## 3. Data and Context Strategy

Do not send the entire field registry by default.

Use lightweight local retrieval:

```text
user request
  ↓
local field registry search
  ↓
top relevant field definitions
  ↓
prompt context
  ↓
DeepSeek response
```

Initial context may include:

```text
current file name
current section name
current key/value under caret
nearby lines
field registry matches
diagnostic summaries
user-selected INI snippet
```

Avoid by default:

```text
all project files
entire INI project
absolute local paths
API keys
hidden files
unrelated user data
```

---

## 4. First Supported Task Kinds

### 4.1 Explain Field

Input:

```text
field key / current caret key
```

Output:

```text
field meaning
value type
applies-to section
examples
related fields
notes / caveats
```

### 4.2 Find Fields by Requirement

Input:

```text
natural language requirement
```

Example:

```text
让坦克免疫心灵控制
```

Output:

```text
candidate fields
why they are relevant
where to apply them
example INI lines
uncertainties
```

### 4.3 Generate Unit Prototype

Input:

```text
unit type, role, balance direction, faction, tech level, weapon intent
```

Output:

```text
INI draft block
required follow-up definitions
field rationale
warnings
```

### 4.4 Generate Weapon Chain Draft

Input:

```text
weapon behavior requirement
```

Output:

```text
Weapon
Warhead
Projectile, if relevant
art/sound reminders, if relevant
field rationale
```

### 4.5 Review INI Snippet

Input:

```text
selected or pasted INI text
```

Output:

```text
unknown fields
likely wrong value types
missing related fields
dangerous assumptions
improvement suggestions
```

---

## 5. UI Placement

AI Assistant placement is governed by:

```text
Docs/AiAgentPanelPlacementContract.md
```

Current decision:

```text
Existing right-side Section area becomes a shared Right Tool Well.
Default page: Section Tree / Navigator.
Second page: AI Assistant.
No non-modal AI tool window fallback.
No second independent right sidebar.
AI opens only through explicit user command.
```

---

## 6. Proposed Right Tool Well AI Panel Layout

AI Assistant tab should contain:

```text
Header
Context Summary
Task Kind Selector
Prompt Input
Actions
Response Area
Field Evidence / References
Draft Preview
Safety Footer
```

Required first-phase controls:

```text
AI 助手
Context summary
Task kind selector
Prompt box
Generate response
Cancel
Copy result
Clear
Response area
Safety footer
```

No Apply button in early phases.

---

## 7. Safety Rules

```text
1. DeepSeek output is a draft or explanation.
2. Generated INI is not considered authoritative.
3. BuiltIn / Ares / Phobos field registry is advisory, not a hard gate.
4. User must explicitly decide whether to copy/use generated output.
5. No automatic file write in early phases.
6. No API key stored in repository.
7. Request context must be visible or explainable to the user.
8. Model errors must not crash the IDE.
```

---

## 8. Architecture Components

Suggested internal IDE components:

```text
RA2IniEditor.IDE/AI/
RA2IniEditor.IDE/AI/Context/
RA2IniEditor.IDE/AI/Prompts/
RA2IniEditor.IDE/AI/Clients/
RA2IniEditor.IDE/AI/ViewModels/
RA2IniEditor.IDE/Views/AI/
```

Suggested internal interfaces:

```csharp
internal interface IRa2AiClient
{
    Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken);
}

internal interface IRa2AiContextProvider
{
    Ra2AiContext BuildContext(Ra2AiContextRequest request);
}

internal interface IRa2AiPromptBuilder
{
    Ra2AiRequest BuildRequest(Ra2AiTaskKind taskKind, Ra2AiContext context, string userInstruction);
}
```

Start with a mock client.

DeepSeek adapter comes later.

---

## 9. Phase Plan

### AI-0P: Panel Placement Contract

Already completed or in progress:

```text
Docs/AiAgentPanelPlacementContract.md
```

### AI-0: Assistant Architecture / Safety Contract

Create:

```text
Docs/AiAssistantArchitecture.md
Docs/AiAssistantSafetyContract.md
```

No source code.

### AI-1: Right Tool Well AI Tab + Mock Client

Implement UI shell with mock response.

No DeepSeek.

No file modification.

### AI-2: Field Registry Retrieval / Context Provider

Build local retrieval from current Field Registry and current document context.

No DeepSeek required yet.

### AI-3: Prompt Builder

Stable prompts for:

```text
explain field
find fields
generate unit prototype
generate weapon chain
review snippet
```

### AI-4: DeepSeek Client Adapter

Add real network client behind `IRa2AiClient`.

API key from environment or user setting outside repo.

### AI-5: Draft Output / Copy Workflow

Let user copy generated draft.

Still no auto-apply.

### AI-6: Confirmed Insert Preview

Only after previous phases are stable.

Explicit preview and confirmation required.

---

## 10. Validation Policy

Every implementation phase must run:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Documentation-only phases may run:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 11. Current Recommendation

Start with AI-0:

```text
Create architecture and safety documents for DeepSeek-powered RA2 Modding Assistant.
Do not implement UI or client yet.
Do not connect DeepSeek yet.
```
