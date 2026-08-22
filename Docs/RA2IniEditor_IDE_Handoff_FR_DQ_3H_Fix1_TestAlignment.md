# FR-DQ-3H-Fix1 Test Alignment Handoff

## Summary

This patch fixes the test failures reported after running `dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build` against `FR-DQ-3H_LightweightHoverTrust`.

## Changes

### Runtime provider precedence

Updated `CompositeRa2FieldDefinitionProvider` primary match selection so local Project/Global field definitions with a concrete or abstract section match override BuiltIn fallback definitions even when the BuiltIn row is more specific. Generic `Unknown` local fallbacks still do not hide more specific Project/Global/BuiltIn definitions.

This restores the intended behavior covered by:

- `Reload_GlobalFieldOverridesBuiltInField`
- `Reload_ProjectFieldOverridesBuiltInOnlyWhenSpecificityMatches`
- `Reload_GlobalSpecificFieldBeatsProjectUnknownFallbackField`
- `Reload_GlobalSpecificFieldBeatsProjectGlobalFallbackField`

### BuiltIn description tests

Updated stale `BuiltInFieldRegistryPackLoaderTests` expectations to match the newer 3G/3H BuiltIn field descriptions for:

- `AA / Projectile`
- `AG / Projectile`
- `CellSpread / Warhead`
- `PercentAtMax / Warhead`
- `Verses / Warhead`
- `ToProtect / AI`
- `Spawned / AI`
- `Speed / Global`
- `Projectile / Techno`

These changes do not weaken the guardrails; they align the tests with the improved wording that now uses `不应作为...` and more precise verified descriptions.

## Validation

Static checks completed in this environment. `dotnet` is not available here, so please rerun locally:

```powershell
dotnet build .\RA2IniEditor.IDE.sln -c Release --no-restore
dotnet test .\RA2IniEditor.IDE.sln -c Release --no-build
```
