# RA2IniEditor.IDE Developer Notes

These notes describe the current IDE-only package structure. Legacy table-style editor projects and root legacy files are intentionally absent.

## 1. Solution Entry

Use `RA2IniEditor.IDE.sln` for current IDE-only development.

Common commands:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Do not restore legacy root `RA2IniEditor.sln` or `RA2IniEditor.csproj` for IDE-only validation.

## 2. Project Structure

- `RA2IniEditor.Core`: core INI document model, parsing, schema, field definitions, and validation primitives.
- `RA2IniEditor.Infrastructure`: infrastructure services, field registry loading, BuiltIn v3.2 fallback registry assets, import / apply support, and IO helpers.
- `RA2IniEditor.Application`: Core-only `net8.0` document query, diagnostics, semantic edit-preview implementation, and Experimental high-level contracts.
- `RA2IniEditor.IDE`: WPF IDE shell, Source Editor integration, project explorer, navigation, completion, hover, diagnostics, save preflight, and field registry UI.
- `RA2IniEditor.Tests`: unit and boundary tests for Core, Infrastructure, and IDE behavior.
- `RA2IniEditor.UiAutomationTests`: opt-in UI automation tests for selected IDE smoke paths.
- `tools/package-source-clean.ps1`: clean source package generator. Use `-Profile IdeOnly` for the current package.

## 3. Source Editor Direction

The IDE package is source-first. The AvalonEdit-based Source Editor is the primary editing surface.

Development should preserve:

- text buffer and caret stability
- dirty-state tracking
- undo / redo expectations
- completion commit behavior
- hover and reference hover behavior
- diagnostics refresh boundaries
- save preflight behavior

Avoid reintroducing old table-editor assumptions into new IDE flows.

## 4. Field Registry Direction

Field metadata is resolved conservatively through:

1. Project registry
2. Global registry
3. BuiltIn fallback

BuiltIn v3.2 fallback data is packaged through `RA2IniEditor.Infrastructure`. Project and Global metadata should remain distinct so provenance and priority stay understandable.

Field learning / import preview flows should remain reviewable before writing changes. Apply and rollback workflows must be explicit.

BuiltIn data-quality invariants are enforced by loader tests: no uniform inferred templates, `auto-extracted` rows, empty/unrecognized quality labels, or duplicate key + appliesTo identities. Evidence-insufficient rows are quarantined rather than relabeled as verified. `Ra2CompletionProvider` excludes VerifiedGuardrail, Obsolete, NonExistent, and PseudoField definitions only from field-name candidates; lookup, Hover, Quick Peek, Diagnostics, value completion, and commit behavior must not be coupled to that visibility filter.

## 5. Diagnostics And References

Diagnostics should help authors inspect parse issues, validation results, unresolved references, and project understanding gaps.

Reference features such as Reference Value Hover, Quick Peek, and Find References should use available project context without assuming every mod-specific value can be resolved.

Warnings should stay conservative where RA2-family mods commonly use soft references or extension-specific behavior.

### Search / Replace Boundary

- `Ra2ProjectSearchService` consumes the canonical Project Explorer descriptor list; it must not enumerate directories.
- The active file uses in-memory editor text; non-active files use `ReadonlyIniContentService`.
- Regex matching retains an explicit timeout and the 10,000-result safety bound.
- Current-file replacement is preview-first and binds to `DocumentId`, `EditRevision`, and original text.
- Replace All uses one existing programmatic semantic Undo transaction and must not call save or disk-write APIs.
- Project-level/multi-file replacement requires a separate contract.

## 6. Save And Safety

Save behavior should remain guarded by preflight checks where applicable.

When a workflow writes project or registry files, prefer explicit backup and rollback paths. Backup / rollback is a safety layer and should not replace version control.

## 7. IDE-Only Package Rules

The IdeOnly package should include only the current IDE solution, supporting projects, tests, tools, documentation, and required field registry assets.

It must not include or restore:

- legacy root `RA2IniEditor.sln`
- legacy root `RA2IniEditor.csproj`
- legacy MainWindow
- legacy table-style editor source
- legacy object workbench, country manager, side manager, or old object copy workflows

## 8. Testing Notes

Ordinary validation should use the non-UI test project:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
```

UI automation tests live in `RA2IniEditor.UiAutomationTests` and should remain opt-in so normal validation does not unexpectedly launch WPF windows.

## 9. AI Assistant Boundaries

- `DeepSeekRa2AiModelCatalog` owns the typed Flash/Pro display-name and API-id mapping; V4 Flash is the default.
- One immutable configuration snapshot must be shared by Shell status presentation and client construction for a request.
- Production code has no Mock/Fake provider path. Deterministic substitutes belong only in tests.
- Remote endpoints require HTTPS; HTTP is allowed only for loopback verification. Sensitive configuration and provider error bodies must not enter UI diagnostics.
- Prompt preparation uses shared outbound sanitization and deterministic per-section/total budgets. Prompts over 8000 characters are rejected before request-session creation.
- Response construction is factory-controlled, request diagnostics are request-local, and callback consumer failures must propagate unchanged.
- Existing Pipeline overloads are advisory-only. Shell may select `CurrentDocumentEditPreview` only for a ready official endpoint and a successfully captured editable `Ra2AuthoringSnapshot`.
- `preview_ini_edit_plan` handles bounded field operations; `expand_ini_content_template` handles only the reviewed current-document template contract. Both argument shapes stay untrusted until the strict adapter rejects malformed JSON, duplicate/unknown properties, unsupported operations, version/argument errors, and snapshot mismatch.
- `Ra2AiAuthoringCoordinator` owns one active proposal and reuses the A3 Preview/apply transaction. Added errors block Apply; Apply always requires explicit confirmation and never calls Save or disk IO.
- Ordinary DeepSeek Tool Calls are not strict-schema guaranteed. The adapter may normalize only unambiguous presentation drift (trailing comma, inferable outcome, missing display summary, one operation object, numeric scalar value). Template arguments additionally accept the declared named object, numeric scalars, boolean-to-Yes/No conversion, and a numeric version string. Unknown/duplicate properties, null/composite template values, and boolean/null/composite field-operation values remain rejected; never add a general JSON repair path.
- A proposal may include the schema-declared bounded string `message` alongside executable arguments. Validate and discard it; it is presentation-only and must never override template identity, version, arguments, Preview, Apply, or Save authority. Clarification still requires a non-empty message and never executes proposal payload.
- An explicit `needs_clarification` outcome remains non-authoring even if a non-strict provider echoes proposal-shaped fields. Return only the validated bounded message and keep every echoed template field inert. For explicit complete-object requests, prompt the provider to choose conservative visible draft tuning values when only gameplay tuning is omitted; clarification is reserved for unresolved owner, slot, or object identity.
- `Ra2AutomationTemplateService` exposes one source-audited Weapon/Projectile/Warhead relationship skeleton. Internal template definitions compile only into the canonical EditPlan; do not add raw section bodies, gameplay defaults, registration edits, persistence, or a parallel Apply service.
- `Ra2AutomationTemplateService` also exposes one source-audited direct-fire complete profile. It must bind one unique existing compatible owner, validate its complete argument set, and compile into the same canonical EditPlan; only explicit skeleton language may route to the sparse profile.
- `Ra2AgentSkillCatalog` loads only bundled `AgentSkills/*/SKILL.md` packages, validates bounded metadata/content, rejects scripts, and selects exact domain plus optional extension/trust guidance. Skills are prompt knowledge, never capabilities or authorization. External roots, hot reload and executable Skills require a later security/versioning contract.
- `Ra2AiUserMode.Chat` is the safe default and exposes zero authoring tools. `Work` enables only the already admitted current-document routes. Mode state is window/process-local and does not change provider, endpoint, Apply, Undo or Save authority.
- `Ra2AuthoringDiffProjectionBuilder` is an internal presentation projection over the successful Preview change set. Keep it cancellable and bounded (8 MiB / 200k input lines / 20k rows / 2k hunks); it must validate the candidate and never own editor or transaction authority.
- Document/session/Field Registry/chat lifecycle changes invalidate both coordinator authority and the visible proposal card. Custom endpoints remain advisory-only.
- Automatic retry, model fallback, thinking-mode selection, and AI persistence remain out of scope.

## 10. Maintenance Principles

- Keep Core free of WPF and IDE shell dependencies.
- Keep Infrastructure services reusable by IDE code without depending on UI state.
- Keep IDE controllers and views responsible for UI glue, not schema rules.
- Avoid broad refactors during release stabilization.
- Do not modify BuiltIn field definitions unless the task explicitly targets field metadata.

## 11. Automation Architecture Direction

The HLI-1 Query, Diagnostics, and semantic Edit Preview algorithms are real, tested,
and live in the Core-only `net8.0` Application assembly. They are Experimental
in-process APIs, not a stable Agent SDK or wire protocol.

The governing boundary is documented in
`Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md` and the completed HLI-1 ledgers:

- a candidate `RA2IniEditor.Application` (`net8.0`) assembly;
- UI-neutral document query, diagnostics and semantic Preview capabilities;
- IDE-host ownership of active editor capture, Apply, Undo and Save;
- later Gateway/CLI/Job/Asset consumers using the same canonical implementation.

HLI-0B, HLI-1A/1B/1C, HLI-2A/2B/2C, CONTENT-1, AGENT-MODE-1, AGENT-KNOWLEDGE-1 and the first
CONTENT-2A Techno dual-armament and CONTENT-2B Projectile/Warhead slices are implemented through the documented vertical slices. The dual profile
creates two closed direct-fire chains for one existing owner; it does not claim cyclic fire. CONTENT-2B adds separate
Arcing/Homing Projectile profiles and one YR-core Warhead profile, without claiming Phobos trajectories or Ares custom armor. The next safe stage is
CONTENT-2C AI programming tuples, followed by the remaining source-gated content profiles before HOST-1. Do not add
public Apply/Save, CLI/wire DTO, session/permission authority, external/executable Skills, or a second Preview path
without the corresponding contract.

## 12. Documentation Authority

Start at `Docs/README.md`. Product goal, current capabilities, current phase and
roadmap have separate owners. Do not append historical phase narratives to
`Codex_CurrentPhase.md` or `RA2IniEditor_IDE_Full_Codex_Context.md`; preserve detail in
the phase Contract/Stage Ledger and keep the two current-state files concise.
