using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.AI;
using System.Text.Json;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ContentTemplateAiIntegrationTests
{
    [Fact]
    public void RouterAndToolCatalog_KeepAdvisoryAndProductionWorkToolsMutuallyExclusive()
    {
        Ra2AiInteractionRoute template = Ra2AiInteractionRouter.Resolve(
            "在当前文件创建武器链，Weapon=TestWeapon，Projectile=TestProjectile，Warhead=TestWarhead",
            Ra2AiEditAvailabilityKind.Available);
        Ra2AiInteractionRoute field = Ra2AiInteractionRouter.Resolve(
            "把当前文件 [E1] 下的 Strength 修改为 150",
            Ra2AiEditAvailabilityKind.Available);
        Ra2AiInteractionRoute advisory = Ra2AiInteractionRouter.Resolve(
            "不要修改，只解释当前文件的 Weapon Projectile Warhead 关系",
            Ra2AiEditAvailabilityKind.Available);
        Ra2AiInteractionRoute unavailable = Ra2AiInteractionRouter.Resolve(
            "在当前文件创建武器链 Weapon Projectile Warhead",
            Ra2AiEditAvailabilityKind.UnsupportedEndpoint);

        Assert.Equal(Ra2AiInteractionRouteKind.CompleteTemplateExplicit, template.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview, template.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.EditExplicit, field.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentEditPreview, field.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.Advisory, advisory.Kind);
        Assert.Empty(Ra2AiAuthoringToolCatalog.GetTools(advisory.CapabilityMode));
        Assert.Equal(Ra2AiInteractionRouteKind.EditUnavailable, unavailable.Kind);

        Ra2AiToolDefinition templateTool = Assert.Single(
            Ra2AiAuthoringToolCatalog.GetTools(template.CapabilityMode));
        Ra2AiToolDefinition fieldTool = Assert.Single(
            Ra2AiAuthoringToolCatalog.GetTools(field.CapabilityMode));
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, templateTool.Name);
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, fieldTool.Name);
        Assert.Equal(templateTool.Name, fieldTool.Name);
    }

    [Fact]
    public void Router_SeparatesDualArmamentFromUnsupportedCyclicFire()
    {
        Ra2AiInteractionRoute dual = Ra2AiInteractionRouter.Resolve(
            "在当前文件为 HTNK 构建主副武器两套完整武器链",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute cyclic = Ra2AiInteractionRouter.Resolve(
            "给 HTNK 添加和主炮循环开火的同轴机枪，同时构建完整武器链",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);

        Assert.Equal(Ra2AiInteractionRouteKind.TechnoDualArmamentExplicit, dual.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentDualArmamentPreview, dual.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.UnsupportedWorkCapability, cyclic.Kind);
        Assert.Equal(Ra2AiCapabilityMode.AdvisoryOnly, cyclic.CapabilityMode);
        Assert.Empty(Ra2AiAuthoringToolCatalog.GetTools(cyclic.CapabilityMode));
    }

    [Fact]
    public void Router_NegatedCyclicFireBoundaryAllowsExistingSecondaryCompleteProfile()
    {
        Ra2AiInteractionRoute route = Ra2AiInteractionRouter.Resolve(
            "修改当前文件，为 HTNK 添加 Secondary 同轴机枪，并构建完整的 Weapon、Projectile、Warhead 链；不要使用循环或交替开火机制。",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute mixedIntent = Ra2AiInteractionRouter.Resolve(
            "修改当前文件，不要循环开火，但要交替开火并构建完整武器链",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute unrelatedNegation = Ra2AiInteractionRouter.Resolve(
            "修改当前文件，不要解释直接添加交替开火武器链",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);

        Assert.Equal(Ra2AiInteractionRouteKind.CompleteTemplateExplicit, route.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview, route.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.UnsupportedWorkCapability, mixedIntent.Kind);
        Assert.Equal(Ra2AiInteractionRouteKind.UnsupportedWorkCapability, unrelatedNegation.Kind);
    }

    [Fact]
    public void Router_ExactSecondaryChainPromptWithNegatedMechanismsRemainsCompleteTemplate()
    {
        Ra2AiInteractionRoute route = Ra2AiInteractionRouter.Resolve(
            "修改当前文件，为 HTNK 添加 Secondary 同轴机枪，并构建完整的 Weapon、Projectile、Warhead 链；不要使用循环或交替开火机制。",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);

        Assert.Equal(Ra2AiInteractionRouteKind.CompleteTemplateExplicit, route.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview, route.CapabilityMode);
    }

    [Fact]
    public void DualArmamentProductionToolSchema_IsGenericModelOwnedPlan()
    {
        Ra2AiToolDefinition tool = Assert.Single(Ra2AiAuthoringToolCatalog.GetTools(
            Ra2AiCapabilityMode.CurrentDocumentDualArmamentPreview));
        using JsonDocument schema = JsonDocument.Parse(tool.ParametersJsonSchema);
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, tool.Name);
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("operations", out _));
        Assert.DoesNotContain("template_id", tool.ParametersJsonSchema, StringComparison.Ordinal);
    }

    [Fact]
    public void PromptBuilder_DualArmamentDelegatesCompleteContentToModelOwnedPlan()
    {
        Ra2AiRequest request = new Ra2AiPromptBuilder().Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = "为当前 Techno 构建主副武器两套完整武器链",
            Context = EmptyContext(),
            CapabilityMode = Ra2AiCapabilityMode.CurrentDocumentDualArmamentPreview
        });

        Assert.Contains("preview_ini_edit_plan exactly once", request.SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("construct every INI field", request.SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("Do not reduce a complete-object request to a skeleton", request.SystemPromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("template_id=", request.SystemPromptText, StringComparison.Ordinal);
        Assert.Equal(Ra2AiToolChoiceMode.Required, request.ToolChoice);
    }

    [Fact]
    public void Router_SeparatesArcingHomingWarheadAndRejectsUnsupportedTrajectoryFamilies()
    {
        Ra2AiInteractionRoute arcing = Ra2AiInteractionRouter.Resolve(
            "给当前武器创建一个完整的曲射 Projectile",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute homing = Ra2AiInteractionRouter.Resolve(
            "给当前武器创建一个完整的追踪导弹弹体",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute warhead = Ra2AiInteractionRouter.Resolve(
            "给当前武器创建一个完整的范围伤害弹头",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute unsupported = Ra2AiInteractionRouter.Resolve(
            "给当前武器创建 Phobos Straight Trajectory 弹体",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);
        Ra2AiInteractionRoute conflicting = Ra2AiInteractionRouter.Resolve(
            "给当前武器创建同时曲射和追踪的弹体",
            Ra2AiEditAvailabilityKind.Available,
            Ra2AiUserMode.Work);

        Assert.Equal(Ra2AiInteractionRouteKind.ArcingProjectileExplicit, arcing.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentArcingProjectilePreview, arcing.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.HomingProjectileExplicit, homing.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentHomingProjectilePreview, homing.CapabilityMode);
        Assert.Equal(Ra2AiInteractionRouteKind.YrCoreWarheadExplicit, warhead.Kind);
        Assert.Equal(Ra2AiCapabilityMode.CurrentDocumentYrCoreWarheadPreview, warhead.CapabilityMode);
        Assert.All([unsupported, conflicting], route =>
        {
            Assert.Equal(Ra2AiInteractionRouteKind.UnsupportedWorkCapability, route.Kind);
            Assert.Empty(Ra2AiAuthoringToolCatalog.GetTools(route.CapabilityMode));
        });
    }

    [Theory]
    [InlineData((int)Ra2AiCapabilityMode.CurrentDocumentArcingProjectilePreview)]
    [InlineData((int)Ra2AiCapabilityMode.CurrentDocumentHomingProjectilePreview)]
    [InlineData((int)Ra2AiCapabilityMode.CurrentDocumentYrCoreWarheadPreview)]
    public void ProjectileWarheadProductionToolSchemas_ExposeGenericModelOwnedPlan(int mode)
    {
        Ra2AiToolDefinition tool = Assert.Single(Ra2AiAuthoringToolCatalog.GetTools((Ra2AiCapabilityMode)mode));
        using JsonDocument schema = JsonDocument.Parse(tool.ParametersJsonSchema);
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, tool.Name);
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("operations", out _));
        Assert.DoesNotContain("template_id", tool.ParametersJsonSchema, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData((int)Ra2AiCapabilityMode.CurrentDocumentArcingProjectilePreview)]
    [InlineData((int)Ra2AiCapabilityMode.CurrentDocumentHomingProjectilePreview)]
    [InlineData((int)Ra2AiCapabilityMode.CurrentDocumentYrCoreWarheadPreview)]
    public void PromptBuilder_ProjectileWarheadRoutesUseGenericModelOwnedPlan(int mode)
    {
        Ra2AiRequest request = new Ra2AiPromptBuilder().Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = "修改当前武器",
            Context = EmptyContext(),
            CapabilityMode = (Ra2AiCapabilityMode)mode
        });

        Assert.Contains("preview_ini_edit_plan exactly once", request.SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("construct every INI field", request.SystemPromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("template_id=", request.SystemPromptText, StringComparison.Ordinal);
        Assert.Equal(Ra2AiToolChoiceMode.Required, request.ToolChoice);
    }

    [Theory]
    [InlineData("arcing", 8)]
    [InlineData("homing", 5)]
    [InlineData("warhead", 13)]
    public void ToolAdapter_ProjectileWarheadProfilesProducePreviewOnlyAtomicPlans(
        string profile,
        int expectedOperations)
    {
        Fixture fixture = new(linkWeaponFromUnit: true, includeExistingWeapon: true);
        object payload = profile switch
        {
            "arcing" => new
            {
                outcome = "proposal",
                template_id = "weapon-projectile-arcing-complete",
                template_version = 1,
                arguments = new Dictionary<string, object>
                {
                    ["weaponId"] = "TestWeapon", ["projectileId"] = "TestShell", ["image"] = "120MM",
                    ["antiAir"] = false, ["antiGround"] = true, ["subjectToWalls"] = true,
                    ["subjectToElevation"] = true, ["subjectToCliffs"] = true
                }
            },
            "homing" => new
            {
                outcome = "proposal",
                template_id = "weapon-projectile-homing-complete",
                template_version = 1,
                arguments = new Dictionary<string, object>
                {
                    ["weaponId"] = "TestWeapon", ["projectileId"] = "TestMissile", ["image"] = "DRAGON",
                    ["rot"] = 8, ["antiAir"] = true, ["antiGround"] = true
                }
            },
            _ => new
            {
                outcome = "proposal",
                template_id = "weapon-warhead-yr-core-complete",
                template_version = 1,
                arguments = new Dictionary<string, object>
                {
                    ["weaponId"] = "TestWeapon", ["warheadId"] = "TestWH",
                    ["verses"] = "100%,100%,100%,75%,75%,75%,50%,50%,50%,100%,100%",
                    ["infDeath"] = 2, ["cellSpread"] = 1.5, ["percentAtMax"] = 0.25,
                    ["proneDamage"] = 0.5, ["conventional"] = true, ["wall"] = true,
                    ["wood"] = true, ["rocker"] = false, ["sparky"] = true,
                    ["tiberium"] = false, ["bright"] = false
                }
            }
        };

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            new Ra2AiToolCall(profile, Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName, JsonSerializer.Serialize(payload)),
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.True(result.Succeeded, result.Message);
        Ra2IniEditPlan plan = Assert.IsType<Ra2IniEditPlan>(result.Plan);
        Assert.Single(plan.SectionCreations);
        Assert.Equal(expectedOperations, plan.Operations.Count);
        Assert.Equal(0, fixture.TransactionPort.ApplyCallCount);
    }

    [Fact]
    public void ToolAdapter_DualArmamentProducesOneAtomicThirtyOperationPlan()
    {
        Fixture fixture = new(linkWeaponFromUnit: true);
        Dictionary<string, object> arguments = new(StringComparer.Ordinal)
        {
            ["ownerSectionId"] = "E1"
        };
        AddDualChainArguments(arguments, "primary", "E1Rifle", 20, 15, 6d, 100);
        AddDualChainArguments(arguments, "secondary", "E1Grenade", 40, 45, 5d, 40);
        string payload = JsonSerializer.Serialize(new
        {
            outcome = "proposal",
            template_id = "techno-primary-secondary-direct-fire-complete",
            template_version = 1,
            arguments
        });

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            new Ra2AiToolCall(
                "dual-armament",
                Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName,
                payload),
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.True(result.Succeeded, result.Message);
        Ra2IniEditPlan plan = Assert.IsType<Ra2IniEditPlan>(result.Plan);
        Assert.Equal(6, plan.SectionCreations.Count);
        Assert.Equal(30, plan.Operations.Count);
        Assert.Contains(plan.Operations, item => item.SectionName == "E1" && item.Key == "Primary" && item.Value == "E1RifleWeapon");
        Assert.Contains(plan.Operations, item => item.SectionName == "E1" && item.Key == "Secondary" && item.Value == "E1GrenadeWeapon");
        Assert.Equal(0, fixture.TransactionPort.ApplyCallCount);
    }

    [Fact]
    public void PromptBuilder_UsesGenericCurrentDocumentPlanForSkeletonCapability()
    {
        Ra2AiRequest request = new Ra2AiPromptBuilder().Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = "在当前文件创建武器链",
            Context = EmptyContext(),
            CapabilityMode = Ra2AiCapabilityMode.CurrentDocumentTemplatePreview
        });

        Ra2AiToolDefinition tool = Assert.Single(request.Tools);
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, tool.Name);
        Assert.Equal(Ra2AiToolChoiceMode.Required, request.ToolChoice);
        Assert.Contains("preview_ini_edit_plan exactly once", request.SystemPromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("template_id=", request.SystemPromptText, StringComparison.Ordinal);
    }

    [Fact]
    public void CompleteCapabilityProductionToolSchema_UsesGenericOperations()
    {
        Ra2AiToolDefinition tool = Assert.Single(Ra2AiAuthoringToolCatalog.GetTools(
            Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview));
        using JsonDocument schema = JsonDocument.Parse(tool.ParametersJsonSchema);
        Assert.Equal(Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, tool.Name);
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("operations", out _));
        Assert.DoesNotContain("template_id", tool.ParametersJsonSchema, StringComparison.Ordinal);
    }

    [Fact]
    public void ToolAdapter_NormalizesCompleteTemplateObjectArgumentsAndNativeScalars()
    {
        Fixture fixture = new(linkWeaponFromUnit: true);
        Ra2AiToolCall call = new(
            "complete-template",
            Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName,
            """
            {
              "template_id":"weapon-projectile-warhead-direct-fire-complete",
              "template_version":"1",
              "message":"Created a complete coaxial direct-fire chain for local preview.",
              "arguments":{
                "ownerSectionId":"E1",
                "ownerWeaponSlot":"Secondary",
                "weaponId":"E1CoaxMG",
                "projectileId":"E1CoaxBullet",
                "warheadId":"E1CoaxWH",
                "damage":15,
                "rof":10,
                "range":6,
                "projectileSpeed":60,
                "verses":"100%,80%,70%,60%,40%,40%,30%,20%,20%,100%,100%",
                "infDeath":0,
                "cellSpread":0.3,
                "percentAtMax":0.5,
                "antiAir":false,
                "antiGround":true,
              },
            }
            """);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            call,
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.True(result.Succeeded, result.Message);
        Ra2IniEditPlan plan = Assert.IsType<Ra2IniEditPlan>(result.Plan);
        Assert.Equal(3, plan.SectionCreations.Count);
        Assert.Equal(15, plan.Operations.Count);
        Assert.Contains(plan.Operations, operation =>
            operation.SectionName == "E1" && operation.Key == "Secondary" && operation.Value == "E1CoaxMG");
        Assert.Contains(plan.Operations, operation =>
            operation.SectionName == "E1CoaxBullet" && operation.Key == "AA" && operation.Value == "no");
        Assert.Contains(plan.Operations, operation =>
            operation.SectionName == "E1CoaxBullet" && operation.Key == "AG" && operation.Value == "yes");
    }

    [Fact]
    public void ToolAdapter_RejectsNonStringProposalMessageWithoutExecutingTemplate()
    {
        Fixture fixture = new(linkWeaponFromUnit: true);
        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            new Ra2AiToolCall(
                "invalid-message",
                Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName,
                """
                {
                  "outcome":"proposal",
                  "template_id":"weapon-projectile-warhead-direct-fire-complete",
                  "template_version":1,
                  "message":{"raw":"must not be accepted"},
                  "arguments":{}
                }
                """),
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidArgumentsJson, result.FailureKind);
        Assert.Contains("message", result.Message, StringComparison.Ordinal);
        Assert.Equal(0, fixture.TransactionPort.ApplyCallCount);
    }

    [Fact]
    public void ToolAdapter_ReportsSpecificSafeConstraintForInvalidWeaponSlot()
    {
        Fixture fixture = new(linkWeaponFromUnit: true);
        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            new Ra2AiToolCall(
                "invalid-slot",
                Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName,
                """
                {
                  "outcome":"proposal",
                  "template_id":"weapon-projectile-warhead-direct-fire-complete",
                  "template_version":1,
                  "arguments":{"ownerWeaponSlot":"Elite"}
                }
                """),
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AiEditProposalFailureKind.InvalidOperation, result.FailureKind);
        Assert.Equal("武器槽位必须是 Primary 或 Secondary。", result.Message);
        Assert.Equal(0, fixture.TransactionPort.ApplyCallCount);
    }

    [Fact]
    public void ToolAdapter_MixedClarificationKeepsProposalPayloadInert()
    {
        Fixture fixture = new(linkWeaponFromUnit: true);
        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            new Ra2AiToolCall(
                "mixed-clarification",
                Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName,
                """
                {
                  "outcome":"needs_clarification",
                  "message":"请确认同轴机枪应绑定 Primary 还是 Secondary。",
                  "template_id":"weapon-projectile-warhead-direct-fire-complete",
                  "template_version":1,
                  "arguments":{
                    "ownerSectionId":"E1",
                    "ownerWeaponSlot":"Secondary",
                    "weaponId":"MustRemainInert"
                  }
                }
                """),
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.True(result.NeedsClarification);
        Assert.False(result.Succeeded);
        Assert.Null(result.Plan);
        Assert.Contains("Primary", result.Message, StringComparison.Ordinal);
        Assert.Equal(0, fixture.TransactionPort.ApplyCallCount);
    }

    [Fact]
    public void PromptBuilder_CompleteCapabilityUsesModelOwnedOperationContract()
    {
        Ra2AiRequest request = new Ra2AiPromptBuilder().Build(new Ra2AiPromptBuildRequest
        {
            UserPrompt = "给当前单位创建完整武器链",
            Context = EmptyContext(),
            CapabilityMode = Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview
        });

        Assert.Contains("construct every INI field", request.SystemPromptText, StringComparison.Ordinal);
        Assert.Contains("Do not reduce a complete-object request to a skeleton", request.SystemPromptText, StringComparison.Ordinal);
        Assert.DoesNotContain("template_id=", request.SystemPromptText, StringComparison.Ordinal);
    }

    [Fact]
    public void ToolAdapter_ExpandsOnlyCataloguedTemplateArgumentsBoundToLocalSnapshot()
    {
        Fixture fixture = new(linkWeaponFromUnit: true);
        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            TemplateCall(),
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.True(result.Succeeded, result.Message);
        Ra2IniEditPlan plan = Assert.IsType<Ra2IniEditPlan>(result.Plan);
        Assert.Equal(fixture.Snapshot.DocumentId, plan.ExpectedDocumentId);
        Assert.Equal(fixture.Snapshot.EditRevision, plan.ExpectedVersion);
        Assert.Equal(fixture.Snapshot.FieldRegistry.Revision, plan.ExpectedFieldRegistryRevision);
        Assert.Equal(3, plan.SectionCreations.Count);
        Assert.Equal(["TestWeapon", "TestProjectile", "TestWarhead"], plan.SectionCreations.Select(item => item.SectionName));
        Assert.Equal(["Projectile", "Warhead"], plan.Operations.Select(item => item.Key));
    }

    [Theory]
    [InlineData("raw", (int)Ra2AiEditProposalFailureKind.UnknownArgumentProperty)]
    [InlineData("version", (int)Ra2AiEditProposalFailureKind.TemplateExpansionRejected)]
    [InlineData("duplicate", (int)Ra2AiEditProposalFailureKind.DuplicateArgumentProperty)]
    public void ToolAdapter_FailsClosedForRawVersionAndDuplicatePayloads(
        string scenario,
        int expected)
    {
        string arguments = scenario switch
        {
            "raw" => ValidArgumentsJson().Replace("\"arguments\":", "\"raw_ini\":\"[W]\\n\",\"arguments\":", StringComparison.Ordinal),
            "version" => ValidArgumentsJson().Replace("\"template_version\":1", "\"template_version\":2", StringComparison.Ordinal),
            _ => ValidArgumentsJson().Replace(
                "{\"name\":\"weaponId\",\"value\":\"TestWeapon\"}",
                "{\"name\":\"weaponId\",\"value\":\"TestWeapon\"},{\"name\":\"weaponId\",\"value\":\"OtherWeapon\"}",
                StringComparison.Ordinal)
        };
        Fixture fixture = new(linkWeaponFromUnit: true);

        Ra2AiEditPlanCreationResult result = new Ra2AiAuthoringToolAdapter().TryCreatePlan(
            new Ra2AiToolCall("template", Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName, arguments),
            new Ra2AiAuthoringRequestContext(fixture.Snapshot));

        Assert.False(result.Succeeded);
        Assert.Equal((Ra2AiEditProposalFailureKind)expected, result.FailureKind);
        Assert.Null(result.Plan);
    }

    [Fact]
    public void Coordinator_TemplateLoopCreatesProposalProjectsSectionsAndAppliesExactlyOnce()
    {
        Fixture fixture = new(linkWeaponFromUnit: true);
        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            Ra2AiResponse.CreateToolCalls([TemplateCall()]),
            CancellationToken.None);

        Assert.True(result.Succeeded, result.Message);
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(result.Proposal);
        Assert.Equal(Ra2AiEditProposalApplyPolicy.Normal, proposal.ApplyPolicy);
        Assert.Equal(3, proposal.Preview.SectionCreationPreviews.Count);
        Assert.All(proposal.Preview.SectionCreationPreviews, item => Assert.True(item.IsClassificationResolved));
        Ra2AiEditProposalViewModel viewModel = new(proposal);
        Assert.Equal(5, viewModel.Operations.Count);
        Assert.Equal(3, viewModel.Operations.Count(item => item.ActionText == "创建 Section"));

        Ra2AiEditProposalApplyResult first = fixture.Coordinator.ApplyConfirmed(proposal);
        Ra2AiEditProposalApplyResult replay = fixture.Coordinator.ApplyConfirmed(proposal);
        Assert.True(first.Succeeded, first.Message);
        Assert.Equal(1, fixture.TransactionPort.ApplyCallCount);
        Assert.Contains("[TestWeapon]", fixture.TransactionPort.CurrentText, StringComparison.Ordinal);
        Assert.Contains("Projectile=TestProjectile", fixture.TransactionPort.CurrentText, StringComparison.Ordinal);
        Assert.True(fixture.TransactionPort.IsDirty);
        Assert.Equal(Ra2AiEditProposalFailureKind.RequestContextStale, replay.FailureKind);
    }

    [Fact]
    public void Coordinator_UnresolvedSectionClassificationRequiresCautionAndStaleOrCanceledCreatesNoProposal()
    {
        Fixture fixture = new(linkWeaponFromUnit: false);
        Ra2AiEditProposalResult caution = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            Ra2AiResponse.CreateToolCalls([TemplateCall()]),
            CancellationToken.None);
        Ra2AiEditProposal proposal = Assert.IsType<Ra2AiEditProposal>(caution.Proposal);
        Assert.Equal(Ra2AiEditProposalApplyPolicy.Caution, proposal.ApplyPolicy);

        Ra2AuthoringSnapshot changed = fixture.ChangedSnapshot(fixture.Snapshot.Text + "; changed\n");
        Ra2AiEditProposalResult stale = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            changed,
            Ra2AiResponse.CreateToolCalls([TemplateCall()]),
            CancellationToken.None);
        Assert.Equal(Ra2AiEditProposalFailureKind.RequestContextStale, stale.FailureKind);

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Ra2AiEditProposalResult canceled = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(changed),
            changed,
            Ra2AiResponse.CreateToolCalls([TemplateCall()]),
            cancellation.Token);
        Assert.Equal(Ra2AiEditProposalFailureKind.PreviewCancelled, canceled.FailureKind);
        Assert.Null(fixture.Coordinator.ActiveProposal);
    }

    [Fact]
    public void Coordinator_BlockedTemplateTrustDoesNotCreateProposalOrTransactionAuthority()
    {
        Fixture fixture = new(linkWeaponFromUnit: true, projectileQuality: "guardrail");

        Ra2AiEditProposalResult result = fixture.Coordinator.PrepareProposal(
            new Ra2AiAuthoringRequestContext(fixture.Snapshot),
            fixture.Snapshot,
            Ra2AiResponse.CreateToolCalls([TemplateCall()]),
            CancellationToken.None);

        Assert.Equal(Ra2AiEditProposalFailureKind.TemplateExpansionRejected, result.FailureKind);
        Assert.Null(result.Proposal);
        Assert.Null(fixture.Coordinator.ActiveProposal);
        Assert.Equal(0, fixture.TransactionPort.ApplyCallCount);
    }

    private static Ra2AiToolCall TemplateCall()
        => new("template-call", Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName, ValidArgumentsJson());

    private static string ValidArgumentsJson()
        => "{\"outcome\":\"proposal\",\"template_id\":\"weapon-projectile-warhead-skeleton\",\"template_version\":1," +
           "\"arguments\":[{\"name\":\"weaponId\",\"value\":\"TestWeapon\"}," +
           "{\"name\":\"projectileId\",\"value\":\"TestProjectile\"}," +
           "{\"name\":\"warheadId\",\"value\":\"TestWarhead\"}]}";

    private static void AddDualChainArguments(
        IDictionary<string, object> arguments,
        string prefix,
        string idPrefix,
        int damage,
        int rof,
        double range,
        int projectileSpeed)
    {
        arguments[$"{prefix}WeaponId"] = $"{idPrefix}Weapon";
        arguments[$"{prefix}ProjectileId"] = $"{idPrefix}Projectile";
        arguments[$"{prefix}WarheadId"] = $"{idPrefix}Warhead";
        arguments[$"{prefix}Damage"] = damage;
        arguments[$"{prefix}Rof"] = rof;
        arguments[$"{prefix}Range"] = range;
        arguments[$"{prefix}ProjectileSpeed"] = projectileSpeed;
        arguments[$"{prefix}Verses"] = "100%,100%,100%,50%,50%,50%,25%,25%,25%,100%,100%";
        arguments[$"{prefix}InfDeath"] = 1;
        arguments[$"{prefix}CellSpread"] = 0d;
        arguments[$"{prefix}PercentAtMax"] = 1d;
        arguments[$"{prefix}AntiAir"] = false;
        arguments[$"{prefix}AntiGround"] = true;
    }

    private static Ra2AiContext EmptyContext()
        => new(
            "rulesmd.ini",
            0,
            1,
            Ra2CaretRegion.Whitespace,
            null,
            null,
            null,
            null,
            null,
            string.Empty,
            0,
            hasSemanticContext: false);

    private sealed class Fixture
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private readonly Ra2FieldRegistryProviderSnapshot _registry;

        public Fixture(
            bool linkWeaponFromUnit,
            string projectileQuality = "source-verified",
            bool includeExistingWeapon = false)
        {
            string text = linkWeaponFromUnit
                ? "[InfantryTypes]\n1=E1\n\n[E1]\nPrimary=TestWeapon\n"
                : "; empty template target\n";
            if (includeExistingWeapon)
                text += "\n[TestWeapon]\nDamage=100\n";
            Session = _sessionService.StartEditing("rulesmd.ini", text);
            _registry = new Ra2FieldRegistryProviderSnapshot(
                new TemplateProvider(projectileQuality),
                revision: 31);
            Snapshot = Capture(Session);
            TransactionPort = new RecordingTransactionPort(Session);
            Ra2AutomationCapabilityGateway gateway = new();
            Ra2IniAuthoringWorkspace workspace = new(
                new Ra2IniEditPreviewService(gateway),
                TransactionPort);
            Coordinator = new Ra2AiAuthoringCoordinator(
                new Ra2AiAuthoringToolAdapter(gateway),
                workspace);
        }

        public Ra2EditableDocumentSession Session { get; }
        public Ra2AuthoringSnapshot Snapshot { get; }
        public RecordingTransactionPort TransactionPort { get; }
        public Ra2AiAuthoringCoordinator Coordinator { get; }

        public Ra2AuthoringSnapshot ChangedSnapshot(string text)
            => Capture(_sessionService.UpdateText(Session, text));

        private Ra2AuthoringSnapshot Capture(Ra2EditableDocumentSession session)
            => Assert.IsType<Ra2AuthoringSnapshot>(Ra2AuthoringSnapshot.Capture(
                session,
                session.DocumentState.CurrentText,
                string.Empty,
                _registry).Snapshot);
    }

    private sealed class TemplateProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition[] _definitions;

        public TemplateProvider(string projectileQuality)
        {
            _definitions =
            [
                Field("Primary", [Ra2SectionKind.Infantry, Ra2SectionKind.Techno], Ra2FieldValueKind.Reference),
                Field("Secondary", [Ra2SectionKind.Infantry, Ra2SectionKind.Techno], Ra2FieldValueKind.Reference),
                Field("Damage", [Ra2SectionKind.Weapon], Ra2FieldValueKind.Integer),
                Field("ROF", [Ra2SectionKind.Weapon], Ra2FieldValueKind.Integer),
                Field("Range", [Ra2SectionKind.Weapon], Ra2FieldValueKind.Float),
                Field("Projectile", [Ra2SectionKind.Weapon], Ra2FieldValueKind.Reference, projectileQuality),
                Field("Speed", [Ra2SectionKind.Weapon], Ra2FieldValueKind.Integer),
                Field("Warhead", [Ra2SectionKind.Weapon], Ra2FieldValueKind.Reference),
                Field("Inviso", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Boolean),
                Field("Image", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Reference),
                Field("AA", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Boolean),
                Field("AG", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Boolean),
                Field("Arcing", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Boolean),
                Field("ROT", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Integer),
                Field("SubjectToWalls", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Boolean),
                Field("SubjectToElevation", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Boolean),
                Field("SubjectToCliffs", [Ra2SectionKind.Projectile], Ra2FieldValueKind.Boolean),
                Field("Verses", [Ra2SectionKind.Warhead], Ra2FieldValueKind.String),
                Field("InfDeath", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Integer),
                Field("CellSpread", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Float),
                Field("PercentAtMax", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Float),
                Field("ProneDamage", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Float),
                Field("Conventional", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Boolean),
                Field("Wall", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Boolean),
                Field("Wood", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Boolean),
                Field("Rocker", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Boolean),
                Field("Sparky", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Boolean),
                Field("Tiberium", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Boolean),
                Field("Bright", [Ra2SectionKind.Warhead], Ra2FieldValueKind.Boolean)
            ];
        }

        private static Ra2FieldDefinition Field(
            string key,
            IReadOnlyList<Ra2SectionKind> appliesTo,
            Ra2FieldValueKind valueKind,
            string quality = "source-verified")
            => new(
                key,
                appliesTo,
                valueKind switch
                {
                    Ra2FieldValueKind.Integer => FieldEditorKind.Integer,
                    Ra2FieldValueKind.Float => FieldEditorKind.Float,
                    Ra2FieldValueKind.Boolean => FieldEditorKind.Boolean,
                    Ra2FieldValueKind.Reference => FieldEditorKind.Reference,
                    _ => FieldEditorKind.Text
                },
                Ra2FieldSourceKind.Yuri,
                valueMetadata: new Ra2FieldValueMetadata(
                    valueKind,
                    valueKind == Ra2FieldValueKind.Boolean
                        ? Ra2FieldBooleanValueStyle.YesNo
                        : Ra2FieldBooleanValueStyle.Unknown),
                registryQuality: quality);

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(item =>
                item.AppliesTo.Contains(sectionKind) && string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(item => item.AppliesTo.Contains(sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => _definitions.Any(item => item.AppliesTo.Contains(sectionKind) && string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class RecordingTransactionPort : IRa2EditorTransactionPort
    {
        private readonly Ra2EditableDocumentSessionService _sessionService = new(
            new Ra2IniTextDocumentParser(),
            new Ra2DirtyStateService());
        private Ra2EditableDocumentSession _session;

        public RecordingTransactionPort(Ra2EditableDocumentSession session) => _session = session;

        public int ApplyCallCount { get; private set; }
        public string CurrentText => _session.DocumentState.CurrentText;
        public bool IsDirty => _session.DocumentState.IsDirty;

        public Ra2IniEditApplyResult Apply(Ra2IniEditPreview preview)
        {
            ApplyCallCount++;
            _session = _sessionService.UpdateText(_session, preview.CandidateText!);
            return Ra2IniEditApplyResult.Applied(preview, _session, 0, preview.CandidateText!.Length);
        }
    }
}
