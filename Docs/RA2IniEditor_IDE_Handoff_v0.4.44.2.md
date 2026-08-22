# RA2IniEditor v0.4.44.2 Handoff

## Scope

This slice adds Chinese user-facing text for the IDE shell and Add Property flow, plus an independent Field Annotation Editor window.

## Main Changes

- Shell toolbar and source editor context menu now use Chinese labels for the main user path.
- Add Property window is localized for title, columns, status, hints, duplicate warnings, and buttons.
- Add Property now exposes `编辑字段注释...` for the currently selected field.
- Field Annotation Editor is a separate window under `Views/FieldAnnotations`.
- Annotation editing supports display name, aliases, and note.
- Annotation sidecar path is `.ra2ide/field-annotations.zh-CN.json`.

## Save Boundary

- Annotation save writes only the annotation sidecar JSON.
- Annotation save does not write INI files.
- Annotation save does not call `ProjectSaveService`.
- Annotation save does not call legacy `IniFileService`.
- Annotation dirty state is local to the annotation editor and separate from the in-memory INI editor state.
- Add Property and completion still insert raw keys such as `Strength`, never Chinese display labels.

## Refresh Behavior

After annotation save:

- Add Property reloads the annotation pack.
- Add Property rebuilds its display resolver and refreshes visible rows.
- Hover and completion use the refreshed sidecar the next time their existing code paths build a display resolver.

## Verification

- `dotnet test -c Release`: 734 passed.
- `dotnet build -c Release --no-incremental`: passed with 26 existing warnings.
- UIA was not run for this slice.

## Manual Smoke Checklist

1. Open IDE and confirm shell buttons show Chinese labels.
2. Open source editor context menu and confirm `添加属性...`, `转到定义`, `查看定义`, `查找全部引用`.
3. Open Add Property and confirm Chinese title, columns, hints, and buttons.
4. Select `Strength`, open `编辑字段注释...`, enter display name / aliases / note, and save.
5. Confirm `.ra2ide/field-annotations.zh-CN.json` is created or updated.
6. Confirm the opened INI text is unchanged and in-memory edit state is not cleared.
7. Reopen Add Property and confirm the selected field display refreshes while insertion still uses raw key.
