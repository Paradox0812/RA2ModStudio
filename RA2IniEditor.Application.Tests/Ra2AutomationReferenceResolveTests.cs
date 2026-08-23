using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationReferenceResolveTests
{
    [Theory]
    [InlineData("Primary", Ra2SectionKind.Weapon)]
    [InlineData("Secondary", Ra2SectionKind.Weapon)]
    public void ResolveReference_UsesSemanticKnownWeaponChain(string key, Ra2SectionKind targetKind)
    {
        string text = $"[E1]\n{key}=MyWeapon ; inline\n[MyWeapon]\nDamage=10\n";

        Ra2AutomationReferenceResolveResult result = new Ra2AutomationDocumentQueryService().ResolveReference(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceResolveQuery("E1", key));

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(Ra2AutomationReferenceResolutionBasis.SemanticKnown, result.Fact!.Basis);
        Assert.Equal("MyWeapon", result.Fact.RawEffectiveToken);
        Assert.Equal(targetKind, result.Fact.TargetSectionKind);
        Assert.True(result.Fact.IsTargetDefined);
        Assert.Equal(1, result.Fact.TargetDefinitionCount);
        Assert.Equal("MyWeapon", AutomationTestSupport.Slice(text, result.Fact.SourceSpan));
    }

    [Fact]
    public void ResolveReference_UsesSemanticKnownProjectileAndWarheadTargets()
    {
        const string text = "[E1]\nPrimary=Gun\n[Gun]\nProjectile=Bullet\nWarhead=AP\n[Bullet]\nImage=none\n[AP]\nVerses=100%\n";
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(text);
        Ra2AutomationDocumentQueryService service = new();

        Ra2AutomationReferenceResolveResult projectile = service.ResolveReference(
            snapshot,
            new Ra2AutomationReferenceResolveQuery("Gun", "Projectile"));
        Ra2AutomationReferenceResolveResult warhead = service.ResolveReference(
            snapshot,
            new Ra2AutomationReferenceResolveQuery("Gun", "Warhead"));

        Assert.Equal(Ra2SectionKind.Projectile, projectile.Fact!.TargetSectionKind);
        Assert.Equal(Ra2SectionKind.Warhead, warhead.Fact!.TargetSectionKind);
    }

    [Fact]
    public void ResolveReference_UsesSchemaDeclaredListWithoutGuessingTargetKind()
    {
        const string text = "[Custom]\nLinks= Alpha | Beta | Gamma ; comment\n[Beta]\nName=Target\n";
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            text,
            new SchemaProvider(Ra2FieldValueKind.ReferenceList, "|"));

        Ra2AutomationReferenceResolveResult result = new Ra2AutomationDocumentQueryService().ResolveReference(
            snapshot,
            new Ra2AutomationReferenceResolveQuery("Custom", "Links", referenceIndex: 1));

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(Ra2AutomationReferenceResolutionBasis.FieldSchemaDeclared, result.Fact!.Basis);
        Assert.Equal("Beta", result.Fact.TargetSectionName);
        Assert.Equal(Ra2SectionKind.Unknown, result.Fact.TargetSectionKind);
        Assert.True(result.Fact.IsSchemaDeclaredReference);
        Assert.True(result.Fact.IsTargetDefined);
        Assert.Equal("Beta", AutomationTestSupport.Slice(text, result.Fact.SourceSpan));
    }

    [Fact]
    public void ResolveReference_MissingAndDuplicateTargetsAreSuccessfulFacts()
    {
        const string text = "[E1]\nPrimary=Missing\n[E2]\nPrimary=Duplicate\n[Duplicate]\nDamage=1\n[Duplicate]\nDamage=2\n";
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(text);
        Ra2AutomationDocumentQueryService service = new();

        Ra2AutomationReferenceResolveResult missing = service.ResolveReference(
            snapshot,
            new Ra2AutomationReferenceResolveQuery("E1", "Primary"));
        Ra2AutomationReferenceResolveResult duplicate = service.ResolveReference(
            snapshot,
            new Ra2AutomationReferenceResolveQuery("E2", "Primary"));

        Assert.True(missing.Succeeded);
        Assert.False(missing.Fact!.IsTargetDefined);
        Assert.Equal(0, missing.Fact.TargetDefinitionCount);
        Assert.True(duplicate.Succeeded);
        Assert.True(duplicate.Fact!.IsTargetDefined);
        Assert.Equal(2, duplicate.Fact.TargetDefinitionCount);
        Assert.Equal(Ra2SectionKind.Weapon, duplicate.Fact.TargetSectionKind);
    }

    [Fact]
    public void ResolveReference_DistinguishesAmbiguityOccurrencesAndIndexFailures()
    {
        const string text = "[Custom]\nLinks=A,,C\nLinks=D,E\n[Custom]\nLinks=F,G\n";
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            text,
            new SchemaProvider(Ra2FieldValueKind.ReferenceList, ","));
        Ra2AutomationDocumentQueryService service = new();

        Assert.Equal(
            Ra2AutomationReferenceResolveFailureKind.AmbiguousSection,
            service.ResolveReference(snapshot, new Ra2AutomationReferenceResolveQuery("Custom", "Links")).FailureKind);
        Assert.Equal(
            Ra2AutomationReferenceResolveFailureKind.AmbiguousField,
            service.ResolveReference(snapshot, new Ra2AutomationReferenceResolveQuery("Custom", "Links", 0)).FailureKind);
        Assert.Equal(
            Ra2AutomationReferenceResolveFailureKind.EmptyReference,
            service.ResolveReference(snapshot, new Ra2AutomationReferenceResolveQuery("Custom", "Links", 0, 0, 1)).FailureKind);
        Assert.Equal(
            Ra2AutomationReferenceResolveFailureKind.ReferenceIndexOutOfRange,
            service.ResolveReference(snapshot, new Ra2AutomationReferenceResolveQuery("Custom", "Links", 1, 0, 3)).FailureKind);

        Ra2AutomationReferenceResolveResult selected = service.ResolveReference(
            snapshot,
            new Ra2AutomationReferenceResolveQuery("Custom", "Links", 1, 0, 1));
        Assert.True(selected.Succeeded);
        Assert.Equal(1, selected.Fact!.SourceSectionOccurrence);
        Assert.Equal(0, selected.Fact.SourceFieldOccurrence);
        Assert.Equal("G", selected.Fact.RawEffectiveToken);
    }

    [Fact]
    public void ResolveReference_UnsupportedCanceledAndTooLargeHaveNoFact()
    {
        Ra2AutomationDocumentQueryService service = new();
        Ra2AutomationDocumentSnapshot ordinary = AutomationTestSupport.Snapshot("[E1]\nStrength=100\n");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2AutomationReferenceResolveResult unsupported = service.ResolveReference(
            ordinary,
            new Ra2AutomationReferenceResolveQuery("E1", "Strength"));
        Ra2AutomationReferenceResolveResult canceled = service.ResolveReference(
            ordinary,
            new Ra2AutomationReferenceResolveQuery("E1", "Strength"),
            cancellation.Token);
        Ra2AutomationReferenceResolveResult tooLarge = service.ResolveReference(
            AutomationTestSupport.Snapshot(new string(';', Ra2AutomationDocumentQueryService.MaximumDocumentCharacters + 1)),
            new Ra2AutomationReferenceResolveQuery("E1", "Primary"));

        Assert.Equal(Ra2AutomationReferenceResolveFailureKind.UnsupportedReference, unsupported.FailureKind);
        Assert.Equal(Ra2AutomationReferenceResolveFailureKind.Canceled, canceled.FailureKind);
        Assert.Equal(Ra2AutomationReferenceResolveFailureKind.DocumentTooLarge, tooLarge.FailureKind);
        Assert.Null(unsupported.Fact);
        Assert.Null(canceled.Fact);
        Assert.Null(tooLarge.Fact);
    }

    private sealed class SchemaProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition _definition;

        public SchemaProvider(Ra2FieldValueKind valueKind, string separator)
        {
            _definition = new Ra2FieldDefinition(
                "Links",
                [Ra2SectionKind.Unknown],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.User,
                "Generic references.",
                new Ra2FieldValueMetadata(valueKind, separator: separator));
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            if (string.Equals(key, _definition.Key, StringComparison.OrdinalIgnoreCase))
            {
                definition = _definition;
                return true;
            }
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind) => [_definition];
        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => string.Equals(key, _definition.Key, StringComparison.OrdinalIgnoreCase);
    }
}
