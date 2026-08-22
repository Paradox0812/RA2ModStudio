using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationEditPreviewTests
{
    [Fact]
    public void Replace_PreservesFormattingCommentAndEmptyValueSpan()
    {
        Ra2AutomationDocumentSnapshot formatted = Snapshot("[E1]\nStrength = 100 ; keep\n");
        Ra2AutomationEditPreviewResult formattedResult = Preview(
            formatted,
            Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Strength", "125"));

        Assert.True(formattedResult.Succeeded);
        Assert.Equal("[E1]\nStrength = 125 ; keep\n", formattedResult.CandidateText);
        Ra2AutomationTextChange formattedChange = Assert.Single(formattedResult.Changes);
        Assert.Equal("100", AutomationTestSupport.Slice(formatted.Text, formattedChange.Span));

        Ra2AutomationDocumentSnapshot empty = Snapshot("[E1]\nStrength= ; keep\n");
        Ra2AutomationEditPreviewResult emptyResult = Preview(
            empty,
            Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Strength", "50"));

        Assert.True(emptyResult.Succeeded);
        Assert.Equal("[E1]\nStrength=50 ; keep\n", emptyResult.CandidateText);
        Assert.Equal(0, Assert.Single(emptyResult.Changes).Span.Length);
    }

    [Theory]
    [InlineData("[E1]\r\nStrength=100\r\n[NEXT]", "[E1]\r\nStrength=100\r\nArmor=steel\r\n[NEXT]")]
    [InlineData("[E1]\nStrength=100\n[NEXT]", "[E1]\nStrength=100\nArmor=steel\n[NEXT]")]
    [InlineData("[E1]\rStrength=100\r[NEXT]", "[E1]\rStrength=100\rArmor=steel\r[NEXT]")]
    [InlineData("[E1]\r\nStrength=100\n[NEXT]", "[E1]\r\nStrength=100\nArmor=steel\n[NEXT]")]
    [InlineData("[E1]\n[NEXT]", "[E1]\nArmor=steel\n[NEXT]")]
    public void Upsert_UsesAnchorLineEndingIncludingEmptyAndMixedSections(string source, string expected)
    {
        Ra2AutomationEditPreviewResult result = Preview(
            Snapshot(source),
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel"));

        Assert.True(result.Succeeded);
        Assert.Equal(expected, result.CandidateText);
    }

    [Fact]
    public void Upsert_UsesDocumentPolicyAtEofAndCoalescesSameOffsetInPlanOrder()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot("[E1]\r\nStrength=100");
        Ra2AutomationEditPlan plan = Plan(
            snapshot,
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel"),
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Primary", "Gun"));

        Ra2AutomationEditPreviewResult result = Preview(snapshot, plan);

        Assert.True(result.Succeeded);
        Assert.Equal("[E1]\r\nStrength=100\r\nArmor=steel\r\nPrimary=Gun", result.CandidateText);
        Ra2AutomationTextChange change = Assert.Single(result.Changes);
        Assert.Equal(0, change.Span.Length);
        Assert.Equal(
            new[] { "Armor", "Primary" },
            result.OperationPreviews.Select(item => item.Operation.Key));
    }

    [Fact]
    public void PreconditionsAndPlanningFailures_ReturnNoApplicablePayload()
    {
        AssertFailure(
            Preview(
                Snapshot("[E1]\n", isEditable: false),
                Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel")),
            Ra2AutomationEditPreviewFailureKind.ReadOnly);

        Ra2AutomationDocumentSnapshot snapshot = Snapshot("[E1]\nStrength=100\n");
        Ra2AutomationEditPlan stale = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            snapshot.Version,
            snapshot.FieldRegistry.Revision,
            [Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel")],
            "test",
            "tests");
        AssertFailure(Preview(snapshot, stale), Ra2AutomationEditPreviewFailureKind.StalePlanTarget);

        AssertFailure(
            Preview(snapshot, Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Armor", "steel")),
            Ra2AutomationEditPreviewFailureKind.FieldNotFound);
        AssertFailure(
            Preview(snapshot, Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Strength", "100")),
            Ra2AutomationEditPreviewFailureKind.NoChanges);
        AssertFailure(
            Preview(
                snapshot,
                Plan(
                    snapshot,
                    Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel"),
                    Operation(Ra2AutomationEditOperationKind.UpsertField, "e1", "armor", "iron"))),
            Ra2AutomationEditPreviewFailureKind.ConflictingOperations);
        AssertFailure(
            Preview(snapshot, Operation(Ra2AutomationEditOperationKind.UpsertField, "MISSING", "Armor", "steel")),
            Ra2AutomationEditPreviewFailureKind.SectionNotFound);
        AssertFailure(
            Preview(
                Snapshot("[E1]\nStrength=100\n[e1]\nArmor=steel\n"),
                Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Primary", "Gun")),
            Ra2AutomationEditPreviewFailureKind.AmbiguousSection);
        AssertFailure(
            Preview(
                Snapshot("[E1]\nStrength=100\nstrength=125\n"),
                Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Strength", "150")),
            Ra2AutomationEditPreviewFailureKind.AmbiguousField);
    }

    [Theory]
    [InlineData(null, Ra2FieldSourceKind.BuiltIn, Ra2AutomationFieldTrustLevel.Verified)]
    [InlineData("source-verified", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.Verified)]
    [InlineData("verified-guardrail", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.VerifiedGuardrail)]
    [InlineData("inferred", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.Inferred)]
    [InlineData("manual-curated", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.ManualCurated)]
    [InlineData("auto-extracted", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.AutoExtracted)]
    [InlineData("obsolete", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.Obsolete)]
    [InlineData("non-existent", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.NonExistent)]
    [InlineData("pseudo-field", Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.PseudoField)]
    [InlineData(null, Ra2FieldSourceKind.User, Ra2AutomationFieldTrustLevel.Unknown)]
    public void Evidence_MapsFieldTrustWithoutDisplayTextInference(
        string? quality,
        Ra2FieldSourceKind sourceKind,
        Ra2AutomationFieldTrustLevel expected)
    {
        Ra2FieldDefinition definition = new(
            "Strength",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            sourceKind,
            registryQuality: quality);
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            "[InfantryTypes]\n0=E1\n[E1]\nStrength=100\n",
            provider: new TestFieldDefinitionProvider([definition]));

        Ra2AutomationEditPreviewResult result = Preview(
            snapshot,
            Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Strength", "150"));

        Ra2AutomationEditOperationPreview evidence = Assert.Single(result.OperationPreviews);
        Assert.True(evidence.IsKnownField);
        Assert.Equal(expected, evidence.FieldTrustLevel);
    }

    [Fact]
    public void DiagnosticDelta_UsesSemanticFingerprintAndPreservesActualFacts()
    {
        Ra2FieldDefinition strength = new(
            "Strength",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer));
        Ra2AutomationDocumentSnapshot invalid = Snapshot(
            "[InfantryTypes]\n0=E1\n[E1]\nStrength=bad\n",
            provider: new TestFieldDefinitionProvider([strength]));

        Ra2AutomationEditPreviewResult removed = Preview(
            invalid,
            Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Strength", "150"));

        Assert.True(removed.Succeeded);
        Assert.Contains(removed.RemovedDiagnostics, diagnostic => diagnostic.Code == "FIELD_NUMBER_INVALID");
        Assert.DoesNotContain(removed.AddedDiagnostics, diagnostic => diagnostic.Code == "FIELD_NUMBER_INVALID");

        Ra2AutomationDocumentSnapshot valid = Snapshot(
            "[InfantryTypes]\n0=E1\n[E1]\nStrength=150\n",
            provider: new TestFieldDefinitionProvider([strength]));
        Ra2AutomationEditPreviewResult added = Preview(
            valid,
            Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Strength", "bad"));

        Assert.Contains(added.AddedDiagnostics, diagnostic => diagnostic.Code == "FIELD_NUMBER_INVALID");
        Assert.Equal(1, added.AddedErrorCount + added.AddedWarningCount);
    }

    [Fact]
    public void DiagnosticPositionOnlyShift_DoesNotProduceFalseDelta()
    {
        Ra2FieldDefinition armor = DefineText("Armor");
        Ra2FieldDefinition speed = new(
            "Speed",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer));
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            "[InfantryTypes]\n0=E1\n1=E2\n[E1]\nStrength=100\n[E2]\nSpeed=bad\n",
            provider: new TestFieldDefinitionProvider([armor, speed]));

        Ra2AutomationEditPreviewResult result = Preview(
            snapshot,
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel"));

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(result.AddedDiagnostics, diagnostic => diagnostic.Code == "FIELD_NUMBER_INVALID");
        Assert.DoesNotContain(result.RemovedDiagnostics, diagnostic => diagnostic.Code == "FIELD_NUMBER_INVALID");
    }

    [Fact]
    public void RepeatedAndParallelCalls_AreSemanticallyDeterministicWithFreshIdentity()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot("[E1]\nStrength=100\n");
        Ra2AutomationEditPlan plan = Plan(
            snapshot,
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel"));
        Ra2AutomationEditPreviewService service = new();

        Ra2AutomationEditPreviewResult[] results = Enumerable.Range(0, 16)
            .AsParallel()
            .Select(_ => service.Preview(snapshot, plan))
            .ToArray();

        Assert.All(results, result => Assert.True(result.Succeeded));
        Assert.Equal(16, results.Select(result => result.PreviewId).Distinct().Count());
        Assert.Single(results.Select(result => result.CandidateText).Distinct(StringComparer.Ordinal));
        Assert.Single(results.Select(result => string.Join("|", result.Changes.Select(
            change => $"{change.Span.Start}:{change.Span.Length}:{change.NewText}:{change.Reason}"))).Distinct());
    }

    [Fact]
    public void CancellationAndCurrentAnalysisFailure_AreTypedAndFatalExceptionsRethrow()
    {
        using CancellationTokenSource source = new();
        Ra2AutomationDocumentSnapshot canceling = Snapshot(
            "[E1]\nStrength=100\n",
            provider: new AutomationTestSupport.CancelingFieldDefinitionProvider(source));
        AssertFailure(
            Preview(
                canceling,
                Plan(canceling, Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel")),
                source.Token),
            Ra2AutomationEditPreviewFailureKind.Canceled);

        Ra2AutomationDocumentSnapshot throwing = Snapshot(
            "[E1]\nStrength=100\n",
            provider: new AutomationTestSupport.ThrowingFieldDefinitionProvider(new InvalidOperationException("secret")));
        Ra2AutomationEditPreviewResult failed = Preview(
            throwing,
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel"));
        AssertFailure(failed, Ra2AutomationEditPreviewFailureKind.CurrentAnalysisFailed);
        Assert.DoesNotContain("secret", failed.Message, StringComparison.Ordinal);

        Ra2AutomationDocumentSnapshot fatal = Snapshot(
            "[E1]\nStrength=100\n",
            provider: new AutomationTestSupport.ThrowingFieldDefinitionProvider(new OutOfMemoryException()));
        Assert.Throws<OutOfMemoryException>(() => Preview(
            fatal,
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel")));
    }

    [Fact]
    public void CandidateAnalysisFailure_IsDistinctAndDoesNotLeakExceptionText()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            "[E1]\nStrength=100\n",
            provider: new ThrowOnSecondTriggerLookupProvider());

        Ra2AutomationEditPreviewResult result = Preview(
            snapshot,
            Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Trigger", "yes"));

        AssertFailure(result, Ra2AutomationEditPreviewFailureKind.CandidateAnalysisFailed);
        Assert.DoesNotContain("candidate-secret", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DiagnosticResultLimit_IsTypedAndReturnsNoPartialPayload()
    {
        const int diagnosticCount = Ra2AutomationEditPreviewService.MaximumDiagnosticItems + 1;
        System.Text.StringBuilder text = new(diagnosticCount * 18 + 64);
        text.AppendLine("[InfantryTypes]");
        text.AppendLine("0=E1");
        text.AppendLine("[E1]");
        for (int index = 0; index < diagnosticCount; index++)
            text.Append("Flag").Append(index).AppendLine("=maybe");

        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            text.ToString(),
            provider: new DynamicBooleanProvider());
        Ra2AutomationEditPreviewResult result = Preview(
            snapshot,
            Operation(Ra2AutomationEditOperationKind.ReplaceFieldValue, "E1", "Flag0", "yes"));

        AssertFailure(result, Ra2AutomationEditPreviewFailureKind.ResultLimitExceeded);
    }

    [Fact]
    public void DocumentAndCandidateCharacterLimits_AreEnforcedWithoutPayload()
    {
        string overLimit = new(';', Ra2AutomationEditPreviewService.MaximumDocumentCharacters + 1);
        Ra2AutomationDocumentSnapshot oversized = Snapshot(overLimit);
        AssertFailure(
            Preview(
                oversized,
                Plan(oversized, Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel"))),
            Ra2AutomationEditPreviewFailureKind.DocumentTooLarge);

        const string prefix = "[E1]\n";
        string atLimit = prefix + new string(';', Ra2AutomationEditPreviewService.MaximumDocumentCharacters - prefix.Length);
        Ra2AutomationDocumentSnapshot candidateOversized = Snapshot(atLimit);
        AssertFailure(
            Preview(
                candidateOversized,
                Operation(Ra2AutomationEditOperationKind.UpsertField, "E1", "Armor", "steel")),
            Ra2AutomationEditPreviewFailureKind.DocumentTooLarge);
    }

    private static Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditOperation operation,
        CancellationToken cancellationToken = default)
        => Preview(snapshot, Plan(snapshot, operation), cancellationToken);

    private static Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default)
        => new Ra2AutomationEditPreviewService().Preview(snapshot, plan, cancellationToken);

    private static Ra2AutomationDocumentSnapshot Snapshot(
        string text,
        bool isEditable = true,
        IRa2FieldDefinitionProvider? provider = null,
        int version = 1)
        => new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            version,
            "rulesmd.ini",
            text,
            isEditable,
            new Ra2AutomationFieldRegistrySnapshot(
                provider ?? new AutomationTestSupport.EmptyFieldDefinitionProvider(),
                7));

    private static Ra2AutomationEditPlan Plan(
        Ra2AutomationDocumentSnapshot snapshot,
        params Ra2AutomationEditOperation[] operations)
        => new(
            Guid.NewGuid(),
            snapshot.DocumentId,
            snapshot.Version,
            snapshot.FieldRegistry.Revision,
            operations,
            "test",
            "tests");

    private static Ra2AutomationEditOperation Operation(
        Ra2AutomationEditOperationKind kind,
        string section,
        string key,
        string value)
        => new(kind, section, key, value);

    private static Ra2FieldDefinition DefineText(string key)
        => new(
            key,
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            registryQuality: "source-verified");

    private static void AssertFailure(
        Ra2AutomationEditPreviewResult result,
        Ra2AutomationEditPreviewFailureKind expected)
    {
        Assert.False(result.Succeeded);
        Assert.Equal(expected, result.FailureKind);
        Assert.Equal(Guid.Empty, result.PreviewId);
        Assert.Null(result.CandidateText);
        Assert.Empty(result.Changes);
        Assert.Empty(result.OperationPreviews);
        Assert.Empty(result.AddedDiagnostics);
        Assert.Empty(result.RemovedDiagnostics);
        Assert.Equal(0, result.AddedErrorCount);
        Assert.Equal(0, result.AddedWarningCount);
        Assert.False(result.RequiresExplicitConfirmation);
    }

    private sealed class TestFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly IReadOnlyList<Ra2FieldDefinition> _definitions;

        public TestFieldDefinitionProvider(IReadOnlyList<Ra2FieldDefinition> definitions)
        {
            _definitions = definitions;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(candidate =>
                candidate.AppliesTo.Contains(sectionKind) &&
                string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(definition => definition.AppliesTo.Contains(sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }

    private sealed class ThrowOnSecondTriggerLookupProvider : IRa2FieldDefinitionProvider
    {
        private int _triggerLookupCount;

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            CheckTrigger(key);

            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
        {
            CheckTrigger(key);
            return false;
        }

        private void CheckTrigger(string key)
        {
            if (string.Equals(key, "Trigger", StringComparison.OrdinalIgnoreCase) &&
                Interlocked.Increment(ref _triggerLookupCount) >= 2)
            {
                throw new InvalidOperationException("candidate-secret");
            }
        }
    }

    private sealed class DynamicBooleanProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = new Ra2FieldDefinition(
                key,
                [sectionKind],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean));
            return true;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => true;
    }
}
