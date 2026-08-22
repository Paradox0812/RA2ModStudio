using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Search;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CurrentFileReplacePlannerTests
{
    private readonly Ra2EditableDocumentSessionService _sessionService = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());
    private readonly Ra2CurrentFileReplacePlanner _planner = new();

    [Fact]
    public void Plan_LiteralReplaceAllBuildsPreviewWithoutMutatingSession()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing(
            "rulesmd.ini",
            "Primary=Gun\nSecondary=Gun");

        Ra2CurrentFileReplacePlan plan = _planner.Plan(session, Options("Gun"), "Laser");

        Assert.True(plan.Success);
        Assert.Equal(2, plan.MatchCount);
        Assert.Equal("Primary=Laser\nSecondary=Laser", plan.UpdatedText);
        Assert.Equal("Primary=Gun\nSecondary=Gun", session.DocumentState.CurrentText);
        Assert.True(plan.IsCurrentFor(session));
    }

    [Fact]
    public void Plan_RegexReplacementSupportsCaptureGroups()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing(
            "rulesmd.ini",
            "Weapon1=Gun\nWeapon2=Laser");

        Ra2CurrentFileReplacePlan plan = _planner.Plan(
            session,
            Options(@"Weapon(\d)=(\w+)", useRegex: true),
            "Slot$1=$2");

        Assert.True(plan.Success);
        Assert.Equal("Slot1=Gun\nSlot2=Laser", plan.UpdatedText);
    }

    [Fact]
    public void Plan_RejectsProjectScope()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing("rulesmd.ini", "Gun");

        Ra2CurrentFileReplacePlan plan = _planner.Plan(
            session,
            Options("Gun") with { Scope = Ra2SearchScope.Project },
            "Laser");

        Assert.Equal(Ra2ReplaceFailureKind.ProjectScopeNotSupported, plan.FailureKind);
    }

    [Fact]
    public void Plan_RejectsZeroLengthRegexMatch()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing("rulesmd.ini", "ABC");

        Ra2CurrentFileReplacePlan plan = _planner.Plan(
            session,
            Options("^", useRegex: true),
            "Prefix");

        Assert.Equal(Ra2ReplaceFailureKind.ZeroLengthMatch, plan.FailureKind);
    }

    [Fact]
    public void Plan_NoOpReplacementReturnsNoChanges()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing("rulesmd.ini", "Gun");

        Ra2CurrentFileReplacePlan plan = _planner.Plan(session, Options("Gun"), "Gun");

        Assert.Equal(Ra2ReplaceFailureKind.NoChanges, plan.FailureKind);
    }

    [Fact]
    public void Plan_BecomesStaleAfterSameDocumentEdit()
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing("rulesmd.ini", "Gun");
        Ra2CurrentFileReplacePlan plan = _planner.Plan(session, Options("Gun"), "Laser");

        Ra2EditableDocumentSession edited = _sessionService.UpdateText(session, "Gun\nArmor=steel");

        Assert.False(plan.IsCurrentFor(edited));
    }

    [Fact]
    public void Plan_IsRejectedForDifferentDocumentIdentity()
    {
        Ra2EditableDocumentSession first = _sessionService.StartEditing("rulesmd.ini", "Gun");
        Ra2CurrentFileReplacePlan plan = _planner.Plan(first, Options("Gun"), "Laser");
        Ra2EditableDocumentSession second = _sessionService.StartEditing("rulesmd.ini", "Gun");

        Assert.False(plan.IsCurrentFor(second));
    }

    private static Ra2SearchOptions Options(string query, bool useRegex = false)
        => new(query, Ra2SearchScope.CurrentFile, false, false, useRegex, "*.ini");
}
