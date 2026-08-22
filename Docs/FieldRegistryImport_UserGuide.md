# Field Registry Import User Guide

## What It Is

Field Registry Manager shows the active local field packs used by the IDE readonly highlighter.

Priority order:

1. Project active fields override Global active fields.
2. Global active fields override BuiltIn fields.
3. BuiltIn fields remain the fallback registry.

`UnknownKey` means a key is not present in the current effective registry. It is a highlighting/registry signal, not proof that the INI key is invalid.

## Field Import Preview

Field Import Preview lets you paste field documentation, inspect parsed candidates, compare them with the current registry, and apply accepted add/update operations to a local active pack after confirmation.

You can also fetch raw field documentation by manually entering a supported URL and clicking `Fetch Raw Text`. Fetch is optional and user-triggered only.

Supported input shapes:

- INI-like lines, for example `Owner=`
- bullet list entries, for example `- MyCustomKey: custom local field`
- markdown tables with `Key`, `AppliesTo`, `Type`, and `Description` columns

Supported fetch URLs:

- `https://raw.githubusercontent.com/<owner>/<repo>/<branch>/<path>`
- `https://github.com/<owner>/<repo>/blob/<branch>/<path>`

Fetch does not run automatically at IDE startup, Open Folder, diagnostics, highlighting, reload, apply, or rollback time. Fetch does not use a GitHub token, OAuth, cookies, or stored credentials.

The import flow is explicit:

1. Paste text or click Fetch Raw Text.
2. Parse & Preview.
3. Review diff, validation issues, and drafts.
4. Build Apply Plan.
5. Apply after confirmation.

After a successful fetch, the raw text box is filled and the source name is updated. The IDE does not automatically parse, build a plan, apply, write an active pack, or reload the provider.

## Remote Source History

Successful manual fetches are recorded in a local history file:

```text
<GlobalFieldRegistryRoot>/remote-sources/history.json
```

The history stores the original URL, resolved raw URL, source name, fetch time, byte count, and cached raw text for recent sources. It is local-only and is not synced.

History actions:

- Refresh History: reloads the local history file. This does not contact the network.
- Use Cached Text: copies cached text into RawText. This does not contact the network and does not parse automatically.
- Re-fetch Selected: fetches the selected source again. This is a user-triggered network action.
- Clear History: clears local remote source history. This does not change RawText, active field packs, or INI files.

## Remote Source Presets

Remote Source Presets are local URL bookmarks for field documentation sources. They are stored in:

```text
<GlobalFieldRegistryRoot>/remote-sources/presets.json
```

Each preset contains a name, URL, optional description, tags, and enabled state. Presets are not update jobs and do not fetch automatically.

Preset actions:

- Refresh Presets: reloads local `presets.json`. This does not contact the network.
- Use Preset URL: copies the selected preset URL into Fetch URL. This does not contact the network, does not change RawText, and does not parse automatically.
- Fetch Selected Preset: fetches the selected enabled preset. This is a user-triggered network action.
- Add/Edit/Remove Preset: changes the local preset catalog only.
- Import/Export Presets: reads or writes preset JSON only.

Fetch Selected Preset fills RawText and updates fetch history after success. It still does not automatically Parse, Build Apply Plan, Apply, write an active pack, or reload the provider.

## Preview Tabs

- Parsed Fields: raw candidates found in the pasted text.
- Validation Issues: validation errors or warnings for parsed candidates.
- Field Drafts: normalized field definitions that could become registry entries.
- Preview Diff: comparison against the current effective registry, including existing provenance.
- Apply Plan: final add/update/skip/reject operations for the selected target and mode.
- Parse Warnings: non-fatal parser warnings, such as ignored or duplicate input rows.

## Apply Targets

Project target writes to:

```text
<ProjectRoot>/.ra2inieditor/field-registry/active/user-import.fields.json
```

Global target writes to:

```text
<GlobalFieldRegistryRoot>/active/user-import.fields.json
```

Apply creates a backup manifest before overwriting an existing target pack. Apply does not edit INI files.

## Rollback

Rollback is manifest based. Field Registry Manager reads recent backup manifests and lets you select one rollback candidate.

Rollback can:

- restore a target active pack from backup;
- delete a target active pack created by apply;
- no-op if the created target pack is already absent.

Rollback does not modify INI files and does not contact the network.

## Not Implemented Yet

- automatic GitHub fetch
- GitHub API / token / login
- remote background update
- Completion
- field registry editor
- INI editing or save integration
- dirty state
- batch rollback
- multi-target pack selection
