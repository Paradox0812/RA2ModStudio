using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace RA2IniEditor.IDE.Views;

/// <summary>
/// 为 AvalonDock 浮动宿主应用项目级外框，同时保留停靠模型与关闭恢复语义。
/// </summary>
internal sealed class ShellDockFloatingChromeController : IDisposable
{
    private const string FloatingWindowStyleKey = "IdeDockFloatingWindowStyle";
    private const string DragRegionPartName = "PART_FloatingDragRegion";
    private const string ContentHostPartName = "PART_FloatingContentHost";
    private const string MinimizeButtonPartName = "PART_FloatingMinimizeButton";
    private const string CloseButtonPartName = "PART_FloatingCloseButton";
    private const string TitleTextPartName = "PART_FloatingTitleText";

    private readonly DockingManager _manager;
    private readonly Dictionary<LayoutFloatingWindowControl, HostRegistration> _registrations = [];
    private readonly Dictionary<LayoutFloatingWindowControl, double> _initialLayoutSuppressedHostOpacities = [];
    private bool _isAttached;
    private bool _isInitialLayoutVisibilitySuppressed;

    internal ShellDockFloatingChromeController(DockingManager manager)
        => _manager = manager ?? throw new ArgumentNullException(nameof(manager));

    internal void Attach()
    {
        if (_isAttached)
            return;

        _isAttached = true;
        _manager.LayoutFloatingWindowControlCreated += Manager_OnFloatingWindowCreated;
        _manager.LayoutFloatingWindowControlClosed += Manager_OnFloatingWindowClosed;
    }

    /// <summary>
    /// 在 Shell 建立默认拓扑并恢复持久化布局时，阻止中间态浮动宿主被绘制。
    /// </summary>
    internal void BeginInitialLayoutVisibilitySuppression()
    {
        if (_isInitialLayoutVisibilitySuppressed)
            return;

        _isInitialLayoutVisibilitySuppressed = true;
        foreach (LayoutFloatingWindowControl host in _registrations.Keys.ToArray())
            SuppressInitialLayoutHost(host);
    }

    /// <summary>
    /// 恢复启动布局期间仍存活的浮动宿主原始透明度。
    /// </summary>
    internal void CompleteInitialLayoutVisibilitySuppression()
    {
        if (!_isInitialLayoutVisibilitySuppressed && _initialLayoutSuppressedHostOpacities.Count == 0)
            return;

        _isInitialLayoutVisibilitySuppressed = false;
        foreach ((LayoutFloatingWindowControl host, double opacity) in _initialLayoutSuppressedHostOpacities.ToArray())
        {
            if (_registrations.ContainsKey(host))
                host.SetCurrentValue(UIElement.OpacityProperty, opacity);
        }

        _initialLayoutSuppressedHostOpacities.Clear();
    }

    /// <summary>
    /// 为布局反序列化期间已创建、但未触发创建事件的浮动宿主补装项目级外框。
    /// </summary>
    internal void RefreshExistingHosts()
    {
        if (!_isAttached || Application.Current is null)
            return;

        foreach (LayoutFloatingWindowControl host in Application.Current.Windows.OfType<LayoutFloatingWindowControl>())
            RegisterHost(host);
    }

    /// <summary>
    /// 恢复承载指定 Dock 内容的已最小化浮动宿主，并在布局稳定后依次激活宿主与内容焦点。
    /// </summary>
    internal bool RestoreAndActivateMinimizedHost(string contentId, Action focusContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentId);
        ArgumentNullException.ThrowIfNull(focusContent);

        LayoutFloatingWindowControl? host = _registrations.Keys.FirstOrDefault(candidate =>
            candidate.Model.Descendents()
                .OfType<LayoutContent>()
                .Any(content => string.Equals(content.ContentId, contentId, StringComparison.Ordinal)));
        if (host is null || host.WindowState != WindowState.Minimized)
            return false;

        SystemCommands.RestoreWindow(host);
        _ = host.Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() =>
            {
                if (!_registrations.ContainsKey(host) || !host.IsVisible)
                    return;

                host.Activate();
                focusContent();
            }));
        return true;
    }

    public void Dispose()
    {
        if (!_isAttached)
            return;

        CompleteInitialLayoutVisibilitySuppression();
        _manager.LayoutFloatingWindowControlCreated -= Manager_OnFloatingWindowCreated;
        _manager.LayoutFloatingWindowControlClosed -= Manager_OnFloatingWindowClosed;
        foreach (LayoutFloatingWindowControl host in _registrations.Keys.ToArray())
            DetachHost(host);
        _isAttached = false;
    }

    private void Manager_OnFloatingWindowCreated(object? sender, LayoutFloatingWindowControlCreatedEventArgs e)
        => RegisterHost(e.LayoutFloatingWindowControl);

    private void RegisterHost(LayoutFloatingWindowControl host)
    {
        if (_registrations.ContainsKey(host))
            return;

        if (_manager.TryFindResource(FloatingWindowStyleKey) is Style hostStyle)
            host.Style = hostStyle;

        RoutedEventHandler loadedHandler = (_, _) => ConfigureLoadedHost(host);
        _registrations.Add(host, new HostRegistration(loadedHandler));
        if (_isInitialLayoutVisibilitySuppressed)
            SuppressInitialLayoutHost(host);
        host.Loaded += loadedHandler;
        if (host.IsLoaded)
            ConfigureLoadedHost(host);
    }

    private void Manager_OnFloatingWindowClosed(object? sender, LayoutFloatingWindowControlClosedEventArgs e)
        => DetachHost(e.LayoutFloatingWindowControl);

    private void ConfigureLoadedHost(LayoutFloatingWindowControl host)
    {
        if (!_registrations.TryGetValue(host, out HostRegistration? registration) || registration.ChromeController is not null)
            return;

        host.ApplyTemplate();
        FrameworkElement? dragRegion = host.Template.FindName(DragRegionPartName, host) as FrameworkElement;
        FrameworkElement? contentHost = host.Template.FindName(ContentHostPartName, host) as FrameworkElement;
        Button? minimizeButton = host.Template.FindName(MinimizeButtonPartName, host) as Button;
        Button? closeButton = host.Template.FindName(CloseButtonPartName, host) as Button;
        TextBlock? titleText = host.Template.FindName(TitleTextPartName, host) as TextBlock;
        if (dragRegion is null || contentHost is null || minimizeButton is null || closeButton is null)
            return;

        host.UpdateLayout();
        LayoutAnchorable[] anchorables = host.Model.Descendents().OfType<LayoutAnchorable>().ToArray();
        bool isSinglePane = host is LayoutAnchorableFloatingWindowControl && anchorables.Length == 1;
        if (isSinglePane && titleText is not null)
        {
            titleText.Text = anchorables[0].Title;
            titleText.Visibility = Visibility.Visible;
        }
        else if (titleText is not null)
        {
            titleText.Visibility = Visibility.Collapsed;
        }

        RoutedEventHandler closeHandler = (_, eventArgs) =>
        {
            eventArgs.Handled = true;
            foreach (LayoutAnchorable anchorable in anchorables)
            {
                if (anchorable.IsVisible)
                    anchorable.Hide();
            }
        };
        RoutedEventHandler minimizeHandler = (_, _) => SystemCommands.MinimizeWindow(host);
        minimizeButton.Click += minimizeHandler;
        closeButton.Click += closeHandler;

        ShellWindowChromeController chromeController = new(host, dragRegion, maximizeRegion: null);
        chromeController.Attach();
        registration.ChromeController = chromeController;
        registration.MinimizeButton = minimizeButton;
        registration.MinimizeHandler = minimizeHandler;
        registration.CloseButton = closeButton;
        registration.CloseHandler = closeHandler;
    }

    private void DetachHost(LayoutFloatingWindowControl host)
    {
        if (!_registrations.Remove(host, out HostRegistration? registration))
            return;

        _initialLayoutSuppressedHostOpacities.Remove(host);
        host.Loaded -= registration.LoadedHandler;
        if (registration.MinimizeButton is not null && registration.MinimizeHandler is not null)
            registration.MinimizeButton.Click -= registration.MinimizeHandler;
        if (registration.CloseButton is not null && registration.CloseHandler is not null)
            registration.CloseButton.Click -= registration.CloseHandler;
        registration.ChromeController?.Dispose();
    }

    private void SuppressInitialLayoutHost(LayoutFloatingWindowControl host)
    {
        if (!_initialLayoutSuppressedHostOpacities.TryAdd(host, host.Opacity))
            return;

        host.SetCurrentValue(UIElement.OpacityProperty, 0.0);
    }

    private sealed class HostRegistration(RoutedEventHandler loadedHandler)
    {
        internal RoutedEventHandler LoadedHandler { get; } = loadedHandler;
        internal ShellWindowChromeController? ChromeController { get; set; }
        internal Button? MinimizeButton { get; set; }
        internal RoutedEventHandler? MinimizeHandler { get; set; }
        internal Button? CloseButton { get; set; }
        internal RoutedEventHandler? CloseHandler { get; set; }
    }
}
