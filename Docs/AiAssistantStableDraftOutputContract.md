# AI Assistant Stable Draft Output Contract

## 1. Purpose

This contract defines stable output rules for AI-generated Red Alert 2 / Yuri's Revenge / Ares / Phobos INI drafts.

The AI Assistant may generate draft text for review and copy, but it must not claim that anything was applied, inserted, saved, fixed, or written to project files.

This document is a contract only. It does not implement PromptBuilder, UI, provider, Apply, Insert, file modification, or Field Registry write behavior.

## 2. Output Goals

AI draft answers must be predictable, reviewable, and safe to copy.

When generating INI drafts, the assistant should use this structure:

1. Assumptions
2. Clean INI draft blocks
3. Required TODO definitions
4. Field rationale / evidence
5. Warnings / validation notes
6. Suggested next steps

The clean INI draft block is for copying. Explanations, rationale, uncertainty, and TODO notes must be outside code blocks.

## 3. Faction And Owner Stability

If the user does not specify faction, side, country, or owner, the assistant must not randomly choose Allied, Soviet, Yuri, or any mod-specific faction.

Required behavior:

1. If current bounded context clearly implies an owner/faction, state that assumption explicitly.
2. If no clear owner/faction exists, use a TODO placeholder.
3. The clean draft must use `Owner=<TODO_OWNER>` instead of inventing a faction.
4. The assumptions section must state that the faction was not specified.

Example:

```markdown
## 假设

- 阵营：用户未指定，草稿使用 `Owner=<TODO_OWNER>` 占位。
- 当前内容仅是 AI 草稿，不会自动写入任何文件。
```

Forbidden:

```ini
Owner=Allies
```

when the user did not specify Allied and the current context does not clearly imply it.

## 4. Clean INI Block Rules

Clean INI blocks must be copyable by default.

Rules:

1. Do not include inline explanatory comments in clean INI blocks by default.
2. Do not include prose inside INI code blocks.
3. Do not include Markdown bullets inside INI code blocks.
4. Do not include uncertainty wording inside clean INI blocks.
5. Explanations belong in sections outside code blocks.
6. If the user explicitly asks for annotated INI, comments may be placed in a separate annotated block, not in the default clean block.

Preferred:

```ini
[LAAV]
Name=Light Anti-Air Vehicle
UIName=Name:LAAV
Primary=LAAVMissile
Strength=200
Armor=light
Speed=10
Owner=<TODO_OWNER>
TechLevel=2
Cost=500
```

Avoid in clean drafts:

```ini
[LAAV] ; new light anti-air vehicle
Strength=200 ; medium durability
Owner=Allies ; guessed faction
```

## 5. File And Section Separation

When output includes multiple target files, separate them clearly.

Required headings:

```markdown
## rulesmd.ini 草稿
```

```markdown
## artmd.ini 草稿
```

Rules:

1. Do not mix `artmd.ini` image/voxel/SHP/cameo fields into `rulesmd.ini` blocks.
2. Do not mix `rulesmd.ini` gameplay fields into `artmd.ini` blocks unless explicitly explaining a cross-file reference.
3. If only one file target is needed, output only that file's draft section.
4. If file target is uncertain, state the uncertainty outside the code block.

## 6. New Reference IDs And TODOs

If the clean draft references new IDs, those IDs must be listed under a TODO section.

Common required TODO definitions include:

- Weapon IDs
- Warhead IDs
- Projectile IDs
- Art IDs
- Voxel / SHP image definitions
- Cameo / icon assets
- Sound IDs
- Animation IDs
- Prerequisite building IDs when newly introduced

Example:

```markdown
## 需要补充的定义

- `[LAAVMissile]` weapon definition
- `[LAAVMissileP]` projectile definition
- `[LAAVMissileWH]` warhead definition
- `artmd.ini` 中 `[LAAV]` 图像定义
- Cameo / voxel asset ID for `LAAV`
```

If the assistant references an ID in clean INI and that ID is not known from current context or Field Registry evidence, it must be listed here.

## 7. Field Registry Evidence Rules

Field Registry evidence is advisory reference data, not absolute truth. The assistant must use it to make draft fields more reviewable.

For important fields in the clean draft:

1. If Field Registry evidence exists, cite the rationale outside the INI block.
2. If Field Registry evidence does not exist, either omit the field from the clean draft or place it under an optional verification section.
3. Unknown or low-confidence fields must not silently appear in the clean draft as if confirmed.
4. Do not invent Field Registry evidence.
5. Do not claim a field is valid only because the AI generated it.

Required wording for uncertain fields:

```markdown
## 可选 / 使用前需验证

- `SomeField=`：当前 Field Registry evidence 未确认该字段；放入 clean draft 前需人工验证。
```

Clean draft rule:

```text
No evidence + not essential = omit from clean draft.
No evidence + potentially useful = optional / verify before use.
```

## 8. Rationale Outside Code Blocks

Every generated draft should include concise rationale outside code blocks.

Example:

```markdown
## 字段依据

- `Primary`：主武器引用；新 ID 已列入 TODO。
- `Strength`：单位生命值 / 耐久度。
- `Armor`：装甲类型；需按项目现有 armor 规则复核。
- `Owner`：用户未指定阵营，使用 `<TODO_OWNER>` 占位。
```

Rationale must not include raw prompt payload, API key, Authorization header, full context payload, selected INI text beyond what is already visible in the answer, or absolute paths.

## 9. Assumptions And Warnings

The assistant must state assumptions before the clean draft when assumptions affect generated INI.

Common assumptions:

- User did not specify faction / owner.
- User did not specify file target.
- User did not specify image asset ID.
- Current context did not include an existing weapon chain.
- Field Registry evidence was unavailable for some fields.

Warnings should be compact and actionable:

```markdown
## 注意事项

- `<TODO_OWNER>` must be replaced with a real owner before use.
- Weapon / projectile / warhead IDs must be defined before this unit can work in-game.
- Fields listed under optional verification should be checked against project rules or Field Registry before copying.
```

## 10. Output Template

For unit prototype generation, use this default shape:

````markdown
## 假设

- 阵营：用户未指定，使用 `Owner=<TODO_OWNER>` 占位。
- 文件目标：单位主体放在 `rulesmd.ini`；图像定义如需要放在 `artmd.ini`。
- 当前内容是 AI 草稿，仅供复制和人工审查，不会自动写入任何文件。

## rulesmd.ini 草稿

```ini
[LAAV]
Name=Light Anti-Air Vehicle
UIName=Name:LAAV
Primary=LAAVMissile
Strength=200
Armor=light
Speed=10
Owner=<TODO_OWNER>
TechLevel=2
Cost=500
```

## artmd.ini 草稿

```ini
[LAAV]
Voxel=yes
Remapable=yes
Cameo=<TODO_CAMEO>
```

## 需要补充的定义

- `[LAAVMissile]` weapon definition
- `[LAAVMissileP]` projectile definition
- `[LAAVMissileWH]` warhead definition
- `<TODO_CAMEO>` cameo asset

## 字段依据

- `Primary`：主武器引用；新 weapon ID 已列入 TODO。
- `Strength`：单位耐久度。
- `Armor`：装甲类型；需按项目规则验证。
- `Owner`：用户未指定阵营，使用 TODO 占位。

## 可选 / 使用前需验证

- Any field not supported by Field Registry evidence should be listed here instead of silently added to the clean draft.

## 下一步

- 替换 `<TODO_OWNER>`。
- 补齐 weapon / projectile / warhead 定义。
- 按项目现有 art 资源替换 `<TODO_CAMEO>`。
````

## 11. PromptBuilder Requirements For Future AI-5B

Future PromptBuilder work should add draft-generation instructions that require:

1. Answer in Chinese.
2. Mark generated INI as draft.
3. Output clean INI blocks without inline comments unless the user asks for annotated INI.
4. Put explanations outside code blocks.
5. Do not randomly choose faction, side, country, or owner.
6. Use TODO placeholders for unspecified owner/faction/art IDs.
7. Separate `rulesmd.ini` and `artmd.ini` draft sections.
8. List new referenced IDs under TODO.
9. Include assumptions and uncertainty.
10. Include field rationale grounded in Field Registry evidence.
11. Omit unsupported fields from clean draft or list them under optional verification.
12. Do not claim any draft was applied, inserted, saved, fixed, or written.

## 12. Tests Planned For Future AI-5B

Future tests should verify PromptBuilder output contains instructions for:

1. No inline comments in copyable INI blocks by default.
2. Assumptions section is required for draft-generation intents.
3. Missing faction uses TODO owner placeholder.
4. `rulesmd.ini` and `artmd.ini` sections are separated.
5. New referenced IDs must be listed under TODO.
6. Field rationale appears outside code blocks.
7. Unknown fields without Field Registry evidence are omitted or marked optional / verify.
8. Output is marked as draft.
9. The model is forbidden from claiming file changes.

## 13. Future Implementation Split

Recommended split:

- AI-5A: Stable draft output contract.
- AI-5B: PromptBuilder draft-output template update.
- AI-5C: AI panel copy-code-block UX.
- AI-5D: Draft validation against Field Registry evidence.

## 14. Non-Goals

This contract does not implement:

- PromptBuilder code changes.
- DeepSeek adapter changes.
- Provider selection changes.
- API key UI.
- Settings persistence.
- Apply / Insert.
- File modification.
- Field Registry writes.
- Whole-project context.
- Diagnostic auto-fix.
