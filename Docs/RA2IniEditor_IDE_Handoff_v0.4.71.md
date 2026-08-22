# RA2IniEditor IDE Handoff v0.4.71

## Target

v0.4.71 focuses on Editor UX Polish Phase 1:

- Stabilize readonly field hover quick info.
- Keep completion trigger UX as a contract for the next slice.

## Hover Quick Info

Hover remains a readonly UI affordance. It does not modify editor text, caret position, dirty state, save state, completion state, or Add Property insert semantics.

Current behavior:

- Hover opens after the pointer remains on a key token for 300 ms.
- Moving to a different document offset restarts the delay.
- Moving away, scrolling, typing, caret movement, completion open, Add Property open, programmatic text sync, and focus loss close hover.
- The hover popup is non-interactive and non-focusable.
- The popup is offset from the pointer to avoid pointer flicker.
- The popup stays open under manual lifecycle control instead of using WPF mouse capture.
- The hover controller records the active offset so pointer movement on the same key does not restart the timer.
- Hover text remains at most two lines:
  - Line 1: `Type Key DisplayName`
  - Line 2: annotation note or field registry description, when available.

## Completion Trigger Contract

v0.4.71 does not implement automatic completion popup.

Kept behavior:

- Ctrl+Space remains the primary manual completion trigger.
- Right-click entry remains a fallback.
- Completion commit behavior is unchanged.

Suggested v0.4.72 direction:

- Trigger completion automatically only in edit mode.
- Debounce text input before opening.
- Open on key context after partial key typing.
- Open on value context after `=` or comma-delimited token typing.
- Never open while hover, Add Property, save confirmation, or another popup owns the current UX.

## Non-goals

- No save pipeline changes.
- No dirty-state changes.
- No completion auto-trigger implementation.
- No field registry changes.
- No Source Editor edit-chain redesign.

## Validation

Recommended commands:

```powershell
dotnet test -c Release --filter FullyQualifiedName~Hover
dotnet test -c Release
dotnet build -c Release --no-incremental
```

Manual smoke:

1. Open IDE and load an INI file.
2. Move pointer over `Armor` or another known key and wait about 300 ms.
3. Confirm hover appears without pointer flicker.
4. Move to a different key and confirm the old hover closes before the new one appears.
5. Scroll, type, move caret, open completion, and open Add Property; each should close hover.
