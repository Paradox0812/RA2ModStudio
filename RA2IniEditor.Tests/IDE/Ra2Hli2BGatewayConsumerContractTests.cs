using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2Hli2BGatewayConsumerContractTests
{
    [Fact]
    public void InjectedGatewayEntersExistingWorkspaceAndPreviewAppliesOnlyOnce()
    {
        Fixture fixture = new("[E1]\nStrength=100\n");
        RecordingGateway gateway = new();
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(new Ra2IniEditPreviewService(gateway), port);

        Ra2IniEditPreview preview = workspace.Preview(fixture.Snapshot, fixture.Plan());
        Ra2IniEditApplyResult first = workspace.Apply(new(preview.PreviewId, true));
        Ra2IniEditApplyResult replay = workspace.Apply(new(preview.PreviewId, true));

        Assert.True(preview.Succeeded, preview.Message);
        Assert.True(first.Succeeded, first.Message);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, replay.OutcomeKind);
        Assert.Equal(1, gateway.PreviewCallCount);
        Assert.Equal(1, port.CallCount);
    }

    [Fact]
    public void CanceledGatewayPreviewNeverEntersActiveWorkspaceSlot()
    {
        Fixture fixture = new("[E1]\nStrength=100\n");
        RecordingGateway gateway = new();
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(new Ra2IniEditPreviewService(gateway), port);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2IniEditPreview preview = workspace.Preview(
            fixture.Snapshot,
            fixture.Plan(),
            cancellation.Token);
        Ra2IniEditApplyResult apply = workspace.Apply(new(preview.PreviewId, true));

        Assert.Equal(Ra2IniEditPreviewFailureKind.Canceled, preview.FailureKind);
        Assert.Equal(Guid.Empty, preview.PreviewId);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, apply.OutcomeKind);
        Assert.Equal(1, gateway.PreviewCallCount);
        Assert.Equal(0, port.CallCount);
    }

    [Fact]
    public void DocumentBeyondGatewayLimitIsTypedFailureWithoutPreviewAuthority()
    {
        string prefix = "[E1]\nStrength=100\n";
        string text = prefix + new string(
            ';',
            Ra2AutomationEditPreviewService.MaximumDocumentCharacters + 1 - prefix.Length);
        Fixture fixture = new(text);
        RecordingGateway gateway = new();
        RecordingTransactionPort port = new(fixture.Session);
        Ra2IniAuthoringWorkspace workspace = new(new Ra2IniEditPreviewService(gateway), port);

        Ra2IniEditPreview preview = workspace.Preview(fixture.Snapshot, fixture.Plan());
        Ra2IniEditApplyResult apply = workspace.Apply(new(preview.PreviewId, true));

        Assert.Equal(Ra2IniEditPreviewFailureKind.DocumentTooLarge, preview.FailureKind);
        Assert.Equal(Guid.Empty, preview.PreviewId);
        Assert.Null(preview.CandidateText);
        Assert.Equal(Ra2IniEditApplyOutcomeKind.PreviewUnavailable, apply.OutcomeKind);
        Assert.Equal(0, port.CallCount);
    }

    [Fact]
    public void ProductionAdapterUsesTypedGatewayAndUnlimitedBypassIsAbsent()
    {
        string root = TestRepositoryRoot.Find();
        string adapter = ReadSource(root, "RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs");
        string production = string.Join(
            '\n',
            Directory.EnumerateFiles(
                    Path.Combine(root, "RA2IniEditor.Application"),
                    "*.cs",
                    SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(
                    Path.Combine(root, "RA2IniEditor.IDE"),
                    "*.cs",
                    SearchOption.AllDirectories))
                .Select(File.ReadAllText));

        Assert.Contains("IRa2AutomationCapabilityGateway _gateway", adapter, StringComparison.Ordinal);
        Assert.Contains("_gateway.Preview(", adapter, StringComparison.Ordinal);
        Assert.DoesNotContain("Ra2AutomationEditPreviewService _", adapter, StringComparison.Ordinal);
        Assert.DoesNotContain("new Ra2AutomationEditPreviewService", adapter, StringComparison.Ordinal);
        Assert.DoesNotContain("PreviewForHost", production, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellSharesGatewayWithAdapterAndUsesDescriptorAsOnlyBudgetSource()
    {
        string root = TestRepositoryRoot.Find();
        string shell = ReadSource(root, "RA2IniEditor.IDE/Views/ShellWindow.xaml.cs");

        Assert.Contains(
            "IRa2AutomationCapabilityGateway _automationCapabilityGateway",
            shell,
            StringComparison.Ordinal);
        Assert.Contains(
            "new Ra2IniEditPreviewService(_automationCapabilityGateway)",
            shell,
            StringComparison.Ordinal);
        Assert.Contains("_automationCapabilityGateway.GetCapabilities()", shell, StringComparison.Ordinal);
        Assert.Contains("Ra2AutomationCapabilityIds.DocumentEditPreview", shell, StringComparison.Ordinal);
        Assert.Contains("editPreviewCapability.MaximumDocumentCharacters", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("8_388_608", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("8388608", shell, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellResourcePreflightOccursBeforePipelineSessionAndProviderSend()
    {
        string root = TestRepositoryRoot.Find();
        string shell = ReadSource(root, "RA2IniEditor.IDE/Views/ShellWindow.xaml.cs");
        string method = ExtractMethod(shell, "private async void GenerateAiAssistantResponse");

        AssertOrdered(method,
            "CaptureCurrentAuthoringSnapshot()",
            "_automationCapabilityGateway.GetCapabilities()",
            "Ra2AiInteractionRouter.Resolve(",
            "CreateAiAssistantPipeline(",
            "_aiAssistantRequestLifecycle.TryStart(",
            "replanCoordinator.ExecuteAsync(");
        Assert.Contains("Ra2AiEditAvailabilityKind.ResourceLimitExceeded", method, StringComparison.Ordinal);
        Assert.Contains("new Ra2AiBoundedStructuredReplanRequest(", method, StringComparison.Ordinal);
    }

    [Fact]
    public void ResourceLimitMessageIsLocalAndExplicitlySaysRequestWasNotSent()
    {
        string root = TestRepositoryRoot.Find();
        string shell = ReadSource(root, "RA2IniEditor.IDE/Views/ShellWindow.xaml.cs");
        string method = ExtractMethod(shell, "private static string FormatAiEditUnavailableMessage");

        Assert.Contains("Ra2AiEditAvailabilityKind.ResourceLimitExceeded", method, StringComparison.Ordinal);
        Assert.Contains("8 MiB", method, StringComparison.Ordinal);
        Assert.Contains("尚未发送", method, StringComparison.Ordinal);
    }

    [Fact]
    public void GatewayCatalogAndPublicSurfaceRemainFrozen()
    {
        IRa2AutomationCapabilityGateway gateway = new Ra2AutomationCapabilityGateway();
        Ra2AutomationCapabilityDescriptor descriptor = Assert.Single(
            gateway.GetCapabilities(),
            capability =>
                string.Equals(
                    capability.Id,
                    Ra2AutomationCapabilityIds.DocumentEditPreview,
                    StringComparison.Ordinal));

        Assert.Equal(Ra2AutomationCapabilityIds.CurrentVersion, descriptor.Version);
        Assert.Equal(Ra2AutomationCapabilityRisk.Edit, descriptor.Risk);
        Assert.Equal(Ra2AutomationEditPreviewService.MaximumDocumentCharacters, descriptor.MaximumDocumentCharacters);
        Assert.Equal(Ra2AutomationEditPreviewService.MaximumDiagnosticItems, descriptor.MaximumResultItems);
        Assert.Equal(Ra2IniEditPlan.MaximumOperationCount, descriptor.MaximumOperations);
        Assert.Equal(77, typeof(Ra2AutomationCapabilityGateway).Assembly.GetExportedTypes().Length);
        Assert.Equal(
            ["ExpandProjectTemplate", "ExpandTemplate", "FindReferences", "GetCapabilities", "GetFieldSchema", "GetSection", "GetTemplates", "Preview", "PreviewProject", "ResolveReference", "Validate"],
            typeof(IRa2AutomationCapabilityGateway).GetMethods().Select(method => method.Name).Order().ToArray());
    }

    private static string ReadSource(string root, string relativePath)
        => File.ReadAllText(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string ExtractMethod(string source, string signature)
    {
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Method not found: {signature}");
        int nextMethod = source.IndexOf("\n    private ", start + signature.Length, StringComparison.Ordinal);
        return nextMethod >= 0 ? source[start..nextMethod] : source[start..];
    }

    private static void AssertOrdered(string source, params string[] tokens)
    {
        int previous = -1;
        foreach (string token in tokens)
        {
            int current = source.IndexOf(token, StringComparison.Ordinal);
            Assert.True(current > previous, $"Expected '{token}' after the previous contract token.");
            previous = current;
        }
    }

    private sealed class Fixture
    {
        public Fixture(string text)
        {
            Ra2EditableDocumentSessionService sessionService = new(
                new Ra2IniTextDocumentParser(),
                new Ra2DirtyStateService());
            Session = sessionService.StartEditing("rulesmd.ini", text);
            Ra2FieldRegistryProviderSnapshot registry = new(
                new BuiltInRa2FieldDefinitionProvider(),
                revision: 1);
            Snapshot = Assert.IsType<Ra2AuthoringSnapshot>(
                Ra2AuthoringSnapshot.Capture(Session, text, string.Empty, registry).Snapshot);
        }

        public Ra2EditableDocumentSession Session { get; }

        public Ra2AuthoringSnapshot Snapshot { get; }

        public Ra2IniEditPlan Plan()
            => new(
                Guid.NewGuid(),
                Snapshot.DocumentId,
                Snapshot.EditRevision,
                Snapshot.FieldRegistry.Revision,
                [new Ra2IniEditOperation(
                    Ra2IniEditOperationKind.ReplaceFieldValue,
                    "E1",
                    "Strength",
                    "125")],
                "HLI-2B contract",
                "Tests");
    }

    private sealed class RecordingGateway : IRa2AutomationCapabilityGateway
    {
        private readonly Ra2AutomationCapabilityGateway _inner = new();

        public int PreviewCallCount { get; private set; }

        public IReadOnlyList<Ra2AutomationCapabilityDescriptor> GetCapabilities()
            => _inner.GetCapabilities();

        public IReadOnlyList<Ra2AutomationTemplateDescriptor> GetTemplates()
            => _inner.GetTemplates();

        public Ra2AutomationFieldSchemaQueryResult GetFieldSchema(
            Ra2AutomationDocumentSnapshot snapshot,
            Ra2AutomationFieldSchemaQuery request,
            CancellationToken cancellationToken = default)
            => _inner.GetFieldSchema(snapshot, request, cancellationToken);

        public Ra2AutomationReferenceResolveResult ResolveReference(
            Ra2AutomationDocumentSnapshot snapshot,
            Ra2AutomationReferenceResolveQuery request,
            CancellationToken cancellationToken = default)
            => _inner.ResolveReference(snapshot, request, cancellationToken);

        public Ra2AutomationSectionQueryResult GetSection(
            Ra2AutomationDocumentSnapshot snapshot,
            Ra2AutomationSectionQuery request,
            CancellationToken cancellationToken = default)
            => _inner.GetSection(snapshot, request, cancellationToken);

        public Ra2AutomationReferenceQueryResult FindReferences(
            Ra2AutomationDocumentSnapshot snapshot,
            Ra2AutomationReferenceQuery request,
            CancellationToken cancellationToken = default)
            => _inner.FindReferences(snapshot, request, cancellationToken);

        public Ra2AutomationDocumentDiagnosticsResult Validate(
            Ra2AutomationDocumentSnapshot snapshot,
            CancellationToken cancellationToken = default)
            => _inner.Validate(snapshot, cancellationToken);

        public Ra2AutomationEditPreviewResult Preview(
            Ra2AutomationDocumentSnapshot snapshot,
            Ra2AutomationEditPlan plan,
            CancellationToken cancellationToken = default)
        {
            PreviewCallCount++;
            return _inner.Preview(snapshot, plan, cancellationToken);
        }

        public Ra2AutomationTemplateExpansionResult ExpandTemplate(
            Ra2AutomationDocumentSnapshot snapshot,
            Ra2AutomationTemplateExpansionRequest request,
            CancellationToken cancellationToken = default)
            => _inner.ExpandTemplate(snapshot, request, cancellationToken);

        public Ra2AutomationProjectEditPreviewResult PreviewProject(
            Ra2AutomationProjectSnapshot snapshot,
            Ra2AutomationProjectEditPlan plan,
            CancellationToken cancellationToken = default)
            => _inner.PreviewProject(snapshot, plan, cancellationToken);

        public Ra2AutomationProjectTemplateExpansionResult ExpandProjectTemplate(
            Ra2AutomationProjectSnapshot snapshot,
            Ra2AutomationTemplateExpansionRequest request,
            CancellationToken cancellationToken = default)
            => _inner.ExpandProjectTemplate(snapshot, request, cancellationToken);
    }

    private sealed class RecordingTransactionPort : IRa2EditorTransactionPort
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private Ra2EditableDocumentSession _session;

        public RecordingTransactionPort(Ra2EditableDocumentSession session)
            => _session = session;

        public int CallCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            CallCount++;
            _session = _sessionService.UpdateText(_session, preview.CandidateText!);
            return Ra2IniEditApplyResult.Applied(preview, _session, 0, preview.CandidateText!.Length);
        }
    }
}
