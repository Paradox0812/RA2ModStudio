using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AgentSkillCatalogTests
{
    [Fact]
    public void BundledCatalog_LoadsValidatedSkillPackagesWithoutScripts()
    {
        Ra2AgentSkillCatalog catalog = Ra2AgentSkillCatalog.LoadBundled();

        Assert.Equal(18, catalog.Skills.Count);
        Assert.All(catalog.Skills, skill =>
        {
            Assert.Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$", skill.Name);
            Assert.NotEmpty(skill.Description);
            Assert.NotEmpty(skill.Instructions);
            Assert.Equal(64, skill.ContentHash.Length);
        });
    }

    [Fact]
    public void Resolver_SelectsOneDomainSkillPlusTrustAndVersionExtension()
    {
        Ra2AgentSkillCatalog catalog = Ra2AgentSkillCatalog.LoadBundled();

        IReadOnlyList<Ra2AgentSkillDescriptor> normal = catalog.Select(
            "weapon-chain",
            Ra2AiUserMode.Work,
            "为当前文件搭建可用武器链");
        IReadOnlyList<Ra2AgentSkillDescriptor> phobos = catalog.Select(
            "projectile-trajectory",
            Ra2AiUserMode.Chat,
            "解释 Phobos Straight trajectory");

        Assert.Equal(["ra2-weapon-chain", "ra2-field-schema-trust"], normal.Select(skill => skill.Name));
        Assert.Equal(
            ["ra2-projectile-trajectory", "ra2-ares-phobos-extensions", "ra2-field-schema-trust"],
            phobos.Select(skill => skill.Name));
    }

    [Fact]
    public void Manifest_IsDeterministicMetadataProjectionWithoutInstructionBodies()
    {
        Ra2AgentSkillCatalog catalog = Ra2AgentSkillCatalog.LoadBundled();

        IReadOnlyList<Ra2AgentSkillManifestEntry> manifest = catalog.CreateManifest();

        Assert.Equal(catalog.Skills.Select(skill => skill.Name), manifest.Select(skill => skill.Id));
        Assert.Equal(manifest.OrderBy(skill => skill.Id, StringComparer.Ordinal), manifest);
        Assert.All(manifest, entry =>
        {
            Ra2AgentSkillDescriptor descriptor = Assert.Single(
                catalog.Skills,
                skill => skill.Name == entry.Id);
            Assert.Equal(descriptor.ContentHash, entry.ContentHash);
            Assert.Equal(descriptor.Instructions.Length, entry.InstructionCharacters);
            Assert.DoesNotContain(descriptor.Instructions, entry.Description, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Resolver_MergesCapabilityRequirementsAndModelOrderWithExplicitDiagnostics()
    {
        Ra2AgentSkillCatalog catalog = Ra2AgentSkillCatalog.LoadBundled();

        Ra2AgentSkillSelectionResolution result = catalog.Resolve(
            ["ra2-art-animation", "missing-skill", "ra2-art-animation"],
            ["object family is unclear", "object family is unclear"],
            "techno-rules-art-binding",
            "art-animation",
            Ra2AiUserMode.Work,
            "bind an existing techno art section");

        Assert.Equal(
            ["ra2-rules-art-binding", "ra2-field-schema-trust", "ra2-art-animation"],
            result.ActiveSkills.Select(skill => skill.Name));
        Assert.Equal(["ra2-rules-art-binding", "ra2-field-schema-trust"], result.RequiredSkillIds);
        Assert.Equal(["ra2-art-animation", "missing-skill"], result.RequestedSkillIds);
        Assert.Equal(["missing-skill"], result.UnavailableSkillIds);
        Assert.Empty(result.OmittedByBudgetSkillIds);
        Assert.Equal(["object family is unclear"], result.KnowledgeGaps);
    }

    [Fact]
    public void IntentAnalysisRequest_ExposesManifestMetadataAndDynamicSkillEnumWithoutBodies()
    {
        Ra2AgentSkillCatalog catalog = Ra2AgentSkillCatalog.LoadBundled();

        Ra2AiRequest request = Ra2AiIntentAnalysisStage.BuildRequest(
            "为当前单位搭建完整武器链",
            EmptyContext(),
            currentSubject: null,
            catalog);

        Ra2AiToolDefinition tool = Assert.Single(request.Tools);
        Assert.Contains("Available built-in RA2 Skill manifest", request.UserContentText, StringComparison.Ordinal);
        Assert.All(catalog.Skills, skill =>
        {
            Assert.Contains($"id={skill.Name}", request.UserContentText, StringComparison.Ordinal);
            Assert.Contains($"\"{skill.Name}\"", tool.ParametersJsonSchema, StringComparison.Ordinal);
            Assert.DoesNotContain(skill.Instructions, request.PromptText, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void IntentAnalysisParser_PreservesBoundedUnknownAndDuplicateSkillRecommendationsForResolver()
    {
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "analysis-1",
                Ra2AiIntentAnalysisStage.ToolName,
                """{"outcome":"authoring","capability_id":"weapon-chain-complete","domain_intent_id":"weapon-chain","request_summary":"complete chain","completion_level":"complete","constraints":[],"selected_skill_ids":["ra2-weapon-chain","future-skill","ra2-weapon-chain"],"knowledge_gaps":["owner id unclear"]}""")
        ]);

        Assert.True(Ra2AiIntentAnalysisStage.TryParse(response, out Ra2AiIntentAnalysisPackage? package, out _));
        Assert.NotNull(package);
        Assert.Equal(["ra2-weapon-chain", "future-skill", "ra2-weapon-chain"], package.SelectedSkillIds);
        Assert.Equal(["owner id unclear"], package.KnowledgeGaps);
    }

    [Fact]
    public void Resolver_OmitsOptionalSkillThatExceedsBodyBudgetWithoutDroppingRequiredSkills()
    {
        Ra2AgentSkillCatalog catalog = new([
            Descriptor("ra2-weapon-chain", "weapon-chain", Ra2AgentSkillMode.Work, 7000),
            Descriptor("ra2-field-schema-trust", "field-schema", Ra2AgentSkillMode.Work, 7000),
            Descriptor("ra2-art-animation", "art-animation", Ra2AgentSkillMode.Work, 1000)
        ]);

        Ra2AgentSkillSelectionResolution result = catalog.Resolve(
            ["ra2-art-animation"],
            [],
            "weapon-chain-complete",
            "weapon-chain",
            Ra2AiUserMode.Work,
            "complete chain");

        Assert.Equal(
            ["ra2-weapon-chain", "ra2-field-schema-trust"],
            result.ActiveSkills.Select(skill => skill.Name));
        Assert.Equal(["ra2-art-animation"], result.OmittedByBudgetSkillIds);
        Assert.Empty(result.UnavailableSkillIds);
        Assert.True(result.ActiveSkills.Sum(skill => skill.Instructions.Length) <= Ra2AgentSkillCatalog.MaximumSelectedSkillCharacters);
    }

    [Fact]
    public void Resolver_RecordsModeIncompatibleRecommendationInsteadOfInjectingIt()
    {
        Ra2AgentSkillCatalog catalog = new([
            Descriptor("chat-only", "ini-document", Ra2AgentSkillMode.Chat, 64),
            Descriptor("ra2-field-schema-trust", "field-schema", Ra2AgentSkillMode.Both, 64)
        ]);

        Ra2AgentSkillSelectionResolution result = catalog.Resolve(
            ["chat-only"],
            [],
            "current-document-field-edit",
            "field-schema",
            Ra2AiUserMode.Work,
            "edit field");

        Assert.Equal(["ra2-field-schema-trust"], result.ActiveSkills.Select(skill => skill.Name));
        Assert.Equal(["chat-only"], result.UnavailableSkillIds);
    }

    [Fact]
    public void PromptBuilder_ExplicitResolutionOverridesLegacyDomainSelectionAndReportsFacts()
    {
        Ra2AgentSkillCatalog catalog = Ra2AgentSkillCatalog.LoadBundled();
        Ra2AgentSkillDescriptor artSkill = Assert.Single(
            catalog.Skills,
            skill => skill.Name == "ra2-art-animation");
        Ra2AgentSkillSelectionResolution selection = new(
            ["ra2-art-animation"],
            [],
            [artSkill],
            ["unknown-skill"],
            [],
            ["object family unclear"]);

        Ra2AiRequest request = new Ra2AiPromptBuilder(catalog).Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = "解释武器链",
            Context = EmptyContext(),
            UserMode = Ra2AiUserMode.Work,
            DomainIntentId = "weapon-chain",
            SkillSelection = selection
        });

        Assert.Contains("Skill ra2-art-animation@", request.PromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("Skill ra2-weapon-chain@", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("unknown-skill", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("object family unclear", request.PromptText, StringComparison.Ordinal);
    }

    [Fact]
    public void IntentAnalysisParser_BoundsSelectionOverflowWithoutRejectingWorkIntent()
    {
        string sevenSkills = string.Join(',', Enumerable.Repeat("\"ra2-weapon-chain\"", 7));
        Ra2AiResponse response = Ra2AiResponse.CreateToolCalls([
            new Ra2AiToolCall(
                "analysis-1",
                Ra2AiIntentAnalysisStage.ToolName,
                $$$"""{"outcome":"authoring","capability_id":"weapon-chain-complete","domain_intent_id":"weapon-chain","request_summary":"complete chain","completion_level":"complete","constraints":[],"selected_skill_ids":[{{{sevenSkills}}}],"knowledge_gaps":[]}""")
        ]);

        Ra2AiIntentAnalysisParseResult result = Ra2AiIntentAnalysisStage.Parse(response);

        Assert.True(result.Succeeded, result.DiagnosticMessage);
        Assert.Equal(6, result.Package!.SelectedSkillIds.Count);
        Assert.Contains(result.RecoveryNotes, note => note.Contains("selected_skill_ids", StringComparison.Ordinal));
    }

    [Fact]
    public void PromptBuilder_InjectsSelectedSkillAsAuthorityNeutralInstructions()
    {
        Ra2AiRequest request = new Ra2AiPromptBuilder().Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = "解释武器链为什么需要弹体和弹头",
            Context = EmptyContext(),
            UserMode = Ra2AiUserMode.Chat,
            DomainIntentId = "weapon-chain"
        });

        Assert.Contains("## Active Built-in RA2 Skills", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("Skill ra2-weapon-chain@1", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("do not grant tools", request.PromptText, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(request.Tools);
    }

    [Fact]
    public void PromptBuilder_ProjectRulesArtRouteInjectsSourceBackedBindingSkill()
    {
        Ra2AiRequest request = new Ra2AiPromptBuilder().Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = "给 HTNK 绑定美术：Art=HTNKART，Body=HTNKBODY，Cameo=HTNKICON。",
            Context = EmptyContext(),
            UserMode = Ra2AiUserMode.Work,
            DomainIntentId = "art-animation",
            CapabilityMode = Ra2AiCapabilityMode.ProjectRulesArtBindingPreview
        });

        Assert.Contains("Skill ra2-rules-art-binding@1", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("Art`, `Body`, and `Cameo`", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("Image=ArtSection", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("target=rules", request.PromptText, StringComparison.Ordinal);
        Assert.Contains("target=art", request.PromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("Skill ra2-art-animation@", request.PromptText, StringComparison.Ordinal);
        Assert.Single(request.Tools);
        Assert.Equal(
            Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
            request.Tools[0].Name);
    }

    [Fact]
    public void Router_SeparatesChatWorkCompleteAndExplicitSkeleton()
    {
        const string prompt = "在当前文件搭建一套可用武器链";
        Ra2AiInteractionRoute chat = Ra2AiInteractionRouter.Resolve(
            prompt,
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Chat);
        Ra2AiInteractionRoute work = Ra2AiInteractionRouter.Resolve(
            prompt,
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute skeleton = Ra2AiInteractionRouter.Resolve(
            "在当前文件只搭一个骨架武器链",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);

        Assert.Equal(Ra2AiInteractionRouteKind.Advisory, chat.Kind);
        Assert.Equal(Ra2AiCapabilityMode.AdvisoryOnly, chat.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.CompleteTemplateExplicit, work.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview, work.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.TemplateExplicit, skeleton.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentTemplatePreview, skeleton.CapabilityMode);
    }

    [Theory]
    [InlineData("为HTNK建立完整武器链并加装同轴机枪")]
    [InlineData("组装HTNK同轴机枪武器链")]
    public void Router_WorkModeTreatsCurrentDocumentAsImplicitForExplicitWeaponChainRequests(string prompt)
    {
        Ra2AiInteractionRoute work = Ra2AiInteractionRouter.Resolve(
            prompt,
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute chat = Ra2AiInteractionRouter.Resolve(
            prompt,
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Chat);

        Assert.Equal(Ra2AiInteractionRouteKind.CompleteTemplateExplicit, work.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview, work.CapabilityMode);
        Assert.Equal("weapon-chain", work.DomainIntentId);
        Assert.Single(Ra2AiAuthoringToolCatalog.GetTools(work.CapabilityMode));
        Assert.Equal(Ra2AiInteractionRouteKind.Advisory, chat.Kind);
        Assert.Empty(Ra2AiAuthoringToolCatalog.GetTools(chat.CapabilityMode));
    }

    private static Ra2AiContext EmptyContext()
        => new(
            "rulesmd.ini",
            caretOffset: 0,
            lineNumber: 1,
            Ra2CaretRegion.Unknown,
            sectionName: null,
            sectionKind: null,
            keyName: null,
            valueText: null,
            selectedText: null,
            nearbyText: string.Empty,
            nearbyLineCount: 0,
            hasSemanticContext: false);

    private static Ra2AgentSkillDescriptor Descriptor(
        string name,
        string domain,
        Ra2AgentSkillMode modes,
        int instructionCharacters)
        => new(
            name,
            $"Description for {name}",
            "1",
            [domain],
            modes,
            new string('x', instructionCharacters),
            new string('a', 64));
}
