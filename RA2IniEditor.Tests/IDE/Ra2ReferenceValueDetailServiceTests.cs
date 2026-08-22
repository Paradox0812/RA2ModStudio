using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ReferenceValueDetailServiceTests
{
    [Fact]
    public void Resolve_OnReferenceValueReturnsTargetSummary()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=120mm

            [120mm];Cannon weapon
            Damage=90
            ROF=80
            Range=5.75
            Projectile=Cannon
            Warhead=AP
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("120mm", StringComparison.Ordinal) + 1;

        Ra2ReferenceValueDetailResult result = new Ra2ReferenceValueDetailService().Resolve(
            new Ra2ReferenceValueDetailRequest(model, offset));

        Assert.Equal(Ra2ReferenceValueDetailStatus.Available, result.Status);
        Assert.NotNull(result.Target);
        Assert.Equal("120mm", result.Target.Title);
        Assert.Equal(Ra2DefinitionTargetKind.ReferenceTarget, result.Target.Kind);
        Assert.Contains("\u76ee\u6807\u5907\u6ce8: Cannon weapon", result.Target.Description);
        Assert.Contains("Damage=90", result.Target.Description);
        Assert.Contains("Projectile=Cannon", result.Target.Description);
        Assert.Equal(7, result.Target.TargetLineNumber);
    }

    [Fact]
    public void Resolve_OnReferenceValueWithInlineCommentUsesEffectiveValueAndPreservesNote()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            ElitePrimary=ATGUNE;90mmE

            [ATGUNE]
            Damage=175
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("ATGUNE;90mmE", StringComparison.Ordinal) + 1;

        Ra2ReferenceValueDetailResult result = new Ra2ReferenceValueDetailService().Resolve(
            new Ra2ReferenceValueDetailRequest(model, offset));

        Assert.Equal(Ra2ReferenceValueDetailStatus.Available, result.Status);
        Assert.NotNull(result.Target);
        Assert.Equal("ATGUNE", result.Reference!.TargetSectionName);
        Assert.Equal("90mmE", result.InlineComment);
        Assert.Contains("\u5f15\u7528\u5907\u6ce8: 90mmE", result.Target.Description);
    }

    [Fact]
    public void Resolve_WithSelectedReferenceValueUsesSelectionCandidate()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            EliteSecondary=ParaBomb;old note

            [ParaBomb]
            Damage=50
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int selectionStart = text.IndexOf("ParaBomb", StringComparison.Ordinal);
        Ra2TextSpan selectionSpan = new(selectionStart, "ParaBomb".Length);
        int offset = selectionStart + 2;

        Ra2ReferenceValueDetailResult result = new Ra2ReferenceValueDetailService().Resolve(
            new Ra2ReferenceValueDetailRequest(model, offset, selectionSpan));

        Assert.Equal(Ra2ReferenceValueDetailStatus.Available, result.Status);
        Assert.Equal("ParaBomb", result.Reference!.TargetSectionName);
        Assert.Equal("old note", result.InlineComment);
    }

    [Fact]
    public void Resolve_OnOrdinaryValueDoesNotReturnReferenceTarget()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Strength=400
            Occupier=yes
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("400", StringComparison.Ordinal) + 1;

        Ra2ReferenceValueDetailResult result = new Ra2ReferenceValueDetailService().Resolve(
            new Ra2ReferenceValueDetailRequest(model, offset));

        Assert.Equal(Ra2ReferenceValueDetailStatus.NotReferenceValue, result.Status);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Resolve_OnMissingReferenceReturnsMissingTargetDetail()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon;old weapon
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("MissingWeapon", StringComparison.Ordinal) + 1;

        Ra2ReferenceValueDetailResult result = new Ra2ReferenceValueDetailService().Resolve(
            new Ra2ReferenceValueDetailRequest(model, offset));

        Assert.Equal(Ra2ReferenceValueDetailStatus.MissingTarget, result.Status);
        Assert.NotNull(result.Target);
        Assert.Equal("MissingWeapon", result.Target.Title);
        Assert.Contains("\u5f53\u524d\u6587\u4ef6\u4e2d\u672a\u627e\u5230\u8be5\u5f15\u7528\u76ee\u6807", result.Target.Description);
        Assert.Contains("\u5f15\u7528\u5907\u6ce8: old weapon", result.Target.Description);
    }

    [Fact]
    public void CreateHoverInfo_UsesCompactReferenceValueDescription()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=M60;GI weapon reference

            [M60];GIWeapon
            Damage=15
            ROF=20
            Range=4
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2ReferenceValueDetailService service = new();
        Ra2ReferenceValueDetailResult result = service.Resolve(
            new Ra2ReferenceValueDetailRequest(model, text.IndexOf("M60;GI", StringComparison.Ordinal) + 1));

        Ra2HoverInfo? hover = service.CreateHoverInfo(result);

        Assert.NotNull(hover);
        Assert.Equal("M60", hover.Title);
        Assert.Equal("Weapon", hover.Kind);
        Assert.Equal("GIWeapon", hover.DisplayName);
        Assert.Equal("\u5f15\u7528\u5907\u6ce8: GI weapon reference", hover.Description);
        Assert.DoesNotContain("Damage=15", hover.Description);
        Assert.DoesNotContain("ROF=20", hover.Description);
        Assert.DoesNotContain("\u76ee\u6807\u5907\u6ce8", hover.Description);
        Assert.Equal("Current document", hover.Source);
    }

    [Fact]
    public void Resolve_OnReferenceTargetWithPrecedingCommentUsesDisplayNote()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=M60

            ; GI Weapon
            [M60]
            Damage=15
            """;
        Ra2DocumentSemanticModel model = Build(text);

        Ra2ReferenceValueDetailService service = new();
        Ra2ReferenceValueDetailResult result = service.Resolve(
            new Ra2ReferenceValueDetailRequest(model, text.IndexOf("M60", StringComparison.Ordinal) + 1));
        Ra2HoverInfo? hover = service.CreateHoverInfo(result);

        Assert.Equal(Ra2ReferenceValueDetailStatus.Available, result.Status);
        Assert.NotNull(result.Target);
        Assert.Contains("\u76ee\u6807\u5907\u6ce8: GI Weapon", result.Target.Description);
        Assert.Contains("Damage=15", result.Target.Description);
        Assert.NotNull(hover);
        Assert.Equal("GI Weapon", hover.DisplayName);
        Assert.Null(hover.Description);
    }

    [Fact]
    public void CreateHoverInfo_OnMissingReferenceStaysCompact()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon;old weapon
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2ReferenceValueDetailService service = new();
        Ra2ReferenceValueDetailResult result = service.Resolve(
            new Ra2ReferenceValueDetailRequest(model, text.IndexOf("MissingWeapon", StringComparison.Ordinal) + 1));

        Ra2HoverInfo? hover = service.CreateHoverInfo(result);

        Assert.NotNull(hover);
        Assert.Equal("MissingWeapon", hover.Title);
        Assert.Equal("\u5f15\u7528\u672a\u627e\u5230", hover.Kind);
        Assert.Null(hover.DisplayName);
        Assert.Contains("\u5f53\u524d\u6587\u4ef6\u4e2d\u672a\u627e\u5230\u8be5\u5f15\u7528\u76ee\u6807", hover.Description);
        Assert.Contains("\u5f15\u7528\u5907\u6ce8: old weapon", hover.Description);
        Assert.DoesNotContain("Line", hover.Description);
    }

    private static Ra2DocumentSemanticModel Build(string text)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 1),
            new EmptyFieldDefinitionProvider());

    private sealed class EmptyFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => false;
    }
}
