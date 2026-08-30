using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AuthoringDiff;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.AI;
using RA2IniEditor.Core.Schema;
using System.Text.Json;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiProjectAuthoringIntegrationTests
{
    [Fact]
    public void ProjectAdmission_YrTestFolderShapeRecognizesMdPairWithEmptyArtAndUnrelatedIniFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), "Ra2AiProjectAdmissionTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "rulesmd.ini"), "[VehicleTypes]\n4=HTNK\n\n[HTNK]\nImage=HTNK\n");
            File.WriteAllText(Path.Combine(root, "artmd.ini"), string.Empty);
            File.WriteAllText(Path.Combine(root, "ddraw.ini"), "[ddraw]\nrenderer=auto\n");
            File.WriteAllText(Path.Combine(root, "RA2MD.INI"), "[Video]\n");
            File.WriteAllText(Path.Combine(root, "Register.ini"), "[Register]\n");

            ProjectOpenResult project = new ProjectOpenService().OpenFolderReadonly(root);
            Ra2AiProjectTargetResolution resolution = Ra2AiProjectAuthoringAdmission.ResolveRulesArtTargets(
                project.Files.Select(file => file.FilePath));

            Assert.Equal(5, project.TotalIniFileCount);
            Assert.True(resolution.Succeeded);
            Assert.Equal(["rulesmd.ini", "artmd.ini"], resolution.TargetFilePaths.Select(Path.GetFileName));
            Assert.Equal(0, Assert.Single(project.Files, file => file.FileName.Equals("artmd.ini", StringComparison.OrdinalIgnoreCase)).FileSizeBytes);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ProjectAdmission_ResolvesOnlyOneCompletePairInStableRulesArtOrder()
    {
        Ra2AiProjectTargetResolution md = Ra2AiProjectAuthoringAdmission.ResolveRulesArtTargets(
            ["C:\\mod\\artmd.ini", "C:\\mod\\rulesmd.ini", "C:\\mod\\map.ini"]);
        Ra2AiProjectTargetResolution classic = Ra2AiProjectAuthoringAdmission.ResolveRulesArtTargets(
            ["C:\\mod\\art.ini", "C:\\mod\\rules.ini"]);
        Ra2AiProjectTargetResolution missing = Ra2AiProjectAuthoringAdmission.ResolveRulesArtTargets(
            ["C:\\mod\\rulesmd.ini"]);
        Ra2AiProjectTargetResolution both = Ra2AiProjectAuthoringAdmission.ResolveRulesArtTargets(
            ["C:\\mod\\rulesmd.ini", "C:\\mod\\artmd.ini", "C:\\mod\\rules.ini", "C:\\mod\\art.ini"]);
        Ra2AiProjectTargetResolution duplicate = Ra2AiProjectAuthoringAdmission.ResolveRulesArtTargets(
            ["C:\\a\\rulesmd.ini", "C:\\b\\rulesmd.ini", "C:\\mod\\artmd.ini"]);

        Assert.True(md.Succeeded);
        Assert.Equal(["rulesmd.ini", "artmd.ini"], md.TargetFilePaths.Select(Path.GetFileName));
        Assert.True(classic.Succeeded);
        Assert.Equal(["rules.ini", "art.ini"], classic.TargetFilePaths.Select(Path.GetFileName));
        Assert.Equal(Ra2AiProjectEditAvailabilityKind.PairMissing, missing.Availability);
        Assert.Equal(Ra2AiProjectEditAvailabilityKind.PairAmbiguous, both.Availability);
        Assert.Equal(Ra2AiProjectEditAvailabilityKind.PairAmbiguous, duplicate.Availability);
    }

    [Theory]
    [InlineData("art-animation", "field")]
    [InlineData("art-animation", "complete")]
    [InlineData("techno", "field")]
    [InlineData("reference-registration", "complete")]
    [InlineData("superweapon", "none")]
    public void IntentAnalysis_ProjectCapabilityUsesProjectAvailabilityAndOneDedicatedTool(
        string providerDomainIntentId,
        string completionLevel)
    {
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "intent-1",
                Ra2AiIntentAnalysisStage.ToolName,
                $$"""
                {
                  "outcome":"authoring",
                  "capability_id":"techno-rules-art-binding",
                  "domain_intent_id":"{{providerDomainIntentId}}",
                  "request_summary":"Bind HTNK to art and future asset ids",
                  "completion_level":"{{completionLevel}}",
                  "constraints":["do not create asset files"],
                  "selected_skill_ids":["ra2-rules-art-binding"],
                  "knowledge_gaps":[]
                }
                """)
        ]);

        Assert.True(Ra2AiIntentAnalysisStage.TryParse(response, out Ra2AiIntentAnalysisPackage? package, out _));
        Assert.Equal("art-animation", package!.DomainIntentId);
        Assert.Equal(Ra2AiIntentCompletionLevel.Field, package.CompletionLevel);
        Ra2AiInteractionRoute available = Ra2AiIntentAnalysisStage.ResolveRoute(
            package,
            new Ra2AiAuthoringAvailability(
                Ra2AiEditAvailabilityKind.Available,
                Ra2AiProjectEditAvailabilityKind.Available));
        Ra2AiInteractionRoute unavailable = Ra2AiIntentAnalysisStage.ResolveRoute(
            package!,
            new Ra2AiAuthoringAvailability(
                Ra2AiEditAvailabilityKind.Available,
                Ra2AiProjectEditAvailabilityKind.PairMissing));

        Assert.Equal(Ra2AiCapabilityMode.ProjectRulesArtBindingPreview, available.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.ProjectRulesArtBindingExplicit, available.Kind);
        Assert.Equal(Ra2AiInteractionRouteKind.EditUnavailable, unavailable.Kind);
        Ra2AiToolDefinition tool = Assert.Single(Ra2AiAuthoringToolCatalog.GetTools(available.CapabilityMode));
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName, tool.Name);
        Assert.Contains("documents", tool.ParametersJsonSchema, StringComparison.Ordinal);
        Assert.Contains("operations", tool.ParametersJsonSchema, StringComparison.Ordinal);
        Assert.DoesNotContain("template_id", tool.ParametersJsonSchema, StringComparison.Ordinal);
        using (JsonDocument schema = JsonDocument.Parse(tool.ParametersJsonSchema))
        {
            string[] requiredDocumentProperties = schema.RootElement
                .GetProperty("properties")
                .GetProperty("documents")
                .GetProperty("items")
                .GetProperty("required")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray();
            Assert.Equal(["target", "operations"], requiredDocumentProperties);
        }

        Ra2AiResponse unknownDomain = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "intent-2",
                Ra2AiIntentAnalysisStage.ToolName,
                """{"outcome":"authoring","capability_id":"techno-rules-art-binding","domain_intent_id":"not-a-domain","request_summary":"bind","completion_level":"field","constraints":[],"selected_skill_ids":[],"knowledge_gaps":[]}""")
        ]);
        Assert.True(Ra2AiIntentAnalysisStage.TryParse(
            unknownDomain,
            out Ra2AiIntentAnalysisPackage? recovered,
            out string recoveryFailure),
            recoveryFailure);
        Assert.Equal("art-animation", recovered!.DomainIntentId);
    }

    [Fact]
    public void Adapter_ProjectToolCreatesClosedPlanAndManifestButRejectsDocumentScope()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiToolCall call = ProjectCall();
        Ra2AiAuthoringToolAdapter adapter = new();

        Ra2AiEditPlanCreationResult result = adapter.TryCreatePlan(
            call,
            Ra2AiAuthoringRequestContext.ForProject(
                snapshot,
                snapshot.Documents.Select(document => document.FilePath).ToArray()));
        Ra2AiEditPlanCreationResult mismatch = adapter.TryCreatePlan(
            call,
            new Ra2AiAuthoringRequestContext(DocumentSnapshot()));

        Assert.True(result.Succeeded, result.Message);
        Assert.NotNull(result.ProjectPlan);
        Assert.Equal(2, result.ProjectPlan!.DocumentPlans.Count);
        Assert.NotNull(result.AssetManifest);
        Assert.Equal(2, result.AssetManifest!.Requirements.Count);
        Assert.Null(result.Plan);
        Assert.Equal(Ra2AiEditProposalFailureKind.UnsupportedTool, mismatch.FailureKind);
    }

    [Fact]
    public void Adapter_GenericProjectPlanLetsModelOwnFieldsAndCreatesMissingSectionsWithoutManifest()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext projectContext = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiAuthoringToolAdapter adapter = new();

        Ra2AiEditPlanCreationResult result = adapter.TryCreatePlan(GenericProjectCall(), projectContext);
        Ra2AiEditPlanCreationResult wrongScope = adapter.TryCreatePlan(
            GenericProjectCall(),
            new Ra2AiAuthoringRequestContext(DocumentSnapshot()));

        Assert.True(result.Succeeded, result.Message);
        Assert.Null(result.AssetManifest);
        Ra2AutomationProjectEditPlan plan = Assert.IsType<Ra2AutomationProjectEditPlan>(result.ProjectPlan);
        Assert.Equal(2, plan.DocumentPlans.Count);
        Assert.Equal(4, plan.DocumentPlans.Sum(document => document.Operations.Count));
        Ra2AutomationEditPlan artPlan = plan.DocumentPlans.Single(document =>
            document.ExpectedDocumentId == snapshot.Documents[1].DocumentId);
        Assert.Equal("HTNKART", Assert.Single(artPlan.SectionCreations).SectionName);
        Assert.Equal(Ra2SectionKind.Unknown, Assert.Single(artPlan.SectionCreations).ExpectedSectionKind);
        Assert.Contains(artPlan.Operations, operation => operation.Key == "UnregisteredCustomField");
        Assert.Equal(Ra2AiEditProposalFailureKind.UnsupportedTool, wrongScope.FailureKind);
    }

    [Fact]
    public void Adapter_GenericProjectPlanKeepsOnlyStructuralScopeAndResourceGuards()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiAuthoringToolAdapter adapter = new();

        Ra2AiEditPlanCreationResult clarification = adapter.TryCreatePlan(
            new Ra2AiToolCall(
                "clarify",
                Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                """{"outcome":"needs_clarification","message":"当前上下文不能确认目标对象是否存在。"}"""),
            context);
        Ra2AiEditPlanCreationResult pathTarget = adapter.TryCreatePlan(
            new Ra2AiToolCall(
                "path",
                Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                """{"outcome":"proposal","documents":[{"target":"../rulesmd.ini","operations":[{"kind":"upsert_field","section":"HTNK","key":"Image","value":"X"}]}]}"""),
            context);
        Ra2AiEditPlanCreationResult duplicateTarget = adapter.TryCreatePlan(
            new Ra2AiToolCall(
                "duplicate",
                Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                """{"outcome":"proposal","documents":[{"target":"rules","operations":[{"kind":"upsert_field","section":"HTNK","key":"Image","value":"A"}]},{"target":"RULES","operations":[{"kind":"upsert_field","section":"HTNK","key":"Cameo","value":"B"}]}]}"""),
            context);
        Ra2AiEditPlanCreationResult unknownProperty = adapter.TryCreatePlan(
            new Ra2AiToolCall(
                "unknown",
                Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                """{"outcome":"PROPOSAL","confidence":0.9,"documents":{"target":"rules","path":"C:\\escape.ini","operations":{"kind":"Upsert-Field","section":"HTNK","key":"Image","value":"X","reason":"model note"}}}"""),
            context);

        Assert.True(clarification.NeedsClarification);
        Assert.Contains("目标对象", clarification.Message, StringComparison.Ordinal);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, pathTarget.FailureKind);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidOperation, duplicateTarget.FailureKind);
        Assert.True(unknownProperty.Succeeded, unknownProperty.Message);
        Assert.Equal(
            snapshot.Documents.Single(document => Path.GetFileName(document.FilePath).Equals("rulesmd.ini", StringComparison.OrdinalIgnoreCase)).DocumentId,
            Assert.Single(unknownProperty.ProjectPlan!.DocumentPlans).ExpectedDocumentId);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("42")]
    public void Adapter_GenericProjectProposalIgnoresNonExecutablePresentationDrift(string displayJson)
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiToolCall call = new(
            "project-display-drift",
            Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
            $$"""
            {
              "outcome":"PRO-POSAL",
              "summary":{{displayJson}},
              "message":{{displayJson}},
              "documents":{
                "target":"rules",
                "operations":{"kind":"upsert_field","section":"HTNK","key":"Secondary","value":"HTNKSupportWeapon"}
              }
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(call, context);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal("AI 项目结构化修改建议", result.ProjectPlan!.Summary);
        Assert.Single(result.ProjectPlan.DocumentPlans);
    }

    [Fact]
    public void Adapter_GenericProjectClarificationKeepsEchoedDocumentsInert()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiToolCall call = new(
            "project-clarification-echo",
            Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
            """
            {
              "outcome":"NEEDS_CLARIFICATION",
              "message":"请确认投放单位。",
              "documents":{"target":"rules","operations":{"kind":"upsert_field","section":"HTNK","key":"Secondary","value":"MustRemainInert"}}
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(call, context);

        Assert.True(result.NeedsClarification);
        Assert.Null(result.ProjectPlan);
        Assert.Equal("请确认投放单位。", result.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    public void Adapter_GenericProjectClarificationStillRequiresReadableMessage(string messageJson)
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        string messageProperty = string.IsNullOrEmpty(messageJson)
            ? string.Empty
            : $",\"message\":{messageJson}";
        Ra2AiToolCall call = new(
            "project-clarification-invalid-message",
            Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
            $"{{\"outcome\":\"needs_clarification\"{messageProperty}}}");

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(call, context);

        Assert.Equal(Ra2AiToolAdaptationOutcomeKind.Failed, result.OutcomeKind);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, result.FailureKind);
        Assert.Null(result.ProjectPlan);
    }

    [Fact]
    public async Task Coordinator_GenericProjectPlanKeepsDiagnosticsAdvisoryAndAppliesWithoutManifest()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        RecordingPort port = new();
        Ra2AiAuthoringCoordinator coordinator = new(
            new Ra2AiAuthoringToolAdapter(),
            new Ra2IniAuthoringWorkspace(
                new Ra2IniEditPreviewService(new Ra2AutomationCapabilityGateway()),
                port,
                new Ra2ProjectEditPreviewService()));

        Ra2AiEditProposalResult prepared = coordinator.PrepareProposal(
            context,
            context,
            Ra2AiResponse.CreateToolCalls([GenericProjectCall()]),
            CancellationToken.None);

        Assert.True(prepared.Succeeded, prepared.Message);
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(prepared.Proposal);
        Assert.Null(proposal.AssetManifest);
        Assert.NotEqual(Ra2AiEditProposalApplyPolicy.Blocked, proposal.ApplyPolicy);
        Ra2AiEditProposalViewModel card = new(proposal);
        Assert.Empty(card.AssetManifestSummary);
        Assert.True(card.IsApplyEnabled);

        Ra2AuthoringDiffViewModel diff = new(card);
        await diff.LoadAsync(CancellationToken.None);
        Assert.Equal(2, diff.Rows.Count(row => row.Kind == Ra2AuthoringDiffRowKind.FileHeader));
        diff.Dispose();

        Ra2AiEditProposalApplyResult applied = coordinator.ApplyConfirmed(proposal);
        Assert.True(applied.Succeeded, applied.Message);
        Assert.Equal(1, port.ProjectApplyCount);
    }

    [Fact]
    public void Adapter_ProjectToolCanonicalizesKnownArgumentCasingAndDerivesOptionalAssetBrief()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiToolCall call = new(
            "project-derived-brief",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            """
            {
              "outcome":"proposal",
              "template_id":"techno-rules-art-asset-binding",
              "template_version":1,
              "arguments":{
                "OwnerSectionId":"HTNK",
                "ARTSECTIONID":"HTNKART",
                "bodyAssetId":"HTNKBODY",
                "cameoAssetId":"HTNKICON"
              }
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            call,
            Ra2AiAuthoringRequestContext.ForProject(
                snapshot,
                snapshot.Documents.Select(document => document.FilePath).ToArray()));

        Assert.True(result.Succeeded, result.Message);
        Assert.NotNull(result.AssetManifest);
        Assert.All(result.AssetManifest!.Requirements, requirement =>
            Assert.Contains("HTNK", requirement.GenerationBrief, StringComparison.Ordinal));
        Assert.Equal(
            ["HTNKART", "HTNKBODY", "HTNKICON"],
            result.ProjectPlan!.DocumentPlans.SelectMany(plan => plan.Operations).Select(operation => operation.Value));
    }

    [Fact]
    public void Adapter_ProjectToolTreatsEmptyBriefAsOmittedAndNormalizesOneShpSuffix()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiToolCall call = new(
            "project-empty-brief",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            """
            {
              "outcome":"proposal",
              "template_id":"techno-rules-art-asset-binding",
              "template_version":1,
              "arguments":{
                "ownerSectionId":"HTNK",
                "artSectionId":"HTNKART",
                "bodyAssetId":"HTNKBODY.shp",
                "cameoAssetId":"HTNKICON.SHP",
                "assetBrief":""
              }
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            call,
            Ra2AiAuthoringRequestContext.ForProject(
                snapshot,
                snapshot.Documents.Select(document => document.FilePath).ToArray()));

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(
            ["HTNKART", "HTNKBODY", "HTNKICON"],
            result.ProjectPlan!.DocumentPlans.SelectMany(plan => plan.Operations).Select(operation => operation.Value));
        Assert.Equal(["HTNKBODY.shp", "HTNKICON.shp"], result.AssetManifest!.Requirements.Select(item => item.FileName));
    }

    [Fact]
    public void Adapter_ProjectToolStillRejectsUnknownAndCaseInsensitiveDuplicateArguments()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiToolCall unknown = ProjectCallWithExtraArguments("\"unexpected\":\"x\"");
        Ra2AiToolCall duplicate = ProjectCallWithExtraArguments("\"OwnerSectionId\":\"OTHER\"");

        Ra2AiEditPlanCreationResult unknownResult = new Ra2AiAuthoringToolAdapter().TryCreatePlan(unknown, context);
        Ra2AiEditPlanCreationResult duplicateResult = new Ra2AiAuthoringToolAdapter().TryCreatePlan(duplicate, context);

        Assert.Equal(Ra2AiEditProposalFailureKind.UnknownArgumentProperty, unknownResult.FailureKind);
        Assert.Equal(Ra2AiEditProposalFailureKind.DuplicateArgumentProperty, duplicateResult.FailureKind);
    }

    [Fact]
    public void Adapter_ProjectTemplateFailuresExposeActionableLocalCategories()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiToolCall duplicateAssetIds = new(
            "project-duplicate-assets",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            ProjectCall().ArgumentsJson.Replace(
                "\"cameoAssetId\":\"HTNKICON\"",
                "\"cameoAssetId\":\"HTNKBODY\"",
                StringComparison.Ordinal));

        Ra2AiEditPlanCreationResult invalidAssets = new Ra2AiAuthoringToolAdapter()
            .TryCreatePlan(duplicateAssetIds, context);

        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidOperation, invalidAssets.FailureKind);
        Assert.Contains("Body 与 Cameo", invalidAssets.Message, StringComparison.Ordinal);

        Ra2AutomationProjectSnapshot missingOwnerSnapshot = ProjectSnapshot(
            "[VehicleTypes]\n0=OTHER\n\n[OTHER]\nImage=OTHER\n");
        Ra2AiEditPlanCreationResult missingOwner = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            ProjectCall(),
            Ra2AiAuthoringRequestContext.ForProject(
                missingOwnerSnapshot,
                missingOwnerSnapshot.Documents.Select(document => document.FilePath).ToArray()));

        Assert.Equal(Ra2AiEditProposalFailureKind.TemplateExpansionRejected, missingOwner.FailureKind);
        Assert.Contains("找不到", missingOwner.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Coordinator_ProjectProposalUsesSingleProjectPreviewApplyAndTargetedDismiss()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        RecordingPort port = new();
        Ra2IniAuthoringWorkspace workspace = new(
            new Ra2IniEditPreviewService(new Ra2AutomationCapabilityGateway()),
            port,
            new Ra2ProjectEditPreviewService());
        Ra2AiAuthoringCoordinator coordinator = new(new Ra2AiAuthoringToolAdapter(), workspace);

        Ra2AiEditProposalResult prepared = coordinator.PrepareProposal(
            context,
            context,
            Ra2AiResponse.CreateToolCalls([ProjectCall()]),
            CancellationToken.None);

        Assert.True(prepared.Succeeded, prepared.Message);
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(prepared.Proposal);
        Assert.Equal(Ra2AiAuthoringScope.Project, proposal.Scope);
        Assert.Equal(2, proposal.ProjectPreview.DocumentPreviews.Count);
        Assert.NotNull(proposal.AssetManifest);

        Ra2AiEditProposalViewModel card = new(proposal);
        Assert.True(card.IsProject);
        Assert.Equal("建议修改当前项目", card.Title);
        Assert.Contains("2 个 INI 文件", card.ProjectSummary, StringComparison.Ordinal);
        Assert.Contains("HTNKBODY.shp", card.AssetManifestSummary, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("应用到项目", card.ApplyButtonText);
        Ra2AuthoringDiffViewModel diff = new(card);
        await diff.LoadAsync(CancellationToken.None);
        Assert.Equal("项目修改预览：rulesmd.ini + artmd.ini", diff.Title);
        Assert.Equal(2, diff.Rows.Count(row => row.Kind == Ra2AuthoringDiffRowKind.FileHeader));
        diff.Dispose();

        Ra2AiEditProposalApplyResult applied = coordinator.ApplyConfirmed(proposal);
        Assert.True(applied.Succeeded, applied.Message);
        Assert.Equal(1, port.ProjectApplyCount);
        Assert.Equal(0, port.DocumentApplyCount);
        Assert.NotNull(applied.ProjectAuthoringResult);
        Assert.Null(applied.AuthoringResult);

        Ra2AiEditProposalResult second = coordinator.PrepareProposal(
            context,
            context,
            Ra2AiResponse.CreateToolCalls([ProjectCall()]),
            CancellationToken.None);
        Assert.True(second.Succeeded, second.Message);
        Assert.True(coordinator.Dismiss(second.Proposal!));
        Assert.False(coordinator.Dismiss(second.Proposal!));
    }

    [Fact]
    public void Coordinator_ProjectSnapshotChangeFailsStaleWithoutPartialProposal()
    {
        Ra2AutomationProjectSnapshot requestSnapshot = ProjectSnapshot();
        Ra2AutomationProjectSnapshot changedSnapshot = new(
            requestSnapshot.ProjectSessionId,
            requestSnapshot.ProjectRevision + 1,
            requestSnapshot.ProjectRootPath,
            requestSnapshot.Documents);
        Ra2AiAuthoringRequestContext request = Ra2AiAuthoringRequestContext.ForProject(
            requestSnapshot,
            requestSnapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiAuthoringRequestContext current = Ra2AiAuthoringRequestContext.ForProject(
            changedSnapshot,
            changedSnapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiAuthoringCoordinator coordinator = new(
            new Ra2AiAuthoringToolAdapter(),
            new Ra2IniAuthoringWorkspace(
                new Ra2IniEditPreviewService(new Ra2AutomationCapabilityGateway()),
                new RecordingPort(),
                new Ra2ProjectEditPreviewService()));

        Ra2AiEditProposalResult result = coordinator.PrepareProposal(
            request,
            current,
            Ra2AiResponse.CreateToolCalls([ProjectCall()]),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AiEditProposalFailureKind.RequestContextStale, result.FailureKind);
        Assert.Null(result.Proposal);
        Assert.Null(coordinator.ActiveProposal);
    }

    [Fact]
    public void Coordinator_WrongProjectTargetReportsCapturedCrossDocumentSectionLocation()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot(
            rulesText: "[HTNK]\nImage=HTNKART\n",
            artText: "[HTNKART]\nImage=HTNKBODY\nCameo=HTNKICON\n");
        Ra2AiAuthoringRequestContext context = Ra2AiAuthoringRequestContext.ForProject(
            snapshot,
            snapshot.Documents.Select(document => document.FilePath).ToArray());
        Ra2AiAuthoringCoordinator coordinator = new(
            new Ra2AiAuthoringToolAdapter(),
            new Ra2IniAuthoringWorkspace(
                new Ra2IniEditPreviewService(new Ra2AutomationCapabilityGateway()),
                new RecordingPort(),
                new Ra2ProjectEditPreviewService()));
        Ra2AiToolCall wrongTarget = new(
            "wrong-project-target",
            Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
            """
            {
              "outcome":"proposal",
              "summary":"set remapable",
              "documents":[
                {
                  "target":"rules",
                  "operations":[
                    {"kind":"replace_field_value","section":"HTNKART","key":"Image","value":"HTNKBODY"}
                  ]
                }
              ]
            }
            """);

        Ra2AiEditProposalResult result = coordinator.PrepareProposal(
            context,
            context,
            Ra2AiResponse.CreateToolCalls([wrongTarget]),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AiEditProposalFailureKind.PreviewRejected, result.FailureKind);
        Assert.Contains("rulesmd.ini", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[HTNKART]", result.Message, StringComparison.Ordinal);
        Assert.Contains("artmd.ini", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("本次未应用", result.Message, StringComparison.Ordinal);
        Assert.Null(result.Proposal);
        Assert.Null(coordinator.ActiveProposal);
    }

    private static Ra2AiToolCall ProjectCall()
        => new(
            "project-1",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            """
            {
              "outcome":"proposal",
              "template_id":"techno-rules-art-asset-binding",
              "template_version":1,
              "message":"仅生成 rules/art 绑定预览，不写入或保存素材文件。",
              "arguments":{
                "ownerSectionId":"HTNK",
                "artSectionId":"HTNKART",
                "bodyAssetId":"HTNKBODY",
                "cameoAssetId":"HTNKICON",
                "assetBrief":"Heavy allied battle tank body and matching cameo"
              }
            }
            """);

    private static Ra2AiToolCall GenericProjectCall()
        => new(
            "project-generic-1",
            Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
            """
            {
              "outcome":"proposal",
              "summary":"Bind HTNK art and complete its project INI fields",
              "documents":[
                {
                  "target":"RULES",
                  "operations":[
                    {"kind":"upsert_field","section":"HTNK","key":"Image","value":"HTNKART"}
                  ]
                },
                {
                  "target":"art",
                  "operations":[
                    {"kind":"upsert_field","section":"HTNKART","key":"Image","value":"HTNKBODY"},
                    {"kind":"upsert_field","section":"HTNKART","key":"Cameo","value":"HTNKICON"},
                    {"kind":"upsert_field","section":"HTNKART","key":"UnregisteredCustomField","value":"AllowedByModel"}
                  ]
                }
              ]
            }
            """);

    private static Ra2AiToolCall ProjectCallWithExtraArguments(string extraArgument)
        => new(
            "project-invalid",
            Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
            $$"""
            {
              "outcome":"proposal",
              "template_id":"techno-rules-art-asset-binding",
              "template_version":1,
              "arguments":{
                "ownerSectionId":"HTNK",
                "artSectionId":"HTNKART",
                "bodyAssetId":"HTNKBODY",
                "cameoAssetId":"HTNKICON",
                "assetBrief":"Heavy tank assets",
                {{extraArgument}}
              }
            }
            """);

    private static Ra2AutomationProjectSnapshot ProjectSnapshot(
        string rulesText = "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nImage=OLDART\n",
        string artText = "")
    {
        Ra2AutomationFieldRegistrySnapshot registry = new(new BuiltInRa2FieldDefinitionProvider(), 19);
        return new Ra2AutomationProjectSnapshot(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            4,
            "C:\\mod",
            [
                new Ra2AutomationDocumentSnapshot(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    9,
                    "C:\\mod\\rulesmd.ini",
                    rulesText,
                    true,
                    registry),
                new Ra2AutomationDocumentSnapshot(
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    9,
                    "C:\\mod\\artmd.ini",
                    artText,
                    true,
                    registry)
            ]);
    }

    private static Ra2AuthoringSnapshot DocumentSnapshot()
    {
        Ra2EditableDocumentSessionService service = new(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService());
        Ra2EditableDocumentSession session = service.StartEditing("C:\\mod\\rulesmd.ini", "[E1]\nStrength=100");
        return Assert.IsType<Ra2AuthoringSnapshot>(Ra2AuthoringSnapshot.Capture(
            session,
            session.DocumentState.CurrentText,
            "C:\\mod",
            new Ra2FieldRegistryProviderSnapshot(new BuiltInRa2FieldDefinitionProvider(), 19)).Snapshot);
    }

    private sealed class RecordingPort : IRa2EditorTransactionPort
    {
        public int DocumentApplyCount { get; private set; }
        public int ProjectApplyCount { get; private set; }

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            DocumentApplyCount++;
            return Ra2IniEditApplyResult.UnexpectedFailure(preview.PreviewId);
        }

        public Ra2ProjectEditApplyResult ApplyProject(Ra2ProjectEditPreview preview)
        {
            ProjectApplyCount++;
            return Ra2ProjectEditApplyResult.Applied(preview, [], 0);
        }
    }
}
