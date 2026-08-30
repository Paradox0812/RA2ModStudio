using System.Reflection;
using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationTemplateServiceTests
{
    private const string TemplateId = "weapon-projectile-warhead-skeleton";

    [Fact]
    public void Catalog_ContainsSkeletonAndCompleteSourceAuditedDescriptors()
    {
        Ra2AutomationTemplateService service = new();
        IReadOnlyList<Ra2AutomationTemplateDescriptor> first = service.GetTemplates();
        Assert.Equal(9, first.Count);
        Ra2AutomationTemplateDescriptor descriptor = Assert.Single(first, item => item.Id == TemplateId);
        Ra2AutomationTemplateDescriptor complete = Assert.Single(first, item => item.Id == "weapon-projectile-warhead-direct-fire-complete");
        Ra2AutomationTemplateDescriptor dual = Assert.Single(first, item => item.Id == "techno-primary-secondary-direct-fire-complete");
        Ra2AutomationTemplateDescriptor arcing = Assert.Single(first, item => item.Id == "weapon-projectile-arcing-complete");
        Ra2AutomationTemplateDescriptor homing = Assert.Single(first, item => item.Id == "weapon-projectile-homing-complete");
        Ra2AutomationTemplateDescriptor warhead = Assert.Single(first, item => item.Id == "weapon-warhead-yr-core-complete");
        Ra2AutomationTemplateDescriptor unitDelivery = Assert.Single(first, item => item.Id == "ares-unitdelivery-superweapon-complete");
        Ra2AutomationTemplateDescriptor genericWarhead = Assert.Single(first, item => item.Id == "ares-genericwarhead-superweapon-complete");

        Assert.Same(first, service.GetTemplates());
        Assert.Equal(TemplateId, descriptor.Id);
        Assert.Equal(1, descriptor.Version);
        Assert.Contains("Weapon", descriptor.DisplayName, StringComparison.Ordinal);
        Assert.Contains("不生成玩法默认值", descriptor.Summary, StringComparison.Ordinal);
        Assert.Equal(Ra2AutomationTemplateOutputKind.Skeleton, descriptor.OutputKind);
        Assert.Equal(Ra2AutomationTemplateOutputKind.CompleteObject, complete.OutputKind);
        Assert.Equal(15, complete.Parameters.Count);
        Assert.Equal(Ra2AutomationTemplateOutputKind.CompleteObject, dual.OutputKind);
        Assert.Equal(27, dual.Parameters.Count);
        Assert.Contains("不表达循环", dual.Summary, StringComparison.Ordinal);
        Assert.Equal(8, arcing.Parameters.Count);
        Assert.Equal(6, homing.Parameters.Count);
        Assert.Equal(14, warhead.Parameters.Count);
        Assert.Equal(15, unitDelivery.Parameters.Count);
        Assert.Equal(15, genericWarhead.Parameters.Count);
        Assert.False(unitDelivery.IsProjectTemplate);
        Assert.False(genericWarhead.ProducesAssetManifest);
        Assert.All([arcing, homing, warhead], item => Assert.Equal(Ra2AutomationTemplateOutputKind.CompleteObject, item.OutputKind));
        Assert.Equal(["weaponId", "projectileId", "warheadId"], descriptor.Parameters.Select(parameter => parameter.Name));
        Assert.All(descriptor.Parameters, parameter =>
        {
            Assert.Equal(Ra2AutomationTemplateParameterKind.Identifier, parameter.Kind);
            Assert.True(parameter.Required);
            Assert.Null(parameter.DefaultValue);
        });
        Assert.Throws<NotSupportedException>(() => ((IList<Ra2AutomationTemplateDescriptor>)first).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Ra2AutomationTemplateParameterDescriptor>)descriptor.Parameters).Clear());
    }

    [Fact]
    public void ExpandArcingProjectile_BindsExistingWeaponAndKeepsTrajectoryFamilyExclusive()
    {
        Ra2AutomationDocumentSnapshot snapshot = WeaponSnapshot(ProjectileWarheadProvider());
        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(
            snapshot,
            new Ra2AutomationTemplateExpansionRequest(
                "weapon-projectile-arcing-complete",
                1,
                [
                    new("weaponId", "TestWeapon"),
                    new("projectileId", "TestShell"),
                    new("image", "120MM"),
                    new("antiAir", "no"),
                    new("antiGround", "yes"),
                    new("subjectToWalls", "yes"),
                    new("subjectToElevation", "yes"),
                    new("subjectToCliffs", "yes")
                ]));

        Assert.True(result.Succeeded, result.Message);
        Assert.Single(result.Plan!.SectionCreations);
        Assert.Equal(8, result.Plan.Operations.Count);
        Assert.Contains(result.Plan.Operations, item => item.SectionName == "TestWeapon" && item.Key == "Projectile" && item.Value == "TestShell");
        Assert.Contains(result.Plan.Operations, item => item.SectionName == "TestShell" && item.Key == "Arcing" && item.Value == "yes");
        Assert.DoesNotContain(result.Plan.Operations, item => item.Key is "ROT" or "Vertical" or "Inviso" or "Trajectory");
    }

    [Fact]
    public void ExpandHomingProjectile_BindsExistingWeaponAndRejectsNonPositiveRot()
    {
        Ra2AutomationDocumentSnapshot snapshot = WeaponSnapshot(ProjectileWarheadProvider());
        Ra2AutomationTemplateExpansionRequest valid = new(
            "weapon-projectile-homing-complete",
            1,
            [
                new("weaponId", "TestWeapon"),
                new("projectileId", "TestMissile"),
                new("image", "DRAGON"),
                new("rot", "8"),
                new("antiAir", "yes"),
                new("antiGround", "yes")
            ]);

        Ra2AutomationTemplateExpansionResult success = new Ra2AutomationTemplateService().ExpandTemplate(snapshot, valid);
        Assert.True(success.Succeeded, success.Message);
        Assert.Single(success.Plan!.SectionCreations);
        Assert.Equal(5, success.Plan.Operations.Count);
        Assert.Contains(success.Plan.Operations, item => item.SectionName == "TestMissile" && item.Key == "ROT" && item.Value == "8");
        Assert.DoesNotContain(success.Plan.Operations, item => item.Key is "Arcing" or "Vertical" or "Inviso" or "Trajectory");

        Ra2AutomationTemplateExpansionResult rejected = new Ra2AutomationTemplateService().ExpandTemplate(
            snapshot,
            new Ra2AutomationTemplateExpansionRequest(
                valid.TemplateId,
                valid.TemplateVersion,
                valid.Arguments.Select(item => item.Name == "rot" ? new Ra2AutomationTemplateArgument("rot", "0") : item)));
        AssertFailure(rejected, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments);
    }

    [Fact]
    public void ExpandYrCoreWarhead_ProducesCompleteBoundedProfileAndRejectsUnsafeRanges()
    {
        Ra2AutomationDocumentSnapshot snapshot = WeaponSnapshot(ProjectileWarheadProvider());
        Ra2AutomationTemplateExpansionRequest valid = YrCoreWarheadRequest();

        Ra2AutomationTemplateExpansionResult success = new Ra2AutomationTemplateService().ExpandTemplate(snapshot, valid);
        Assert.True(success.Succeeded, success.Message);
        Assert.Single(success.Plan!.SectionCreations);
        Assert.Equal(13, success.Plan.Operations.Count);
        Assert.Contains(success.Plan.Operations, item => item.SectionName == "TestWeapon" && item.Key == "Warhead" && item.Value == "TestWH");
        Assert.Contains(success.Plan.Operations, item => item.SectionName == "TestWH" && item.Key == "ProneDamage" && item.Value == "0.5");
        Assert.Contains(success.Plan.Operations, item => item.SectionName == "TestWH" && item.Key == "Bright" && item.Value == "no");
        Assert.DoesNotContain(success.Plan.Operations, item => item.Key.StartsWith("Versus.", StringComparison.OrdinalIgnoreCase));

        foreach ((string name, string value) in new[]
                 {
                     ("infDeath", "11"),
                     ("cellSpread", "11.1"),
                     ("percentAtMax", "-0.1"),
                     ("proneDamage", "-1")
                 })
        {
            Ra2AutomationTemplateExpansionResult rejected = new Ra2AutomationTemplateService().ExpandTemplate(
                snapshot,
                ReplaceArgument(valid, name, value));
            AssertFailure(rejected, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments);
        }

        Ra2AutomationTemplateExpansionResult customArmorRejected = new Ra2AutomationTemplateService().ExpandTemplate(
            WeaponSnapshot(ProjectileWarheadProvider(), "\n[ArmorTypes]\npaper=steel\n"),
            valid);
        AssertFailure(customArmorRejected, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments);
    }

    [Fact]
    public void ExpandDualArmamentTemplate_BindsBothSlotsAndCreatesTwoClosedChains()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            Provider(
                Field("Primary", Ra2SectionKind.Vehicle, Ra2FieldValueKind.Reference),
                Field("Secondary", Ra2SectionKind.Vehicle, Ra2FieldValueKind.Reference),
                Field("Damage", Ra2SectionKind.Weapon, Ra2FieldValueKind.Integer),
                Field("ROF", Ra2SectionKind.Weapon, Ra2FieldValueKind.Integer),
                Field("Range", Ra2SectionKind.Weapon, Ra2FieldValueKind.Float),
                Field("Projectile", Ra2SectionKind.Weapon, Ra2FieldValueKind.Reference),
                Field("Speed", Ra2SectionKind.Weapon, Ra2FieldValueKind.Integer),
                Field("Warhead", Ra2SectionKind.Weapon, Ra2FieldValueKind.Reference),
                Field("Inviso", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
                Field("Image", Ra2SectionKind.Projectile, Ra2FieldValueKind.Reference),
                Field("AA", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
                Field("AG", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
                Field("Verses", Ra2SectionKind.Warhead, Ra2FieldValueKind.String),
                Field("InfDeath", Ra2SectionKind.Warhead, Ra2FieldValueKind.Integer),
                Field("CellSpread", Ra2SectionKind.Warhead, Ra2FieldValueKind.Float),
                Field("PercentAtMax", Ra2SectionKind.Warhead, Ra2FieldValueKind.Float)),
            "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nPrimary=OldWeapon\n");

        List<Ra2AutomationTemplateArgument> arguments = [new("ownerSectionId", "HTNK")];
        AddChainArguments(arguments, "primary", "HTNKMain", "120", "60", "5.75", "40", false, true);
        AddChainArguments(arguments, "secondary", "HTNKCoax", "15", "10", "6", "100", false, true);

        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(
            snapshot,
            new Ra2AutomationTemplateExpansionRequest(
                "techno-primary-secondary-direct-fire-complete",
                1,
                arguments));

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(6, result.Plan!.SectionCreations.Count);
        Assert.Equal(30, result.Plan.Operations.Count);
        Assert.Contains(result.Plan.Operations, item => item.SectionName == "HTNK" && item.Key == "Primary" && item.Value == "HTNKMainWeapon");
        Assert.Contains(result.Plan.Operations, item => item.SectionName == "HTNK" && item.Key == "Secondary" && item.Value == "HTNKCoaxWeapon");
        Assert.All(result.Plan.SectionCreations, section =>
            Assert.Contains(result.Plan.Operations, operation => operation.SectionName == section.SectionName));
    }

    [Fact]
    public void ExpandCompleteTemplate_BindsExistingOwnerAndCreatesClosedNonEmptyChain()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(
            Provider(
                Field("Primary", Ra2SectionKind.Vehicle, Ra2FieldValueKind.Reference),
                Field("Damage", Ra2SectionKind.Weapon, Ra2FieldValueKind.Integer),
                Field("ROF", Ra2SectionKind.Weapon, Ra2FieldValueKind.Integer),
                Field("Range", Ra2SectionKind.Weapon, Ra2FieldValueKind.Float),
                Field("Projectile", Ra2SectionKind.Weapon, Ra2FieldValueKind.Reference),
                Field("Speed", Ra2SectionKind.Weapon, Ra2FieldValueKind.Integer),
                Field("Warhead", Ra2SectionKind.Weapon, Ra2FieldValueKind.Reference),
                Field("Inviso", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
                Field("Image", Ra2SectionKind.Projectile, Ra2FieldValueKind.Reference),
                Field("AA", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
                Field("AG", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
                Field("Verses", Ra2SectionKind.Warhead, Ra2FieldValueKind.String),
                Field("InfDeath", Ra2SectionKind.Warhead, Ra2FieldValueKind.Integer),
                Field("CellSpread", Ra2SectionKind.Warhead, Ra2FieldValueKind.Float),
                Field("PercentAtMax", Ra2SectionKind.Warhead, Ra2FieldValueKind.Float)),
            "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nPrimary=OldWeapon\n");
        Ra2AutomationTemplateExpansionRequest request = new(
            "weapon-projectile-warhead-direct-fire-complete",
            1,
            [
                new("ownerSectionId", "HTNK"), new("ownerWeaponSlot", "Primary"),
                new("weaponId", "HTNKCoaxMG"), new("projectileId", "HTNKCoaxBullet"),
                new("warheadId", "HTNKCoaxWH"), new("damage", "25"), new("rof", "20"),
                new("range", "5"), new("projectileSpeed", "100"),
                new("verses", "100%,100%,100%,50%,50%,50%,25%,25%,25%,100%,100%"),
                new("infDeath", "1"), new("cellSpread", "0"), new("percentAtMax", "1"),
                new("antiAir", "no"), new("antiGround", "yes")
            ]);

        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(snapshot, request);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(3, result.Plan!.SectionCreations.Count);
        Assert.Equal(15, result.Plan.Operations.Count);
        Assert.Contains(result.Plan.Operations, operation =>
            operation.SectionName == "HTNK" && operation.Key == "Primary" && operation.Value == "HTNKCoaxMG");
        Assert.All(result.Plan.SectionCreations, section =>
            Assert.Contains(result.Plan.Operations, operation => operation.SectionName == section.SectionName));
    }

    [Fact]
    public void Request_DefensivelyCopiesArgumentsAndPreservesDuplicatesForTypedFailure()
    {
        List<Ra2AutomationTemplateArgument> arguments =
        [
            new("weaponId", "W"),
            new("weaponId", "W2")
        ];
        Ra2AutomationTemplateExpansionRequest request = new(TemplateId, 1, arguments);
        arguments.Clear();

        Assert.Equal(2, request.Arguments.Count);
        Assert.Throws<NotSupportedException>(() => ((IList<Ra2AutomationTemplateArgument>)request.Arguments).Clear());
        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(
            Snapshot(Provider()),
            request);
        AssertFailure(result, Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument);
    }

    [Fact]
    public void ExpandTemplate_ProducesBoundOrderedPlanAndCanonicalPreview()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(Provider(
            Field("Projectile", "source-verified"),
            Field("Warhead", "source-verified")));
        Ra2AutomationTemplateExpansionRequest request = Request("TestWeapon", "TestProjectile", "TestWarhead");
        Ra2AutomationTemplateExpansionResult expansion = new Ra2AutomationTemplateService().ExpandTemplate(snapshot, request);

        Assert.True(expansion.Succeeded, expansion.Message);
        Assert.Equal(snapshot.DocumentId, expansion.DocumentId);
        Assert.Equal(snapshot.Version, expansion.Version);
        Assert.Equal(snapshot.FieldRegistry.Revision, expansion.FieldRegistryRevision);
        Assert.Equal(["TestWeapon", "TestProjectile", "TestWarhead"], expansion.Plan!.SectionCreations.Select(section => section.SectionName));
        Assert.Equal(["Projectile", "Warhead"], expansion.Plan.Operations.Select(operation => operation.Key));
        Assert.Equal(["TestProjectile", "TestWarhead"], expansion.Plan.Operations.Select(operation => operation.Value));

        Ra2AutomationEditPreviewResult preview = new Ra2AutomationEditPreviewService().Preview(snapshot, expansion.Plan);
        Assert.True(preview.Succeeded, preview.Message);
        Assert.Contains("[TestWeapon]\nProjectile=TestProjectile\nWarhead=TestWarhead", preview.CandidateText, StringComparison.Ordinal);
        Assert.Contains("[TestProjectile]", preview.CandidateText, StringComparison.Ordinal);
        Assert.Contains("[TestWarhead]", preview.CandidateText, StringComparison.Ordinal);
        Assert.Equal(3, preview.SectionCreationPreviews.Count);
        Assert.All(preview.SectionCreationPreviews, item => Assert.False(item.IsClassificationResolved));
    }

    [Theory]
    [InlineData("missing", Ra2AutomationTemplateExpansionFailureKind.MissingRequiredArgument)]
    [InlineData("unknown", Ra2AutomationTemplateExpansionFailureKind.UnknownArgument)]
    [InlineData("duplicate", Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument)]
    [InlineData("invalid", Ra2AutomationTemplateExpansionFailureKind.InvalidArguments)]
    public void ExpandTemplate_ReturnsTypedArgumentFailures(
        string scenario,
        Ra2AutomationTemplateExpansionFailureKind expected)
    {
        Ra2AutomationTemplateArgument[] arguments = scenario switch
        {
            "missing" => [new("weaponId", "W"), new("projectileId", "P")],
            "unknown" => [new("weaponId", "W"), new("projectileId", "P"), new("warheadId", "WH"), new("extra", "x")],
            "duplicate" => [new("weaponId", "W"), new("weaponId", "W2"), new("projectileId", "P"), new("warheadId", "WH")],
            _ => [new("weaponId", "[bad]"), new("projectileId", "P"), new("warheadId", "WH")]
        };

        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(
            Snapshot(Provider(Field("Projectile", "source-verified"), Field("Warhead", "source-verified"))),
            new Ra2AutomationTemplateExpansionRequest(TemplateId, 1, arguments));

        AssertFailure(result, expected);
    }

    [Fact]
    public void ExpandTemplate_FailsClosedForIdentitySchemaTrustSizeCancellationAndExistingSection()
    {
        Ra2AutomationTemplateService service = new();
        Ra2AutomationDocumentSnapshot normal = Snapshot(Provider(
            Field("Projectile", "source-verified"),
            Field("Warhead", "source-verified")));

        AssertFailure(
            service.ExpandTemplate(normal, new Ra2AutomationTemplateExpansionRequest("missing", 1, [])),
            Ra2AutomationTemplateExpansionFailureKind.TemplateNotFound);
        AssertFailure(
            service.ExpandTemplate(normal, new Ra2AutomationTemplateExpansionRequest(TemplateId, 2, [])),
            Ra2AutomationTemplateExpansionFailureKind.TemplateVersionMismatch);
        AssertFailure(
            service.ExpandTemplate(Snapshot(Provider(Field("Projectile", "source-verified"))), Request()),
            Ra2AutomationTemplateExpansionFailureKind.FieldSchemaNotFound);
        AssertFailure(
            service.ExpandTemplate(Snapshot(Provider(Field("Projectile", "guardrail"), Field("Warhead", "source-verified"))), Request()),
            Ra2AutomationTemplateExpansionFailureKind.BlockedFieldTrust);
        AssertFailure(
            service.ExpandTemplate(
                Snapshot(Provider(Field("Projectile", "source-verified"), Field("Warhead", "source-verified")), new string(';', Ra2AutomationDocumentQueryService.MaximumDocumentCharacters + 1)),
                Request()),
            Ra2AutomationTemplateExpansionFailureKind.DocumentTooLarge);
        AssertFailure(
            service.ExpandTemplate(
                Snapshot(Provider(Field("Projectile", "source-verified"), Field("Warhead", "source-verified")), "[TestWeapon]\n"),
                Request()),
            Ra2AutomationTemplateExpansionFailureKind.ExpansionFailed);

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        AssertFailure(service.ExpandTemplate(normal, Request(), cancellation.Token), Ra2AutomationTemplateExpansionFailureKind.Canceled);
    }

    [Fact]
    public void ExpandTemplate_ProjectsTypedCautionWarning()
    {
        Ra2AutomationTemplateExpansionResult result = new Ra2AutomationTemplateService().ExpandTemplate(
            Snapshot(Provider(Field("Projectile", "inferred"), Field("Warhead", "source-verified"))),
            Request());

        Assert.True(result.Succeeded, result.Message);
        Ra2AutomationTemplateWarningFact warning = Assert.Single(result.Warnings);
        Assert.Equal(Ra2AutomationTemplateWarningKind.FieldTrustCaution, warning.Kind);
        Assert.Equal("TestWeapon", warning.SectionName);
        Assert.Equal("Projectile", warning.Key);
        Assert.Equal(Ra2AutomationFieldTrustLevel.Inferred, warning.TrustLevel);
    }

    [Fact]
    public async Task Gateway_MatchesDirectServiceAndExpansionIsDeterministicAndThreadSafe()
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(Provider(
            Field("Projectile", "source-verified"),
            Field("Warhead", "source-verified")));
        Ra2AutomationTemplateExpansionRequest request = Request();
        Ra2AutomationTemplateService direct = new();
        Ra2AutomationCapabilityGateway gateway = new();

        Assert.Same(gateway.GetTemplates(), gateway.GetTemplates());
        Ra2AutomationTemplateExpansionResult directResult = direct.ExpandTemplate(snapshot, request);
        Ra2AutomationTemplateExpansionResult gatewayResult = gateway.ExpandTemplate(snapshot, request);
        AssertResultsEqual(directResult, gatewayResult);

        Ra2AutomationTemplateExpansionResult[] results = await Task.WhenAll(
            Enumerable.Range(0, 16).Select(_ => Task.Run(() => gateway.ExpandTemplate(snapshot, request))));
        Assert.All(results, result =>
        {
            Assert.True(result.Succeeded, result.Message);
            Assert.Equal(directResult.Plan!.Operations.Select(ProjectOperation), result.Plan!.Operations.Select(ProjectOperation));
            Assert.Equal(directResult.Plan.SectionCreations.Select(ProjectSection), result.Plan.SectionCreations.Select(ProjectSection));
        });
    }

    [Fact]
    public void TemplateContracts_HaveExactImmutablePublicShapeAndNoSerializationAttributes()
    {
        Type[] types =
        [
            typeof(IRa2AutomationTemplateService),
            typeof(Ra2AutomationTemplateService),
            typeof(Ra2AutomationTemplateDescriptor),
            typeof(Ra2AutomationTemplateOutputKind),
            typeof(Ra2AutomationTemplateParameterDescriptor),
            typeof(Ra2AutomationTemplateParameterKind),
            typeof(Ra2AutomationTemplateArgument),
            typeof(Ra2AutomationTemplateExpansionRequest),
            typeof(Ra2AutomationTemplateExpansionResult),
            typeof(Ra2AutomationTemplateExpansionFailureKind),
            typeof(Ra2AutomationTemplateWarningKind),
            typeof(Ra2AutomationTemplateWarningFact)
        ];
        Assert.All(types, type =>
            Assert.DoesNotContain(type.GetCustomAttributes(), attribute =>
                attribute.GetType().Namespace?.Contains("Serialization", StringComparison.Ordinal) == true ||
                attribute.GetType().Name.Contains("Json", StringComparison.Ordinal)));
        Assert.Empty(typeof(Ra2AutomationTemplateDescriptor).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(typeof(Ra2AutomationTemplateParameterDescriptor).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(typeof(Ra2AutomationTemplateWarningFact).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.All(
            types.Where(type => type.IsClass).SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)),
            property => Assert.Null(property.SetMethod));
        Assert.Equal(
            ["None", "TemplateNotFound", "TemplateVersionMismatch", "InvalidArguments", "MissingRequiredArgument", "UnknownArgument", "DuplicateArgument", "FieldSchemaNotFound", "BlockedFieldTrust", "OperationLimitExceeded", "DocumentTooLarge", "Canceled", "ExpansionFailed", "RequiredSectionNotFound", "RequiredSectionKindMismatch", "ProjectDocumentNotFound", "ProjectDocumentAmbiguous"],
            Enum.GetNames<Ra2AutomationTemplateExpansionFailureKind>());
    }

    private static Ra2AutomationTemplateExpansionRequest Request(
        string weapon = "TestWeapon",
        string projectile = "TestProjectile",
        string warhead = "TestWarhead")
        => new(
            TemplateId,
            1,
            [new("weaponId", weapon), new("projectileId", projectile), new("warheadId", warhead)]);

    private static void AddChainArguments(
        ICollection<Ra2AutomationTemplateArgument> arguments,
        string prefix,
        string idPrefix,
        string damage,
        string rof,
        string range,
        string speed,
        bool antiAir,
        bool antiGround)
    {
        arguments.Add(new($"{prefix}WeaponId", $"{idPrefix}Weapon"));
        arguments.Add(new($"{prefix}ProjectileId", $"{idPrefix}Projectile"));
        arguments.Add(new($"{prefix}WarheadId", $"{idPrefix}Warhead"));
        arguments.Add(new($"{prefix}Damage", damage));
        arguments.Add(new($"{prefix}Rof", rof));
        arguments.Add(new($"{prefix}Range", range));
        arguments.Add(new($"{prefix}ProjectileSpeed", speed));
        arguments.Add(new($"{prefix}Verses", "100%,100%,100%,50%,50%,50%,25%,25%,25%,100%,100%"));
        arguments.Add(new($"{prefix}InfDeath", "1"));
        arguments.Add(new($"{prefix}CellSpread", "0"));
        arguments.Add(new($"{prefix}PercentAtMax", "1"));
        arguments.Add(new($"{prefix}AntiAir", antiAir ? "yes" : "no"));
        arguments.Add(new($"{prefix}AntiGround", antiGround ? "yes" : "no"));
    }

    private static Ra2AutomationTemplateExpansionRequest YrCoreWarheadRequest()
        => new(
            "weapon-warhead-yr-core-complete",
            1,
            [
                new("weaponId", "TestWeapon"),
                new("warheadId", "TestWH"),
                new("verses", "100%,100%,100%,75%,75%,75%,50%,50%,50%,100%,100%"),
                new("infDeath", "2"),
                new("cellSpread", "1.5"),
                new("percentAtMax", "0.25"),
                new("proneDamage", "0.5"),
                new("conventional", "yes"),
                new("wall", "yes"),
                new("wood", "yes"),
                new("rocker", "no"),
                new("sparky", "yes"),
                new("tiberium", "no"),
                new("bright", "no")
            ]);

    private static Ra2AutomationTemplateExpansionRequest ReplaceArgument(
        Ra2AutomationTemplateExpansionRequest request,
        string name,
        string value)
        => new(
            request.TemplateId,
            request.TemplateVersion,
            request.Arguments.Select(item => item.Name == name ? new Ra2AutomationTemplateArgument(name, value) : item));

    private static Ra2AutomationDocumentSnapshot WeaponSnapshot(
        IRa2FieldDefinitionProvider provider,
        string suffix = "")
        => Snapshot(
            provider,
            "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nPrimary=TestWeapon\n\n[TestWeapon]\nDamage=100\n" + suffix);

    private static IRa2FieldDefinitionProvider ProjectileWarheadProvider()
        => Provider(
            Field("Primary", Ra2SectionKind.Vehicle, Ra2FieldValueKind.Reference),
            Field("Damage", Ra2SectionKind.Weapon, Ra2FieldValueKind.Integer),
            Field("Projectile", Ra2SectionKind.Weapon, Ra2FieldValueKind.Reference),
            Field("Warhead", Ra2SectionKind.Weapon, Ra2FieldValueKind.Reference),
            Field("Image", Ra2SectionKind.Projectile, Ra2FieldValueKind.Reference),
            Field("AA", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
            Field("AG", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
            Field("Arcing", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
            Field("ROT", Ra2SectionKind.Projectile, Ra2FieldValueKind.Integer),
            Field("SubjectToWalls", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
            Field("SubjectToElevation", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
            Field("SubjectToCliffs", Ra2SectionKind.Projectile, Ra2FieldValueKind.Boolean),
            Field("Verses", Ra2SectionKind.Warhead, Ra2FieldValueKind.String),
            Field("InfDeath", Ra2SectionKind.Warhead, Ra2FieldValueKind.Integer),
            Field("CellSpread", Ra2SectionKind.Warhead, Ra2FieldValueKind.Float),
            Field("PercentAtMax", Ra2SectionKind.Warhead, Ra2FieldValueKind.Float),
            Field("ProneDamage", Ra2SectionKind.Warhead, Ra2FieldValueKind.Float),
            Field("Conventional", Ra2SectionKind.Warhead, Ra2FieldValueKind.Boolean),
            Field("Wall", Ra2SectionKind.Warhead, Ra2FieldValueKind.Boolean),
            Field("Wood", Ra2SectionKind.Warhead, Ra2FieldValueKind.Boolean),
            Field("Rocker", Ra2SectionKind.Warhead, Ra2FieldValueKind.Boolean),
            Field("Sparky", Ra2SectionKind.Warhead, Ra2FieldValueKind.Boolean),
            Field("Tiberium", Ra2SectionKind.Warhead, Ra2FieldValueKind.Boolean),
            Field("Bright", Ra2SectionKind.Warhead, Ra2FieldValueKind.Boolean));

    private static Ra2AutomationDocumentSnapshot Snapshot(IRa2FieldDefinitionProvider provider, string text = "")
        => new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            12,
            "rulesmd.ini",
            text,
            isEditable: true,
            new Ra2AutomationFieldRegistrySnapshot(provider, 19));

    private static Ra2FieldDefinition Field(string key, string quality)
        => new(
            key,
            [Ra2SectionKind.Weapon],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.Yuri,
            valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference),
            registryQuality: quality);

    private static Ra2FieldDefinition Field(
        string key,
        Ra2SectionKind sectionKind,
        Ra2FieldValueKind valueKind)
        => new(
            key,
            [sectionKind],
            valueKind switch
            {
                Ra2FieldValueKind.Integer => FieldEditorKind.Integer,
                Ra2FieldValueKind.Float => FieldEditorKind.Float,
                Ra2FieldValueKind.Boolean => FieldEditorKind.Boolean,
                Ra2FieldValueKind.Reference => FieldEditorKind.Reference,
                _ => FieldEditorKind.Text
            },
            Ra2FieldSourceKind.Yuri,
            valueMetadata: new Ra2FieldValueMetadata(valueKind,
                valueKind == Ra2FieldValueKind.Boolean ? Ra2FieldBooleanValueStyle.YesNo : Ra2FieldBooleanValueStyle.Unknown),
            registryQuality: "source-verified");

    private static IRa2FieldDefinitionProvider Provider(params Ra2FieldDefinition[] definitions)
        => new StaticProvider(definitions);

    private static string ProjectOperation(Ra2AutomationEditOperation item)
        => $"{item.Kind}|{item.SectionName}|{item.Key}|{item.Value}";

    private static string ProjectSection(Ra2AutomationSectionCreateOperation item)
        => $"{item.SectionName}|{item.ExpectedSectionKind}";

    private static void AssertFailure(
        Ra2AutomationTemplateExpansionResult result,
        Ra2AutomationTemplateExpansionFailureKind expected)
    {
        Assert.False(result.Succeeded);
        Assert.Equal(expected, result.FailureKind);
        Assert.Null(result.Plan);
        Assert.Empty(result.Warnings);
    }

    private static void AssertResultsEqual(
        Ra2AutomationTemplateExpansionResult expected,
        Ra2AutomationTemplateExpansionResult actual)
    {
        Assert.Equal(expected.Succeeded, actual.Succeeded);
        Assert.Equal(expected.FailureKind, actual.FailureKind);
        Assert.Equal(expected.DocumentId, actual.DocumentId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.FieldRegistryRevision, actual.FieldRegistryRevision);
        Assert.Equal(expected.Plan?.Operations.Select(ProjectOperation), actual.Plan?.Operations.Select(ProjectOperation));
        Assert.Equal(expected.Plan?.SectionCreations.Select(ProjectSection), actual.Plan?.SectionCreations.Select(ProjectSection));
        Assert.Equal(expected.Warnings.Select(item => $"{item.Kind}|{item.SectionName}|{item.Key}|{item.TrustLevel}"),
            actual.Warnings.Select(item => $"{item.Kind}|{item.SectionName}|{item.Key}|{item.TrustLevel}"));
    }

    private sealed class StaticProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition[] _definitions;

        public StaticProvider(IEnumerable<Ra2FieldDefinition> definitions) => _definitions = definitions.ToArray();

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
}
