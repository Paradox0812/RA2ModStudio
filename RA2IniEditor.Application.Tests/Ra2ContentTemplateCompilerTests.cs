using RA2IniEditor.Application.Automation;
using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2ContentTemplateCompilerTests
{
    [Fact]
    public void Definition_EnforcesIdentityImmutabilityAndDeclaredParameterReferences()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2ContentTemplateDefinition(
            "sample", 0, "Sample", [], [Section("S", Ra2SectionKind.Weapon)]));
        Assert.Throws<ArgumentException>(() => new Ra2ContentTemplateDefinition(
            "sample", 1, "Sample",
            [Parameter("id"), Parameter("id")],
            [Section("S", Ra2SectionKind.Weapon)]));
        Assert.Throws<ArgumentException>(() => new Ra2ContentTemplateDefinition(
            "sample", 1, "Sample", [],
            [new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Parameter("missing"),
                Ra2SectionKind.Weapon,
                [])]));
        Assert.Throws<ArgumentException>(() => new Ra2ContentTemplateDefinition(
            "sample", 1, "Sample",
            [new Ra2ContentTemplateParameter("optional", Ra2ContentTemplateParameterKind.String, required: false)],
            [new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Literal("S"),
                Ra2SectionKind.Weapon,
                [new Ra2ContentTemplateFieldSpec(
                    "Damage",
                    Ra2ContentTemplateValueSource.Parameter("optional"))]) ]));

        Ra2ContentTemplateDefinition definition = Definition();
        Assert.Equal("weapon-skeleton", definition.Id);
        Assert.Equal(1, definition.Version);
        Assert.Throws<NotSupportedException>(() => ((IList<Ra2ContentTemplateParameter>)definition.Parameters).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Ra2ContentTemplateSectionSpec>)definition.Sections).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Ra2ContentTemplateRegistrationSpec>)definition.Registrations).Clear());

        Assert.Throws<ArgumentException>(() => new Ra2ContentTemplateDefinition(
            "bad-registration-source", 1, "Bad registration source", [],
            [Section("S", Ra2SectionKind.Vehicle)],
            [Registration("VehicleTypes", "missing", Ra2SectionKind.Vehicle)]));
    }

    [Fact]
    public void Compile_BindsDefaultsAndProducesStableOrderedPlan()
    {
        Ra2ContentTemplateDefinition definition = Definition(includeDamage: true);
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(Provider(
            Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference),
            Field(Ra2SectionKind.Weapon, "Damage", Ra2FieldValueKind.Integer)));
        Ra2ContentTemplateCompiler compiler = new();

        Ra2ContentTemplateCompilationResult first = compiler.Compile(
            definition,
            [new("weaponId", "TestWeapon"), new("projectileId", "TestProjectile")],
            snapshot);
        Ra2ContentTemplateCompilationResult second = compiler.Compile(
            definition,
            [new("weaponId", "TestWeapon"), new("projectileId", "TestProjectile")],
            snapshot);

        Assert.True(first.Succeeded, first.Message);
        Assert.True(second.Succeeded, second.Message);
        Assert.Equal(["TestWeapon", "TestProjectile"], first.Plan!.SectionCreations.Select(item => item.SectionName));
        Assert.Equal(["Projectile", "Damage"], first.Plan.Operations.Select(item => item.Key));
        Assert.Equal(["TestProjectile", "100"], first.Plan.Operations.Select(item => item.Value));
        Assert.Equal(first.Plan.SectionCreations.Select(ProjectSection), second.Plan!.SectionCreations.Select(ProjectSection));
        Assert.Equal(first.Plan.Operations.Select(ProjectOperation), second.Plan.Operations.Select(ProjectOperation));
        Assert.NotEqual(first.Plan.PlanId, second.Plan.PlanId);
        Assert.Equal(snapshot.DocumentId, first.Plan.ExpectedDocumentId);
        Assert.Equal(snapshot.Version, first.Plan.ExpectedVersion);
        Assert.Equal(snapshot.FieldRegistry.Revision, first.Plan.ExpectedFieldRegistryRevision);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("invalid")]
    public void Compile_RejectsInvalidArgumentSetsWithoutPartialPlan(string scenario)
    {
        IEnumerable<KeyValuePair<string, string>> arguments = scenario switch
        {
            "missing" => [new("projectileId", "P")],
            "unknown" => [new("weaponId", "W"), new("projectileId", "P"), new("extra", "x")],
            "duplicate" => [new("weaponId", "W"), new("weaponId", "W2"), new("projectileId", "P")],
            _ => [new("weaponId", "[bad]"), new("projectileId", "P")]
        };

        Ra2ContentTemplateCompilationResult result = new Ra2ContentTemplateCompiler().Compile(
            Definition(), arguments, Snapshot(Provider(Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference))));

        Ra2ContentTemplateCompilationFailureKind expected = scenario switch
        {
            "missing" => Ra2ContentTemplateCompilationFailureKind.MissingArgument,
            "unknown" => Ra2ContentTemplateCompilationFailureKind.UnknownArgument,
            "duplicate" => Ra2ContentTemplateCompilationFailureKind.DuplicateArgument,
            _ => Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue
        };
        AssertFailure(result, expected);
    }

    [Fact]
    public void Compile_RequiresEffectiveSchemaAndValidatesSchemaValue()
    {
        Ra2ContentTemplateDefinition definition = Definition(includeDamage: true);
        Ra2ContentTemplateCompiler compiler = new();
        KeyValuePair<string, string>[] validArguments = [new("weaponId", "W"), new("projectileId", "P"), new("damage", "100")];
        KeyValuePair<string, string>[] invalidArguments = [new("weaponId", "W"), new("projectileId", "P"), new("damage", "not-an-integer")];

        Ra2ContentTemplateCompilationResult missing = compiler.Compile(
            definition,
            validArguments,
            Snapshot(Provider(Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference))));
        Ra2ContentTemplateCompilationResult invalid = compiler.Compile(
            definition,
            invalidArguments,
            Snapshot(Provider(
                Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference),
                Field(Ra2SectionKind.Weapon, "Damage", Ra2FieldValueKind.Integer))));

        AssertFailure(missing, Ra2ContentTemplateCompilationFailureKind.FieldSchemaNotFound);
        AssertFailure(invalid, Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue);

        Ra2ContentTemplateDefinition schemaInvalidDefinition = new(
            "schema-invalid", 1, "Schema invalid",
            [Parameter("weaponId")],
            [new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Parameter("weaponId"),
                Ra2SectionKind.Weapon,
                [new Ra2ContentTemplateFieldSpec("Damage", Ra2ContentTemplateValueSource.Literal("not-an-integer"))])]);
        Ra2ContentTemplateCompilationResult schemaInvalid = compiler.Compile(
            schemaInvalidDefinition,
            [new("weaponId", "W")],
            Snapshot(Provider(Field(Ra2SectionKind.Weapon, "Damage", Ra2FieldValueKind.Integer))));
        AssertFailure(schemaInvalid, Ra2ContentTemplateCompilationFailureKind.InvalidFieldValue);
    }

    [Fact]
    public void Compile_MapsBlockedAndCautionTrustWithoutPartialFailures()
    {
        Ra2ContentTemplateDefinition definition = Definition();
        KeyValuePair<string, string>[] arguments = [new("weaponId", "W"), new("projectileId", "P")];
        Ra2ContentTemplateCompiler compiler = new();

        Ra2ContentTemplateCompilationResult blocked = compiler.Compile(
            definition,
            arguments,
            Snapshot(Provider(Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference, "guardrail"))));
        Ra2ContentTemplateCompilationResult caution = compiler.Compile(
            definition,
            arguments,
            Snapshot(Provider(Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference, "inferred"))));

        AssertFailure(blocked, Ra2ContentTemplateCompilationFailureKind.BlockedFieldTrust);
        Assert.True(caution.Succeeded, caution.Message);
        Ra2ContentTemplateCompilationWarning warning = Assert.Single(caution.Warnings);
        Assert.Equal("W", warning.SectionName);
        Assert.Equal("Projectile", warning.Key);
        Assert.Equal(Ra2AutomationFieldTrustLevel.Inferred, warning.TrustLevel);
    }

    [Fact]
    public void Compile_RejectsResolvedSectionConflictsAndExistingSections()
    {
        Ra2ContentTemplateDefinition sameNames = new(
            "same", 1, "Same",
            [Parameter("first"), Parameter("second")],
            [
                new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("first"), Ra2SectionKind.Weapon, []),
                new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("second"), Ra2SectionKind.Projectile, [])
            ]);
        Ra2ContentTemplateCompiler compiler = new();

        Ra2ContentTemplateCompilationResult collision = compiler.Compile(
            sameNames,
            [new("first", "SameName"), new("second", "samename")],
            Snapshot(Provider()));
        Ra2ContentTemplateCompilationResult existing = compiler.Compile(
            Definition(),
            [new("weaponId", "Existing"), new("projectileId", "P")],
            Snapshot(Provider(Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference)), "[Existing]\nDamage=1\n"));

        AssertFailure(collision, Ra2ContentTemplateCompilationFailureKind.ConflictingSections);
        AssertFailure(existing, Ra2ContentTemplateCompilationFailureKind.SectionAlreadyExists);
    }

    [Fact]
    public void Compile_HonorsCancellationAndTotalWorkBudget()
    {
        using CancellationTokenSource source = new();
        source.Cancel();
        Ra2ContentTemplateCompilationResult canceled = new Ra2ContentTemplateCompiler().Compile(
            Definition(),
            [new("weaponId", "W"), new("projectileId", "P")],
            Snapshot(Provider(Field(Ra2SectionKind.Weapon, "Projectile", Ra2FieldValueKind.Reference))),
            source.Token);

        AssertFailure(canceled, Ra2ContentTemplateCompilationFailureKind.Canceled);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2ContentTemplateDefinition(
            "oversized", 1, "Oversized", [],
            Enumerable.Range(0, Ra2AutomationEditPlan.MaximumOperationCount + 1)
                .Select(index => Section($"S{index}", Ra2SectionKind.Weapon))));

        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2ContentTemplateDefinition(
            "oversized-with-registration", 1, "Oversized with registration", [],
            Enumerable.Range(0, Ra2AutomationEditPlan.MaximumOperationCount - 1)
                .Select(index => Section($"S{index}", Ra2SectionKind.Vehicle)),
            [
                new Ra2ContentTemplateRegistrationSpec(
                    "VehicleTypes",
                    Ra2ContentTemplateValueSource.Literal("S0"),
                    Ra2SectionKind.Vehicle),
                new Ra2ContentTemplateRegistrationSpec(
                    "VehicleTypes",
                    Ra2ContentTemplateValueSource.Literal("S1"),
                    Ra2SectionKind.Vehicle)
            ]));
    }

    [Theory]
    [InlineData("[VehicleTypes]\n", "0")]
    [InlineData("[VehicleTypes]\n0=HTNK\n2=MTNK\n", "3")]
    public void Compile_AllocatesNextRegistrationIndexAndCanonicalPreview(string text, string expectedKey)
    {
        Ra2AutomationDocumentSnapshot snapshot = Snapshot(Provider(), text, isEditable: true);
        Ra2ContentTemplateCompilationResult result = new Ra2ContentTemplateCompiler().Compile(
            RegisteredVehicleDefinition(),
            [new("vehicleId", "NEWTANK")],
            snapshot);

        Assert.True(result.Succeeded, result.Message);
        Ra2AutomationEditOperation operation = Assert.Single(result.Plan!.Operations);
        Assert.Equal(Ra2AutomationEditOperationKind.UpsertField, operation.Kind);
        Assert.Equal("VehicleTypes", operation.SectionName);
        Assert.Equal(expectedKey, operation.Key);
        Assert.Equal("NEWTANK", operation.Value);

        Ra2AutomationEditPreviewResult preview = new Ra2AutomationEditPreviewService().Preview(snapshot, result.Plan);
        Assert.True(preview.Succeeded, preview.Message);
        Assert.Contains($"{expectedKey}=NEWTANK", preview.CandidateText, StringComparison.Ordinal);
        Assert.Contains("[NEWTANK]", preview.CandidateText, StringComparison.Ordinal);
    }

    [Fact]
    public void Compile_ExistingUniqueRegistrationIsIdempotent()
    {
        Ra2ContentTemplateCompilationResult result = new Ra2ContentTemplateCompiler().Compile(
            RegisteredVehicleDefinition(),
            [new("vehicleId", "newtank")],
            Snapshot(Provider(), "[VehicleTypes]\n7=NEWTANK\n"));

        Assert.True(result.Succeeded, result.Message);
        Assert.Empty(result.Plan!.Operations);
        Assert.Equal("newtank", Assert.Single(result.Plan.SectionCreations).SectionName);
    }

    [Fact]
    public void Compile_AllocatesMultipleRegistrationsInDeclarationOrder()
    {
        Ra2ContentTemplateDefinition definition = new(
            "two-vehicles", 1, "Two vehicles",
            [Parameter("first"), Parameter("second")],
            [
                new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("first"), Ra2SectionKind.Vehicle, []),
                new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("second"), Ra2SectionKind.Vehicle, [])
            ],
            [
                Registration("VehicleTypes", "first", Ra2SectionKind.Vehicle),
                Registration("VehicleTypes", "second", Ra2SectionKind.Vehicle)
            ]);

        Ra2ContentTemplateCompilationResult result = new Ra2ContentTemplateCompiler().Compile(
            definition,
            [new("first", "A"), new("second", "B")],
            Snapshot(Provider(), "[VehicleTypes]\n4=BASE\n"));

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(
            ["VehicleTypes|5|A", "VehicleTypes|6|B"],
            result.Plan!.Operations.Select(operation => $"{operation.SectionName}|{operation.Key}|{operation.Value}"));
    }

    [Theory]
    [InlineData("[VehicleTypes]\nName=HTNK\n", "InvalidRegistrationList")]
    [InlineData("[VehicleTypes]\n-1=HTNK\n", "InvalidRegistrationList")]
    [InlineData("[VehicleTypes]\n0=HTNK\n00=MTNK\n", "InvalidRegistrationList")]
    [InlineData("[VehicleTypes]\n0=HTNK\n1=htnk\n", "DuplicateRegistration")]
    [InlineData("[VehicleTypes]\n0=[BAD]\n", "InvalidRegistrationList")]
    [InlineData("[VehicleTypes]\n2147483647=HTNK\n", "RegistrationIndexOverflow")]
    public void Compile_FailsClosedForMalformedOrExhaustedRegistrationLists(
        string text,
        string expectedFailureName)
    {
        Ra2ContentTemplateCompilationResult result = new Ra2ContentTemplateCompiler().Compile(
            RegisteredVehicleDefinition(),
            [new("vehicleId", "NEWTANK")],
            Snapshot(Provider(), text));

        AssertFailure(result, Enum.Parse<Ra2ContentTemplateCompilationFailureKind>(expectedFailureName));
    }

    [Fact]
    public void Compile_FailsClosedForMissingMismatchedUndeclaredAndUnsupportedRegistrations()
    {
        Ra2AutomationDocumentSnapshot noRegistry = Snapshot(Provider());
        AssertFailure(
            new Ra2ContentTemplateCompiler().Compile(
                RegisteredVehicleDefinition(),
                [new("vehicleId", "NEWTANK")],
                noRegistry),
            Ra2ContentTemplateCompilationFailureKind.RegistrationSectionNotFound);

        Ra2ContentTemplateDefinition mismatched = new(
            "mismatched", 1, "Mismatched",
            [Parameter("vehicleId")],
            [new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("vehicleId"), Ra2SectionKind.Vehicle, [])],
            [Registration("InfantryTypes", "vehicleId", Ra2SectionKind.Vehicle)]);
        AssertFailure(
            new Ra2ContentTemplateCompiler().Compile(
                mismatched,
                [new("vehicleId", "NEWTANK")],
                Snapshot(Provider(), "[InfantryTypes]\n")),
            Ra2ContentTemplateCompilationFailureKind.RegistrationSectionKindMismatch);

        Ra2ContentTemplateDefinition undeclared = new(
            "undeclared", 1, "Undeclared",
            [Parameter("vehicleId"), Parameter("otherId")],
            [new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("otherId"), Ra2SectionKind.Vehicle, [])],
            [Registration("VehicleTypes", "vehicleId", Ra2SectionKind.Vehicle)]);
        AssertFailure(
            new Ra2ContentTemplateCompiler().Compile(
                undeclared,
                [new("vehicleId", "NEWTANK"), new("otherId", "OTHER")],
                Snapshot(Provider(), "[VehicleTypes]\n")),
            Ra2ContentTemplateCompilationFailureKind.RegistrationTargetNotDeclared);

        Ra2ContentTemplateDefinition unsupported = new(
            "unsupported", 1, "Unsupported",
            [Parameter("vehicleId")],
            [new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("vehicleId"), Ra2SectionKind.Vehicle, [])],
            [new Ra2ContentTemplateRegistrationSpec(
                "VehicleTypes",
                Ra2ContentTemplateValueSource.Parameter("vehicleId"),
                Ra2SectionKind.Vehicle,
                Ra2ContentRegistrationPolicy.CrossFileArtifact)]);
        AssertFailure(
            new Ra2ContentTemplateCompiler().Compile(
                unsupported,
                [new("vehicleId", "NEWTANK")],
                Snapshot(Provider(), "[VehicleTypes]\n")),
            Ra2ContentTemplateCompilationFailureKind.InvalidRegistrationList);
    }

    private static Ra2ContentTemplateDefinition Definition(bool includeDamage = false)
    {
        List<Ra2ContentTemplateParameter> parameters =
        [
            Parameter("weaponId"),
            Parameter("projectileId")
        ];
        List<Ra2ContentTemplateFieldSpec> fields =
        [
            new("Projectile", Ra2ContentTemplateValueSource.Parameter("projectileId"))
        ];
        if (includeDamage)
        {
            parameters.Add(new Ra2ContentTemplateParameter("damage", Ra2ContentTemplateParameterKind.Integer, required: false, defaultValue: "100"));
            fields.Add(new Ra2ContentTemplateFieldSpec("Damage", Ra2ContentTemplateValueSource.Parameter("damage")));
        }

        return new Ra2ContentTemplateDefinition(
            "weapon-skeleton",
            1,
            "Weapon skeleton",
            parameters,
            [
                new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("weaponId"), Ra2SectionKind.Weapon, fields),
                new Ra2ContentTemplateSectionSpec(Ra2ContentTemplateValueSource.Parameter("projectileId"), Ra2SectionKind.Projectile, [])
            ]);
    }

    private static Ra2ContentTemplateParameter Parameter(string name)
        => new(name, Ra2ContentTemplateParameterKind.Identifier, required: true);

    private static Ra2ContentTemplateDefinition RegisteredVehicleDefinition()
        => new(
            "registered-vehicle", 1, "Registered vehicle",
            [Parameter("vehicleId")],
            [new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Parameter("vehicleId"),
                Ra2SectionKind.Vehicle,
                [])],
            [Registration("VehicleTypes", "vehicleId", Ra2SectionKind.Vehicle)]);

    private static Ra2ContentTemplateRegistrationSpec Registration(
        string registrySectionName,
        string objectIdParameter,
        Ra2SectionKind expectedObjectKind)
        => new(
            registrySectionName,
            Ra2ContentTemplateValueSource.Parameter(objectIdParameter),
            expectedObjectKind);

    private static Ra2ContentTemplateSectionSpec Section(string name, Ra2SectionKind kind)
        => new(Ra2ContentTemplateValueSource.Literal(name), kind, []);

    private static Ra2AutomationDocumentSnapshot Snapshot(
        IRa2FieldDefinitionProvider provider,
        string text = "",
        bool isEditable = false)
        => AutomationTestSupport.Snapshot(text, provider, version: 9, isEditable: isEditable);

    private static IRa2FieldDefinitionProvider Provider(params Ra2FieldDefinition[] definitions)
        => new StaticProvider(definitions);

    private static Ra2FieldDefinition Field(
        Ra2SectionKind kind,
        string key,
        Ra2FieldValueKind valueKind,
        string quality = "source-verified")
        => new(
            key,
            [kind],
            valueKind is Ra2FieldValueKind.Reference or Ra2FieldValueKind.ReferenceList ? FieldEditorKind.Reference : FieldEditorKind.Text,
            Ra2FieldSourceKind.BuiltIn,
            valueMetadata: new Ra2FieldValueMetadata(valueKind),
            registryQuality: quality);

    private static string ProjectSection(Ra2AutomationSectionCreateOperation operation)
        => $"{operation.SectionName}|{operation.ExpectedSectionKind}";

    private static string ProjectOperation(Ra2AutomationEditOperation operation)
        => $"{operation.Kind}|{operation.SectionName}|{operation.Key}|{operation.Value}";

    private static void AssertFailure(Ra2ContentTemplateCompilationResult result, Ra2ContentTemplateCompilationFailureKind expected)
    {
        Assert.False(result.Succeeded);
        Assert.Equal(expected, result.FailureKind);
        Assert.Null(result.Plan);
        Assert.Empty(result.Warnings);
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
