using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Controllers.Hover;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SourceEditorHoverControllerTests
{
    [Fact]
    public void OnPointerMoved_WhenCompletionDropdownIsOpenRequestsClose()
    {
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());

        Ra2SourceEditorHoverPointerMoveResult result = controller.OnPointerMoved(
            isCompletionDropdownOpen: true,
            documentOffset: 10,
            isDelayTimerEnabled: false);

        Assert.Equal(Ra2SourceEditorHoverPointerMoveAction.Close, result.Action);
        Assert.Null(controller.ConsumePendingOffset());
    }

    [Fact]
    public void OnPointerMoved_WhenSameOffsetAndTimerEnabledIgnores()
    {
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());
        controller.OnPointerMoved(
            isCompletionDropdownOpen: false,
            documentOffset: 10,
            isDelayTimerEnabled: false);

        Ra2SourceEditorHoverPointerMoveResult result = controller.OnPointerMoved(
            isCompletionDropdownOpen: false,
            documentOffset: 10,
            isDelayTimerEnabled: true);

        Assert.Equal(Ra2SourceEditorHoverPointerMoveAction.Ignore, result.Action);
        Assert.Equal(10, controller.ConsumePendingOffset());
    }

    [Fact]
    public void OnPointerMoved_WhenOffsetChangesWhileTimerEnabledRestartsDelay()
    {
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());
        controller.OnPointerMoved(
            isCompletionDropdownOpen: false,
            documentOffset: 10,
            isDelayTimerEnabled: false);

        Ra2SourceEditorHoverPointerMoveResult result = controller.OnPointerMoved(
            isCompletionDropdownOpen: false,
            documentOffset: 16,
            isDelayTimerEnabled: true);

        Assert.Equal(Ra2SourceEditorHoverPointerMoveAction.StartDelay, result.Action);
        Assert.Equal(16, controller.ConsumePendingOffset());
    }

    [Fact]
    public void OnPointerMoved_WhenHoverAlreadyShownAtSameOffsetIgnores()
    {
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());
        controller.MarkHoverShown(10);

        Ra2SourceEditorHoverPointerMoveResult result = controller.OnPointerMoved(
            isCompletionDropdownOpen: false,
            documentOffset: 10,
            isDelayTimerEnabled: false);

        Assert.Equal(Ra2SourceEditorHoverPointerMoveAction.Ignore, result.Action);
        Assert.Null(controller.ConsumePendingOffset());
    }

    [Fact]
    public void OnPointerMoved_WhenHoverAlreadyShownAtDifferentOffsetStartsNewDelay()
    {
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());
        controller.MarkHoverShown(10);

        Ra2SourceEditorHoverPointerMoveResult result = controller.OnPointerMoved(
            isCompletionDropdownOpen: false,
            documentOffset: 16,
            isDelayTimerEnabled: false);

        Assert.Equal(Ra2SourceEditorHoverPointerMoveAction.StartDelay, result.Action);
        Assert.Equal(16, controller.ConsumePendingOffset());
    }

    [Fact]
    public void ResolveHover_OnKnownKeyReturnsTooltipText()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Strength=125
            """;
        Ra2FieldDefinition definition = new(
            "Strength",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            "Hit points");
        TestFieldProvider fieldProvider = new(definition);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        int offset = text.IndexOf("Strength", StringComparison.Ordinal) + 1;
        Ra2CaretContext context = ContextAt(model, offset);
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());

        Ra2SourceEditorHoverResolveResult result = controller.ResolveHover(new Ra2SourceEditorHoverRequest(
            model,
            context,
            offset,
            new Ra2FieldDisplayResolver(
                fieldProvider,
                new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty())),
            new TestProvenanceProvider(definition)));

        Assert.True(result.Success);
        Assert.Equal($"Integer Strength{Environment.NewLine}Hit points", result.ToolTipText);
    }

    [Fact]
    public void ResolveHover_OnReferenceValueReturnsTargetTooltipText()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=120mm;main weapon

            [120mm];Cannon weapon
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("120mm", StringComparison.Ordinal) + 1;
        Ra2CaretContext context = ContextAt(model, offset);
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());

        Ra2SourceEditorHoverResolveResult result = controller.ResolveHover(new Ra2SourceEditorHoverRequest(
            model,
            context,
            offset,
            new Ra2FieldDisplayResolver(
                new TestFieldProvider(),
                new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty())),
            new TestProvenanceProvider()));

        Assert.True(result.Success);
        Assert.Contains("Weapon 120mm Cannon weapon", result.ToolTipText);
        Assert.Contains("\u5f15\u7528\u5907\u6ce8: main weapon", result.ToolTipText);
        Assert.DoesNotContain("Damage=90", result.ToolTipText);
    }

    [Fact]
    public void ResolveHover_OnOrdinaryValueTokenReturnsEmpty()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Strength=125
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("125", StringComparison.Ordinal) + 1;
        Ra2CaretContext context = ContextAt(model, offset);
        Ra2SourceEditorHoverController controller = new(new Ra2HoverProvider());

        Ra2SourceEditorHoverResolveResult result = controller.ResolveHover(new Ra2SourceEditorHoverRequest(
            model,
            context,
            offset,
            new Ra2FieldDisplayResolver(
                new TestFieldProvider(),
                new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty())),
            new TestProvenanceProvider()));

        Assert.False(result.Success);
        Assert.Null(result.ToolTipText);
    }

    private static Ra2DocumentSemanticModel Build(string text, IRa2FieldDefinitionProvider? provider = null)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 1),
            provider ?? new TestFieldProvider());

    private static Ra2CaretContext ContextAt(Ra2DocumentSemanticModel model, int offset)
        => new Ra2CaretContextService().GetContext(model, offset);

    private sealed class TestFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition? _definition;

        public TestFieldProvider(Ra2FieldDefinition? definition = null)
        {
            _definition = definition;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            if (_definition is not null && string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                definition = _definition;
                return true;
            }

            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definition is null ? [] : [_definition];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => _definition is not null && string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        private readonly Ra2FieldDefinition? _definition;

        public TestProvenanceProvider(Ra2FieldDefinition? definition = null)
        {
            _definition = definition;
        }

        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
        {
            if (_definition is null || !string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase))
                return FieldRegistryProvenanceLookupResult.NotFound;

            return FieldRegistryProvenanceLookupResult.FromEntry(new FieldRegistryProvenanceEntry(
                key,
                sectionKind,
                FieldRegistryProvenanceScope.Project,
                "Project",
                "project.fields.json",
                _definition));
        }
    }
}
