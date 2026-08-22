# FR-DQ-4 Context Capsule

## 1. Scope

- Project: RA2IniEditor.IDE-only
- Package: FR-DQ-4 PlaceholderRetirementAndTrustCleanup
- Updated: 2026-07-20, FR-DQ-4 package completion
- Contract: `Docs/Codex_RA2IniEditor_IDE_FR_DQ_4_PlaceholderRetirement.md`

## 2. Current goal

Remove misleading placeholder behavior from the BuiltIn Field Registry without deleting valid fields by name or quality alone. Preserve diagnostic facts, promote source-backed fields, quarantine evidence-insufficient fallbacks, and quantify compatibility effects before packaging.

## 3. Current invariants

- IDE-only solution and legacy removal remain authoritative.
- Project > Global > BuiltIn priority and exact -> Unit -> Techno -> Global -> Unknown lookup order are frozen.
- Diagnostic guardrails remain available to lookup, Hover and Diagnostics but do not enter key Completion.
- Completion commit/value candidates, Hover source, Quick Peek, Diagnostics core, parser, save, Shell/XAML and project files are out of scope.
- Disposition identity is case-insensitive key + sorted exact appliesTo + original quality + description digest; JSON index is never an identity.

## 4. Recently completed

| Task | Status | Key change | Verification |
|---|---|---|---|
| FR-DQ-4-0 | Completed | Full clean-source and BuiltIn JSON rollback anchors | Hashes/exclusions recorded in contract |
| FR-DQ-4A | Completed | 2947-row candidate manifest | Identity/hash/disposition static validation passed |
| FR-DQ-4B | Completed | Diagnostic-only trust excluded from key Completion | Real AA/AG and focused tests 22/22 |
| FR-DQ-4C | Completed | 12 proven Global-replaced Techno fallbacks removed | Combined scoped tests 627/627 |
| FR-DQ-4D | Completed | Five bounded Techno ranges removed the remaining 1147 uniform templates | BuiltIn/Completion/trust tests 703/703 |
| FR-DQ-4E | Completed | Five source/key-range cards quarantined all 810 unverified auto-extracted rows | BuiltIn/Completion/trust tests 709/709 |
| FR-DQ-4F | Completed | Quality families normalized/reviewed; five mojibake empty-quality rows repaired | BuiltIn/Completion/trust tests 712/712 |
| FR-DQ-4G | Completed | Real v3.2 surface regression and fixed three-row Unknown Key delta | Surface-focused tests 900/900 |
| FR-DQ-4H | Completed | Full verification, documentation closure and clean package | Restore/build/full tests/package passed |

## 5. Current data shape

- Runtime rows: 2604.
- Uniform inferred templates: 0.
- Auto-extracted rows: 0.
- Empty quality rows: 0; unrecognized runtime trust rows: 0.
- Exact key + appliesTo duplicates: 0.
- Manifest: DiagnosticOnlyKeep 349; SupersededRemove 13; PromoteAndRewrite 65; RetainReviewed 259; Quarantine 2261; PendingManualReview 0.
- `AA/AG / Projectile` remain canonical; `AA/AG / Techno, Weapon` remain diagnostic guardrails and are hidden only from key Completion.

## 6. Key files

- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json`: runtime data under staged review.
- `Docs/FieldRegistryPlaceholderRetirementCandidates_2026-07-20.csv`: original candidate identity plus current disposition.
- `Docs/FieldRegistryPlaceholderRetirementAudit_2026-07-20.md`: metrics, evidence and checkpoints.
- `RA2IniEditor.IDE/Language/Ra2CompletionProvider.cs`: completed narrow diagnostic-only key filter; do not expand it casually.
- `RA2IniEditor.Tests/Infrastructure/BuiltInFieldRegistryPackLoaderTests.cs`: data and context regression gate.

## 7. Open risks and debt

- No repository `.ini` corpus exists; 4G could measure only a fixed representative `+3` Unknown Key delta, not real-project occurrence counts.
- Quarantine will increase real-project Unknown Key results; 4G must measure rather than hide that change by restoring placeholders.
- A pre-existing nullable CS8602 warning in `BuiltInFieldRegistryPackLoaderTests.cs` may appear during builds; it was not introduced by FR-DQ-4.
- Windows PowerShell's default console code page corrupted one non-ASCII rewrite attempt. It was recovered from the 4-0 JSON anchor; future bulk rewrites must use encoding-stable source, explicit UTF-8, unique identity assertions, JSON round-trip validation and atomic replacement.

## 8. Public API / contract notes

- No external public API, DTO shape, serialization or persistence change.
- The Completion filter is private IDE behavior and reuses `Ra2FieldTrustClassifier`.

## 9. Decisions

- Inferred template text indicates insufficient description evidence, not automatic field invalidity.
- Same-key higher-quality presence is only a review signal; SupersededRemove requires canonical context evidence.
- Quarantine removes a row from runtime without declaring the key nonexistent; rollback anchors preserve the original full row.
- Rejected: bulk deletion by quality, description length or same-key presence alone.

## 10. Next task

FR-DQ-4 is closed. Start any later work from the final clean package; recover quarantined identities only with direct evidence and a new bounded contract.

Allowed: BuiltIn v3.2 JSON, candidate manifest, audit/contract ledger, BuiltIn loader tests, and governance docs at the next flush.

Forbidden: provider priority, lookup hierarchy, Completion commit/value candidates, Hover source, Quick Peek, Diagnostics core, parser, save, Shell/XAML, project files and legacy.

Stop if a public schema/lookup change is required, evidence does not support the proposed context, exact duplicates appear, or scoped tests fail outside the current card.

## 11. Verification baseline

- Latest scoped gate: 900/900 surface-focused tests passed.
- Package gate: restore passed; Debug build 0 warnings / 0 errors; full non-UI tests 2274/2274; IdeOnly clean package hygiene passed.
- JSON parse and exact identity duplicate checks passed.
- Runtime JSON SHA-256: `0A4024853D78CA57A13796E5C405ECDF86D4BED7124758891A0E491524B81A3C`; manifest SHA-256: `696384F56045B18047B24AC25C28FB54433400C36B90B9A5F8EF682D13404EB1`.
- Final runtime JSON SHA-256: `0A4024853D78CA57A13796E5C405ECDF86D4BED7124758891A0E491524B81A3C`; final manifest SHA-256: `696384F56045B18047B24AC25C28FB54433400C36B90B9A5F8EF682D13404EB1`.

## 12. Required reading

1. `AGENTS.md`
2. `Docs/Codex_CurrentPhase.md`
3. `Docs/Codex_RA2IniEditor_IDE_FR_DQ_4_PlaceholderRetirement.md`
4. `Docs/FieldRegistryPlaceholderRetirementAudit_2026-07-20.md`
5. This capsule
