extern alias Ra2Application;

using RA2IniEditor.IDE.AI;
using Ra2VoxelConfirmedUnitClass = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelConfirmedUnitClass;
using Ra2VoxelUnitAdaptationCatalog = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitAdaptationCatalog;
using Ra2VoxelUnitAdaptationPolicy = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitAdaptationPolicy;
using Ra2VoxelUnitClassEvidence = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassEvidence;
using Ra2VoxelUnitClassProposal = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelUnitClassProposal;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelColourSkillRouteFailureKind
{
    None = 0,
    UnitClassConfirmationStale,
    ClassifierSkillUnavailable,
    ColourSkillUnavailable,
    ColourSkillMismatch,
    InstructionLimitExceeded
}

internal sealed class Ra2VoxelColourSkillRoute
{
    internal Ra2VoxelColourSkillRoute(
        Ra2AgentSkillDescriptor classifierSkill,
        Ra2AgentSkillDescriptor colourSkill,
        Ra2VoxelUnitAdaptationPolicy adaptation)
    {
        ClassifierSkill = classifierSkill;
        ColourSkill = colourSkill;
        Adaptation = adaptation;
    }

    internal Ra2AgentSkillDescriptor ClassifierSkill { get; }
    internal Ra2AgentSkillDescriptor ColourSkill { get; }
    internal Ra2VoxelUnitAdaptationPolicy Adaptation { get; }
}

internal sealed record Ra2VoxelColourSkillRouteResult(
    Ra2VoxelColourSkillRouteFailureKind FailureKind,
    string Message,
    Ra2VoxelColourSkillRoute? Route)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelColourSkillRouteFailureKind.None && Route is not null;
}

internal static class Ra2VoxelColourSkillRouter
{
    internal static Ra2VoxelColourSkillRouteResult Resolve(
        Ra2VoxelUnitClassEvidence evidence,
        Ra2VoxelConfirmedUnitClass confirmation,
        Ra2AgentSkillCatalog catalog,
        int additionalInstructionCharacters = 0)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(confirmation);
        ArgumentNullException.ThrowIfNull(catalog);
        if (!string.Equals(evidence.EvidenceHash, confirmation.EvidenceHash, StringComparison.Ordinal))
            return Failure(Ra2VoxelColourSkillRouteFailureKind.UnitClassConfirmationStale,
                "The confirmed unit class targets stale evidence.");

        Ra2VoxelUnitAdaptationPolicy adaptation = Ra2VoxelUnitAdaptationCatalog.For(confirmation.UnitClass);
        Ra2AgentSkillDescriptor? classifier = FindExact(catalog, Ra2VoxelUnitClassProposal.RequiredClassifierSkillId);
        if (classifier is null || !HasValidIdentity(classifier))
            return Failure(Ra2VoxelColourSkillRouteFailureKind.ClassifierSkillUnavailable,
                "The required unit-classification Skill is unavailable.");
        Ra2AgentSkillDescriptor? colour = FindExact(catalog, adaptation.ColouringSkillId);
        if (colour is null || !HasValidIdentity(colour))
            return Failure(Ra2VoxelColourSkillRouteFailureKind.ColourSkillUnavailable,
                "The colouring Skill required by the confirmed unit class is unavailable.");
        if (!string.Equals(colour.Name, adaptation.ColouringSkillId, StringComparison.Ordinal))
            return Failure(Ra2VoxelColourSkillRouteFailureKind.ColourSkillMismatch,
                "The selected colouring Skill does not match the confirmed unit class.");

        int instructionCharacters;
        try
        {
            instructionCharacters = checked(additionalInstructionCharacters + colour.Instructions.Length);
        }
        catch (OverflowException)
        {
            instructionCharacters = int.MaxValue;
        }
        if (additionalInstructionCharacters < 0 || instructionCharacters > Ra2AgentSkillCatalog.MaximumSelectedSkillCharacters)
            return Failure(Ra2VoxelColourSkillRouteFailureKind.InstructionLimitExceeded,
                "The compiler and class-specific Skill exceed the instruction limit.");

        return new(Ra2VoxelColourSkillRouteFailureKind.None, string.Empty,
            new Ra2VoxelColourSkillRoute(classifier, colour, adaptation));
    }

    private static Ra2AgentSkillDescriptor? FindExact(Ra2AgentSkillCatalog catalog, string id)
    {
        Ra2AgentSkillDescriptor[] matches = catalog.Skills
            .Where(skill => string.Equals(skill.Name, id, StringComparison.Ordinal))
            .ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static bool HasValidIdentity(Ra2AgentSkillDescriptor skill) =>
        IsIdentifier(skill.Name) && IsIdentifier(skill.Version) &&
        skill.ContentHash.Length == 64 && skill.ContentHash.All(char.IsAsciiHexDigit);

    private static bool IsIdentifier(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 96 && char.IsAsciiLetterOrDigit(value[0]) &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '-' or '_');

    private static Ra2VoxelColourSkillRouteResult Failure(
        Ra2VoxelColourSkillRouteFailureKind kind,
        string message) => new(kind, message, null);
}
