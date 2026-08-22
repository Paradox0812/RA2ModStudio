# Codex Task: RA2IniEditor.IDE AI-5C-P2 Markdown Table / Inline Formatting Rendering Fix

## 0. Context

Manual smoke after AI-5C-P Markdown Rich Rendering shows that the AI response is still not rendered correctly.

Observed from screenshots:

```text
1. Markdown table text is displayed as raw pipe text:
   | ID | 类型 | 说明 |
   |----|------|------|
2. Inline bold syntax such as **原版** is still displayed literally.
3. Bullet list rendering is partially working, but wrapped text still feels plain/raw.
4. The result is readable but does not yet feel like rendered Markdown.
```

This task is a focused Markdown rendering fix.

Do not change provider behavior.

Do not change PromptBuilder behavior unless explicitly needed for tests.

---

## 1. Goal

Improve the AI Assistant Markdown renderer so common DeepSeek Markdown output is rendered more naturally.

Required additions:

```text
1. Pipe table rendering.
2. Basic inline emphasis rendering for **bold**.
3. Better fallback for unsupported Markdown.
4. Preserve existing fenced code block rendering and copy behavior.
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
DeepSeek adapter behavior
PromptBuilder behavior
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn Field Registry JSON
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
RA2IniEditor.IDE/AI/Ra2AiMarkdownBlock.cs
RA2IniEditor.IDE/AI/Ra2AiMarkdownResponseParser.cs
RA2IniEditor.IDE/AI/Ra2AiMarkdownBlockKind.cs, if already present or useful
RA2IniEditor.IDE/AI/Ra2AiMarkdownTable.cs, if useful
RA2IniEditor.Tests/IDE/Ra2AiMarkdownResponseParserTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not modify project/solution files in this phase.

Do not add Markdig / WebView / HTML renderer unless the user explicitly approves a dependency change.

---

## 4. Required Markdown Support

### 4.1 Pipe tables

Support GitHub-style pipe tables:

```markdown
| ID | 类型 | 说明 |
|----|------|------|
| LAAV | 单位 | 轻型防空车 |
| LAAVWeapon | 武器 | 普通武器 |
```

Renderer should show a simple table-like UI:

```text
header row
body rows
wrapped cell text
light borders or row separators
```

Minimum acceptable fallback:

```text
render table as monospaced aligned block, not raw paragraph text
```

Preferred:

```text
Grid / ItemsControl table card
```

Rules:

```text
1. Detect header row + separator row.
2. Parse cells by pipe delimiters.
3. Trim cell whitespace.
4. Preserve cell text.
5. Support 2+ columns.
6. If malformed table, fallback to paragraph.
7. Do not parse tables inside fenced code blocks.
```

### 4.2 Inline bold

Support basic bold syntax:

```markdown
**原版**
```

Render as bold inline text.

Minimum acceptable:

```text
Convert common **text** spans into inline Run with FontWeight=Bold.
```

Rules:

```text
1. Only implement simple non-nested bold.
2. Unsupported/nested cases may fall back to raw text.
3. Do not affect fenced code block content.
```

### 4.3 Lists and wrapping

Existing bullet rendering should remain.

Ensure wrapped list text stays aligned reasonably.

No need to implement full Markdown nesting.

---

## 5. Parser / Block Model

If current block model lacks table support, add a new kind:

```csharp
internal enum Ra2AiMarkdownBlockKind
{
    Paragraph,
    Heading,
    Bullet,
    Numbered,
    Code,
    Table
}
```

Suggested table model:

```csharp
internal sealed class Ra2AiMarkdownTable
{
    public IReadOnlyList<string> Headers { get; init; }
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }
}
```

or store table rows inside `Ra2AiMarkdownBlock` if simpler.

Keep implementation small and deterministic.

---

## 6. UI Rendering Requirements

Render Markdown blocks as:

```text
Heading -> bold/larger TextBlock
Paragraph -> wrapped text with inline bold support
Bullet -> bullet row with wrapped text and inline bold support
Numbered -> numbered row with wrapped text and inline bold support
Code -> existing code block card with copy button
Table -> simple table card
```

Required behavior:

```text
1. All text wraps within the right panel width.
2. Table cells wrap instead of clipping.
3. Fenced code block copy still copies only code content.
4. Message copy still copies original full Markdown.
5. No Apply / Insert button appears.
```

---

## 7. Copy Behavior Must Remain

Preserve AI-5C behavior:

```text
1. Copy assistant message copies the original full Markdown response.
2. Copy code block copies only the code content without fence markers.
3. Copy does not modify editor text.
4. Copy does not mark document dirty.
```

For tables:

```text
No separate table-copy action required in this phase.
```

---

## 8. AutomationIds

Preserve existing:

```text
AiAssistant.ChatHistory
AiAssistant.AssistantMessageList
AiAssistant.LatestAssistantMessage
AiAssistant.AssistantMessageCopyButton
AiAssistant.CodeBlock
AiAssistant.CodeBlockCopyButton
AiAssistant.CodeBlockLanguage
AiAssistant.MarkdownBlock
AiAssistant.MarkdownHeading
AiAssistant.MarkdownParagraph
AiAssistant.MarkdownListItem
```

Allowed additions:

```text
AiAssistant.MarkdownTable
AiAssistant.MarkdownTableHeader
AiAssistant.MarkdownTableRow
AiAssistant.MarkdownTableCell
```

Forbidden:

```text
AiAssistant.ApplyButton
AiAssistant.InsertButton
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
```

---

## 9. Tests

### 9.1 Parser tests

Add/update tests for:

```text
1. Parses a simple pipe table.
2. Parses header and body rows.
3. Trims table cell whitespace.
4. Malformed table falls back safely.
5. Does not parse table inside fenced code block.
6. Existing code block parser tests still pass.
7. Existing heading/paragraph/list tests still pass.
```

### 9.2 Inline formatting tests

Add tests for:

```text
1. Parses or renders simple **bold** span.
2. Leaves unsupported nested/malformed bold safe.
3. Does not apply bold parsing inside fenced code block.
```

If inline formatting is only UI-level and hard to unit test, add source/boundary tests that verify the inline rendering helper exists and is used.

### 9.3 UI / boundary tests

Add/update tests for:

```text
1. Markdown table AutomationId exists if rendered.
2. Code block copy button still exists.
3. Message copy button still exists.
4. No Apply / Insert button exists.
5. Provider behavior remains unchanged.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 10. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Generate or paste a response containing a pipe table, bold text, bullet list, and ini code block.
3. Confirm table renders as table/card rather than raw pipe text.
4. Confirm **bold** renders as bold text.
5. Confirm list wrapping is acceptable.
6. Confirm ini code block renders as code card.
7. Confirm copy full message still copies original Markdown.
8. Confirm copy code block still copies only INI code.
9. Confirm no editor text changes and no dirty state.
10. Confirm no Apply / Insert button exists.
```

---

## 11. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 12. Final Report Format

Report:

```text
1. Phase completed: AI-5C-P2.
2. Files changed.
3. Table rendering strategy.
4. Inline bold rendering strategy.
5. Copy behavior confirmation.
6. AutomationIds preserved/added.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation provider behavior unchanged.
12. Confirmation no Apply/Insert/file modification behavior added.
13. Manual smoke steps or result.
14. Remaining risks.
15. Recommended next phase.
```
