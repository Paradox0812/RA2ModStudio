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
        string[] fileNames =
        [
            "Ra2AuthoringSnapshot.cs",
            "Ra2IniEditOperation.cs",
            "Ra2IniEditPlan.cs",
            "Ra2IniEditPreview.cs",
            "IRa2IniEditPreviewService.cs",
            "Ra2IniEditPreviewService.cs",
            "Ra2IniEditPreviewCurrency.cs",
            "Ra2IniEditApplyResult.cs",
            "IRa2EditorTransactionPort.cs",
            "Ra2IniAuthoringWorkspace.cs"
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

        foreach (string fileName in fileNames)
        {
            string source = File.ReadAllText(Path.Combine(
                root,
                "RA2IniEditor.IDE",
                "Editing",
                fileName));
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
    public void Preview_CancellationAfterCurrentAnalysisStopsBeforeCandidateAnalysis()
    {
        using CancellationTokenSource cancellation = new();
        CancelAfterFirstAnalysisService analysis = new(
            new Ra2IniLanguageAnalysisService(),
            cancellation);
        Ra2IniEditPreviewService service = new(analysis, new Ra2AddPropertyInsertPlanner());
        Ra2AuthoringSnapshot snapshot = Snapshot("[E1]\nStrength=100");

        Ra2IniEditPreview preview = service.Preview(
            snapshot,
            Plan(snapshot),
            cancellation.Token);

        Assert.Equal(Ra2IniEditPreviewFailureKind.Canceled, preview.FailureKind);
        Assert.Equal(1, analysis.CallCount);
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

    private static Ra2AuthoringSnapshot Snapshot(string text)
    {
        Ra2EditableDocumentSessionService sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        Ra2EditableDocumentSession session = sessionService.StartEditing("rulesmd.ini", text);
        Ra2FieldRegistryProviderSnapshot registry = new(
            new BuiltInRa2FieldDefinitionProvider(),
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

    private sealed class CancelAfterFirstAnalysisService : IRa2IniLanguageAnalysisService
    {
        private readonly IRa2IniLanguageAnalysisService _inner;
        private readonly CancellationTokenSource _cancellation;

        public CancelAfterFirstAnalysisService(
            IRa2IniLanguageAnalysisService inner,
            CancellationTokenSource cancellation)
        {
            _inner = inner;
            _cancellation = cancellation;
        }

        public int CallCount { get; private set; }

        public Ra2IniLanguageAnalysisResult Analyze(Ra2LanguageAnalysisRequest request)
        {
            CallCount++;
            Ra2IniLanguageAnalysisResult result = _inner.Analyze(request);
            if (CallCount == 1)
                _cancellation.Cancel();

            return result;
        }
    }
}
