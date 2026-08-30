# ASSET-VOX-2C Agent-Led Geometry Proposal — Code Fact Audit

Status: audited / implementation authorized  
Date: 2026-08-28

## Current production path

`Ra2VoxelStylePreviewCoordinator.AnalyzeStructureAsync` currently executes:

1. `Ra2VoxelSemanticSymmetryCompiler.CompileAsync`;
2. exactly two required-tool model calls;
3. complete classification of every Host region;
4. `Ra2VoxelSemanticPartitionReconciler` agreement at confidence >= 0.80;
5. `Ra2VoxelSemanticSymmetryExecutor`, which edits only `SymmetricCore` pairs.

The current path is bounded and safe, but the Host owns too much semantic policy. A valid model judgement can be
discarded because a second round changes a label, a region is omitted, a protected aggregate contains one protected
cell, or the coverage threshold selects a different edit direction. This is the observed reason that structure
recognition can succeed while producing no visible candidate.

## Reusable authority

- `Ra2VoxelSceneSnapshot` remains the only canonical voxel truth.
- `Ra2VoxelSymmetryEvidencePackage` remains the immutable, hash-bound, coordinate-owning evidence package.
- Existing mirror mapping, mesh coverage, quality analysis, connectivity checks, derived snapshot creation and native
  3D Difference review remain reusable.
- `IRa2AiClient` remains the only model transport seam.
- The existing Voxel Style workspace remains the only product surface; no XAML or second workspace is needed.

## Required boundary change

The model must return sparse executable intent (`target + operation`) instead of a complete semantic partition. Omitted
targets mean preserve. The Host may expand only known target IDs into its own coordinates and enforce minimum geometry
safety, but it must not reclassify a target or choose a different edit direction.

One optional evidence-detail request is allowed before the primary proposal. A second model pass reviews the normalized
primary proposal. If and only if their executable fingerprints differ, a third analysis arbitrates. With the optional
evidence request the absolute provider-call ceiling is four.

## Frozen areas

No Shell/XAML, project Apply/Save, VXL/HVA writer, provider protocol, public API, persistence, INI parser, Field Registry,
diagnostics, completion or legacy behavior is in scope. No real DeepSeek or Tencent call is authorized.
