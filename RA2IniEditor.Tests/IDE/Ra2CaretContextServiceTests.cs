using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CaretContextServiceTests
{
    [Fact]
    public void GetContext_OnKeyReturnsKeyRegion()
    {
        const string text = "[NEWINF]\nStrength=300\nPrimary=120mm";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.IndexOf("Strength", StringComparison.Ordinal) + 2);

        Assert.Equal(Ra2CaretRegion.Key, context.Region);
        Assert.Equal("Strength", context.KeyValue!.Key);
        Assert.Equal("NEWINF", context.Section!.Name);
        Assert.Equal("Strength", context.TokenText);
    }

    [Fact]
    public void GetContext_OnValueReturnsValueRegion()
    {
        const string text = "[NEWINF]\nStrength=300\nPrimary=120mm";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.IndexOf("120mm", StringComparison.Ordinal) + 1);

        Assert.Equal(Ra2CaretRegion.Value, context.Region);
        Assert.Equal("Primary", context.KeyValue!.Key);
        Assert.Equal("120mm", context.TokenText);
    }

    [Fact]
    public void GetContext_OnSectionHeaderReturnsSectionHeaderRegion()
    {
        const string text = "[NEWINF]\nStrength=300";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.IndexOf("NEWINF", StringComparison.Ordinal));

        Assert.Equal(Ra2CaretRegion.SectionHeader, context.Region);
        Assert.Equal("NEWINF", context.Section!.Name);
        Assert.Equal("[NEWINF]", context.TokenText);
    }

    [Fact]
    public void GetContext_OnSectionHeaderWithInlineCommentReturnsOnlyHeaderToken()
    {
        const string text = "[NEWINF] ; infantry\nStrength=300";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.IndexOf("NEWINF", StringComparison.Ordinal));

        Assert.Equal(Ra2CaretRegion.SectionHeader, context.Region);
        Assert.Equal("NEWINF", context.Section!.Name);
        Assert.Equal("[NEWINF]", context.TokenText);
    }

    [Fact]
    public void GetContext_OnCommentLineReturnsCommentRegion()
    {
        const string text = "; comment\n[NEWINF]\nStrength=300";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.IndexOf("comment", StringComparison.Ordinal));

        Assert.Equal(Ra2CaretRegion.Comment, context.Region);
        Assert.Equal("; comment", context.TokenText);
    }

    [Fact]
    public void GetContext_OnWhitespaceReturnsWhitespaceRegion()
    {
        const string text = "[NEWINF]\n   \nStrength=300";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.IndexOf("   ", StringComparison.Ordinal) + 1);

        Assert.Equal(Ra2CaretRegion.Whitespace, context.Region);
        Assert.Equal("NEWINF", context.Section!.Name);
    }

    [Fact]
    public void GetContext_CrlfOffsetsAreStable()
    {
        const string text = "[NEWINF]\r\nStrength=300\r\nPrimary=120mm";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.IndexOf("300", StringComparison.Ordinal));

        Assert.Equal(Ra2CaretRegion.Value, context.Region);
        Assert.Equal("300", context.TokenText);
        Assert.Equal(2, context.KeyValue!.LineNumber);
    }

    [Fact]
    public void GetContext_FinalLineWithoutNewlineReturnsValueRegion()
    {
        const string text = "[NEWINF]\nPrimary=120mm";
        Ra2DocumentSemanticModel model = Build(text);

        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, text.Length - 1);

        Assert.Equal(Ra2CaretRegion.Value, context.Region);
        Assert.Equal("120mm", context.TokenText);
    }

    private static Ra2DocumentSemanticModel Build(string text)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 3),
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
