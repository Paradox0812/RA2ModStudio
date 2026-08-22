using System.Windows;
using AvalonDock;
using AvalonDock.Layout;

namespace RA2IniEditor.IDE.Views;

internal enum ShellDockHomeZone
{
    Bottom,
    Right,
    Floating
}

/// <summary>描述托管工具的稳定身份和编译期默认位置。</summary>
internal readonly record struct ShellDockToolProfile(
    string ContentId,
    ShellDockHomeZone HomeZone,
    int DefaultOrder,
    bool DefaultVisible,
    double PreferredFloatingWidth,
    double PreferredFloatingHeight);

/// <summary>管理当前 AvalonDock 模型图上的工具交互，不持有可被反序列化替换的模型引用。</summary>
internal sealed class ShellDockLayoutCoordinator
{
    private const double FloatingViewportInset = 32.0;
    private const double FloatingTopOffset = 56.0;
    private const double MinimumFloatingExtent = 420.0;

    private readonly DockingManager _manager;
    private readonly Func<Rect> _viewportBoundsProvider;
    private readonly ShellDockToolProfile[] _profiles;
    private readonly Dictionary<string, ShellDockToolProfile> _profilesByContentId;
    private readonly HashSet<LayoutAnchorable> _pendingFloatingHideRecovery = [];
    private bool _isApplyingLayout;
    private bool _isShellClosing;

    public ShellDockLayoutCoordinator(
        DockingManager manager,
        Func<Size> viewportSizeProvider,
        IEnumerable<ShellDockToolProfile> profiles)
        : this(
            manager,
            () => new Rect(new Point(), viewportSizeProvider()),
            profiles)
    {
        ArgumentNullException.ThrowIfNull(viewportSizeProvider);
    }

    public ShellDockLayoutCoordinator(
        DockingManager manager,
        Func<Rect> viewportBoundsProvider,
        IEnumerable<ShellDockToolProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(viewportBoundsProvider);
        ArgumentNullException.ThrowIfNull(profiles);

        _manager = manager;
        _viewportBoundsProvider = viewportBoundsProvider;
        _profiles = profiles.OrderBy(profile => profile.HomeZone).ThenBy(profile => profile.DefaultOrder).ToArray();
        _profilesByContentId = new Dictionary<string, ShellDockToolProfile>(StringComparer.Ordinal);
        foreach (ShellDockToolProfile profile in _profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.ContentId))
                throw new ArgumentException("Managed dock tools require a stable ContentId.", nameof(profiles));
            if (!_profilesByContentId.TryAdd(profile.ContentId, profile))
                throw new ArgumentException($"Duplicate managed ContentId '{profile.ContentId}'.", nameof(profiles));
        }
    }

    public IEnumerable<ShellDockToolProfile> Profiles => _profiles;

    public void ApplyInitialFloatingGeometry()
        => ApplyPreferredFloatingGeometry();

    public void ApplyCompiledDefaultTopology()
    {
        ExecuteLayoutUpdate(() =>
        {
            foreach (ShellDockToolProfile profile in _profiles)
            {
                LayoutAnchorable? tool = FindTool(profile.ContentId);
                if (tool is null)
                    continue;

                PlaceAtHome(profile, tool);
                if (profile.DefaultVisible)
                    EnsureVisible(tool);
            }
        });
    }

    public void ApplyCompiledDefaultVisibility()
    {
        foreach (ShellDockToolProfile profile in _profiles)
            ApplyToolCompiledDefaultVisibility(profile.ContentId);
    }

    public void BeginShellClose()
    {
        _isShellClosing = true;
        _pendingFloatingHideRecovery.Clear();
    }

    public void CancelShellClose()
        => _isShellClosing = false;

    public LayoutAnchorable? FindTool(string contentId)
        => _manager.Layout.Descendents()
            .OfType<LayoutAnchorable>()
            .SingleOrDefault(tool => string.Equals(tool.ContentId, contentId, StringComparison.Ordinal));

    public LayoutAnchorable[] GetTools(ShellDockHomeZone homeZone)
        => _profiles
            .Where(profile => profile.HomeZone == homeZone)
            .Select(profile => FindTool(profile.ContentId))
            .Where(tool => tool is not null)
            .Cast<LayoutAnchorable>()
            .ToArray();

    public bool TryBeginFloatingHideRecovery(LayoutAnchorable tool)
    {
        if (_isShellClosing || _isApplyingLayout || !tool.IsFloating ||
            !TryGetProfile(tool, out ShellDockToolProfile profile) ||
            profile.HomeZone == ShellDockHomeZone.Floating)
            return false;

        return _pendingFloatingHideRecovery.Add(tool);
    }

    public void PlaceToolAtCompiledDefaultHome(string contentId)
    {
        if (!_profilesByContentId.TryGetValue(contentId, out ShellDockToolProfile profile) ||
            FindTool(contentId) is not { } tool)
            return;

        ExecuteLayoutUpdate(() => PlaceAtHome(profile, tool));
    }

    public void ApplyToolCompiledDefaultVisibility(string contentId)
    {
        if (!_profilesByContentId.TryGetValue(contentId, out ShellDockToolProfile profile) ||
            FindTool(contentId) is not { } tool)
            return;

        if (profile.DefaultVisible)
            EnsureVisible(tool);
        else if (tool.IsVisible)
            tool.Hide();
    }

    public void ReturnToolHome(LayoutAnchorable tool, bool activate)
    {
        try
        {
            if (!TryGetProfile(tool, out ShellDockToolProfile profile))
                return;

            ExecuteLayoutUpdate(() =>
            {
                PlaceAtHome(profile, tool);
                EnsureVisible(tool);
                if (activate)
                    Activate(tool);
            });
        }
        finally
        {
            _pendingFloatingHideRecovery.Remove(tool);
        }
    }

    public void ReturnFloatingToolsHome()
    {
        LayoutAnchorable[] floatingTools = _profiles
            .Select(profile => FindTool(profile.ContentId))
            .Where(tool => tool is { IsFloating: true })
            .Cast<LayoutAnchorable>()
            .ToArray();
        LayoutAnchorable? activeTool = floatingTools.FirstOrDefault(tool => tool.IsActive);

        ExecuteLayoutUpdate(() =>
        {
            foreach (LayoutAnchorable tool in floatingTools)
            {
                ShellDockToolProfile profile = _profilesByContentId[tool.ContentId];
                if (profile.HomeZone == ShellDockHomeZone.Floating)
                    RecoverFloatingHomeBoundsIfNeeded(profile, tool);
                else
                    PlaceAtHome(profile, tool);
                EnsureVisible(tool);
            }

            if (activeTool is not null)
                Activate(activeTool);
        });
    }

    public void ShowAndActivate(string contentId)
    {
        if (!_profilesByContentId.TryGetValue(contentId, out ShellDockToolProfile profile))
            return;

        LayoutAnchorable? tool = FindTool(contentId);
        if (tool is null)
            return;

        if (!tool.IsVisible)
            tool.Show();

        if (!tool.IsVisible || tool.Parent is not LayoutAnchorablePane)
        {
            ExecuteLayoutUpdate(() =>
            {
                PlaceAtHome(profile, tool);
                EnsureVisible(tool);
            });
        }

        Activate(tool);
    }

    public bool RecoverFloatingGeometry(ShellMonitorWorkAreaProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ShellMonitorWorkAreaSnapshot snapshot = provider.GetCurrentSnapshot();
        bool usedFallback = false;
        foreach (ShellDockToolProfile profile in _profiles)
        {
            LayoutAnchorable? tool = FindTool(profile.ContentId);
            if (tool is not { IsFloating: true })
                continue;

            ShellDockGeometryRecoveryResult recovery = ShellMonitorWorkAreaProvider.RecoverGeometry(
                new Rect(tool.FloatingLeft, tool.FloatingTop, tool.FloatingWidth, tool.FloatingHeight),
                new Size(profile.PreferredFloatingWidth, profile.PreferredFloatingHeight),
                snapshot);
            tool.FloatingLeft = recovery.Bounds.Left;
            tool.FloatingTop = recovery.Bounds.Top;
            tool.FloatingWidth = recovery.Bounds.Width;
            tool.FloatingHeight = recovery.Bounds.Height;
            usedFallback |= recovery.UsedFallback;
        }

        return usedFallback;
    }

    private void ExecuteLayoutUpdate(Action update)
    {
        if (_isApplyingLayout || _isShellClosing)
            return;

        _isApplyingLayout = true;
        try
        {
            update();
            _manager.Layout.CollectGarbage();
        }
        finally
        {
            _isApplyingLayout = false;
        }
    }

    private void PlaceAtHome(ShellDockToolProfile profile, LayoutAnchorable tool)
    {
        if (tool.IsAutoHidden)
            tool.ToggleAutoHide();

        if (profile.HomeZone == ShellDockHomeZone.Floating)
        {
            ApplyDefaultFloatingBounds(profile, tool);
            if (!tool.IsFloating)
                tool.Float();
            return;
        }

        LayoutAnchorablePane? homePane = FindHomePane(profile.HomeZone, tool);
        if (homePane is null)
        {
            tool.Parent?.RemoveChild(tool);
            tool.AddToLayout(
                _manager,
                profile.HomeZone == ShellDockHomeZone.Bottom
                    ? AnchorableShowStrategy.Bottom
                    : AnchorableShowStrategy.Right);
            homePane = tool.Parent as LayoutAnchorablePane
                ?? throw new InvalidOperationException($"AvalonDock did not create a Home pane for '{profile.ContentId}'.");
        }

        int insertionIndex = _profiles.Count(candidate =>
            candidate.HomeZone == profile.HomeZone &&
            candidate.DefaultOrder < profile.DefaultOrder &&
            FindTool(candidate.ContentId)?.Parent == homePane);

        if (ReferenceEquals(tool.Parent, homePane))
        {
            int currentIndex = homePane.IndexOfChild(tool);
            if (currentIndex == insertionIndex)
                return;
            homePane.RemoveChildAt(currentIndex);
        }
        else
        {
            tool.Parent?.RemoveChild(tool);
        }

        homePane.InsertChildAt(Math.Min(insertionIndex, homePane.ChildrenCount), tool);
    }

    private LayoutAnchorablePane? FindHomePane(ShellDockHomeZone homeZone, LayoutAnchorable movingTool)
    {
        if (homeZone == ShellDockHomeZone.Floating)
            return null;

        foreach (ShellDockToolProfile candidate in _profiles.Where(profile => profile.HomeZone == homeZone))
        {
            LayoutAnchorable? tool = FindTool(candidate.ContentId);
            if (tool is null || ReferenceEquals(tool, movingTool) || tool.IsFloating)
                continue;
            if (tool.Parent is LayoutAnchorablePane pane)
                return pane;
        }

        return !movingTool.IsFloating ? movingTool.Parent as LayoutAnchorablePane : null;
    }

    private void ApplyPreferredFloatingGeometry()
    {
        Size viewport = _viewportBoundsProvider().Size;
        foreach (ShellDockToolProfile profile in _profiles)
        {
            LayoutAnchorable? tool = FindTool(profile.ContentId);
            if (tool is null)
                continue;
            tool.FloatingWidth = ClampPreferredSize(profile.PreferredFloatingWidth, viewport.Width);
            tool.FloatingHeight = ClampPreferredSize(profile.PreferredFloatingHeight, viewport.Height);
        }
    }

    private void ApplyDefaultFloatingBounds(ShellDockToolProfile profile, LayoutAnchorable tool)
    {
        Rect viewport = _viewportBoundsProvider();
        double width = ClampPreferredSize(profile.PreferredFloatingWidth, viewport.Width);
        double height = ClampPreferredSize(profile.PreferredFloatingHeight, viewport.Height);
        tool.FloatingWidth = width;
        tool.FloatingHeight = height;

        if (!double.IsFinite(viewport.Width) || !double.IsFinite(viewport.Height) ||
            viewport.Width <= FloatingViewportInset || viewport.Height <= FloatingViewportInset)
            return;

        double minimumLeft = viewport.Left + FloatingViewportInset;
        double minimumTop = viewport.Top + FloatingViewportInset;
        double maximumLeft = Math.Max(minimumLeft, viewport.Right - width - FloatingViewportInset);
        double maximumTop = Math.Max(minimumTop, viewport.Bottom - height - FloatingViewportInset);
        tool.FloatingLeft = Math.Clamp(viewport.Left + ((viewport.Width - width) / 2.0), minimumLeft, maximumLeft);
        tool.FloatingTop = Math.Clamp(viewport.Top + FloatingTopOffset, minimumTop, maximumTop);
    }

    private void RecoverFloatingHomeBoundsIfNeeded(ShellDockToolProfile profile, LayoutAnchorable tool)
    {
        Rect viewport = _viewportBoundsProvider();
        Rect bounds = new(tool.FloatingLeft, tool.FloatingTop, tool.FloatingWidth, tool.FloatingHeight);
        bool hasFiniteUsableBounds = double.IsFinite(bounds.Left) &&
                                     double.IsFinite(bounds.Top) &&
                                     double.IsFinite(bounds.Width) &&
                                     double.IsFinite(bounds.Height) &&
                                     bounds.Width >= MinimumFloatingExtent &&
                                     bounds.Height >= MinimumFloatingExtent;
        bool isReachable = double.IsFinite(viewport.Width) &&
                           double.IsFinite(viewport.Height) &&
                           viewport.Width > 0 &&
                           viewport.Height > 0 &&
                           bounds.Left >= viewport.Left &&
                           bounds.Top >= viewport.Top &&
                           bounds.Right <= viewport.Right &&
                           bounds.Bottom <= viewport.Bottom;
        if (!hasFiniteUsableBounds || !isReachable)
            ApplyDefaultFloatingBounds(profile, tool);
    }

    private static double ClampPreferredSize(double preferred, double available)
    {
        if (!double.IsFinite(available) || available <= FloatingViewportInset)
            return preferred;
        double maximum = Math.Max(1.0, available - (FloatingViewportInset * 2.0));
        double minimum = Math.Min(MinimumFloatingExtent, maximum);
        return Math.Clamp(preferred, minimum, maximum);
    }

    private bool TryGetProfile(LayoutAnchorable tool, out ShellDockToolProfile profile)
        => _profilesByContentId.TryGetValue(tool.ContentId, out profile) && ReferenceEquals(FindTool(tool.ContentId), tool);

    private static void EnsureVisible(LayoutAnchorable tool)
    {
        if (!tool.IsVisible)
            tool.Show();
    }

    private static void Activate(LayoutAnchorable tool)
    {
        tool.IsSelected = true;
        tool.IsActive = true;
    }
}
