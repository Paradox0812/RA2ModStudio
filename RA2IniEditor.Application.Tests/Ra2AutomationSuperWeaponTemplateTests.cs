using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationSuperWeaponTemplateTests
{
    [Fact]
    public void UnitDelivery_BuildingProvider_ProducesClosedRegisteredPlanWithoutRegistrySchemas()
    {
        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(
            Snapshot(),
            UnitDeliveryRequest());

        Assert.True(result.Succeeded, result.Message);
        Assert.Single(result.Plan!.SectionCreations, section => section.SectionName == "GAREINFORCEMENTS");
        Assert.Contains(result.Plan.Operations, operation => operation.SectionName == "SuperWeaponTypes" && operation.Key == "3" && operation.Value == "GAREINFORCEMENTS");
        Assert.Contains(result.Plan.Operations, operation => operation.SectionName == "GAPOWR" && operation.Key == "SuperWeapon2" && operation.Value == "GAREINFORCEMENTS");
        Assert.Contains(result.Plan.Operations, operation => operation.SectionName == "GAREINFORCEMENTS" && operation.Key == "Type" && operation.Value == "UnitDelivery");
        Assert.Contains(result.Plan.Operations, operation => operation.SectionName == "GAREINFORCEMENTS" && operation.Key == "Deliver.Types" && operation.Value == "E1,FV");
        Assert.DoesNotContain(result.Plan.Operations, operation => operation.Key == "SW.AlwaysGranted");
    }

    [Fact]
    public void UnitDelivery_AlwaysGranted_HasNoProviderOperation()
    {
        Ra2AutomationTemplateExpansionRequest request = ReplaceArguments(
            UnitDeliveryRequest(),
            new Dictionary<string, string?>
            {
                ["providerMode"] = "always-granted",
                ["providerBuildingId"] = null,
                ["providerSlot"] = null
            });

        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(Snapshot(), request);

        Assert.True(result.Succeeded, result.Message);
        Assert.Contains(result.Plan!.Operations, operation => operation.SectionName == "GAREINFORCEMENTS" && operation.Key == "SW.AlwaysGranted" && operation.Value == "yes");
        Assert.DoesNotContain(result.Plan.Operations, operation => operation.SectionName == "GAPOWR");
    }

    [Fact]
    public void UnitDelivery_UniqueNameAliasesResolveToCanonicalSectionIds()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            "[SuperWeaponTypes]\n0=OLD1\n\n[BuildingTypes]\n0=GAPOWR\n\n[InfantryTypes]\n0=E1\n\n[VehicleTypes]\n0=FV\n\n[GAPOWR]\nUIName=Name:GAPOWR\nName=Allied Power Plant\n\n[E1]\nUIName=Name:E1\nName=GI\n\n[FV]\nUIName=Name:FV\nName=IFV\n");
        Ra2AutomationTemplateExpansionRequest request = ReplaceArguments(
            UnitDeliveryRequest(),
            new Dictionary<string, string?>
            {
                ["providerBuildingId"] = "Allied Power Plant",
                ["deliveryTypeIds"] = "GI,IFV"
            });

        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(snapshot, request);

        Assert.True(result.Succeeded, result.Message);
        Assert.Contains(result.Plan!.Operations, operation => operation.SectionName == "GAPOWR" && operation.Key == "SuperWeapon2");
        Assert.Contains(result.Plan.Operations, operation => operation.SectionName == "GAREINFORCEMENTS" && operation.Key == "Deliver.Types" && operation.Value == "E1,FV");
    }

    [Fact]
    public void UnitDelivery_AmbiguousNameAliasRemainsRejected()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            "[SuperWeaponTypes]\n0=OLD1\n\n[BuildingTypes]\n0=PWR1\n1=PWR2\n\n[InfantryTypes]\n0=E1\n\n[VehicleTypes]\n0=FV\n\n[PWR1]\nName=Allied Power Plant\n\n[PWR2]\nName=Allied Power Plant\n\n[E1]\nName=GI\n\n[FV]\nName=IFV\n");
        Ra2AutomationTemplateExpansionRequest request = ReplaceArguments(
            UnitDeliveryRequest(),
            new Dictionary<string, string?> { ["providerBuildingId"] = "Allied Power Plant" });

        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(snapshot, request);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.FailureKind,
            new[]
            {
                Ra2AutomationTemplateExpansionFailureKind.InvalidArguments,
                Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound
            });
        Assert.Null(result.Plan);
    }

    [Fact]
    public void GenericWarhead_UsesExistingWarheadAndDoesNotModifyIt()
    {
        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(
            Snapshot(),
            GenericWarheadRequest());

        Assert.True(result.Succeeded, result.Message);
        Assert.Contains(result.Plan!.Operations, operation => operation.SectionName == "NAEMPBLAST" && operation.Key == "Type" && operation.Value == "GenericWarhead");
        Assert.Contains(result.Plan.Operations, operation => operation.SectionName == "NAEMPBLAST" && operation.Key == "SW.Warhead" && operation.Value == "EMPWH");
        Assert.Contains(result.Plan.Operations, operation => operation.SectionName == "NAEMPBLAST" && operation.Key == "SW.Damage" && operation.Value == "1");
        Assert.DoesNotContain(result.Plan.Operations, operation => operation.SectionName == "EMPWH");
    }

    [Theory]
    [InlineData("missing-delivery", Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound)]
    [InlineData("wrong-warhead", Ra2AutomationTemplateExpansionFailureKind.RequiredSectionKindMismatch)]
    [InlineData("duplicate-delivery", Ra2AutomationTemplateExpansionFailureKind.InvalidArguments)]
    [InlineData("mixed-provider", Ra2AutomationTemplateExpansionFailureKind.InvalidArguments)]
    [InlineData("bad-action", Ra2AutomationTemplateExpansionFailureKind.InvalidArguments)]
    public void Profiles_FailAtomicallyForInvalidClosure(string scenario, Ra2AutomationTemplateExpansionFailureKind expected)
    {
        Ra2AutomationDocumentSnapshot snapshot = scenario == "wrong-warhead"
            ? Snapshot("[SuperWeaponTypes]\n2=OLDSW\n\n[BuildingTypes]\n0=GAPOWR\n1=NANRCT\n\n[InfantryTypes]\n0=E1\n\n[VehicleTypes]\n0=FV\n1=EMPWH\n\n[GAPOWR]\n\n[NANRCT]\n\n[E1]\n\n[FV]\n\n[EMPWH]\nPrimary=Bad\n")
            : Snapshot();
        Ra2AutomationTemplateExpansionRequest request = scenario switch
        {
            "missing-delivery" => ReplaceArguments(UnitDeliveryRequest(), new Dictionary<string, string?> { ["deliveryTypeIds"] = "E1,MISSING" }),
            "wrong-warhead" => GenericWarheadRequest(),
            "duplicate-delivery" => ReplaceArguments(UnitDeliveryRequest(), new Dictionary<string, string?> { ["deliveryTypeIds"] = "E1,e1" }),
            "mixed-provider" => ReplaceArguments(UnitDeliveryRequest(), new Dictionary<string, string?> { ["providerMode"] = "always-granted" }),
            _ => ReplaceArguments(UnitDeliveryRequest(), new Dictionary<string, string?> { ["action"] = " " })
        };

        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(snapshot, request);

        Assert.False(result.Succeeded);
        Assert.Equal(expected, result.FailureKind);
        Assert.Null(result.Plan);
    }

    private static Ra2AutomationTemplateExpansionRequest UnitDeliveryRequest()
        => new(
            "ares-unitdelivery-superweapon-complete",
            1,
            [
                new("superWeaponId", "GAREINFORCEMENTS"), new("providerMode", "building"),
                new("providerBuildingId", "GAPOWR"), new("providerSlot", "SuperWeapon2"),
                new("uiName", "NAME:Reinforcements"), new("name", "Reinforcements"),
                new("isPowered", "yes"), new("rechargeTime", "5"), new("action", "Custom"),
                new("sidebarImage", "REINICON"), new("showTimer", "yes"),
                new("disableableFromShell", "no"), new("aiTargeting", "ParaDrop"),
                new("deliveryTypeIds", "E1,FV"), new("deliveryOwner", "invoker")
            ]);

    private static Ra2AutomationTemplateExpansionRequest GenericWarheadRequest()
        => new(
            "ares-genericwarhead-superweapon-complete",
            1,
            [
                new("superWeaponId", "NAEMPBLAST"), new("providerMode", "building"),
                new("providerBuildingId", "NANRCT"), new("providerSlot", "SuperWeapon"),
                new("uiName", "NAME:EMPBlast"), new("name", "EMP Blast"),
                new("isPowered", "yes"), new("rechargeTime", "7"), new("action", "Custom"),
                new("sidebarImage", "EMPICON"), new("showTimer", "yes"),
                new("disableableFromShell", "no"), new("aiTargeting", "Offensive"),
                new("warheadId", "EMPWH"), new("damage", "1")
            ]);

    private static Ra2AutomationTemplateExpansionRequest ReplaceArguments(
        Ra2AutomationTemplateExpansionRequest request,
        IReadOnlyDictionary<string, string?> replacements)
        => new(
            request.TemplateId,
            request.TemplateVersion,
            request.Arguments
                .Where(argument => !replacements.TryGetValue(argument.Name, out string? value) || value is not null)
                .Select(argument => replacements.TryGetValue(argument.Name, out string? value) && value is not null
                    ? new Ra2AutomationTemplateArgument(argument.Name, value)
                    : argument));

    private static Ra2AutomationDocumentSnapshot Snapshot(string? text = null)
        => new(
            Guid.Parse("78787878-7878-7878-7878-787878787878"),
            4,
            "rulesmd.ini",
            text ?? "[SuperWeaponTypes]\n0=OLD1\n2=OLDSW\n\n[BuildingTypes]\n0=GAPOWR\n1=NANRCT\n\n[InfantryTypes]\n0=E1\n\n[VehicleTypes]\n0=FV\n\n[Warheads]\n0=EMPWH\n\n[GAPOWR]\n\n[NANRCT]\n\n[E1]\n\n[FV]\n\n[EMPWH]\nCellSpread=1\n",
            true,
            new Ra2AutomationFieldRegistrySnapshot(new EmptyProvider(), 5));

    private sealed class EmptyProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind) => [];
        public bool IsKnownField(Ra2SectionKind sectionKind, string key) => false;
    }
}
