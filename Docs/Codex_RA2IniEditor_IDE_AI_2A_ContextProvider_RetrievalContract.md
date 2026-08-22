# Codex Task: RA2IniEditor.IDE AI-2A Context Provider / Field Registry Retrieval Contract

## 0. Current Baseline

AI-1C has been completed.

Reported state:

```text
Right Tool Well / AI Assistant chat UI exists.
Section Tree remains default.
AI Assistant can append local user messages and deterministic mock assistant responses.
Copy / Clear only operate on local chat content.
Cancel remains disabled.
No DeepSeek / network / API key / ContextProvider / PromptBuilder / Apply / Insert exists.
No file modification behavior exists.
Tests: 1299 passed.
IdeOnly package: passed, packaged file count 677.
Legacy not restored.
```

Next phase:

```text
AI-2A: Context Provider / Field Registry Retrieval Contract
```

This is a planning / contract phase first.

Do not implement context collection yet.

---

## 1. Goal

Define how the AI Assistant will collect bounded, explainable context and retrieve relevant Field Registry evidence for later AI prompts.

The purpose is to make future DeepSeek responses grounded in the current RA2IniEditor.IDE state without uploading whole projects or entire field registries.

---

## 2. Hard Boundaries

Do not modify source code in this contract phase.

Do not implement:

```text
ContextProvider
Field Registry retrieval index
PromptBuilder
DeepSeek client
network calls
API key configuration
AI apply / insert
file modification
Field Registry writes
whole-project context
auto-send context
```

Do not modify:

```text
XAML
code-behind
ViewModels
tests
scripts
Field Registry JSON
solution / project files
legacy files
```

---

## 3. Documents to Read First

Read:

```text
AGENTS.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
Docs/Codex_CurrentPhase.md
Docs/AiAgentPanelPlacementContract.md
Docs/AiAssistantArchitecture.md
Docs/AiAssistantSafetyContract.md
Docs/AiAssistantRightToolWellImplementationContract.md
```

Then inspect existing language service and Field Registry components only in read-only mode.

---

## 4. Required Source Inspection

Inspect and report existing types / services related to:

```text
current document snapshot
caret context
semantic model
section / key / value symbols
diagnostics summaries
Field Registry provider / composite provider
Field definition model
known field lookup
local/project/global/builtin source precedence
Completion / Hover / Quick Peek field lookup usage
```

Likely areas to inspect:

```text
RA2IniEditor.IDE/Language/
RA2IniEditor.IDE/FieldRegistry-related ViewModels/services
RA2IniEditor.Infrastructure/FieldRegistry-related loaders/providers
RA2IniEditor.Core field schema/model types
existing tests for completion / hover / quick peek / diagnostics / field registry
```

Use actual discovered paths.

Do not edit files.

---

## 5. Context Provider Contract

The future context provider should build a bounded context package.

Suggested future model:

```csharp
internal sealed class Ra2AiContext
{
    public string? DocumentDisplayName { get; }
    public string? SectionName { get; }
    public string? KeyName { get; }
    public string? ValueText { get; }
    public string NearbyText { get; }
    public IReadOnlyList<Ra2AiFieldEvidence> FieldEvidence { get; }
    public IReadOnlyList<Ra2AiDiagnosticSummary> Diagnostics { get; }
}
```

This is a contract suggestion only. Do not implement in AI-2A.

Allowed context categories:

```text
current file display name
current Section
current Key / Value
explicit selected text
small nearby line range
relevant diagnostics
top Field Registry matches
```

Forbidden by default:

```text
whole project
whole repository
all INI files
entire Field Registry
absolute local paths
API keys / environment variables
hidden files
bin / obj / .vs / artifacts / TestResults
```

---

## 6. Field Registry Retrieval Contract

The future retrieval system should be local and bounded.

Input:

```text
task kind / auto intent
user prompt
current key / section
selected INI snippet
diagnostic field references
```

Output:

```text
top N relevant field definitions
source/provenance
value kind
section kind
description/example if available
uncertainty notes
```

Rules:

```text
1. Retrieval is local.
2. Retrieval does not call DeepSeek.
3. Retrieval does not write registry files.
4. BuiltIn / Ares / Phobos evidence is advisory.
5. Project > Global > BuiltIn priority remains unchanged.
6. The prompt should include only top relevant evidence, not the entire registry.
```

---

## 7. Context Summary UI Contract

The AI panel must eventually show what will be included before generation.

The context summary should show:

```text
当前文件
当前 Section
当前 Key / Value
附近行数
字段依据数量
诊断数量
是否包含选中文本
```

For AI-2 implementation, this summary should be visible before any real model call.

---

## 8. Privacy / Safety Rules

Future context collection must:

```text
1. be explicit or user-command triggered
2. be bounded
3. be explainable in UI
4. not run as background upload
5. not include whole project by default
6. not include secrets
7. not include absolute paths unless user explicitly asks or path is necessary
```

---

## 9. Output Required

Create or update:

```text
Docs/AiAssistantContextProviderContract.md
```

Suggested structure:

```markdown
# AI Assistant Context Provider / Field Registry Retrieval Contract

## 1. Scope and Baseline
## 2. Existing Source Inspection
## 3. Context Categories
## 4. Forbidden Context
## 5. Field Registry Retrieval Strategy
## 6. Diagnostic Summary Strategy
## 7. Context Summary UI
## 8. Future Model Types
## 9. Future Interfaces
## 10. Tests to Add / Update
## 11. Risks
## 12. Recommended Implementation Plan
## 13. Acceptance Criteria
```

---

## 10. Future Implementation Split

Recommend a staged implementation after contract approval:

```text
AI-2B: Current document / caret context provider with mock UI summary
AI-2C: Field Registry retrieval for current key and natural-language prompt
AI-2D: Diagnostic summary integration
AI-2E: Context summary shown in AI panel
```

Do not implement all in one large task unless explicitly approved.

---

## 11. Tests to Plan

Plan future tests for:

```text
context is bounded by nearby line count
selected text included only when explicit
field registry retrieval returns top relevant matches
entire registry is not included
absolute paths are not included by default
diagnostics are summarized, not dumped wholesale
context collection does not modify document
context collection does not mark document dirty
context collection does not write files
```

---

## 12. Validation Commands

For this documentation-only task:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

If build output is missing:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 13. Final Report Format

Report:

```text
1. Phase completed: AI-2A.
2. Files changed.
3. Existing services/types inspected.
4. Context categories defined.
5. Field Registry retrieval strategy.
6. Safety/privacy boundaries.
7. Recommended implementation split.
8. Commands run.
9. Test result.
10. Package result.
11. Confirmation no source code changed.
12. Recommended next phase.
```
