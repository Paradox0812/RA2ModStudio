# AI Assistant Context Provider / Field Registry Retrieval Contract

## 1. Scope and Baseline

AI-2A defines the contract for future bounded AI context collection and local Field Registry evidence retrieval.

Baseline:

- AI-1C completed the Right Tool Well AI chat skeleton with deterministic local mock responses.
- The AI Assistant is a DeepSeek-powered RA2 Modding Assistant, not a Codex-like file editing agent.
- Current AI UI has no DeepSeek client, network access, API key, ContextProvider, PromptBuilder, Apply, Insert, file modification, Field Registry write, or whole-project context behavior.
- Section Tree remains the default Right Tool Well page.

AI-2A is documentation and inspection only. It does not authorize source changes.

## 2. Existing Source Inspection

### Current document snapshot

Inspected files:

- `RA2IniEditor.IDE/Diagnostics/CurrentSourceSnapshot.cs`
- `RA2IniEditor.IDE/Diagnostics/SourceEditorState.cs`
- `RA2IniEditor.IDE/ViewModels/ShellViewModel.cs`
- `RA2IniEditor.IDE/Views/ShellWindow.xaml.cs`
- `RA2IniEditor.IDE/Language/Ra2DocumentSnapshot.cs`

Relevant existing model:

- `CurrentSourceSnapshot` stores `ProjectRootPath`, `FilePath`, `FileName`, `Text`, `Version`, and `SourceEditorState`.
- `CanRunDiagnostics` is true only when `State == SourceEditorState.Loaded`.
- `Ra2DocumentSnapshot` stores `FilePath`, `Text`, and `Version`.
- Shell language helpers currently build `Ra2DocumentSnapshot` from `CurrentSnapshot.FilePath`, `SourceTextEditor.Document.Text`, and `CurrentSnapshot.Version`.

AI context must not pass full `Text` through to prompts. It may use it locally to extract bounded nearby lines and selected text.

### Semantic model, section, key, and value symbols

Inspected files:

- `RA2IniEditor.IDE/Language/Ra2DocumentSemanticModel.cs`
- `RA2IniEditor.IDE/Language/Ra2DocumentSemanticModelBuilder.cs`
- `RA2IniEditor.IDE/Language/Ra2SectionSymbol.cs`
- `RA2IniEditor.IDE/Language/Ra2KeyValueSymbol.cs`
- `RA2IniEditor.IDE/Language/Ra2TextSpan.cs`

Relevant existing model:

- `Ra2DocumentSemanticModel` exposes `Snapshot`, `Classification`, `Sections`, `KeyValues`, and `References`.
- `FindSectionAtOffset`, `FindKeyValueAtOffset`, and `FindSectionByName` already support caret-local lookup.
- `Ra2SectionSymbol` exposes section `Name`, `Kind`, `HeaderLineNumber`, `HeaderSpan`, `BodySpan`, `InlineComment`, `PrecedingComment`, and `DisplayNote`.
- `Ra2KeyValueSymbol` exposes `SectionName`, `SectionKind`, `Key`, `Value`, `RawValue`, `InlineComment`, `LineNumber`, `LineSpan`, `KeySpan`, `ValueSpan`, and `IsKnownKey`.
- `Ra2TextSpan` exposes `Start`, `Length`, `End`, and `Contains`.

Future AI context should reuse these symbols instead of reparsing INI text independently.

### Caret context

Inspected files:

- `RA2IniEditor.IDE/Language/Ra2CaretContext.cs`
- `RA2IniEditor.IDE/Language/Ra2CaretContextService.cs`
- `RA2IniEditor.IDE/Language/IRa2CaretContextService.cs`

Relevant existing model:

- `Ra2CaretContext` exposes `Offset`, `Region`, `Section`, `KeyValue`, `TokenText`, and `TokenSpan`.
- `Ra2CaretContextService.GetContext` classifies caret location as section header, key, value, comment, whitespace, or unknown.
- The service clamps offsets to the current snapshot length.

Future AI context should use caret context to decide current Section, current Key/Value, and whether selected/caret content is relevant.

### Diagnostics

Inspected files:

- `RA2IniEditor.IDE/Diagnostics/CurrentFileReadonlyDiagnosticService.cs`
- `RA2IniEditor.IDE/Diagnostics/Ra2FieldDiagnosticService.cs`
- `RA2IniEditor.IDE/ViewModels/IdeDiagnosticIssueViewModel.cs`
- `RA2IniEditor.IDE/ViewModels/IssuesViewModel.cs`
- `RA2IniEditor.IDE/ViewModels/ShellViewModel.cs`

Relevant existing model:

- `CurrentFileReadonlyDiagnosticService.Analyze` accepts a `CurrentSourceSnapshot` and optional `IRa2FieldDefinitionProvider`.
- It runs parser/validator issues and, when a provider exists, field diagnostics, reference diagnostics, and chain diagnostics.
- `IdeDiagnosticIssueViewModel` exposes `Code`, `SourceKind`, `Severity`, `Message`, `FilePath`, `LineNumber`, `ColumnNumber`, `SectionId`, `Key`, and `Version`.
- `IssuesViewModel.Items` contains currently filtered issues; its internal `_allItems` is private.

Future AI context should summarize diagnostics, not dump the entire issues list blindly. It should prefer current file, current Section, current Key, selected line range, and current snapshot version.

### Field Registry provider and precedence

Inspected files:

- `RA2IniEditor.IDE/Services/FieldRegistryRuntimeService.cs`
- `RA2IniEditor.Core/Schema/Ra2FieldSchema.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/LocalRa2FieldDefinitionProvider.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/Provenance/IFieldRegistryProvenanceProvider.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/Provenance/FieldRegistryProvenanceSnapshot.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/Provenance/FieldRegistryProvenanceEntry.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/Provenance/FieldRegistryProvenanceLookupResult.cs`

Relevant existing model:

- `FieldRegistryRuntimeService.CurrentProvider` is the active readonly `IRa2FieldDefinitionProvider`.
- `FieldRegistryRuntimeService.CurrentProvenanceProvider` is the active provenance lookup provider.
- `Reload(projectRootPath)` builds providers in this order: Project, Global, BuiltIn.
- `CompositeRa2FieldDefinitionProvider` queries providers in the passed priority order and enriches weak local definitions from lower-priority built-in metadata.
- `LocalRa2FieldDefinitionProvider` supports section-specific lookup, abstract lookup kinds, `Global`, and `Unknown`.
- `Ra2FieldDefinition` exposes `Key`, `AppliesTo`, `EditorKind`, `SourceKind`, `Description`, `ValueMetadata`, `DisplayName`, `Aliases`, and `Examples`.
- `FieldRegistryProvenanceLookupResult` exposes `Found`, `Scope`, `SourceName`, `SourcePath`, and `Definition`.

Future AI retrieval must preserve Project > Global > BuiltIn priority and treat all evidence as advisory.

### Completion field lookup

Inspected files:

- `RA2IniEditor.IDE/Language/Ra2CompletionProvider.cs`
- `RA2IniEditor.IDE/Language/Ra2CompletionRequest.cs`
- `RA2IniEditor.IDE/Language/FieldRegistryRa2FieldValueCompletionCatalog.cs`
- `RA2IniEditor.IDE/Language/CompositeRa2FieldValueCompletionCatalog.cs`

Relevant existing behavior:

- Key completion uses `request.FieldProvider.GetFields(sectionKind)` and filters by prefix.
- Value completion uses `request.FieldProvider.TryGetField(sectionKind, key, out definition)` and then reads `definition.ValueMetadata` through value completion catalogs.
- Reference value completion uses current document sections and does not query the Field Registry.

AI retrieval should align with this lookup behavior, but it must not change completion candidates or completion commit behavior.

### Hover field lookup

Inspected files:

- `RA2IniEditor.IDE/Language/Ra2HoverProvider.cs`
- `RA2IniEditor.IDE/FieldAnnotations/Ra2FieldDisplayResolver.cs`
- `RA2IniEditor.IDE/FieldAnnotations/Ra2FieldDisplayInfo.cs`

Relevant existing behavior:

- Key hover uses `Ra2FieldDisplayResolver.Resolve(sectionKind, key)`.
- It uses provenance via `TryGetFieldWithProvenance(sectionKind, key)` to display source scope.
- Value hover uses `Ra2ReferenceValueDetailService` for current-document reference detail.
- Section hover uses the current `Ra2SectionSymbol`.

AI field evidence can reuse the same display/provenance concepts, but AI-2 must not change hover data source or formatting.

### Quick Peek field lookup

Inspected files:

- `RA2IniEditor.IDE/Language/FieldQuickPeek/Ra2FieldQuickPeekService.cs`
- `RA2IniEditor.IDE/Language/Ra2ReferenceValueDetailService.cs`

Relevant existing behavior:

- Field Quick Peek resolves caret key/value line through `Ra2CaretContextService`.
- It first queries `IFieldRegistryProvenanceProvider.TryGetFieldWithProvenance`.
- It falls back to `IRa2FieldDefinitionProvider.TryGetField`.
- Reference detail summaries are bounded to the current document and prefer a short set of key/value lines for weapons, projectiles, and warheads.

AI context may reuse the concept of compact reference summaries, but it must not read all project files by default.

## 3. Context Categories

Allowed future context categories:

- Current file display name, not absolute path by default.
- Current Section name and section kind.
- Current Key, Value, and caret region.
- Explicit selected text, only when selected by the user or pasted into the prompt.
- Small nearby line range around the caret.
- Compact diagnostic summaries relevant to current file, current Section, current Key, selected line range, or user prompt.
- Top Field Registry evidence matches.
- Optional current-document reference summary for the reference under caret.

Recommended bounds:

- Nearby text: default 3 lines before and 3 lines after caret line, configurable only by a later contract.
- Selected text: include exact selection only when selection is explicit; cap by character count in implementation.
- Diagnostics: top relevant items only; include code, severity, message, Section, key, and line/column.
- Field evidence: top N only, recommended default N = 5.

## 4. Forbidden Context

Forbidden by default:

- Whole project content.
- Whole repository content.
- All INI files.
- Entire Field Registry.
- Absolute local paths.
- API keys, tokens, passwords, environment variables, credentials, or user-local secrets.
- Hidden user files.
- Clipboard content unless explicitly pasted by the user.
- `.vs`, `bin`, `obj`, `artifacts`, `TestResults`, package output, build output, and generated directories.
- Raw prompt logs, raw response logs, full context payload logs, or raw INI snippets in normal logs.

Context collection must be explicit or user-command triggered. It must not run as background upload on caret movement, diagnostics update, file open, project load, hover, completion, or Section selection.

## 5. Field Registry Retrieval Strategy

Future retrieval is local and bounded.

Inputs:

- Task kind or auto intent from the AI UI.
- User prompt text.
- Current `Ra2CaretContext`.
- Current `Ra2SectionSymbol`.
- Current `Ra2KeyValueSymbol`.
- Explicit selected snippet fields.
- Relevant diagnostic `Key` / `SectionId` values.

Retrieval order:

1. Exact current key match through `CurrentProvider.TryGetField(currentSectionKind, key)`.
2. Provenance lookup through `CurrentProvenanceProvider.TryGetFieldWithProvenance(currentSectionKind, key)`.
3. Alias matches from `CurrentProvider.GetFields(sectionKind)` where aliases match the current key, selected keys, prompt tokens, or diagnostic keys.
4. Prefix/token matches from `GetFields(sectionKind)` for user prompt terms.
5. Section-kind fallback evidence using existing provider behavior: specific section kind, abstract kinds, Global, Unknown.

Output evidence should include only compact summaries:

- Field key.
- Display name when available.
- Applies-to / section kind.
- Editor kind or value kind.
- Source kind and provenance scope/source name.
- Short description.
- Aliases.
- First few allowed values or examples, capped.
- Confidence or reason, such as exact key, alias, prompt token, diagnostic key, or fallback.
- Uncertainty note when evidence is missing, ambiguous, or only fallback.

Rules:

- Retrieval must not call DeepSeek.
- Retrieval must not write Field Registry files.
- Retrieval must not change Project > Global > BuiltIn priority.
- BuiltIn / Ares / Phobos evidence remains advisory.
- Prompt Builder, when implemented later, must include only top relevant evidence, not the whole registry.

## 6. Diagnostic Summary Strategy

Future diagnostic summaries should use existing `IdeDiagnosticIssueViewModel` data.

Recommended filters:

- Same current file version where possible.
- Same Section as caret.
- Same Key as caret.
- Same selected line range.
- Diagnostic keys mentioned in user prompt.
- Highest severity first.

Recommended summary fields:

- `Code`
- `SourceText`
- `SeverityText`
- `Message`
- `LocationText`
- `SectionId`
- `Key`

Do not include absolute `FilePath` by default. Use display file name instead.

Do not rerun diagnostics automatically just to build AI context unless the future implementation contract explicitly allows it. Prefer current `IssuesViewModel.Items` or an explicitly user-triggered diagnostic refresh.

## 7. Context Summary UI

The AI panel should eventually show a compact context summary before any real model request.

Required summary items:

- Current file display name.
- Current Section.
- Current Key / Value.
- Nearby line count.
- Field evidence count.
- Diagnostic count.
- Whether explicit selected text is included.
- Whether the response is still mock/local, or whether a future provider is enabled.

Suggested labels for future localized UI:

- Current file
- Current Section
- Current Key / Value
- Nearby lines
- Field evidence
- Diagnostics
- Selected text

The summary must be display-only. It must not trigger IO, reload, provider calls, or document mutation on its own.

## 8. Future Model Types

Contract-only suggested models:

```csharp
internal sealed class Ra2AiContext
{
    public string? DocumentDisplayName { get; }
    public string? SectionName { get; }
    public string? SectionKind { get; }
    public string? KeyName { get; }
    public string? ValueText { get; }
    public string NearbyText { get; }
    public bool HasExplicitSelection { get; }
    public IReadOnlyList<Ra2AiFieldEvidence> FieldEvidence { get; }
    public IReadOnlyList<Ra2AiDiagnosticSummary> Diagnostics { get; }
}
```

```csharp
internal sealed class Ra2AiFieldEvidence
{
    public string Key { get; }
    public string? DisplayName { get; }
    public string AppliesTo { get; }
    public string Source { get; }
    public string? Description { get; }
    public IReadOnlyList<string> Aliases { get; }
    public IReadOnlyList<string> ExampleValues { get; }
    public string MatchReason { get; }
    public string? UncertaintyNote { get; }
}
```

```csharp
internal sealed class Ra2AiDiagnosticSummary
{
    public string Code { get; }
    public string Severity { get; }
    public string Source { get; }
    public string Message { get; }
    public string Location { get; }
    public string? SectionName { get; }
    public string? Key { get; }
}
```

These are not approved implementation signatures. They are contract anchors for a later AI-2B/AI-2C implementation task.

## 9. Future Interfaces

Contract-only suggested interfaces:

```csharp
internal interface IRa2AiContextProvider
{
    Ra2AiContext BuildContext(Ra2AiContextRequest request);
}
```

```csharp
internal interface IRa2AiFieldEvidenceRetriever
{
    IReadOnlyList<Ra2AiFieldEvidence> Retrieve(Ra2AiFieldEvidenceRequest request);
}
```

```csharp
internal sealed class Ra2AiContextRequest
{
    public string UserPrompt { get; }
    public string? TaskKind { get; }
    public bool IncludeExplicitSelection { get; }
    public int NearbyLineRadius { get; }
    public int MaxFieldEvidenceCount { get; }
    public int MaxDiagnosticCount { get; }
}
```

Future implementation should receive current IDE state from Shell/ViewModel boundaries already available to Shell, but it must not directly own file loading, Field Registry reload, save, apply, rollback, or diagnostics mutation.

## 10. Tests to Add / Update

Future tests should cover:

- Context is bounded by nearby line count.
- Current file display name is included, but absolute path is not included by default.
- Current Section, Key, Value, and caret region are correctly summarized.
- Explicit selected text is included only when explicitly requested.
- Field Registry retrieval returns top relevant matches for current key.
- Alias matches are included when aliases match prompt or diagnostic keys.
- Entire Field Registry is not included.
- Diagnostics are summarized and capped, not dumped wholesale.
- Context collection does not modify editor text.
- Context collection does not mark document dirty.
- Context collection does not write files.
- Context collection does not reload or write Field Registry.
- Context collection does not call DeepSeek, network, or API key code.
- Context summary UI shows counts before any future real provider call.

Suggested test files for future implementation:

- `RA2IniEditor.Tests/IDE/AiAssistantContextProviderTests.cs`
- `RA2IniEditor.Tests/IDE/AiAssistantFieldEvidenceRetrieverTests.cs`
- `RA2IniEditor.Tests/IDE/AiAssistantSafetyBoundaryTests.cs`
- Existing `IdeShellBoundaryTests.cs` and `WpfAutomationHarnessBoundaryTests.cs` only for AutomationId/context-summary UI boundary assertions.

## 11. Risks

- `CurrentSourceSnapshot.Text` and `Ra2DocumentSnapshot.Text` contain the full current file. Future code must avoid passing this wholesale into prompts.
- `CurrentSourceSnapshot` contains absolute paths. Future prompt context should use display file name unless a separate contract approves path inclusion.
- `IssuesViewModel.Items` is filtered, while `_allItems` is private. Future context should define whether it uses visible issues or requires a safe read-only summary API.
- `CurrentProvenanceProvider` is internal to `FieldRegistryRuntimeService`; future AI implementation may need to stay in IDE assembly internals or add a narrow read-only access point.
- Natural-language prompt matching can become fuzzy and expensive. AI-2C should start with exact key, alias, and simple token matching before broader retrieval.
- Field Registry definitions can be incomplete or weak learned definitions. Evidence must include uncertainty notes.
- Any future UI summary must remain display-only and must not trigger reloads, diagnostics runs, or provider calls unexpectedly.

## 12. Recommended Implementation Plan

Recommended split:

1. AI-2B: Current document / caret context provider with mock UI summary
   - Build context from current snapshot, semantic model, caret context, and bounded nearby text.
   - No Field Registry retrieval beyond current key.
   - No DeepSeek, network, PromptBuilder, Apply, Insert, or file writes.

2. AI-2C: Field Registry evidence retriever
   - Retrieve top relevant Field Registry matches locally.
   - Preserve Project > Global > BuiltIn priority.
   - Include provenance/source summaries.
   - Cap evidence count.

3. AI-2D: Diagnostic summary integration
   - Summarize current relevant diagnostics.
   - Cap count and remove absolute paths.
   - Do not rerun diagnostics automatically unless explicitly approved.

4. AI-2E: Context summary shown in AI panel
   - Display categories and counts before any future provider request.
   - Keep summary display-only.
   - Still no DeepSeek, network, API key, Apply, or Insert.

AI-3 should separately contract PromptBuilder. AI-4 should separately contract DeepSeek adapter.

## 13. Acceptance Criteria

AI-2 implementation will be acceptable only when:

- Context collection is explicit/user-command triggered.
- Context is bounded and explainable.
- Current file display name, Section, Key/Value, nearby lines, diagnostic count, and field evidence count are visible in the AI context summary.
- Whole project content is not collected by default.
- Entire Field Registry is not included by default.
- Absolute paths are not included by default.
- Field Registry retrieval is local only.
- Project > Global > BuiltIn priority remains unchanged.
- Field evidence is advisory and marked with uncertainty when needed.
- No DeepSeek, network, API key, PromptBuilder, Apply, Insert, file write, Field Registry write, or shell command path is introduced in AI-2.
- Existing parser, completion, hover, quick peek, diagnostics, save preflight, backup, rollback, and Field Registry semantics remain unchanged.
- Tests verify bounded context, no document mutation, no dirty-state mutation, no file writes, and no provider/network dependency.
