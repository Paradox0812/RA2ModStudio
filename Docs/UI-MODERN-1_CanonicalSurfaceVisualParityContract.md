# UI-MODERN-1 Canonical Surface / Visual Parity Contract

Status: M0A reference baseline frozen; production XAML not started  
Baseline date: 2026-07-21  
Product: RA2IniEditor.IDE-only

## 1. Purpose

This contract freezes the approved visual intent for the modern WPF IDE redesign. The reference images are design-intent evidence, not runtime data fixtures and not automatically generated XAML. Before production XAML changes, M0D must translate the images into deterministic WPF/DIP dimensions and responsive rules.

## 2. Frozen reference images

| Surface | File | Pixel size | SHA-256 |
|---|---|---:|---|
| Light Shell | `Docs/UiVisualBaselines/UI-SHELL-Light.png` | 1672 x 941 | `23AC91BF5EAAC5AD8D966A0671515ABF684F174A7D4B7AFFB409CC80EADD3462` |
| Light Search workspace | `Docs/UiVisualBaselines/UI-SEARCH-Light.png` | 1672 x 941 | `6042ECB72CED9BAA31948A2CEDFDFEB49F0F5D28BB0E09DA9A0485B704A1ACC5` |
| Light Field Registry | `Docs/UiVisualBaselines/UI-FR-Light.png` | 1672 x 941 | `10D9979917F4F4235C069334DB30B15771D39659FD4AC3777B5081F2B378E687` |
| Dark Shell palette | `Docs/UiVisualBaselines/UI-SHELL-Dark.png` | 1672 x 941 | `652EDA59805DDD14CAC7CFDCF25CE818D43530CC7113BE7F6F636551B3F9F82B` |
| User-supplied Visual Studio layout ratio | `Docs/UiVisualBaselines/UI-SHELL-1920x1080-LayoutReference.png` | 2559 x 1389 | `B3C010577AB160DBFD9FD4DD5CA1A7C472D6A855461ABBA0AB9D25FA2DF0E186` |

The files and hashes above are immutable acceptance inputs. A later stage may add real WPF screenshots, annotated overlays, or corrected contracts, but must not silently regenerate or overwrite these five source images.

## 3. Authority of each reference

### 3.1 Light Shell

Authoritative for:

- top chrome density and hierarchy;
- editor-first workbench composition;
- right tool-well placement;
- bottom tool-panel placement;
- restrained borders, fills, and command styling.

### 3.2 Search workspace

Authoritative for:

- Search as a bottom tool surface rather than a standalone primary window;
- compact query controls;
- file-oriented result hierarchy;
- status and result density.

### 3.3 Field Registry

Authoritative for:

- one primary Field Registry Center;
- internal navigation and persistent priority information;
- main field list plus read-only details hierarchy;
- separation of daily browsing from high-risk write workflows.

### 3.4 Dark Shell

Authoritative only for:

- dark palette relationships;
- contrast and surface elevation;
- selection, focus, and divider treatment.

The left activity rail visible in the dark reference is explicitly excluded from UI-MODERN-1. The current product already exposes navigation through the main menu, toolbar, right tool well, and bottom tool panel; adding a second rail would duplicate ownership.

### 3.5 User-supplied Visual Studio layout ratio

Authoritative for geometry and proportions only:

- default design canvas is 1920 x 1080 at 100% scaling;
- compact top chrome occupies roughly 7% of screen height;
- the editor-side workspace occupies roughly 84% of screen width;
- the right tool well occupies roughly 16% and spans the complete workspace height;
- the bottom tool well occupies only the editor column and roughly one quarter of screen height when opened;
- document/editor space remains visually dominant even while bottom and right tools are visible.

The screenshot's product name, menus, code, dark colors, extensions, icons, tool-window content, pet overlay, and exact Visual Studio commands are illustrative only. They must not be copied into runtime data or treated as feature requirements.

## 4. Illustrative-only content

The following are not runtime contracts:

- sample file names and project trees;
- source text shown in the editor;
- field names, counts, warnings, and trust values;
- search queries and result counts;
- AI conversation text;
- any generated icon that does not already exist in project resources.

Production UI must bind to current project state and existing services. No example data from the images may be hard-coded.

## 5. Modern WPF implementation rule

Interactive controls must be implemented as WPF controls with project-owned `Style`, `ControlTemplate`, vector geometry, dynamic resources, focus state, and automation metadata. Raster images and image-generation tools must not be used to draw buttons, tabs, inputs, grids, menus, scrollbars, status indicators, or other interactive components.

Existing icon resources are reused first. Missing simple icons may be added as project-owned vector `PathGeometry` after contract review. No third-party UI or icon dependency is authorized by this contract.

## 6. Visual acceptance hierarchy

When evidence conflicts, use this order:

1. approved semantic and lifecycle contracts;
2. M0D WPF/DIP dimensional specification;
3. approved real WPF screenshots from the latest visual gate;
4. frozen design-intent images in this document;
5. local implementation preference.

The generated references guide visual direction, while the approved real WPF screenshots become the regression baseline after each visual stage.

## 7. Non-goals and protected behavior

UI-MODERN-1 does not authorize changes to parser semantics, Field Registry load/apply/rollback/import/learning semantics, provider priority, completion, Hover, Quick Peek data semantics, diagnostics, Save Preflight, backup, Undo/Redo, BuiltIn definitions, AI streaming/reliability/model behavior, or legacy projects.

## 8. Next mandatory entry

M0B captures the current real WPF Shell, Search placeholder, and Field Registry surfaces. M0C inventories control-template gaps, hard-coded presentation values, DPI risks, and AutomationIds. M0D then produces WPF/DIP dimension drawings and stops for user visual review before any production XAML change.
