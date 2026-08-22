# RA2IniEditor.IDE FR-DQ-3H Lightweight Hover Trust Handoff

## 1. Phase completed

`FR-DQ-3H-LightweightHoverTrustAndDiagnosticPolish` has been completed on top of `FR-DQ-3G-P0P1P2-UnifiedBuiltInMerge`.

The goal was to use the field quality metadata from the 3G BuiltIn merge without making Hover noisy.

## 2. Design outcome

The implementation follows a layered information model:

```text
Hover: lightweight field description + example + only risk footnotes
Quick Peek / Add Property details: full trust summary
Issues: only actionable risk diagnostics
Field Registry data: original quality tag retained in Ra2FieldDefinition
```

Verified fields do not show extra Hover badges. Inferred / guardrail / obsolete / non-existent fields show only one short footnote when appropriate.

## 3. Modified files

Core / Infrastructure:

- `RA2IniEditor.Core/Schema/Ra2FieldSchema.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/FieldRegistryFieldDto.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/LocalFieldRegistryLoader.cs`

IDE:

- `RA2IniEditor.IDE/FieldTrust/Ra2FieldTrustLevel.cs`
- `RA2IniEditor.IDE/FieldTrust/Ra2FieldTrustInfo.cs`
- `RA2IniEditor.IDE/FieldTrust/Ra2FieldTrustClassifier.cs`
- `RA2IniEditor.IDE/Language/Ra2HoverProvider.cs`
- `RA2IniEditor.IDE/ViewModels/FieldDetails/Ra2FieldDetailsViewModel.cs`
- `RA2IniEditor.IDE/Views/FieldQuickPeek/Ra2FieldQuickPeekWindow.xaml`
- `RA2IniEditor.IDE/Views/FieldBrowser/Ra2AddPropertyWindow.xaml`
- `RA2IniEditor.IDE/Diagnostics/Ra2FieldDiagnosticService.cs`

Tests:

- `RA2IniEditor.Tests/IDE/Ra2FieldTrustClassifierTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2HoverProviderTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldDiagnosticServiceTests.cs`
- `RA2IniEditor.Tests/Infrastructure/LocalFieldRegistryLoaderTests.cs`

Docs:

- `Docs/RA2IniEditor_IDE_FR_DQ_3H_LightweightHoverTrust_Handoff.md`
- `Docs/Codex_CurrentPhase.md`
- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `AGENTS.md`

## 4. Key implementation details

### 4.1 Registry quality is preserved

`Ra2FieldDefinition` now has:

```csharp
public string? RegistryQuality { get; }
```

`LocalFieldRegistryLoader` reads the JSON `quality` field and preserves it on each loaded definition. This is display / diagnostic metadata only; it does not affect saving, value parsing, or field lookup priority.

### 4.2 Trust classifier

`Ra2FieldTrustClassifier` maps raw `quality` tags into stable levels:

```text
Verified
VerifiedGuardrail
Inferred
ManualCurated
AutoExtracted
Obsolete
NonExistent
PseudoField
Unknown
```

It returns `Ra2FieldTrustInfo`, which contains:

```text
ShortLabel
HoverFootnote
DetailText
ShouldShowInHover
ShouldShowWarningStyle
```

### 4.3 Hover remains lightweight

`Ra2HoverProvider` only appends `HoverFootnote` when `ShouldShowInHover=true`.

Examples:

```text
Verified field: no extra footnote
Inferred field: 可信度：推断说明，仅供参考。
Guardrail field: 诊断：疑似上下文错误或保护性字段。
Obsolete field: 状态：废弃字段，不建议继续使用。
Non-existent field: 状态：未实现 / 不建议使用。
```

### 4.4 Quick Peek shows detailed trust information

`Ra2FieldDetailsViewModel` exposes:

```text
TrustDisplay
TrustDetail
HasTrustDetail
```

Quick Peek shows trust as an extra badge and a detailed `可信度` section. Add Property details show the short trust label.

### 4.5 Diagnostics are risk-focused

`Ra2FieldDiagnosticService` now has additional issue codes:

```text
FIELD_WRONG_CONTEXT
FIELD_OBSOLETE_KEY
FIELD_NON_EXISTENT_KEY
FIELD_PSEUDO_FIELD
FIELD_INFERRED_FALLBACK
```

The implementation only emits Issues for actionable risk categories:

- Global-only field used outside Global -> `FIELD_WRONG_CONTEXT`
- guardrail field -> `FIELD_WRONG_CONTEXT`
- obsolete field -> `FIELD_OBSOLETE_KEY`
- non-existent field -> `FIELD_NON_EXISTENT_KEY`
- pseudo field -> `FIELD_PSEUDO_FIELD`

Inferred / auto-extracted fields are intentionally not emitted into Issues by default, to avoid polluting the Issues panel. They are visible in Hover / Quick Peek instead.

## 5. Validation

Static validation completed:

```text
BuiltIn v3.2 JSON parse: passed
Runtime BuiltIn field count: 4878
needs-more-evidence rows: 0
schema.type=Text rows: 0
bin/obj/TestResults/artifacts in source tree: not found
```

`dotnet restore/build/test` was not run because the patch environment has no dotnet CLI.

Recommended local validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```

## 6. Notes for next phase

Recommended next phase:

```text
FR-DQ-3I-FieldRegistryQualityGateAndManagerStats
```

Suggested work:

1. Add BuiltIn quality gate tests for illegal schema, placeholder descriptions, duplicate key+appliesTo, missing source metadata.
2. Add Field Registry Manager stats for verified / inferred / guardrail / obsolete / non-existent counts.
3. Add small real INI regression samples for Known / WrongContext / Obsolete / NonExistent diagnostics.
4. Keep Hover lightweight; do not add full source lists or raw quality strings to Hover.
