using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniEditPlanContractTests
{
    [Fact]
    public void Operation_NormalizesSectionAndKeyButPreservesValue()
    {
        Ra2IniEditOperation operation = new(
            Ra2IniEditOperationKind.UpsertField,
            " E1 ",
            " Strength ",
            " 125 ");

        Assert.Equal("E1", operation.SectionName);
        Assert.Equal("Strength", operation.Key);
        Assert.Equal(" 125 ", operation.Value);
    }

    [Theory]
    [InlineData("A\nB", "Key", "Value")]
    [InlineData("[A]", "Key", "Value")]
    [InlineData("A", "K=ey", "Value")]
    [InlineData("A", "Key", "A\rB")]
    [InlineData("A", "Key", "A\0B")]
    public void Operation_RejectsUnsafeStructuredText(string section, string key, string value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Ra2IniEditOperation(
            Ra2IniEditOperationKind.UpsertField,
            section,
            key,
            value));
    }

    [Fact]
    public void Plan_DefensivelyCopiesOperations()
    {
        List<Ra2IniEditOperation> operations =
        [
            new(Ra2IniEditOperationKind.UpsertField, "E1", "Strength", "125")
        ];

        Ra2IniEditPlan plan = CreatePlan(operations);
        operations.Clear();

        Assert.Single(plan.Operations);
    }

    [Fact]
    public void Plan_RejectsEmptyAndOversizedOperationCollections()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePlan([]));

        Ra2IniEditOperation operation = new(
            Ra2IniEditOperationKind.UpsertField,
            "E1",
            "Strength",
            "125");
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePlan(
            Enumerable.Repeat(operation, Ra2IniEditPlan.MaximumOperationCount + 1)));
    }

    [Theory]
    [InlineData("summary\ninjection", "AI")]
    [InlineData("summary", "AI\rsource")]
    [InlineData("", "AI")]
    public void Plan_RejectsUnsafeDisplayText(string summary, string origin)
    {
        Assert.ThrowsAny<ArgumentException>(() => CreatePlan(
            [new(Ra2IniEditOperationKind.UpsertField, "E1", "Strength", "125")],
            summary,
            origin));
    }

    private static Ra2IniEditPlan CreatePlan(
        IEnumerable<Ra2IniEditOperation> operations,
        string summary = "Update E1",
        string origin = "BuiltInAI")
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            expectedEditRevision: 2,
            expectedFieldRegistryRevision: 3,
            operations,
            summary,
            origin);
}
