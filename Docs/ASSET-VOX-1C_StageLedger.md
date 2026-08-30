# ASSET-VOX-1C Stage Ledger

Date: 2026-08-26  
Package state: Completed / automated verified

| Stage | Goal | Evidence | State | Next entry |
|---|---|---|---|---|
| 1C-0A | Audit existing provider, process, workspace and runtime boundaries | Code fact audit | Completed | 1C-0B |
| 1C-0B | Freeze revised R4 protocol/data/lifecycle/verification contract | Final contract, self-review and proposed decision | Completed | User approval |
| 1C-1 | Add headless AssetHost, exact internal API and readiness probe | Exact seam/export tests; ready/hash/identity/cancel tests | Completed | 1C-2 |
| 1C-2 | Implement locked lease, bounded workspace, TTL janitor, artifact validation and provenance | Containment/lease/orphan/budget/promotion tests | Completed | 1C-3 |
| 1C-3 | Implement local-process protocol, concurrent stream pumps and terminal arbitration | 15 lifecycle/backpressure/cancel/timeout/process-tree tests | Completed | 1C-4 |
| 1C-4 | Add deterministic probe/generation fixture provider and replay matrix | Same-seed equality, mutated seed/input drift, probe matrix, provenance | Completed | 1C-5 |
| 1C-5 | Regression, package and documentation closeout | AssetHost 38/38; Application 228/228; IDE 2779/2779; build 0/0; package 1295 | Completed | 1C-P1 or 1D audit |
| 1C-P1 | Configure and certify a real image-to-3D provider | Provider-specific adapter/license/environment/manual evidence | Deferred | Explicit separate authorization |

## Current gate

Initial risk was R3. Audit raised it to R4 because the child-process JSON protocol and file-backed artifact handoff are
compatibility boundaries. The user approved the revised final contract and continuous 1C-1 through 1C-5 execution.

The frozen `ProbeAsync`/`RunAsync`/async workspace lease, marker/lock/TTL orphan janitor and bounded concurrent stream
arbitration are now implemented and tested. R4 closeout passed without promoting the internal boundary to public API.

## Deferred governance queue

### Public API — closed for 1C

- Verified zero Application public API changes, exact Application allowlist 77 and zero exported AssetHost types in 1C.
- Promotion to a public plugin contract is deferred until at least one real provider proves the internal protocol.

### Architecture decisions — accepted for 1C

- Accepted: isolate provider execution in a new headless assembly.
- Accepted: expose only one internal Probe/Run Host seam and a read-only async-disposable workspace lease.
- Accepted: clean only marker-valid, unlocked, TTL-expired runs under the dedicated workspace root.
- Accepted: keep 1C runs transient and defer general Job/Event/Artifact persistence.
- Accepted: treat process isolation as fault isolation, not an OS sandbox.
- Accepted: certify real TRELLIS/Hunyuan adapters separately from the provider-neutral Host.

### Explicitly deferred debt

- Remote HTTP adapter and secret storage.
- OS sandbox/AppContainer/container enforcement.
- Persistent run recovery/artifact repository.
- Real provider installation, license acceptance and deterministic replay certification.
- UI/Work integration and project commit.
