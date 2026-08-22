# RA2IniEditor.IDE v0.5.0 Context Baseline For Codex

Date: 2026-06-12

Workspace used for this pass: `H:\RA2\RA2IniEditor_IDE`

The originally supplied folder name was treated as renamed by the user. The active workspace folder is the target SourceClean Codex baseline for this context pass.

## 1. Purpose And Scope

This document records the current project context for RA2IniEditor IDE v0.5.0 SourceClean CodexBaseline.

This pass is documentation-only:

- No production code was changed.
- No Reference Graph was implemented.
- No Run Profile feature was implemented.
- No asset validation, MIX parsing, or missing-resource scanning was implemented.
- No UI refactor was performed.
- No files were deleted.

The goal is to preserve the current architecture and feature boundaries so that the next phase, especially `v0.5.1 Project Semantic Index + Reference Graph Minimal`, can reuse the correct existing foundations without disturbing stable v0.5.0 behavior.

## 2. Source Material Read During Context Pass

Root-level project context:

- `README.md`
- `AGENTS.md`
- `IDE_ONLY_PACKAGE_NOTE.md`
- `RA2IniEditor.IDE.sln`

Documentation context:

- `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`
- `Docs/Codex_CurrentPhase.md`
- `Docs/FeatureOverview.md`
- `Docs/DeveloperNotes.md`
- `Docs/UserGuide_v0.5.0-preview.md`
- `Docs/ReleaseNotes_v0.5.0-preview.md`
- `Docs/KnownIssues_v0.5.0-preview.md`
- `Docs/SmokeChecklist_v0.5.0-preview.md`
- `Docs/HandoffArchiveIndex.md`
- `Docs/DocumentationMaintenance.md`
- `Docs/RA2IniEditor_IDE_FR_DQ_3H_LightweightHoverTrust_Handoff.md`
- `Docs/RA2IniEditor_FR_DQ_3I_ReleaseIconAndUserDocs_Report.md`
- `Docs/RA2IniEditor_FR_DQ_3J_CompactHoverAndIconPolish_Report.md`
- `Docs/RA2IniEditor_FR_DQ_3K_C_IconPrototypeIntegration_Report.md`
- `Docs/AiAssistantArchitecture.md`
- `Docs/AiAssistantContextProviderContract.md`
- `Docs/AiAssistantConversationContextContract.md`
- `Docs/AiAssistantPromptBuilderContract.md`
- `Docs/AiAssistantDeepSeekAdapterContract.md`
- `Docs/AiAssistantSafetyContract.md`
- `Docs/FieldRegistrySurfacesUiContract.md`
- `Docs/FieldRegistryTrustLevels.md`
- `Docs/RA2IniEditor_IDE_EditorSession_Boundary_Map_v0.4.54.md`
- `Docs/RA2IniEditor_IDE_SourceEditor_Boundary_Map_v0.4.57.md`
- `Docs/RA2IniEditor_IDE_ShellWindow_Responsibility_Map_v0.4.48.md`

Core source directories inspected:

- `RA2IniEditor.Core`
- `RA2IniEditor.Infrastructure`
- `RA2IniEditor.IDE`
- `RA2IniEditor.Tests`
- `RA2IniEditor.UiAutomationTests`

## 3. Current Product Identity

The repository is IDE-only. The active product is a source-first INI IDE for Red Alert 2, Yuri's Revenge, Ares, and Phobos mod files.

The removed legacy table-style editor is not part of the active baseline. The SourceClean package is expected to use:

- `RA2IniEditor.IDE.sln`
- `RA2IniEditor.Core`
- `RA2IniEditor.Infrastructure`
- `RA2IniEditor.IDE`
- `RA2IniEditor.Tests`
- `RA2IniEditor.UiAutomationTests`

The baseline must not restore or depend on:

- `RA2IniEditor.sln`
- root legacy `RA2IniEditor.csproj`
- legacy `MainWindow`
- old table-style key-value editor
- old object workbench
- old Country / Side manager
- old object copy or weapon-chain copy workflows

## 4. Current Capability Overview

The IDE currently provides:

- Source-first INI editing with a text editor workflow.
- Project folder opening for top-level `.ini` files.
- Project Explorer grouping for known RA2/YR/Ares/Phobos section types.
- Section Navigator for current-file section navigation.
- Current-file semantic model for sections, key-values, and limited value references.
- Field Registry lookup with Project > Global > BuiltIn priority.
- BuiltIn v3.2 RA2/YR/Ares/Phobos field definitions.
- Lightweight Hover for known fields and current-file references.
- Quick Peek with richer field, provenance, and trust details.
- Completion for keys and selected value contexts.
- Add Property workflow built on field registry definitions.
- Go-to-definition support for fields and current-document value references.
- Find References support inside the current document.
- Current-file readonly diagnostics.
- Reference diagnostics for missing known reference targets.
- Weapon / Projectile / Warhead chain diagnostics.
- Manual Full Diagnostics that can scan project files and build a transient project reference catalog.
- Save preflight, dirty tracking, backup, rollback, undo, redo, and revert boundaries.
- AI Assistant panel with bounded RA2 context and DeepSeek-compatible client plumbing.
- Release/user documentation for the v0.5.0-preview IDE-only package.
- Optional UI automation smoke tests gated separately from normal unit tests.

## 5. Architecture Layers

### RA2IniEditor.Core

Core owns INI parsing, serialization, validation primitives, and field schema abstractions.

Important areas:

- `Core/IniDocument.cs`
- `Core/IniParser.cs`
- `Core/IniSerializer.cs`
- `Core/IniValidator.cs`
- `Schema/Ra2FieldSchema.cs`

Key responsibilities:

- Parse INI source into sections and entries.
- Preserve enough source structure for diagnostics and source-first workflows.
- Define `Ra2FieldDefinition`, `IRa2FieldDefinitionProvider`, and the composite field-provider behavior.
- Keep core logic independent from WPF, editor state, and infrastructure storage.

### RA2IniEditor.Infrastructure

Infrastructure owns file IO, encoding, atomic writes, and Field Registry loading/apply/rollback/import behavior.

Important areas:

- `FieldRegistry/`
- `FieldRegistry/BuiltIn/`
- `FieldRegistry/LocalFieldRegistryLoader.cs`
- `FieldRegistry/FieldRegistryProvenanceSnapshotBuilder.cs`
- `IO/IniFileStore.cs`
- `IO/AtomicFileWriter.cs`

Key responsibilities:

- Load BuiltIn v3.2 field registry data.
- Load project/global active field-registry packs.
- Build provenance snapshots for Project / Global / BuiltIn definitions.
- Preserve encoding/newline metadata for source-first save.
- Provide atomic writing support.

### RA2IniEditor.IDE

IDE owns WPF shell, editor state, semantic services, diagnostics orchestration, completion, hover, quick peek, save workflow, project/navigation services, and AI Assistant UI/pipeline.

Important areas:

- `Language/`
- `Diagnostics/`
- `Services/`
- `Editing/`
- `FieldAnnotations/`
- `FieldTrust/`
- `AI/`
- `ViewModels/`
- `Views/`

Key responsibilities:

- Maintain current document snapshot/session state.
- Build current-document semantic models.
- Drive source navigation, completion, hover, definition, diagnostics, save, backup, and AI context.
- Keep UI behavior stable under documented Shell and source-editor boundary maps.

### RA2IniEditor.Tests

Tests cover core parser/schema behavior, infrastructure field registry behavior, IDE semantic services, diagnostics, save/backup/dirty state, completion, hover, quick peek, and AI plumbing.

Important areas:

- `Core/`
- `Infrastructure/`
- `IDE/`
- `ViewModels/`
- `FieldAnnotations/`

### RA2IniEditor.UiAutomationTests

Optional FlaUI smoke tests cover main-path UI workflows such as opening a project, editing, completion, add property, revert, save, dirty navigation, and field import/apply/rollback.

These tests are intentionally separate from normal unit test expectations and are generally opt-in.

## 6. Key Source Entrypoints

### Project And Source Navigation

- `RA2IniEditor.IDE/Services/ProjectOpenService.cs`
- `RA2IniEditor.IDE/Services/ReadonlyIniContentService.cs`
- `RA2IniEditor.IDE/Services/ReadonlyNavigatorIndexService.cs`
- `RA2IniEditor.IDE/Services/ReadonlyProjectExplorerGroupingService.cs`
- `RA2IniEditor.IDE/Services/ReadonlySourceSectionNavigationResolver.cs`
- `RA2IniEditor.IDE/ViewModels/ShellViewModel.cs`

The project-open path is intentionally top-level `.ini` oriented and read-safe. Large-file handling, section indexing, grouping, and navigation are separated into small services.

### Current Document Semantic Model

- `RA2IniEditor.IDE/Language/Ra2DocumentSnapshot.cs`
- `RA2IniEditor.IDE/Language/Ra2DocumentSemanticModel.cs`
- `RA2IniEditor.IDE/Language/Ra2DocumentSemanticModelBuilder.cs`
- `RA2IniEditor.IDE/Language/Ra2SectionClassifier.cs`
- `RA2IniEditor.IDE/Language/Ra2SectionSymbol.cs`
- `RA2IniEditor.IDE/Language/Ra2KeyValueSymbol.cs`
- `RA2IniEditor.IDE/Language/Ra2ValueReferenceSymbol.cs`

The current model is document-scoped. It is not a persistent project index and not a graph.

### Reference And Definition

- `RA2IniEditor.IDE/Language/Ra2ReferenceFinder.cs`
- `RA2IniEditor.IDE/Language/Ra2DefinitionProvider.cs`
- `RA2IniEditor.IDE/Language/Ra2ReferenceValueDetailService.cs`

These services operate on the current semantic model and the field registry provider/provenance provider. Current value references resolve only to sections visible in the relevant semantic model/catalog.

### Completion And Add Property

- `RA2IniEditor.IDE/Language/Ra2CompletionProvider.cs`
- `RA2IniEditor.IDE/Language/Ra2CompletionRequest.cs`
- `RA2IniEditor.IDE/Editing/Ra2CompletionCommitPlanner.cs`
- `RA2IniEditor.IDE/Editing/Ra2CompletionCommitCoordinator.cs`
- `RA2IniEditor.IDE/Editing/Ra2TextChangeApplier.cs`
- `RA2IniEditor.IDE/Services/Ra2AddPropertyService.cs`
- `RA2IniEditor.IDE/Services/Ra2AddPropertyWindowService.cs`

Completion is source-first and in-memory before save. It does not directly write to disk.

### Hover And Quick Peek

- `RA2IniEditor.IDE/Language/Ra2HoverProvider.cs`
- `RA2IniEditor.IDE/Services/Ra2FieldQuickPeekService.cs`
- `RA2IniEditor.IDE/Services/Ra2FieldDetailsViewModelFactory.cs`
- `RA2IniEditor.IDE/FieldAnnotations/Ra2FieldDisplayResolver.cs`
- `RA2IniEditor.IDE/FieldTrust/Ra2FieldTrustClassifier.cs`

Hover stays lightweight. Quick Peek carries the richer trust/provenance/details surface.

### Diagnostics

- `RA2IniEditor.IDE/Diagnostics/CurrentFileReadonlyDiagnosticService.cs`
- `RA2IniEditor.IDE/Diagnostics/ManualFullDiagnosticsService.cs`
- `RA2IniEditor.IDE/Diagnostics/Ra2FieldDiagnosticService.cs`
- `RA2IniEditor.IDE/Diagnostics/Ra2ReferenceDiagnosticCatalogBuilder.cs`
- `RA2IniEditor.IDE/Diagnostics/Ra2ReferenceDiagnosticService.cs`
- `RA2IniEditor.IDE/Diagnostics/Ra2ChainDiagnosticService.cs`

Diagnostics are read-only analysis services. Current-file diagnostics and manual full diagnostics share the same field/reference/chain diagnostic foundations.

### Field Registry Runtime

- `RA2IniEditor.IDE/Services/FieldRegistryRuntimeService.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/BuiltInFieldRegistryPackLoader.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/FieldRegistryProvenanceSnapshotBuilder.cs`
- `RA2IniEditor.Core/Schema/Ra2FieldSchema.cs`

Runtime registry composition follows Project > Global > BuiltIn priority and must remain stable unless a field-registry task explicitly authorizes changes.

### Save, Backup, Rollback, Dirty State

- `RA2IniEditor.IDE/Services/Ra2SaveCurrentFileService.cs`
- `RA2IniEditor.IDE/Services/Ra2SaveCurrentFileOrchestrator.cs`
- `RA2IniEditor.IDE/Services/Ra2SaveCurrentFilePlanBuilder.cs`
- `RA2IniEditor.IDE/Services/Ra2BackupPlanBuilder.cs`
- `RA2IniEditor.IDE/Services/Ra2BackupService.cs`
- `RA2IniEditor.IDE/Services/Ra2TextFirstFileWriter.cs`
- `RA2IniEditor.IDE/Services/Ra2SaveRollbackService.cs`
- `RA2IniEditor.IDE/Services/Ra2DirtyStateService.cs`
- `RA2IniEditor.IDE/Services/Ra2EditableDocumentSessionService.cs`

Save remains text-first and guarded by preflight/backup/rollback boundaries.

### AI Assistant

- `RA2IniEditor.IDE/AI/Ra2AiAssistantPipeline.cs`
- `RA2IniEditor.IDE/AI/Ra2AiPromptBuilder.cs`
- `RA2IniEditor.IDE/AI/Ra2CurrentDocumentAiContextProvider.cs`
- `RA2IniEditor.IDE/AI/Ra2FieldRegistryAiEvidenceProvider.cs`
- `RA2IniEditor.IDE/AI/Ra2CurrentFileAiDiagnosticSummaryProvider.cs`
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClient.cs`
- `RA2IniEditor.IDE/AI/DeepSeekRa2AiClientFactory.cs`
- `RA2IniEditor.IDE/AI/FakeRa2AiClient.cs`

The AI Assistant is advisory. It does not automatically edit files, write Field Registry data, run tools, or apply patches.

## 7. Current INI Semantic Capabilities

The current semantic layer can:

- Parse the current source document into sections and key-value entries.
- Track section header spans, body spans, key spans, value spans, line numbers, inline comments, and selected preceding comments.
- Classify sections using registry sections, known list sections, explicit known RA2 section families, and limited reference-based inference.
- Identify known/unknown keys through the active field provider.
- Build value reference symbols for a limited set of RA2 weapon-chain relationships.
- Resolve caret context for fields, values, and section headers.
- Feed Hover, Quick Peek, Definition, Reference Finder, Completion, and Diagnostics.

The model is intentionally current-document first. It is a good basis for a future project semantic index, but it is not that index yet.

## 8. Important Types And Services

### Ra2DocumentSemanticModelBuilder

Builds a `Ra2DocumentSemanticModel` from a `Ra2DocumentSnapshot` and `IRa2FieldDefinitionProvider`.

Current reference extraction is intentionally narrow:

- Techno/object weapon keys:
  - `Primary`
  - `Secondary`
  - `ElitePrimary`
  - `EliteSecondary`
  - `DeathWeapon`
  - `OpenToppedWeapon`
  - `Weapon1` through `Weapon10`
- Weapon `Projectile`
- Weapon `Warhead`

Neutral values such as empty values, `none`, `<none>`, `null`, booleans, and numeric values are not treated as target references.

### Ra2DocumentSemanticModel

Holds:

- `Snapshot`
- `Classification`
- `Sections`
- `KeyValues`
- `References`

Provides current-document lookup helpers such as section-at-offset, key-value-at-offset, and section-by-name lookup.

### Ra2SectionSymbol

Represents a section with name, kind, header line/span, body span, inline comment, preceding comment, and display note.

### Ra2KeyValueSymbol

Represents a key-value line with section context, key, value, raw value, comment, spans, line number, and whether the key is known in the active field provider.

### Ra2ValueReferenceSymbol

Represents a value-side reference from a source key to a target section name/kind. Current reference kinds cover Weapon, Projectile, and Warhead relationships.

### Ra2ReferenceFinder

Finds references in the current document model. It can derive a target from a selected section, selected value, or caret context, then scans existing `Ra2ValueReferenceSymbol` entries.

It does not search other files and does not build a graph.

### Ra2DefinitionProvider

Provides:

- Field definitions for keys using the active field provider and provenance provider.
- Current-document target section definitions for value references.
- Current section definitions for section headers.

It does not resolve definitions through a project-level index.

### Ra2ReferenceDiagnosticCatalogBuilder

Builds a lookup catalog of section names/kinds/locations from either the current document or a supplied set of documents.

The multi-document mode is currently used by manual full diagnostics as a transient catalog. It is not persisted and is not a project semantic index.

### Ra2ReferenceDiagnosticService

Emits missing-target reference diagnostics for reference symbols that cannot be resolved in the supplied catalog.

Main boundary:

- Only already-modeled references are checked.
- Complex, neutral, allowed literal, and unknown reference targets are filtered out.
- Scope label can be current file or current project depending on caller.

### Ra2ChainDiagnosticService

Emits specific chain diagnostics for Weapon, Projectile, and Warhead reference gaps:

- `CHAIN_WEAPON_MISSING`
- `CHAIN_PROJECTILE_MISSING`
- `CHAIN_WARHEAD_MISSING`

It shares the same limited reference universe as the semantic model builder.

### ManualFullDiagnosticsService

Runs a user-triggered project-wide readonly diagnostic pass.

It:

- Reads project explorer files.
- Skips backup/build/output-like locations such as `Backups`, `artifacts`, `.vs`, `bin`, and `obj`.
- Skips very large files above the service limit.
- Builds semantic models per readable document.
- Builds a transient project reference catalog.
- Reuses current-file diagnostic logic with project-scope catalog.

It is the nearest existing foundation for a future project semantic index, but it does not keep an incremental or persistent model.

### Ra2CompletionProvider

Provides:

- Key completion from the active field provider.
- Value completion from current-document reference targets for weapon-chain fields.
- Value completion from field registry and built-in value completion catalogs.

It suppresses completion in comments and section headers, and it avoids noisy numeric-prefix value completion.

### Ra2HoverProvider

Provides lightweight hover content for:

- Known keys through the field display resolver.
- Current-document reference values through the reference value detail service.
- Section headers through section display notes.

Verified fields intentionally avoid extra trust noise in Hover. Riskier registry quality states use compact footnotes.

### Ra2FieldQuickPeekService

Provides richer field details for the caret line. It uses provenance first, then falls back to the active field definition provider.

Quick Peek is the correct surface for more detailed trust/provenance/source information.

### FieldRegistryRuntimeService

Owns runtime field-provider composition:

- Project active pack if present.
- Global active pack if present.
- BuiltIn v3.2 fallback pack.

It also builds the active provenance snapshot. Provider priority and weak-definition enrichment are important compatibility behavior and should not be changed casually.

## 9. Current Reference And Diagnostics Boundaries

Current reference support is deliberately minimal and chain-focused.

Implemented:

- Current-document references for weapon-like keys on object/Techno sections.
- Current-document references from Weapon to Projectile.
- Current-document references from Weapon to Warhead.
- Current-file reference missing diagnostics.
- Current-file weapon/projectile/warhead chain diagnostics.
- Manual full diagnostics using a transient project-wide section catalog.

Not implemented:

- Persistent project semantic index.
- Reference Graph data model.
- Cross-file Go to Definition in normal editing.
- Cross-file Find References in normal editing.
- Incremental project update tracking.
- Full dependency graph traversal.
- Generalized references for every RA2/Ares/Phobos key type.
- Asset/resource existence validation.
- MIX parsing.
- Missing SHP/VXL/HVA/PCX/PAL/sound resource scanning.
- Run Profile or game launch orchestration.

## 10. Current Field Registry / BuiltIn v3.2 Boundaries

The Field Registry is an advisory metadata layer used by Hover, Quick Peek, Completion, Add Property, Diagnostics, and AI evidence.

Important current behavior:

- Provider priority is Project > Global > BuiltIn.
- BuiltIn v3.2 is the stable fallback field library.
- Provenance matters for display and trust.
- Hover is intentionally lightweight.
- Quick Peek displays more detailed trust/provenance.
- Inferred or fallback metadata should not pollute Issues unless a diagnostic is explicitly designed for that risk.
- Registry quality and source evidence are metadata, not a save-blocking gate.

Do not alter provider priority, BuiltIn data, trust classification, Hover behavior, Quick Peek behavior, or diagnostic severity without an explicit Field Registry task contract.

## 11. Save Preflight / Backup / Rollback Boundaries

The save path is text-first:

- Edits occur in an editable document session.
- Completion and add-property changes are in-memory before save.
- Save planning preserves encoding/newline metadata.
- Backup is created before writing when possible.
- Write failure can trigger rollback from backup.
- Dirty state remains guarded after failure.

This area is high-risk because it protects user source files. It should not be touched during v0.5.1 semantic-index work unless a bug in the save path itself is being fixed.

## 12. AI Assistant Boundaries

The AI Assistant panel currently provides bounded RA2 modding assistance with current document context, field evidence, and diagnostics summaries.

Current constraints:

- The assistant is advisory, not an autonomous code or INI editing agent.
- It does not automatically apply edits.
- It does not write to source files.
- It does not write Field Registry data.
- It does not run diagnostics/tools by itself.
- It can use Fake or DeepSeek-compatible clients depending on environment configuration.

Future semantic-index work may provide richer readonly context to the AI Assistant, but should not change its write/apply permissions without a separate contract.

## 13. Key Test Entrypoints

Normal validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Debug --no-build
```

Important test areas:

- `RA2IniEditor.Tests/Core`
- `RA2IniEditor.Tests/Infrastructure`
- `RA2IniEditor.Tests/IDE`
- `RA2IniEditor.Tests/ViewModels`
- `RA2IniEditor.Tests/FieldAnnotations`
- `RA2IniEditor.UiAutomationTests`

Representative coverage includes:

- INI parser/serializer/validator behavior.
- Field schema and composite provider behavior.
- BuiltIn field registry loading.
- Field registry apply/rollback/import/provenance behavior.
- Current document semantic model.
- Section classifier.
- Reference finder and definition provider.
- Reference diagnostics and chain diagnostics.
- Field diagnostics and trust classification.
- Completion provider, value completion, commit coordinator, and dropdown behavior.
- Hover and Quick Peek.
- Source editor/session boundaries.
- Save preflight, backup, rollback, dirty state, undo, redo, and revert.
- AI Assistant context/prompt/pipeline/client behavior.
- Optional UI automation smoke tests for main IDE workflows.

## 14. Existing Foundations For v0.5.1 Project Semantic Index + Reference Graph Minimal

Reusable foundations:

- `Ra2DocumentSemanticModelBuilder`
  - Reuse for per-document parse/classify/reference extraction.
- `Ra2DocumentSemanticModel`
  - Reuse as the per-file semantic unit.
- `Ra2SectionSymbol`, `Ra2KeyValueSymbol`, `Ra2ValueReferenceSymbol`
  - Reuse as the minimal symbol vocabulary.
- `Ra2ReferenceDiagnosticCatalogBuilder`
  - Reuse or evolve into a project catalog builder.
- `ManualFullDiagnosticsService`
  - Reuse its file enumeration, skip policy, text loading approach, and project-scope diagnostic pattern.
- `Ra2ReferenceDiagnosticService`
  - Reuse diagnostic logic once a project catalog/index is supplied.
- `Ra2ChainDiagnosticService`
  - Reuse weapon/projectile/warhead chain checks against a project-scope catalog.
- `Ra2ReferenceFinder`
  - Reuse target extraction behavior, but do not stretch it into cross-file behavior without a clear adapter/index contract.
- `Ra2DefinitionProvider`
  - Reuse field-definition and current-document behavior, then add project-index backed lookup through a minimal interface if approved.
- `FieldRegistryRuntimeService`
  - Reuse active provider/provenance composition as semantic-index input.
- Existing tests around semantic model, reference diagnostics, manual diagnostics, definition provider, and completion.

Recommended minimal v0.5.1 direction:

1. Introduce a readonly project semantic index contract.
2. Populate it from existing document semantic models.
3. Keep the initial graph minimal: section declarations and value-reference edges only for currently supported Weapon/Projectile/Warhead chains.
4. Keep current-document behavior unchanged unless the user explicitly invokes project-scope commands.
5. Make cross-file definition/reference behavior opt-in or clearly scoped.
6. Add tests for index construction, duplicate section handling, cross-file reference resolution, and stale-file refresh policy.

## 15. Risks

Main risks for the next phase:

- Accidentally turning Manual Full Diagnostics into a hidden persistent background index without lifecycle design.
- Expanding reference extraction too broadly and creating noisy diagnostics.
- Breaking current-file behavior while adding project-scope behavior.
- Changing provider priority or provenance display while trying to improve semantic lookup.
- Making Hover too heavy instead of keeping details in Quick Peek.
- Touching Shell layout or source editor UI while implementing backend semantic services.
- Reading output folders or backup folders as source files.
- Treating asset/resource references as normal section references.
- Adding cross-file definition behavior without duplicate-section and source-file precedence rules.
- Introducing file watchers or incremental updates before index invalidation rules are defined.

## 16. Content Not Recommended For Immediate Next Work

Do not prioritize these in v0.5.1 minimal semantic-index work:

- ShellWindow redesign or docking/layout changes.
- Project Explorer UI redesign.
- Section Navigator UI redesign.
- Hover visual redesign.
- Quick Peek UI redesign.
- Completion commit behavior changes.
- Save preflight behavior changes.
- Backup/rollback changes.
- BuiltIn v3.2 data-quality editing.
- Field Registry provider priority changes.
- AI Assistant write/apply automation.
- MIX archive parsing.
- SHP/VXL/HVA/PCX/PAL/sound resource validation.
- Full Run Profile/game launch feature.
- Global project asset validation.
- Large generalized RA2 reference graph covering every possible field.

## 17. Recommended Next Phase

Recommended next phase:

```text
v0.5.1 Project Semantic Index + Reference Graph Minimal
```

Suggested contract-first scope:

- Goal: provide a readonly project semantic index and minimal reference graph for already-supported section/value references.
- Non-goal: no asset validation, no MIX parsing, no Run Profile, no broad UI redesign, no BuiltIn field-library rewrite.
- Initial supported edge kinds: Weapon, Projectile, Warhead.
- Initial consumers: manual full diagnostics and optional cross-file definition/reference commands.
- Required design decisions before implementation:
  - Index lifetime and invalidation.
  - How open unsaved current document text overrides disk text.
  - Duplicate section handling.
  - File precedence/display rules.
  - Whether normal current-file diagnostics should remain current-file by default.
  - Public contract shape for readonly query APIs.

Suggested implementation stages:

1. Contract-only design for project semantic index and minimal graph DTOs.
2. Tests for index construction from multiple `Ra2DocumentSemanticModel` instances.
3. Minimal index builder reusing `Ra2DocumentSemanticModelBuilder`.
4. Adapt manual full diagnostics to consume the index only if this does not change user-visible diagnostics unexpectedly.
5. Add optional project-scope reference/definition services behind explicit commands.
6. Update docs and smoke checklist after behavior is confirmed.

## 18. Verification Commands For This Baseline

The requested validation commands for this context pass are:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Debug --no-build
```

This document records the intended context. Command results are reported in the Codex delivery summary for the pass.
