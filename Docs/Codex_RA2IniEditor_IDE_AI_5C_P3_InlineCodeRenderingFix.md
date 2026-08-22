# Codex Task: RA2IniEditor.IDE AI-5C-P3 Inline Code Rendering Fix

## 0. Context

Manual smoke after AI-5C-P2 shows that table rendering now works, but inline code spans are still not rendered correctly.

Observed issue:

```text
Markdown inline code such as `TODO_OWNER`, `Allied, Soviet, Yuri`, `Image=LAAV`, `Voxel`
is still shown with raw backtick markers, or otherwise not styled as inline code.
```

User feedback:

```text
目前表格可以渲染了，但是 `...` 没有被渲染出来
```

This task is a focused Markdown inline rendering fix.

Do not change provider behavior.

Do not change PromptBuilder behavior.

Do not change code block copy behavior.

---

## 1. Goal

Add support for Markdown inline code spans in AI Assistant rendered messages.

Required behavior:

```text
1. Inline code spans using single backticks render as inline code.
2. Backtick markers are not displayed.
3. Inline code uses a subtle monospace/code style.
4. Inline code works inside paragraph and list text.
5. Fenced code blocks are unaffected.
```

Examples:

```markdown
使用 `Owner=<TODO_OWNER>` 占位。
使用 `Image=LAAV` 对应素材。
可选阵营：`Allied, Soviet, Yuri`。
```

Should render visually as:

```text
使用 [Owner=<TODO_OWNER>] 占位。
使用 [Image=LAAV] 对应素材。
可选阵营：[Allied, Soviet, Yuri]。
```

where brackets above represent inline-code styling, not actual output.

---

## 2. Hard Boundaries

Do not implement or change:

```text
Apply / Insert
automatic file modification
Field Registry writes
Markdown-to-file conversion
automatic code block insertion
draft validation against Field Registry
DeepSeek adapter behavior
provider switching behavior
PromptBuilder behavior
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
RA2IniEditor.IDE/AI/Ra2AiMarkdownInline.cs, if useful
RA2IniEditor.IDE/AI/Ra2AiMarkdownInlineKind.cs, if useful
RA2IniEditor.Tests/IDE/Ra2AiMarkdownResponseParserTests.cs
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Use actual project conventions.

Do not modify project/solution files.

Do not add Markdig / WebView / HTML renderer unless the user explicitly approves a dependency change.

---

## 4. Inline Code Parsing Rules

Support simple inline code spans:

```markdown
`text`
```

Rules:

```text
1. Single-backtick inline code is recognized in Paragraph, Bullet, Numbered, and Heading text if existing renderer supports it safely.
2. Backticks are removed from visible output.
3. Inline code content is preserved exactly between backticks.
4. Inline code is not parsed inside fenced code blocks.
5. Unterminated inline backtick falls back to raw text safely.
6. Empty inline code span may render as empty or fallback safely.
7. Nested/complex Markdown is not required.
```

Examples to support:

```markdown
`TODO_OWNER`
`Owner=<TODO_OWNER>`
`Allied, Soviet, Yuri`
`Image=LAAV`
```

---

## 5. Rendering Rules

Inline code should render with a subtle code style:

```text
monospace font
slightly tinted background or border, if local style allows
small horizontal padding, if simple
```

If adding background/border is too much, minimum acceptable:

```text
monospace font + no visible backticks
```

Do not create large boxes for inline code.

Do not break line wrapping.

---

## 6. Copy Behavior Must Remain

Preserve AI-5C behavior:

```text
1. Copy assistant message copies original full Markdown response, including backticks.
2. Copy code block copies only fenced code content without fence markers.
3. Inline rendering does not alter copied original message.
4. Copy does not modify editor text.
5. Copy does not mark document dirty.
```

No separate inline-code copy action is required.

---

## 7. AutomationIds

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
AiAssistant.MarkdownTable
```

Allowed addition:

```text
AiAssistant.MarkdownInlineCode
```

Forbidden:

```text
AiAssistant.ApplyButton
AiAssistant.InsertButton
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
```

---

## 8. Tests

### 8.1 Parser / inline tests

Add/update tests for:

```text
1. Parses or renders single inline code span.
2. Parses multiple inline code spans in one paragraph.
3. Inline code content preserves special characters such as <TODO_OWNER> and Image=LAAV.
4. Unterminated backtick falls back safely.
5. Inline code is not parsed inside fenced code block.
6. Existing bold/table/list/code block tests still pass.
```

If inline parsing is implemented only at rendering level, add source/boundary tests that verify the inline rendering helper exists and is used.

### 8.2 UI / boundary tests

Add/update tests for:

```text
1. MarkdownInlineCode AutomationId exists if rendered.
2. Markdown paragraph/list rendering remains.
3. Code block copy button still exists.
4. Message copy button still exists.
5. No Apply / Insert button exists.
6. Provider behavior remains unchanged.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Generate or paste a response containing inline code:
   - `TODO_OWNER`
   - `Owner=<TODO_OWNER>`
   - `Allied, Soviet, Yuri`
   - `Image=LAAV`
3. Confirm inline code renders without visible backticks.
4. Confirm inline code has a subtle code/monospace style.
5. Confirm fenced INI code blocks still render as code cards.
6. Confirm tables still render as tables.
7. Confirm copy full message still copies original Markdown with backticks.
8. Confirm copy code block still copies only INI code.
9. Confirm no editor text changes and no dirty state.
10. Confirm no Apply / Insert button exists.
```

---

## 10. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: AI-5C-P3.
2. Files changed.
3. Inline code parsing/rendering strategy.
4. Copy behavior confirmation.
5. AutomationIds preserved/added.
6. Commands run.
7. Build result.
8. Test result.
9. Package result.
10. Confirmation provider behavior unchanged.
11. Confirmation no Apply/Insert/file modification behavior added.
12. Manual smoke steps or result.
13. Remaining risks.
14. Recommended next phase.
```
