# RA2IniEditor IDE Handoff v0.4.27

## Version

v0.4.27 User-triggered GitHub Fetcher Spike

## Scope

This version adds an explicit raw text fetch entry point to Field Import Preview. It is intentionally narrow:

- user enters a URL;
- user clicks `Fetch Raw Text`;
- the IDE fetches raw text from an allowed GitHub/raw URL;
- the fetched text is copied into `RawText`;
- the user still manually runs Parse & Preview, Build Apply Plan, and Apply.

No automatic network access was added.

## Supported URLs

Allowed:

- `https://raw.githubusercontent.com/<owner>/<repo>/<branch>/<path>`
- `https://github.com/<owner>/<repo>/blob/<branch>/<path>`

The second form is resolved to `raw.githubusercontent.com`.

Rejected:

- non-HTTPS URLs;
- non-GitHub domains;
- GitHub URLs that are not `/blob/` file URLs;
- empty or invalid URLs.

## Fetch Boundary

Network access only occurs from the explicit Field Import Preview button click:

```text
Fetch Raw Text
```

Fetch is not connected to:

- app startup;
- Open Folder;
- Reload Local Field Registry;
- highlighting;
- diagnostics;
- Parse & Preview;
- Build Apply Plan;
- Apply;
- Rollback.

Fetch does not use GitHub token, OAuth, Credential Manager, cookies, or stored credentials.

## Safety Limits

The fetcher:

- limits downloaded content to 512 KB by default;
- uses a default 15 second HTTP timeout;
- accepts text/markdown/plain, JSON-like, and octet-stream raw content;
- rejects unsupported content types;
- preserves existing `RawText` on failure;
- supports cancellation.

## User Flow

1. Open Field Registry Manager.
2. Open Field Import Preview.
3. Enter a supported URL.
4. Click `Fetch Raw Text`.
5. Confirm `RawText` is filled.
6. Click `Parse & Preview` manually.
7. Continue with the existing apply/rollback flow.

## Guardrails

Still not implemented:

- automatic GitHub fetch;
- GitHub API / token / login;
- remote source cache;
- fetch history;
- Completion;
- INI save / dirty / edit chain;
- field registry editor;
- batch rollback;
- multi-target pack selection.
