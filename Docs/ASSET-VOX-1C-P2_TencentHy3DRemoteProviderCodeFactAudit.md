# ASSET-VOX-1C-P2 Tencent Hunyuan 3D Remote Provider Code-Fact Audit

Date: 2026-08-26  
State: Completed / implementation gate passed  
Risk: R4

## 1. Goal and evidence

P2 connects Tencent Hunyuan 3D's OpenAI-compatible asynchronous API to the already verified internal
`ra2-voxel-generation/1` Host. The provider is reference-image to shape-only GLB and remains CandidateOnly.

Official facts verified on 2026-08-26:

- base URL: `https://api.ai3d.cloud.tencent.com`;
- submit: `POST /v1/ai3d/submit`; query: `POST /v1/ai3d/query`;
- authorization uses the dedicated `sk-...` API key in the `Authorization` header;
- professional model `3.1` supports image input and `Geometry` white-model output;
- jobs are asynchronous: `WAIT` / `RUN` / `DONE` / `FAIL`;
- successful queries expose `ResultFile3Ds`, including a `GLB` URL and optional preview URL;
- free credit is manually claimed and inspected in the Tencent console. No documented API can prove the remaining free
  balance before submission. Free packages are consumed before paid resources, and postpaid is opt-in.

Authoritative references:

- <https://cloud.tencent.com/document/product/1804/126189>
- <https://cloud.tencent.com/document/product/1804/123447>
- <https://cloud.tencent.com/document/product/1804/123448>
- <https://cloud.tencent.com/document/product/1804/123461>

## 2. Existing reuse path

The existing Host already owns all process, workspace, cancellation, timeout, protocol, artifact-size/hash/magic,
lease, replay and cleanup policy. It must not gain HTTP, Tencent, billing or secret behavior.

The minimal reuse path is one new local executable adapter:

```text
AssetHost -> trusted adapter process -> Tencent HTTPS submit/query -> signed HTTPS artifact download -> Host workspace
```

The adapter reads the Host's staged `ra2-generation-request/1`, emits the unchanged JSON-lines protocol and writes only
under `staging/provider-output`. It does not reference or duplicate the 1B canonical voxel model.

## 3. Environment and cost facts

Approved dedicated settings:

```text
RA2INI_HY3D_API_KEY
RA2INI_HY3D_BASE_URL                 optional; exact official origin only
RA2INI_HY3D_FREE_ONLY_CONFIRMED      must equal 1 before generation
```

The adapter never falls back to `OPENAI_API_KEY`, `DEEPSEEK_API_KEY` or Tencent CAM credentials. Probe is non-networked
and reports ready only when the dedicated key and free-only confirmation are present.

The later live attempt audit found that the Host's complete child-environment clear removed Windows runtime and temporary
path variables. An invalid-key, non-billable probe reached HTTP 401 in the normal environment, failed with only
`TEMP/TMP`, failed locally with only `SystemRoot/WINDIR`, and reached HTTP 401 with all four. The canonical Host runner
therefore now clears inherited state and copies only `SystemRoot`, `WINDIR`, `TEMP` and `TMP`. Dedicated keys and proxy
variables are not inherited.

## 4. Risk classification and boundaries

Risk is R4 because a generation submission can consume billable credit, a secret crosses a child-process boundary and
the output is remote/untrusted. The user explicitly authorized at most three shape-only free-credit calls and required a
stop before any possible paid call.

Frozen boundaries:

- no AssetHost protocol/API/lifecycle change; approved narrow exceptions surface the provider's already bounded/sanitized
  failure message and retain the four minimum Windows child-runtime variables after environment clearing;
- no Application canonical voxel access or duplication;
- no Shell/XAML/UI/AutomationId change;
- no INI, Field Registry, parser, diagnostics, editor, save/apply or project mutation;
- no VXL/HVA conversion and no visual/GameReady certification.

## 5. Data and lifecycle decision

The remote job ID, provider request ID, terminal status, reported credit consumption and input/artifact hashes are
transient provider evidence. They are written only to the provider JSON artifact inside the Host lease. API keys, image
bytes, signed result URLs, absolute paths and raw HTTP bodies are never persisted or printed.

Submit is at-most-once per Host run. Query polling never resubmits. Cancellation/timeout kills the adapter through the
existing Host process-tree arbiter; the remote job may continue server-side and is recorded as a known limitation.

## 6. Post-fix real certification facts

After the original 3/3 attempt budget exposed and isolated the child-environment failure, the user explicitly authorized
one additional post-fix call. Call 4 reached `DONE` after 42 query polls (about 2 minutes 10 seconds) and produced:

- `mesh.glb`: 8,991,920 bytes, SHA-256 `22FD5BE5BEB833C8ECAF05E16A8A070B699FF1C9339F24E51054A330CB57F709`;
- `preview.png`: 77,888 bytes, SHA-256 `6AB9D84EAAE241BC9088F6B0CB237F1C43EA6E9DCBE739F9DDB73319877F294F`;
- `provider-report.json`: 1,076 bytes, sanitized and free of signed URLs, secrets and absolute paths.

The GLB is structurally valid glTF 2.0 with exact declared length, one scene/node/mesh/primitive, 249,567 vertices and
499,698 triangles. The response did not include credit-consumption fields, so the Tencent console remains authoritative
for free-pack accounting. The evidence directory is under excluded `artifacts/` and is not part of the clean source
package.
