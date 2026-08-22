# RA2IniEditor IDE Handoff v0.4.29

## Scope

v0.4.29 adds Remote Source Presets to Field Import Preview as a local manual source catalog.

Implemented:

- local preset storage at `<GlobalFieldRegistryRoot>/remote-sources/presets.json`;
- preset add, edit, remove, import, and export;
- Use Preset URL, which only copies the URL into Fetch URL;
- Fetch Selected Preset, which is the only new preset action that can contact the network;
- preset editor window and AutomationId coverage;
- store, ViewModel, editor, and guardrail tests.

## Safety Boundary

Remote Source Presets are URL bookmarks, not automatic update sources.

- IDE startup does not fetch.
- Open Folder does not fetch.
- Opening Field Import Preview does not fetch.
- Refresh Presets does not fetch.
- Use Preset URL does not fetch.
- Add/Edit/Remove/Import/Export do not fetch.
- Fetch Selected Preset fetches only after the user clicks it.
- Fetch success does not automatically Parse, Build Apply Plan, Apply, write active packs, reload providers, edit INI files, save INI files, or mark dirty.
- No GitHub API, token, OAuth, cookies, or credentials are used.

## Main Types

Infrastructure:

- `FieldRegistryRemoteSourcePreset`
- `FieldRegistryRemoteSourcePresetCollection`
- `IFieldRegistryRemoteSourcePresetStore`
- `FieldRegistryRemoteSourcePresetStore`

IDE:

- `FieldRegistryRemoteSourcePresetViewModel`
- `FieldRegistryRemoteSourcePresetEditModel`
- `RemoteSourcePresetEditorViewModel`
- `RemoteSourcePresetEditorWindow`

## UI

Field Import Preview now contains a `Remote Presets` tab with:

- `Refresh Presets`
- `Use Preset URL`
- `Fetch Selected Preset`
- `Add Preset`
- `Edit Preset`
- `Remove Preset`
- `Import Presets`
- `Export Presets`

Preset Editor validates name and GitHub raw/blob URL before saving. Invalid input keeps the editor open.

## Tests

Added and updated tests for:

- preset save/load;
- AddOrUpdate;
- remove missing/existing preset;
- import merge with invalid URL skip warning;
- export readable JSON;
- invalid presets file fallback;
- invalid URL rejection;
- Use Preset URL without fetch;
- Fetch Selected Preset with no auto parse/apply;
- disabled preset fetch guard;
- remove confirmation;
- editor validation;
- AutomationId coverage and no-auto-fetch guardrails.

## Next

Recommended follow-up is either:

- v0.4.30 Remote Source Preset UX hardening; or
- v0.4.30 Completion contract.

Do not move to Completion if preset UI or UIA smoke becomes unstable.
