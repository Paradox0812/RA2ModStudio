# ASSET-VOX-2A-R2 Stage Ledger

| Stage | State | Evidence |
|---|---|---|
| R2-0 baseline and fixtures | Completed | rod/plate/noise/determinism fixtures; focused core 13/13 |
| R2-1 protected structure zones | Completed | frozen/transition mask plus endpoint/branch/component/cavity facts |
| R2-2 masked distance refinement | Completed | Conservative/Balanced deterministic discrete-distance candidates; destructive cleanup removed |
| R2-3 admission and selection | Completed | topology/volume/silhouette/quality gates and typed `NoSafeImprovement` |
| R2-4 review UI | Completed | 3D difference view, admission and structure evidence; focused IDE 11/11 |

Shell, project Apply/Save, real model calls, VXL/HVA and persistence remain out of scope for every row.

Final automated evidence: Application 271/271; Debug solution build 0 errors; IDE full 2815/2816 with the documented
WPF Popup teardown flake, isolated rerun 1/1 passed. Manual 1920x1080 review against the user's real vehicle remains pending.

## Physical review correction 1

- User review showed that a no-improvement fallback exposed Direct as Refined/Difference and that axis-agnostic thin-cell
  connectivity merged large body surface networks into the blue protection overlay.
- Thin structures are now grouped by directional signatures: rods follow their major axis and plates remain in their thin
  plane. Ordinary solid body surfaces no longer freeze as one component.
- Added a bounded topology-safe surface pass: it removes attached noise only when the attachment body remains well
  supported, preserves barrel endpoints, and fills only cells with at least five face neighbours.
- Refined and Difference are disabled unless an admitted candidate has a different canonical hash and non-zero delta.
- Physical product-path probe (`H:\RA2\YR_Test\body-candidate.vox + mesh.glb`): Conservative admitted; Direct 18,301,
  Refined 18,267, Added 30, Removed 64, Frozen 126, Transition 50. Temporary probe code was removed.
- Final evidence: focused core 15/15, affected IDE 21/21, Application 273/273, IDE full 2816/2816, build 0 errors / one
  pre-existing nullable warning.

## Physical review correction 2

- The second screenshot proved that the admitted Conservative result was still not a smoothing candidate: its direct-grid
  scan removed locally redundant cells one at a time, producing scattered red salt-and-pepper deltas across the hull.
- The direct-grid deletion pass and the unused distance-filter path were removed from production candidate generation.
  Both candidates now start from one bounded weighted surface proposal, require matching 2x GLB occupancy evidence for
  every addition/removal direction, and retain only 26-neighbour delta components of at least two/three cells.
- Thin frozen/transition coordinates, source mesh, Direct snapshot and every existing hard gate remain unchanged. A
  singleton change cannot enter either candidate even if it improves a scalar quality metric.
- Physical product-path probe (`H:\RA2\YR_Test\body-candidate.vox + mesh.glb`): Balanced admitted; Direct 18,301,
  Refined 18,286, Added 34, Removed 49; zero singleton delta components; one connected component; zero cavities; maximum
  silhouette delta 1.21%; roughness 1.5526 -> 1.5348; low-support cells 76 -> 62.
- Temporary threshold-study code was removed. After the running IDE was closed, the standard Debug build passed with
  zero errors / zero warnings; Application passed 273/273 and the full IDE suite passed 2816/2816.

