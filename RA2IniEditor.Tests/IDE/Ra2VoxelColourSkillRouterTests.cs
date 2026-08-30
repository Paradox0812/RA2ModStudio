using RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelColourSkillRouterTests
{
    [Theory]
    [InlineData((int)Ra2VoxelUnitClass.Ground, "ra2-ground-voxel-colour-techniques", false)]
    [InlineData((int)Ra2VoxelUnitClass.Air, "ra2-air-voxel-colour-techniques", false)]
    [InlineData((int)Ra2VoxelUnitClass.LargeSurface, "ra2-large-surface-voxel-colour-techniques", false)]
    [InlineData((int)Ra2VoxelUnitClass.Unknown, "ra2-voxel-colour-techniques", true)]
    public void Router_ConfirmedClassSelectsExactlyOneExpectedSkill(
        int unitClassValue,
        string expectedSkill,
        bool forceNeedsReview)
    {
        Ra2VoxelUnitClass unitClass = (Ra2VoxelUnitClass)unitClassValue;
        Ra2VoxelUnitClassEvidence evidence = Ra2VoxelUnitClassClassifierTests.CreateEvidence('A');
        Ra2VoxelColourSkillRouteResult result = Ra2VoxelColourSkillRouter.Resolve(
            evidence,
            Ra2VoxelUnitClassClassifierTests.Confirm(evidence, unitClass),
            Ra2AgentSkillCatalog.LoadBundled());

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(expectedSkill, result.Route!.ColourSkill.Name);
        Assert.Equal(unitClass, result.Route.Adaptation.UnitClass);
        Assert.Equal(forceNeedsReview, result.Route.Adaptation.ForceNeedsReview);
    }

    [Fact]
    public void Router_AcceptsOnlyConfirmedTypeAndRejectsStaleConfirmation()
    {
        Type[] parameters = Assert.Single(typeof(Ra2VoxelColourSkillRouter).GetMethods(
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic),
            method => method.Name == "Resolve").GetParameters().Select(value => value.ParameterType).ToArray();
        Assert.Contains(typeof(Ra2VoxelConfirmedUnitClass), parameters);
        Assert.DoesNotContain(typeof(Ra2VoxelUnitClassProposal), parameters);

        Ra2VoxelUnitClassEvidence oldEvidence = Ra2VoxelUnitClassClassifierTests.CreateEvidence('A');
        Ra2VoxelUnitClassEvidence currentEvidence = Ra2VoxelUnitClassClassifierTests.CreateEvidence('B');
        Ra2VoxelColourSkillRouteResult result = Ra2VoxelColourSkillRouter.Resolve(
            currentEvidence,
            Ra2VoxelUnitClassClassifierTests.Confirm(oldEvidence, Ra2VoxelUnitClass.Ground),
            Ra2AgentSkillCatalog.LoadBundled());
        Assert.Equal(Ra2VoxelColourSkillRouteFailureKind.UnitClassConfirmationStale, result.FailureKind);
    }

    [Fact]
    public void Router_MissingOrOversizedSkillFailsClosed()
    {
        Ra2VoxelUnitClassEvidence evidence = Ra2VoxelUnitClassClassifierTests.CreateEvidence('A');
        Ra2VoxelConfirmedUnitClass confirmation = Ra2VoxelUnitClassClassifierTests.Confirm(evidence, Ra2VoxelUnitClass.Ground);
        Ra2AgentSkillCatalog bundled = Ra2AgentSkillCatalog.LoadBundled();

        Ra2AgentSkillCatalog missing = new(bundled.Skills.Where(skill => skill.Name != "ra2-ground-voxel-colour-techniques"));
        Assert.Equal(
            Ra2VoxelColourSkillRouteFailureKind.ColourSkillUnavailable,
            Ra2VoxelColourSkillRouter.Resolve(evidence, confirmation, missing).FailureKind);

        Ra2AgentSkillDescriptor ground = Assert.Single(bundled.Skills, skill => skill.Name == "ra2-ground-voxel-colour-techniques");
        Ra2AgentSkillCatalog oversized = new(bundled.Skills.Select(skill => skill.Name == ground.Name
            ? skill with { Instructions = new string('x', Ra2AgentSkillCatalog.MaximumSelectedSkillCharacters + 1) }
            : skill));
        Assert.Equal(
            Ra2VoxelColourSkillRouteFailureKind.InstructionLimitExceeded,
            Ra2VoxelColourSkillRouter.Resolve(evidence, confirmation, oversized).FailureKind);
    }
}
