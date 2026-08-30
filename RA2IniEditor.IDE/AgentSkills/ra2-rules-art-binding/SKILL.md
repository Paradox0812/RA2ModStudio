---
name: ra2-rules-art-binding
description: Build or review source-backed TechnoType bindings between rules(md).ini, art(md).ini, SHP/VXL/HVA body assets, and cameo assets. Use for explicit rules/art project edits, Image redirects, body artwork, cameos, or cross-document visual bindings.
metadata:
  version: "1"
  ra2-domains: rules-art-binding
  ra2-modes: chat,work
---

# Techno rules/art binding workflow

## Evidence and scope

- Apply this workflow to InfantryType, VehicleType, AircraftType, or BuildingType visual bindings. Preserve unrelated fields on an existing object.
- Treat captured project content and explicit runtime/version facts as strongest evidence. Use Yuri's Revenge behavior unless the context explicitly establishes an Ares/Phobos feature.
- `Art`, `Body`, and `Cameo` in a user request are semantic role labels, not literal INI keys. Never write `Art=` or `Body=` on a normal TechnoType merely because the prompt uses those labels.

## Canonical reference graph

- The rules object is `[OwnerSection]` in rules(md).ini. Its `Image=ArtSection` selects the art(md).ini section; when `Image` is absent, the art section defaults to the owner Section ID.
- The art object is `[ArtSection]` in art(md).ini. Put `Cameo=CameoAsset` and, only when requested, `AltCameo=AltCameoAsset` there. Do not place Techno cameos in rules(md).ini.
- An existing owner already registered in InfantryTypes, VehicleTypes, AircraftTypes, or BuildingTypes needs no new registration. A newly created owner must be added to exactly its correct numbered type list without renumbering unrelated entries. Art sections themselves have no numbered registration list.
- Use bare resource identifiers for ordinary SHP/VXL/HVA and SHP cameo references. Do not claim the physical asset exists merely because an INI reference was added.

## Body image rules

- In vanilla Yuri's Revenge, an InfantryType, VehicleType, or AircraftType normally loads its main body from the art section name. For those families, do not use `art:[ArtSection] Image=DifferentBody` as a universal rename mechanism.
- BuildingType and Animation art sections support `Image=BodyAsset` as a resource-basename override.
- Phobos can extend art-side `Image` to all TechnoTypes only when `[General] ArtImageSwap=true` is established. Do not silently enable that project-wide switch. If the user supplies different ArtSection and BodyAsset IDs for Infantry/Vehicle/Aircraft but the captured context does not establish a compatible mapping, return `needs_clarification` and name this missing fact.
- For a VXL/HVA VehicleType or AircraftType art entry, use `Voxel=yes`; the art section basename selects the `.vxl`/`.hva` body and conventional turret/barrel companions. For SHP artwork, omit `Voxel` or use `Voxel=no` only when an explicit change is needed.
- `Remapable`, palettes, sequences, facings, FLH offsets, turrets, buildup/damage animations, theatre flags, and alternate art are independent properties. Add them only when the request or captured evidence requires them.

## Proposal construction

- For a simple existing-Techno binding, the minimum graph is normally rules `[OwnerSection] Image=ArtSection` plus art `[ArtSection] Cameo=CameoAsset`; add only the body mechanism valid for the classified object family/runtime.
- Emit operations against `target=rules` and `target=art` according to field ownership. Never collapse a cross-document binding into rules-only fields.
- Prefer `upsert_field` so existing values remain reviewable in Diff and missing fields/sections can be created by the Host.
- If owner identity, object family, runtime feature, or distinct ArtSection/BodyAsset semantics are indispensable and cannot be established, ask one concrete clarification instead of guessing. Field Registry and Diagnostics are advisory evidence for this model-owned project plan and do not veto a source-backed operation.

## Source basis

- ModEnc `TechnoTypes`, `Image/On most objects in rules(md).ini`, `How The Engine Uses Files`, `Cameo`, and `Voxel (INI flag)` document the YR reference graph and object-family ownership.
- Phobos `Customizable unit image in art` documents the opt-in `ArtImageSwap` exception.
- Ares `PCX Cameos` and `Custom Cameo Palettes` apply only when the corresponding extension-specific fields are explicitly requested.
