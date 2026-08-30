# ASSET-VOX-1C-P2 Tencent Hunyuan 3D Remote Provider Final Contract

Date: 2026-08-26  
State: Final / approved by user / self-reviewed  
Risk: R4  
Governance: Continuous StagePackage / Deferred Governance

## 1. Outcome

P2 delivers one internal, provider-specific executable that maps a single reference image to one Tencent Hunyuan 3D
3.1 `Geometry` GLB candidate through the unchanged `ra2-voxel-generation/1` Host.

Success certifies transport, protocol, containment and provenance. It does not certify visual quality, topology,
voxelization, VXL/HVA compatibility or game readiness.

## 2. Continuous stages

| Stage | Deliverable | Mandatory gate |
|---|---|---|
| P2-0 | Official API/cost and existing Host audit; final contract | R4/user scope and free-only rule explicit |
| P2-1 | Provider executable, request/query mapping and protocol output | model-free contract tests pass |
| P2-2 | secret, endpoint, at-most-once, polling, download and provenance gates | no secret/URL/path leak; Host probe integration passes |
| P2-3 | up to three authorized real shape-only calls | dedicated key visible and free balance/postpaid state externally verified |
| P2-4 | regression, package and documentation closeout | all touched gates pass; diff audit clean |

A failed mandatory gate stops later stages. P2-3 may be blocked without invalidating completed P2-0/P2-2 work.

## 3. Frozen provider identity

```text
ProviderId: tencent-hy3d-openai-compatible
ProviderVersion: 1.0.0
ModelId: hunyuan-3d-professional
ModelRevision: 3.1-geometry
Capability: ReferenceImageToMesh
SeedBehavior: Unsupported
References: exactly 1
Candidates: exactly 1
GenerateType: Geometry
EnablePBR: false
```

The adapter uses the professional 3D request schema's flat `ImageBase64` value through the OpenAI-compatible submit
endpoint. It does not send `Prompt` together with the image because the provider's professional request contract makes
image and prompt mutually exclusive. The Host prompt remains request/provenance intent, not a provider-side
image-generation parameter. The compatibility page's `ImageUrl.Url` wording is not treated as stronger authority than
the professional 3D parameter table after a real nested-field request was rejected before Job creation.

## 4. Secret and cost contract

- Read the API key only from `RA2INI_HY3D_API_KEY`.
- Never echo, serialize, hash, log or include the key in exceptions.
- Accept only `https://api.ai3d.cloud.tencent.com` as the API origin.
- `probe` performs no network request and consumes no credit.
- `generate` refuses to submit unless `RA2INI_HY3D_FREE_ONLY_CONFIRMED=1`.
- Product code cannot prove free balance because Tencent exposes it only in the console. P2-3 therefore also requires
  external confirmation that a valid free pack remains and postpaid is disabled.
- This approved certification run may submit at most three jobs total. Any indication of paid consumption stops the run.
- The adapter never retries `submit`; repeated `query` calls address the same JobId and are not new generations.
- The Host clears inherited child environment, then retains only `SystemRoot`, `WINDIR`, `TEMP` and `TMP` so the .NET
  child can resolve Windows TLS/runtime services and create temporary files. It does not inherit API keys, proxy settings,
  `PATH` or arbitrary user variables; the adapter reads its dedicated settings from Windows User scope.

## 5. HTTP and artifact contract

- JSON request/response bodies are capped at 1 MiB; the source image is capped at 6 MiB before base64.
- Poll every 3 seconds within the Host deadline; accept only `WAIT`, `RUN`, `DONE`, `FAIL`.
- Select exactly one `GLB` entry from `ResultFile3Ds`.
- Download only HTTPS URLs, without forwarding the API authorization header.
- Follow at most three HTTPS redirects; cap each artifact at 256 MiB.
- Optional preview is downloaded only when requested and must be PNG.
- Write only to Host `provider-output`; emit uppercase SHA-256, exact length and relative names.
- Provider JSON records sanitized identity, request/input hashes, JobId, RequestId, terminal status and reported credits.
  It excludes secrets, signed URLs, image bytes, raw bodies and absolute paths.

## 6. Failure semantics

Configuration and safe provider errors use existing failure kinds only:

| Condition | Existing failure |
|---|---|
| key/free-only confirmation absent, invalid official origin | `ProviderNotReady` |
| malformed/unsupported Host request or image | `InvalidRequest` |
| submit/query/provider `FAIL`/invalid response | `ProviderReportedFailure` |
| missing GLB | `OutputMissing` |
| oversized response/artifact | `ResourceLimitExceeded` |
| Host deadline/cancellation | existing `TimedOut` / `Canceled` arbitration |

Messages are bounded and sanitized. HTTP response bodies and signed URLs never cross the result seam.

## 7. Verification

Model-free tests cover payload shape, wrapped/flat response parsing, at-most-once submit, polling, GLB selection,
HTTPS/redirect/size enforcement, secret redaction, provider JSON and Host probe protocol integration.
The process-boundary tests also freeze the exact child-environment allowlist and reject key/proxy inheritance.

Repository gates:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

The live test is explicit opt-in and disabled by default. It copies evidence only under excluded `artifacts/` and never
adds the supplied image or generated model to a source package.

## 8. Self-review result

Passed. The contract reuses the existing Host rather than adding parallel lifecycle/storage logic; keeps all new types
internal; distinguishes local readiness from billable remote generation; prevents credential fallback; freezes
at-most-once submission and free-only stop rules; and preserves 1B/1C/UI/INI boundaries.

## 9. Certification result

The original P2-3 budget ended at 3/3 attempts with zero JobIds and zero artifacts. Non-billable probes then proved that
clearing all child environment variables removed both required Windows runtime roots and temporary paths. After the
minimum four-variable allowlist was implemented, the user explicitly authorized one additional call. Call 4 reached
`DONE`, produced a Host-validated glTF 2.0 GLB plus preview and sanitized provider report, and made no duplicate submit.

The provider did not return `ResultCreditConsumed` or `ResultCreditDetails`; free-pack consumption must still be checked
in the Tencent console. This certifies the remote transport/Host artifact path, not visual quality, voxelization,
VXL/HVA compatibility or GameReady status.
