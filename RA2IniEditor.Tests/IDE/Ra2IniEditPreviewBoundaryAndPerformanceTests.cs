using System.Diagnostics;
using System.Text;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using Xunit;
using Xunit.Abstractions;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniEditPreviewBoundaryAndPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public Ra2IniEditPreviewBoundaryAndPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void AuthoringSources_DoNotReferenceUiShellAiWriterOrFileMutation()
    {
        string root = TestRepositoryRoot.Find();
        string[] relativePaths =
        [
            "RA2IniEditor.IDE/Editing/Ra2AuthoringSnapshot.cs",
            "RA2IniEditor.IDE/Editing/Ra2IniEditPreview.cs",
            "RA2IniEditor.IDE/Editing/IRa2IniEditPreviewService.cs",
            "RA2IniEditor.IDE/Editing/Ra2IniEditPreviewService.cs",
            "RA2IniEditor.IDE/Editing/Ra2IniEditPreviewCurrency.cs",
            "RA2IniEditor.IDE/Editing/Ra2IniEditApplyResult.cs",
            "RA2IniEditor.IDE/Editing/IRa2EditorTransactionPort.cs",
            "RA2IniEditor.IDE/Editing/Ra2IniAuthoringWorkspace.cs",
            "RA2IniEditor.Application/Automation/Experimental/Ra2AutomationEditContracts.cs",
            "RA2IniEditor.Application/Automation/Experimental/Ra2AutomationEditPreviewService.cs",
            "RA2IniEditor.Application/Editing/Ra2AutomationEditPreviewEngine.cs"
        ];
        string[] forbiddenTokens =
        [
            "System.Windows",
            "ICSharpCode.AvalonEdit",
            "ShellWindow",
            "ViewModels",
            "RA2IniEditor.IDE.AI",
            "RA2IniEditor.IDE.Search",
            "Ra2Save",
            "Writer",
            "File.Write",
            "Directory.",
            "FieldRegistryRuntimeService"
        ];

        foreach (string relativePath in relativePaths)
        {
            string source = File.ReadAllText(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            foreach (string token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PreviewContract_HasNoDirectApplySurface()
    {
        Assert.Null(typeof(Ra2IniEditPreview).GetMethod("Apply"));
        Assert.Null(typeof(IRa2IniEditPreviewService).GetMethod("Apply"));
        Assert.NotNull(typeof(IRa2IniEditPreviewService).GetMethod("Preview"));
    }

    [Fact]
    public void Preview_CancellationDuringCurrentAnalysisReturnsTypedFailure()
    {
        using CancellationTokenSource cancellation = new();
        Ra2IniEditPreviewService service = new(
            new Ra2IniLanguageAnalysisService(),
            new Ra2AddPropertyInsertPlanner());
        Ra2AuthoringSnapshot snapshot = Snapshot(
            "[E1]\nStrength=100",
            new CancelingProvider(cancellation));

        Ra2IniEditPreview preview = service.Preview(
            snapshot,
            Plan(snapshot),
            cancellation.Token);

        Assert.Equal(Ra2IniEditPreviewFailureKind.Canceled, preview.FailureKind);
        Assert.Null(preview.CandidateText);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(7)]
    public void Preview_LargeDocumentRecordsCompletionWithoutMutatingSource(int approximateMegabytes)
    {
        string text = BuildLargeDocument(approximateMegabytes);
        Ra2AuthoringSnapshot snapshot = Snapshot(text);
        Ra2IniEditPreviewService service = new(
            new Ra2IniLanguageAnalysisService(),
            new Ra2AddPropertyInsertPlanner());
        Stopwatch stopwatch = Stopwatch.StartNew();

        Ra2IniEditPreview preview = service.Preview(snapshot, Plan(snapshot));

        stopwatch.Stop();
        _output.WriteLine(
            "{0} MiB preview: {1} ms; source chars={2}; candidate chars={3}",
            approximateMegabytes,
            stopwatch.ElapsedMilliseconds,
            text.Length,
            preview.CandidateText?.Length ?? 0);
        Assert.True(preview.Succeeded, preview.Message);
        Assert.Equal(text, snapshot.Text);
        Assert.NotEqual(text, preview.CandidateText);
    }

    private static string BuildLargeDocument(int approximateMegabytes)
    {
        int targetLength = approximateMegabytes * 1024 * 1024;
        StringBuilder builder = new(targetLength + 64);
        builder.AppendLine("[E1]");
        builder.AppendLine("Strength=100");
        const string filler = "; authoring preview performance filler 0123456789\n";
        while (builder.Length < targetLength)
            builder.Append(filler);

        return builder.ToString();
    }

    private static Ra2AuthoringSnapshot Snapshot(
        string text,
        IRa2FieldDefinitionProvider? provider = null)
    {
        Ra2EditableDocumentSessionService sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        Ra2EditableDocumentSession session = sessionService.StartEditing("rulesmd.ini", text);
        Ra2FieldRegistryProviderSnapshot registry = new(
            provider ?? new BuiltInRa2FieldDefinitionProvider(),
            revision: 1);
        return Assert.IsType<Ra2AuthoringSnapshot>(
            Ra2AuthoringSnapshot.Capture(session, text, string.Empty, registry).Snapshot);
    }

    private static Ra2IniEditPlan Plan(Ra2AuthoringSnapshot snapshot)
        => new(
            Guid.NewGuid(),
            snapshot.DocumentId,
            snapshot.EditRevision,
            snapshot.FieldRegistry.Revision,
            [new Ra2IniEditOperation(
                Ra2IniEditOperationKind.ReplaceFieldValue,
                "E1",
                "Strength",
                "125")],
            "Large document preview",
            "Tests");

    private sealed class CancelingProvider : IRa2FieldDefinitionProvider
    {
        private readonly CancellationTokenSource _cancellation;

        public CancelingProvider(CancellationTokenSource cancellation)
            => _cancellation = cancellation;

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            _cancellation.Cancel();
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => false;
    }
}
