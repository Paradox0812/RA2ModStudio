using RA2IniEditor.Core;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class CurrentFileReadonlyDiagnosticServiceTests
{
    [Fact]
    public void Analyze_WhenSnapshotIsLoaded_ReturnsCoreStructureIssues()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = new(
            "C:\\mod",
            "C:\\mod\\rules.ini",
            "rules.ini",
            """
            [E1]
            Name=GI
            Name=Duplicate
            """,
            7,
            SourceEditorState.Loaded);

        var issues = service.Analyze(snapshot);

        var issue = Assert.Single(issues);
        Assert.Equal("INI_STRUCTURE", issue.Code);
        Assert.Equal("CoreParserValidator", issue.SourceKind);
        Assert.Equal("C:\\mod\\rules.ini", issue.FilePath);
        Assert.Equal(7, issue.Version);
        Assert.Equal(3, issue.LineNumber);
        Assert.Null(issue.ColumnNumber);
        Assert.Equal("E1", issue.SectionId);
        Assert.Equal("Name", issue.Key);
    }

    [Theory]
    [InlineData(SourceEditorState.Empty)]
    [InlineData(SourceEditorState.Loading)]
    [InlineData(SourceEditorState.DeferredLargeFile)]
    [InlineData(SourceEditorState.ReadFailed)]
    public void Analyze_WhenSnapshotIsNotLoaded_ReturnsNoIssues(SourceEditorState state)
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = new("C:\\mod", "C:\\mod\\rules.ini", "rules.ini", "unknown text", 9, state);

        var issues = service.Analyze(snapshot);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_WhenParserOrValidatorThrows_ReturnsDiagnosticExceptionIssue()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = new(
            "C:\\mod",
            "C:\\mod\\rules.ini",
            "rules.ini",
            null!,
            12,
            SourceEditorState.Loaded);

        var issue = Assert.Single(service.Analyze(snapshot));

        Assert.Equal(CurrentFileReadonlyDiagnosticService.DiagnosticExceptionCode, issue.Code);
        Assert.Equal("DiagnosticService", issue.SourceKind);
        Assert.Equal("Diagnostic Service", issue.SourceText);
        Assert.Equal(IniIssueSeverity.Error, issue.Severity);
        Assert.StartsWith("Diagnostics failed:", issue.Message, StringComparison.Ordinal);
        Assert.Equal("C:\\mod\\rules.ini", issue.FilePath);
        Assert.Equal(12, issue.Version);
        Assert.Null(issue.LineNumber);
        Assert.Null(issue.ColumnNumber);
        Assert.Null(issue.SectionId);
        Assert.Null(issue.Key);
        Assert.Equal("-", issue.LocationText);
    }

    [Fact]
    public void Analyze_WhenSnapshotIsNull_ReturnsDiagnosticExceptionIssue()
    {
        CurrentFileReadonlyDiagnosticService service = new();

        var issue = Assert.Single(service.Analyze(null));

        Assert.Equal(CurrentFileReadonlyDiagnosticService.DiagnosticExceptionCode, issue.Code);
        Assert.Equal("DiagnosticService", issue.SourceKind);
        Assert.Equal(IniIssueSeverity.Error, issue.Severity);
        Assert.Equal(string.Empty, issue.FilePath);
        Assert.Equal(0, issue.Version);
        Assert.Null(issue.LineNumber);
    }

    [Fact]
    public void Analyze_WhenLoadedTextIsEmpty_FollowsCoreBehaviorWithoutThrowing()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = new("C:\\mod", "C:\\mod\\empty.ini", "empty.ini", string.Empty, 13, SourceEditorState.Loaded);

        var issues = service.Analyze(snapshot);

        Assert.Empty(issues);
    }

    [Fact]
    public void Constructor_DoesNotRequireFileOrProjectServices()
    {
        var constructors = typeof(CurrentFileReadonlyDiagnosticService).GetConstructors();

        Assert.Contains(constructors, constructor => constructor.GetParameters().Length == 0);
        Assert.DoesNotContain(
            constructors.SelectMany(constructor => constructor.GetParameters()),
            parameter => parameter.ParameterType == typeof(IIniFileStore) ||
                         parameter.ParameterType.Name.Contains("ProjectOpenService", StringComparison.Ordinal));
    }

    [Fact]
    public void Analyze_UsesSnapshotTextAndDoesNotRequireFileToExist()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = new(
            "C:\\missing",
            "C:\\missing\\missing.ini",
            "missing.ini",
            "unknown text",
            11,
            SourceEditorState.Loaded);

        var issue = Assert.Single(service.Analyze(snapshot));

        Assert.Equal("C:\\missing\\missing.ini", issue.FilePath);
        Assert.Equal(11, issue.Version);
        Assert.Equal(1, issue.LineNumber);
    }
}
