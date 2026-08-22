using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionDisplayEnhancerTests
{
    [Fact]
    public void Enhance_KeyCompletionUsesAnnotationForDisplayOnly()
    {
        Ra2CompletionResult result = new(
            [
                new Ra2CompletionItem(
                    "Strength",
                    Ra2CompletionItemKind.Key,
                    "Type: Integer",
                    "Hit points",
                    "Strength=",
                    100,
                    Ra2CompletionItemSourceKind.FieldRegistry)
            ],
            new Ra2TextSpan(10, 3));
        Ra2FieldDefinition definition = new(
            "Strength",
            [Ra2SectionKind.Vehicle],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            "Hit points");
        IRa2FieldDisplayResolver resolver = new Ra2FieldDisplayResolver(
            new TestFieldProvider(definition),
            new Ra2FieldAnnotationProvider(new Ra2FieldAnnotationPack(
                1,
                "zh-CN",
                [new Ra2FieldAnnotationEntry("Vehicle", "Strength", "生命值", ["HP"], "单位耐久度")])));

        Ra2CompletionResult enhanced = new Ra2CompletionDisplayEnhancer().Enhance(
            result,
            Ra2SectionKind.Vehicle,
            resolver);

        Ra2CompletionItem item = Assert.Single(enhanced.Items);
        Assert.Equal("Strength", item.Label);
        Assert.Equal("Strength=", item.InsertText);
        Assert.Equal(result.ReplacementSpan.Start, enhanced.ReplacementSpan.Start);
        Assert.Equal(result.ReplacementSpan.Length, enhanced.ReplacementSpan.Length);
        Assert.Contains("生命值", item.Detail);
        Assert.Contains("Aliases: HP", item.Detail);
        Assert.Equal("单位耐久度", item.Documentation);
    }

    [Fact]
    public void Enhance_ReferenceCompletionKeepsOriginalDisplayAndInsertText()
    {
        Ra2CompletionItem reference = new(
            "120mm",
            Ra2CompletionItemKind.Reference,
            "Weapon section",
            "Line 10",
            "120mm",
            100,
            Ra2CompletionItemSourceKind.CurrentDocumentSection);
        Ra2CompletionResult result = new([reference], new Ra2TextSpan(20, 0));
        IRa2FieldDisplayResolver resolver = new Ra2FieldDisplayResolver(
            new TestFieldProvider(),
            new Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack.Empty()));

        Ra2CompletionResult enhanced = new Ra2CompletionDisplayEnhancer().Enhance(
            result,
            Ra2SectionKind.Vehicle,
            resolver);

        Assert.Same(reference, Assert.Single(enhanced.Items));
    }

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
}
