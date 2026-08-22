# RA2IniEditor IDE Handoff v0.4.28

## Version

v0.4.28 Remote Source Cache + Fetch History

## Scope

This version adds local history/cache for remote raw sources that the user explicitly fetched from Field Import Preview.

It does not add automatic fetch, GitHub API, token login, Completion, INI save, dirty state, editable source text, automatic parse, automatic apply, or active pack writes outside the existing manual Apply flow.

## Storage

Remote source history is stored under the global field registry root:

```text
<GlobalFieldRegistryRoot>/remote-sources/history.json
```

The store is global-only for now. Project-specific remote source history is not implemented.

## History Model

Each entry records:

- original URL;
- resolved raw URL;
- source name;
- fetched UTC time;
- byte count;
- optional cached raw text.

Entries are deduplicated by resolved URL. The newest entry is kept first. The store keeps at most 20 entries and limits cached text to the existing 512 KB fetch limit.

Bad history JSON does not crash the UI. The list loads as empty and the status text reports the load failure.

## Field Import Preview Behavior

Manual Fetch Raw Text still works as before:

```text
URL -> Fetch Raw Text -> RawText / SourceName
```

After a successful fetch, the result is saved to local history and the history grid refreshes.

Remote History actions:

- Refresh History: reads local history only, no network.
- Use Cached Text: copies cached text to RawText, no network.
- Re-fetch Selected: user-triggered network fetch for the selected source.
- Clear History: clears local history after confirmation; RawText and active packs are unchanged.

No history action automatically runs Parse & Preview, Build Apply Plan, Apply, reload provider, or write active packs.

## Guardrails

Network access remains limited to explicit user actions:

- Fetch Raw Text
- Re-fetch Selected

Still not implemented:

- startup fetch;
- Open Folder fetch;
- Field Registry Manager open fetch;
- highlighter / diagnostics / apply / rollback fetch;
- GitHub API / token / OAuth;
- background timer refresh;
- batch fetch;
- Completion;
- INI save / dirty / edit chain;
- field registry editor.
