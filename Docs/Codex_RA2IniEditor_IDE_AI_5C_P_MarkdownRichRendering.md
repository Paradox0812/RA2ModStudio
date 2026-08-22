# Codex Task: RA2IniEditor.IDE AI-5C-P Markdown Rich Rendering

## 0. Context

AI-5C has stabilized Markdown response parsing and code block copy behavior.

User feedback:

```text
这一版稳定了，但是我更希望能渲染出 markdown。
```

This task is a limited UI rendering improvement for AI Assistant messages.

The goal is to render common Markdown structures in the chat panel, instead of showing raw Markdown text as plain text.

---

## 1. Goal

Implement lightweight Markdown rendering for AI Assistant response messages.

Required result:

```text
1. Assistant messages render Markdown headings as headings.
2. Paragraph text renders as readable wrapped text.
3. Bullet / numbered lists render as list-like rows.
4. Fenced code blocks render as existing code block cards.
5. Code block copy still works.
6. Message copy still copies the original full Markdown response.
7. No Apply / Insert / file modification behavior is added.
```

---

## 2. Recommended Strategy

Use a **small internal Markdown block renderer**, not a full Markdown engine in this phase.

Reason:

```text
1. The AI output format is controlled by PromptBuilder.
2. We only need headings, paragraphs, lists, and fenced code blocks.
3. Avoid adding NuGet/package/project-file changes unless explicitly approved.
4. Keep tests deterministic.
```

Do not add Markdig / WebView / HTML renderer in this phase unless user explicitly approves a dependency change.

---

## 3. Supported Markdown Subset

Render these:

```text
# Heading 1
## Heading 2
### Heading 3
- bullet
* bullet
1. numbered item
paragraph text
```ini fenced code blocks
```

Optional:

```text
**bold** may remain raw text in this phase
inline code may remain raw text in this phase
tables may remain raw text in this phase
```

Unsupported Markdown should fall back to plain wrapped text.

---

## 4. Hard Boundaries

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

## 5. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs
RA2IniEditor.IDE/AI/Ra2AiMarkdownBlock.cs
RA2IniEditor.IDE/AI/Ra2AiMarkdownResponseParser.cs
RA2IniEditor.IDE/AI/Ra2AiMarkdownBlockKind.cs, if useful
RA2IniEditor.Tests/IDE/Ra2AiMarkdownResponseParserTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project conventions.

Do not modify project/solution files in this phase.

---

## 6. Parser / Block Model Requirements

If current parser only separates text vs code blocks, extend it minimally.

Suggested block kinds:

```csharp
internal enum Ra2AiMarkdownBlockKind
{
    Paragraph,
    Heading,
    Bullet,
    Numbered,
    Code
}
```

Suggested block model extension:

```csharp
internal sealed class Ra2AiMarkdownBlock
{
    public Ra2AiMarkdownBlockKind Kind { get; init; }
    public int HeadingLevel { get; init; }
    public string? Language { get; init; }
    public string Text { get; init; } = string.Empty;
}
```

Adjust to existing implementation.

Rules:

```text
1. Preserve original full assistant response for message-level copy.
2. Parse fenced code blocks exactly as AI-5C already does.
3. Parse simple Markdown outside code blocks into block kinds.
4. Unsupported markdown falls back to Paragraph.
5. Unterminated fenced blocks remain safe fallback.
```

---

## 7. UI Rendering Requirements

Assistant message card should render blocks with an ItemsControl or equivalent simple WPF structure.

Expected rendering:

```text
Heading block -> larger/bolder TextBlock
Paragraph -> wrapped TextBlock
Bullet -> bullet marker + wrapped TextBlock
Numbered -> number marker + wrapped TextBlock
Code -> existing code card with language label and copy button
```

Required behavior:

```text
1. Text must wrap within the right panel width.
2. Long code blocks scroll or wrap according to existing code card behavior.
3. The UI must not become a WebView.
4. No large WPF form-style borders.
5. Keep existing chat panel layout.
```

---

## 8. Copy Behavior

Preserve AI-5C behavior:

```text
1. Copy assistant message copies the original full Markdown response.
2. Copy code block copies only the code content without fence markers.
3. Copy does not modify editor text.
4. Copy does not mark document dirty.
```

---

## 9. AutomationIds

Preserve existing:

```text
AiAssistant.ChatHistory
AiAssistant.AssistantMessageList
AiAssistant.LatestAssistantMessage
AiAssistant.AssistantMessageCopyButton
AiAssistant.CodeBlock
AiAssistant.CodeBlockCopyButton
AiAssistant.CodeBlockLanguage
```

Allowed additions:

```text
AiAssistant.MarkdownBlock
AiAssistant.MarkdownHeading
AiAssistant.MarkdownParagraph
AiAssistant.MarkdownListItem
```

Forbidden:

```text
AiAssistant.ApplyButton
AiAssistant.InsertButton
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
```

---

## 10. Tests

### 10.1 Parser tests

Add/update tests for:

```text
1. Parses heading blocks.
2. Parses paragraph blocks.
3. Parses bullet list blocks.
4. Parses numbered list blocks.
5. Keeps fenced ini code block as code block.
6. Does not parse markdown inside code block.
7. Falls back safely for unsupported markdown.
8. Existing code block copy content behavior still passes.
```

### 10.2 UI / boundary tests

Add/update tests for:

```text
1. Markdown heading AutomationId exists if rendered.
2. Markdown paragraph/list block AutomationId exists if rendered.
3. Code block copy button still exists.
4. Message copy button still exists.
5. No Apply / Insert button exists.
6. Provider behavior remains unchanged.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

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

## 12. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Generate or paste a response containing headings, bullets, paragraphs, and ini code block.
3. Confirm headings render visually as headings.
4. Confirm lists render as list-like rows.
5. Confirm paragraphs wrap.
6. Confirm ini code block renders as code card.
7. Confirm copy full message still copies original Markdown.
8. Confirm copy code block still copies only INI code.
9. Confirm no editor text changes and no dirty state.
10. Confirm no Apply / Insert button exists.
```

---

## 13. Final Report Format

Report:

```text
1. Phase completed: AI-5C-P.
2. Files changed.
3. Markdown rendering strategy.
4. Supported Markdown subset.
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
