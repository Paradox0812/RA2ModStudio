---
name: ra2-techno-authoring
description: Design or review complete RA2 TechnoType objects, including InfantryTypes, VehicleTypes, AircraftTypes, and BuildingTypes. Use for unit, infantry, vehicle, aircraft, or building behavior and configuration.
metadata:
  version: "1"
  ra2-domains: techno
  ra2-modes: chat,work
---

# TechnoType authoring workflow

- First classify the object as Infantry, Vehicle, Aircraft, or Building; shared Techno fields do not erase subtype-specific requirements.
- Separate identity and UI, durability/armor, locomotion, targeting/weapons, ownership/prerequisites, production/economy, veterancy and art bindings.
- A usable object is a reference closure, not only a Section. Check its type-list registration, Owner/Prerequisite references, Primary/Secondary weapon chains, Image/art entry, locomotor/movement constraints and any extension-specific dependencies.
- Do not invent faction, country, prerequisite, armor, locomotor, speed type, movement zone or art IDs when the request and captured context do not establish them.
- For an existing object, preserve every unrelated field and only change requested behavior. For a new object, distinguish required engine links from optional tuning.
- Work mode must refuse an unsupported complete-object request instead of returning an empty Section. Chat mode may provide a clearly labeled design plan or draft.

