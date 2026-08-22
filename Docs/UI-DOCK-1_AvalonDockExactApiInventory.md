# UI-DOCK-1 AvalonDock 4.74.1 Exact API Inventory

Source of truth: the restored package at `%USERPROFILE%\.nuget\packages\dirkster.avalondock\4.74.1` and reflection over its `net48` assembly, whose public API matches the package's `net5.0-windows7.0` asset selected by the `net8.0-windows` application. Assembly identity: `AvalonDock, Version=4.74.1.0, PublicKeyToken=3e4669d2f30244f4`.

This inventory prevents accidental use of the incompatible Xceed namespace or AvalonDock 5.x preview APIs.

## XAML namespace

```xml
xmlns:avalondock="https://github.com/Dirkster99/AvalonDock"
```

The CLR namespaces used by this package are `AvalonDock`, `AvalonDock.Controls`, `AvalonDock.Layout`, `AvalonDock.Layout.Serialization`, and `AvalonDock.Themes`. Do not use `Xceed.Wpf.AvalonDock`.

## DockingManager

```csharp
namespace AvalonDock;

public class DockingManager
{
    public DockingManager();
    public LayoutRoot Layout { get; set; }
    public object ActiveContent { get; set; }
    public double GridSplitterWidth { get; set; }
    public double GridSplitterHeight { get; set; }
    public bool AllowMixedOrientation { get; set; }
    public bool IsVirtualizingAnchorable { get; set; }
    public bool IsVirtualizingDocument { get; set; }
    public Theme Theme { get; set; }
    public LayoutItem GetLayoutItemFromModel(LayoutContent model);

    public event EventHandler LayoutChanging;
    public event EventHandler LayoutChanged;
    public event EventHandler<AnchorableHidingEventArgs> AnchorableHiding;
    public event EventHandler<AnchorableHiddenEventArgs> AnchorableHidden;
    public event EventHandler<ContentFloatingEventArgs> ContentFloating;
    public event EventHandler<ContentFloatedEventArgs> ContentFloated;
    public event EventHandler<ContentDockingEventArgs> ContentDocking;
    public event EventHandler<ContentDockedEventArgs> ContentDocked;
}

public class AnchorableHidingEventArgs : CancelEventArgs
{
    public LayoutAnchorable Anchorable { get; set; }
    public bool CloseInsteadOfHide { get; set; }
}
```

## Layout tree

```csharp
namespace AvalonDock.Layout;

public class LayoutRoot
{
    public LayoutRoot();
    public LayoutPanel RootPanel { get; set; }
    public LayoutContent ActiveContent { get; set; }
    public ObservableCollection<LayoutAnchorable> Hidden { get; }
    public ObservableCollection<LayoutFloatingWindow> FloatingWindows { get; }
    public void CollectGarbage();
}

public interface ILayoutContainer
{
    IEnumerable<ILayoutElement> Children { get; }
    int ChildrenCount { get; }
    void RemoveChild(ILayoutElement element);
    void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);
}

public interface ILayoutGroup : ILayoutContainer
{
    int IndexOfChild(ILayoutElement element);
    void InsertChildAt(int index, ILayoutElement element);
    void RemoveChildAt(int index);
    void ReplaceChildAt(int index, ILayoutElement element);
}

public class LayoutPanel
{
    public LayoutPanel();
    public LayoutPanel(ILayoutPanelElement child);
    public Orientation Orientation { get; set; }
    public bool CanDock { get; set; }
}

public class LayoutDocumentPane
{
    public LayoutDocumentPane();
    public LayoutDocumentPane(LayoutContent child);
    public int SelectedContentIndex { get; set; }
    public bool ShowHeader { get; set; }
}

public class LayoutAnchorablePane
{
    public LayoutAnchorablePane();
    public LayoutAnchorablePane(LayoutAnchorable child);
    public int SelectedContentIndex { get; set; }
    public string Name { get; set; }
}

public class LayoutDocumentPaneGroup
{
    public LayoutDocumentPaneGroup();
    public LayoutDocumentPaneGroup(LayoutDocumentPane child);
    public Orientation Orientation { get; set; }
}

public class LayoutAnchorablePaneGroup
{
    public LayoutAnchorablePaneGroup();
    public LayoutAnchorablePaneGroup(LayoutAnchorablePane child);
    public Orientation Orientation { get; set; }
}
```

Positionable layout groups expose writable `GridLength DockWidth`, `GridLength DockHeight`, `double DockMinWidth`, and `double DockMinHeight`. These are the supported geometry inputs for the 300-DIP right group and 260-DIP bottom group.

## Document and tool content

`LayoutDocument` and `LayoutAnchorable` inherit these writable members from `LayoutContent`:

```csharp
public object Content { get; set; }
public string ContentId { get; set; }
public string Title { get; set; }
public bool CanClose { get; set; }
public bool CanFloat { get; set; }
public bool IsActive { get; set; }
public bool IsSelected { get; set; }
public ILayoutContainer Parent { get; set; }
public double FloatingWidth { get; set; }
public double FloatingHeight { get; set; }
public double FloatingLeft { get; set; }
public double FloatingTop { get; set; }
public void Dock();
public void DockAsDocument();
public void Float();
```

Additional exact members:

```csharp
public class LayoutDocument
{
    public LayoutDocument();
    public bool CanMove { get; set; }
    public bool IsVisible { get; set; }
    public void Close();
}

public class LayoutAnchorable
{
    public LayoutAnchorable();
    public bool CanAutoHide { get; set; }
    public bool CanDockAsTabbedDocument { get; set; }
    public bool CanHide { get; set; }
    public bool CanMove { get; set; }
    public bool IsHidden { get; }
    public bool IsVisible { get; set; }
    public bool IsFloating { get; }
    public bool IsAutoHidden { get; }
    public void Show();
    public void Hide();
    public void ToggleAutoHide();
    public void AddToLayout(DockingManager manager, AnchorableShowStrategy strategy);
}

[Flags]
public enum AnchorableShowStrategy
{
    Most = 1,
    Left = 2,
    Right = 4,
    Top = 16,
    Bottom = 32
}
```

## XML layout serialization

```csharp
namespace AvalonDock.Layout.Serialization;

public class XmlLayoutSerializer : LayoutSerializer
{
    public XmlLayoutSerializer(DockingManager manager);
    public event EventHandler<LayoutSerializationCallbackEventArgs> LayoutSerializationCallback;
    public void Serialize(string filePath);
    public void Serialize(Stream stream);
    public void Serialize(TextWriter writer);
    public void Serialize(XmlWriter writer);
    public void Deserialize(string filePath);
    public void Deserialize(Stream stream);
    public void Deserialize(TextReader reader);
    public void Deserialize(XmlReader reader);
}

public class LayoutSerializationCallbackEventArgs : CancelEventArgs
{
    public LayoutContent Model { get; set; }
    public object Content { get; set; }
}
```

Reflection and the pinned 4.74.1 source confirm that deserialization creates a new `LayoutRoot`, assigns it to `DockingManager.Layout`, and reconnects callback content to new `LayoutDocument` / `LayoutAnchorable` models. Constructor-time model and Pane references therefore become stale after restore.

`LayoutAnchorablePane` exposes `Name`, but Pane/Group identity is not an approved persistence authority: user rearrangement may create unnamed Panes and empty Home Panes may be garbage-collected. UI-DOCK-4 must use current-layout traversal by ContentId and `LayoutAnchorable.AddToLayout(DockingManager, AnchorableShowStrategy)` as the no-Home fallback.

Serialization remains forbidden until the user approves `Docs/UI-DOCK-4_LayoutPersistenceContract.md`. When enabled, callback content resolution must use the approved ContentId allow-list and must not instantiate arbitrary types from layout data.

## Styling controls confirmed in 4.74.1

```text
AvalonDock.Controls.LayoutDocumentPaneControl
AvalonDock.Controls.LayoutAnchorablePaneControl
AvalonDock.Controls.LayoutDocumentTabItem
AvalonDock.Controls.LayoutAnchorableTabItem
```

`DockingManager` exposes project-owned `DocumentHeaderTemplate`, `DocumentTitleTemplate`, `AnchorableHeaderTemplate`, `AnchorableTitleTemplate`, pane styles, item styles, splitter styles, and a `DictionaryTheme` hook. UI-DOCK-2 may use header templates for stable automation anchors; UI-DOCK-3 owns complete visual templating.

## Forbidden API / guessing list

- Do not use any `Xceed.Wpf.AvalonDock.*` namespace.
- Do not use 5.x `AvalonDock.Core`, MVVM, DI, JSON serializer, or modular theme APIs.
- Do not assume `LayoutAnchorablePane.CanClose` or `CanHide` is writable; configure the contained `LayoutAnchorable` instead.
- Do not assign `IsFloating`; use `Float()` and `Dock()`.
- Do not create tool content during deserialization; resolve only existing Shell-owned instances.
- Do not persist or restore layout in UI-DOCK-2.
