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

        Assert.Equal(15, catalog.Skills.Count);
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
}
