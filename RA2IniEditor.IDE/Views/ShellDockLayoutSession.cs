using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Data;
using System.Xml.Linq;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace RA2IniEditor.IDE.Views;

internal enum ShellDockLayoutFailureKind
{
    None,
    NotFound,
    UnsupportedVersion,
    TooLarge,
    UnsafeXml,
    DefaultNotCaptured,
    InvalidContentIdentity,
    InvalidLayoutInvariant,
    IoFailure,
    SerializerFailure
}

internal readonly record struct ShellDockLayoutOperationResult(bool Succeeded, ShellDockLayoutFailureKind FailureKind)
{
    public static ShellDockLayoutOperationResult Success => new(true, ShellDockLayoutFailureKind.None);
}

/// <summary>拥有 Shell 会话内的内容目录、编译默认快照和模型重绑定流程。</summary>
internal sealed class ShellDockLayoutSession
{
    private readonly DockingManager _manager;
    private readonly Dictionary<string, ContentRegistration> _registrations;
    private readonly Dictionary<ShellDockHomeZone, HomeAutomationRegistration> _homeAutomation;
    private readonly Dictionary<ShellDockHomeZone, string[]> _homeContentIds;
    private string? _compiledDefaultLayout;

    public ShellDockLayoutSession(
        DockingManager manager,
        IEnumerable<LayoutContent> managedContents,
        IEnumerable<ShellDockToolProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(managedContents);
        ArgumentNullException.ThrowIfNull(profiles);
        _manager = manager;
        _registrations = managedContents
            .Select(ContentRegistration.Capture)
            .ToDictionary(registration => registration.ContentId, StringComparer.Ordinal);
        ShellDockToolProfile[] profileSnapshot = profiles.ToArray();
        _homeContentIds = profileSnapshot
            .Where(profile => profile.HomeZone != ShellDockHomeZone.Floating)
            .GroupBy(profile => profile.HomeZone)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(profile => profile.DefaultOrder).Select(profile => profile.ContentId).ToArray());
        _homeAutomation = CaptureHomeAutomation();
    }

    public ShellDockLayoutOperationResult CaptureCompiledDefault()
    {
        ShellDockLayoutOperationResult result = TrySerializeCurrent(out string? serialized);
        if (result.Succeeded)
            _compiledDefaultLayout = serialized;
        return result;
    }

    public ShellDockLayoutOperationResult ResetToCompiledDefault()
        => _compiledDefaultLayout is null
            ? new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.DefaultNotCaptured)
            : TryRestore(_compiledDefaultLayout);

    public ShellDockLayoutOperationResult TrySerializeCurrent(out string? serialized)
    {
        serialized = null;
        if (!TryGetValidatedContents(out _))
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.InvalidLayoutInvariant);

        try
        {
            using StringWriter writer = new Utf8StringWriter();
            new XmlLayoutSerializer(_manager).Serialize(writer);
            serialized = writer.ToString();
            return ShellDockLayoutOperationResult.Success;
        }
        catch
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.SerializerFailure);
        }
    }

    public ShellDockLayoutOperationResult TryRestore(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.SerializerFailure);

        ShellDockLayoutOperationResult preflight = PreflightContentIdentity(serialized);
        if (!preflight.Succeeded)
            return preflight;

        bool invalidIdentity = false;
        try
        {
            XmlLayoutSerializer serializer = new(_manager);
            serializer.LayoutSerializationCallback += (_, args) =>
            {
                if (!_registrations.TryGetValue(args.Model.ContentId, out ContentRegistration? registration) ||
                    !registration.Accepts(args.Model))
                {
                    invalidIdentity = true;
                    args.Cancel = true;
                    return;
                }

                args.Content = registration.Content;
            };

            using StringReader reader = new(serialized);
            serializer.Deserialize(reader);
        }
        catch
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.SerializerFailure);
        }

        if (invalidIdentity || !TryGetValidatedContents(out Dictionary<string, LayoutContent>? contents))
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.InvalidContentIdentity);

        foreach ((string contentId, LayoutContent model) in contents!)
            _registrations[contentId].ApplyTo(model);
        ApplyHomeAutomation(contents);
        return ShellDockLayoutOperationResult.Success;
    }

    public LayoutContent? FindContent(string contentId)
        => _manager.Layout.Descendents()
            .OfType<LayoutContent>()
            .SingleOrDefault(content => string.Equals(content.ContentId, contentId, StringComparison.Ordinal));

    private bool TryGetValidatedContents(out Dictionary<string, LayoutContent>? contents)
    {
        contents = null;
        LayoutContent[] current = _manager.Layout.Descendents().OfType<LayoutContent>().ToArray();
        if (current.Length != _registrations.Count || current.Any(content => string.IsNullOrWhiteSpace(content.ContentId)))
            return false;

        Dictionary<string, LayoutContent> byId = new(StringComparer.Ordinal);
        foreach (LayoutContent content in current)
        {
            if (!_registrations.TryGetValue(content.ContentId, out ContentRegistration? registration) ||
                !registration.Accepts(content) ||
                !byId.TryAdd(content.ContentId, content))
                return false;
        }

        if (byId.Count != _registrations.Count)
            return false;
        contents = byId;
        return true;
    }

    private ShellDockLayoutOperationResult PreflightContentIdentity(string serialized)
    {
        ShellDockLayoutOperationResult safeXml = ShellDockLayoutStore.ValidateSafeXml(serialized);
        if (!safeXml.Succeeded)
            return safeXml;

        try
        {
            XDocument document = XDocument.Parse(serialized, LoadOptions.None);
            Dictionary<string, bool> identities = new(StringComparer.Ordinal);
            foreach (XElement element in document.Descendants().Where(element =>
                         element.Name.LocalName is "LayoutDocument" or "LayoutAnchorable"))
            {
                string? contentId = element.Attribute("ContentId")?.Value;
                if (string.IsNullOrWhiteSpace(contentId) ||
                    !_registrations.TryGetValue(contentId, out ContentRegistration? registration) ||
                    registration.IsDocument != (element.Name.LocalName == "LayoutDocument") ||
                    !identities.TryAdd(contentId, registration.IsDocument))
                    return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.InvalidContentIdentity);
            }

            return identities.Count == _registrations.Count &&
                   _registrations.Keys.All(identities.ContainsKey)
                ? ShellDockLayoutOperationResult.Success
                : new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.InvalidContentIdentity);
        }
        catch
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);
        }
    }

    private Dictionary<ShellDockHomeZone, HomeAutomationRegistration> CaptureHomeAutomation()
    {
        Dictionary<ShellDockHomeZone, HomeAutomationRegistration> result = [];
        foreach ((ShellDockHomeZone zone, string[] contentIds) in _homeContentIds)
        {
            LayoutAnchorable? tool = contentIds.Select(FindContent).OfType<LayoutAnchorable>().FirstOrDefault();
            if (tool?.Parent is not LayoutAnchorablePane pane)
                continue;
            string paneId = AutomationProperties.GetAutomationId(pane);
            string groupId = pane.Parent is DependencyObject parent
                ? AutomationProperties.GetAutomationId(parent)
                : string.Empty;
            result[zone] = new HomeAutomationRegistration(paneId, groupId);
        }

        return result;
    }

    private void ApplyHomeAutomation(IReadOnlyDictionary<string, LayoutContent> contents)
    {
        foreach ((ShellDockHomeZone zone, HomeAutomationRegistration registration) in _homeAutomation)
        {
            LayoutAnchorablePane? pane = _homeContentIds[zone]
                .Select(id => contents.TryGetValue(id, out LayoutContent? value) ? value as LayoutAnchorable : null)
                .FirstOrDefault(tool => tool is { IsFloating: false, Parent: LayoutAnchorablePane })
                ?.Parent as LayoutAnchorablePane;
            if (pane is null)
                continue;
            if (!string.IsNullOrEmpty(registration.PaneAutomationId))
                AutomationProperties.SetAutomationId(pane, registration.PaneAutomationId);
            if (!string.IsNullOrEmpty(registration.GroupAutomationId) && pane.Parent is DependencyObject parent)
                AutomationProperties.SetAutomationId(parent, registration.GroupAutomationId);
        }
    }

    private sealed record HomeAutomationRegistration(string PaneAutomationId, string GroupAutomationId);

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    private sealed record ContentRegistration(
        string ContentId,
        bool IsDocument,
        object? Content,
        string? Title,
        BindingBase? TitleBinding,
        string AutomationId,
        bool CanClose,
        bool CanFloat,
        bool CanMove,
        bool CanHide,
        bool CanAutoHide,
        bool CanDockAsTabbedDocument)
    {
        public static ContentRegistration Capture(LayoutContent model)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(model.ContentId);
            return new ContentRegistration(
                model.ContentId,
                model is LayoutDocument,
                model.Content,
                model.Title,
                BindingOperations.GetBindingBase(model, LayoutContent.TitleProperty),
                AutomationProperties.GetAutomationId(model),
                model.CanClose,
                model.CanFloat,
                model switch { LayoutDocument document => document.CanMove, LayoutAnchorable tool => tool.CanMove, _ => false },
                model is LayoutAnchorable anchorable && anchorable.CanHide,
                model is LayoutAnchorable autoHide && autoHide.CanAutoHide,
                model is LayoutAnchorable tabbed && tabbed.CanDockAsTabbedDocument);
        }

        public bool Accepts(LayoutContent model)
            => IsDocument ? model is LayoutDocument : model is LayoutAnchorable;

        public void ApplyTo(LayoutContent model)
        {
            model.ContentId = ContentId;
            model.Content = Content;
            model.CanClose = CanClose;
            model.CanFloat = CanFloat;
            AutomationProperties.SetAutomationId(model, AutomationId);
            if (TitleBinding is not null)
                BindingOperations.SetBinding(model, LayoutContent.TitleProperty, TitleBinding);
            else
                model.Title = Title;

            if (model is LayoutDocument document)
                document.CanMove = CanMove;
            else if (model is LayoutAnchorable tool)
            {
                tool.CanMove = CanMove;
                tool.CanHide = CanHide;
                tool.CanAutoHide = CanAutoHide;
                tool.CanDockAsTabbedDocument = CanDockAsTabbedDocument;
            }
        }
    }
}
