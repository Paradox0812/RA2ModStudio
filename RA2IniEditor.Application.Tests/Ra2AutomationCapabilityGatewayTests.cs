using System.Reflection;
using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationCapabilityGatewayTests
{
    [Fact]
    public void Catalog_HasExactContractOrderIdentityRiskStabilityAndLimits()
    {
        Ra2AutomationCapabilityDescriptor[] capabilities =
            new Ra2AutomationCapabilityGateway().GetCapabilities().ToArray();

        Assert.Equal(9, capabilities.Length);
        Assert.Equal(
            new[]
            {
                Ra2AutomationCapabilityIds.DocumentSectionGet,
                Ra2AutomationCapabilityIds.DocumentReferencesFind,
                Ra2AutomationCapabilityIds.DocumentDiagnosticsValidate,
                Ra2AutomationCapabilityIds.DocumentEditPreview,
                Ra2AutomationCapabilityIds.DocumentFieldSchemaGet,
                Ra2AutomationCapabilityIds.DocumentReferenceResolve,
                Ra2AutomationCapabilityIds.ContentTemplateExpand,
                Ra2AutomationCapabilityIds.ProjectEditPreview,
                Ra2AutomationCapabilityIds.ProjectContentTemplateExpand
            },
            capabilities.Select(capability => capability.Id));
        Assert.All(capabilities, capability =>
        {
            Assert.Equal(Ra2AutomationCapabilityIds.CurrentVersion, capability.Version);
            Assert.Equal(Ra2AutomationCapabilityStability.Experimental, capability.Stability);
            Assert.Equal(
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                capability.MaximumDocumentCharacters);
        });

        Assert.Equal(Ra2AutomationCapabilityRisk.Query, capabilities[0].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Query, capabilities[1].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Query, capabilities[2].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Edit, capabilities[3].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Query, capabilities[4].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Query, capabilities[5].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Edit, capabilities[6].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Edit, capabilities[7].Risk);
        Assert.Equal(Ra2AutomationCapabilityRisk.Edit, capabilities[8].Risk);
        Assert.Equal(Ra2AutomationDocumentQueryService.MaximumResultItems, capabilities[0].MaximumResultItems);
        Assert.Equal(Ra2AutomationDocumentQueryService.MaximumResultItems, capabilities[1].MaximumResultItems);
        Assert.Equal(Ra2AutomationDocumentQueryService.MaximumResultItems, capabilities[2].MaximumResultItems);
        Assert.Equal(Ra2AutomationEditPreviewService.MaximumDiagnosticItems, capabilities[3].MaximumResultItems);
        Assert.Equal(1, capabilities[4].MaximumResultItems);
        Assert.Equal(1, capabilities[5].MaximumResultItems);
        Assert.Null(capabilities[6].MaximumResultItems);
        Assert.Equal(Ra2AutomationProjectSnapshot.MaximumDocumentCount, capabilities[7].MaximumResultItems);
        Assert.Equal(Ra2AutomationProjectSnapshot.MaximumDocumentCount, capabilities[8].MaximumResultItems);
        Assert.Null(capabilities[0].MaximumOperations);
        Assert.Null(capabilities[1].MaximumOperations);
        Assert.Null(capabilities[2].MaximumOperations);
        Assert.Equal(Ra2AutomationEditPlan.MaximumOperationCount, capabilities[3].MaximumOperations);
        Assert.Null(capabilities[4].MaximumOperations);
        Assert.Null(capabilities[5].MaximumOperations);
        Assert.Equal(Ra2AutomationEditPlan.MaximumOperationCount, capabilities[6].MaximumOperations);
        Assert.Equal(Ra2AutomationProjectEditPlan.MaximumAggregateWorkCount, capabilities[7].MaximumOperations);
        Assert.Equal(Ra2AutomationProjectEditPlan.MaximumAggregateWorkCount, capabilities[8].MaximumOperations);
    }

    [Fact]
    public void Catalog_IsCreatedOnceAndCannotBeMutatedByCaller()
    {
        Ra2AutomationCapabilityGateway gateway = new();
        IReadOnlyList<Ra2AutomationCapabilityDescriptor> first = gateway.GetCapabilities();

        Assert.Same(first, gateway.GetCapabilities());
        IList<Ra2AutomationCapabilityDescriptor> list =
            Assert.IsAssignableFrom<IList<Ra2AutomationCapabilityDescriptor>>(first);
        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list.Clear());
        Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
        Assert.Equal(9, first.Count);
    }

    [Fact]
    public void CapabilityContracts_HaveExactImmutablePublicSurface()
    {
        Type descriptor = typeof(Ra2AutomationCapabilityDescriptor);
        Assert.True(descriptor.IsSealed);
        Assert.Empty(descriptor.GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        Dictionary<string, Type> expectedProperties = new(StringComparer.Ordinal)
        {
            [nameof(Ra2AutomationCapabilityDescriptor.Id)] = typeof(string),
            [nameof(Ra2AutomationCapabilityDescriptor.Version)] = typeof(int),
            [nameof(Ra2AutomationCapabilityDescriptor.Risk)] = typeof(Ra2AutomationCapabilityRisk),
            [nameof(Ra2AutomationCapabilityDescriptor.Stability)] = typeof(Ra2AutomationCapabilityStability),
            [nameof(Ra2AutomationCapabilityDescriptor.MaximumDocumentCharacters)] = typeof(int),
            [nameof(Ra2AutomationCapabilityDescriptor.MaximumResultItems)] = typeof(int?),
            [nameof(Ra2AutomationCapabilityDescriptor.MaximumOperations)] = typeof(int?)
        };
        PropertyInfo[] properties = descriptor.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(expectedProperties.Count, properties.Length);
        Assert.All(properties, property =>
        {
            Assert.True(expectedProperties.TryGetValue(property.Name, out Type? expectedType));
            Assert.Equal(expectedType, property.PropertyType);
            Assert.Null(property.SetMethod);
        });

        Assert.Equal(new[] { "Query", "Edit" }, Enum.GetNames<Ra2AutomationCapabilityRisk>());
        Assert.Equal(new[] { 0, 1 }, Enum.GetValues<Ra2AutomationCapabilityRisk>().Select(value => (int)value));
        Assert.Equal(new[] { "Experimental" }, Enum.GetNames<Ra2AutomationCapabilityStability>());
        Assert.Equal(new[] { 0 }, Enum.GetValues<Ra2AutomationCapabilityStability>().Select(value => (int)value));
    }

    [Fact]
    public void CapabilityIds_HaveExactConstantSurfaceAndValues()
    {
        FieldInfo[] fields = typeof(Ra2AutomationCapabilityIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(10, fields.Length);
        Assert.All(fields, field => Assert.True(field.IsLiteral));
        Assert.Equal(1, (int)fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.CurrentVersion))
            .GetRawConstantValue()!);
        Assert.Equal(
            "ini.document.section.get",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.DocumentSectionGet))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.document.references.find",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.DocumentReferencesFind))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.document.diagnostics.validate",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.DocumentDiagnosticsValidate))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.document.edit.preview",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.DocumentEditPreview))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.document.field-schema.get",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.DocumentFieldSchemaGet))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.document.reference.resolve",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.DocumentReferenceResolve))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.content.template.expand",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.ContentTemplateExpand))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.project.edit.preview",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.ProjectEditPreview))
                .GetRawConstantValue());
        Assert.Equal(
            "ini.project.content.template.expand",
            fields.Single(field => field.Name == nameof(Ra2AutomationCapabilityIds.ProjectContentTemplateExpand))
                .GetRawConstantValue());
    }

    [Fact]
    public void Gateway_HasExactTypedInterfaceAndConcreteSurface()
    {
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.GetCapabilities),
            typeof(IReadOnlyList<Ra2AutomationCapabilityDescriptor>));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.GetTemplates),
            typeof(IReadOnlyList<Ra2AutomationTemplateDescriptor>));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.GetFieldSchema),
            typeof(Ra2AutomationFieldSchemaQueryResult),
            typeof(Ra2AutomationDocumentSnapshot),
            typeof(Ra2AutomationFieldSchemaQuery),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.ResolveReference),
            typeof(Ra2AutomationReferenceResolveResult),
            typeof(Ra2AutomationDocumentSnapshot),
            typeof(Ra2AutomationReferenceResolveQuery),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.GetSection),
            typeof(Ra2AutomationSectionQueryResult),
            typeof(Ra2AutomationDocumentSnapshot),
            typeof(Ra2AutomationSectionQuery),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.FindReferences),
            typeof(Ra2AutomationReferenceQueryResult),
            typeof(Ra2AutomationDocumentSnapshot),
            typeof(Ra2AutomationReferenceQuery),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.Validate),
            typeof(Ra2AutomationDocumentDiagnosticsResult),
            typeof(Ra2AutomationDocumentSnapshot),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.Preview),
            typeof(Ra2AutomationEditPreviewResult),
            typeof(Ra2AutomationDocumentSnapshot),
            typeof(Ra2AutomationEditPlan),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.ExpandTemplate),
            typeof(Ra2AutomationTemplateExpansionResult),
            typeof(Ra2AutomationDocumentSnapshot),
            typeof(Ra2AutomationTemplateExpansionRequest),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.PreviewProject),
            typeof(Ra2AutomationProjectEditPreviewResult),
            typeof(Ra2AutomationProjectSnapshot),
            typeof(Ra2AutomationProjectEditPlan),
            typeof(CancellationToken));
        AssertMethod(
            typeof(IRa2AutomationCapabilityGateway),
            nameof(IRa2AutomationCapabilityGateway.ExpandProjectTemplate),
            typeof(Ra2AutomationProjectTemplateExpansionResult),
            typeof(Ra2AutomationProjectSnapshot),
            typeof(Ra2AutomationTemplateExpansionRequest),
            typeof(CancellationToken));

        Type gateway = typeof(Ra2AutomationCapabilityGateway);
        Assert.True(gateway.IsSealed);
        Assert.Equal(typeof(IRa2AutomationCapabilityGateway), Assert.Single(gateway.GetInterfaces()));
        ConstructorInfo constructor = Assert.Single(gateway.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(constructor.GetParameters());
        Assert.Equal(
            new[] { "ExpandProjectTemplate", "ExpandTemplate", "FindReferences", "GetCapabilities", "GetFieldSchema", "GetSection", "GetTemplates", "Preview", "PreviewProject", "ResolveReference", "Validate" },
            gateway.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
    }

    [Fact]
    public void GetSection_MatchesDirectServiceForSuccessFailureAndCancellation()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot("[E1]\nStrength=100\n");
        Ra2AutomationSectionQuery request = new("E1");
        Ra2AutomationCapabilityGateway gateway = new();
        Ra2AutomationDocumentQueryService direct = new();

        AssertSectionResultEqual(direct.GetSection(snapshot, request), gateway.GetSection(snapshot, request));
        Ra2AutomationSectionQuery missing = new("Missing");
        AssertSectionResultEqual(direct.GetSection(snapshot, missing), gateway.GetSection(snapshot, missing));

        using CancellationTokenSource source = new();
        source.Cancel();
        AssertSectionResultEqual(
            direct.GetSection(snapshot, request, source.Token),
            gateway.GetSection(snapshot, request, source.Token));
    }

    [Fact]
    public void GetFieldSchema_MatchesDirectServiceForSuccessFailureAndCancellation()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot("[E1]\nStrength=100\n");
        Ra2AutomationFieldSchemaQuery request = new(Ra2SectionKind.Techno, "Strength");
        Ra2AutomationCapabilityGateway gateway = new();
        Ra2AutomationDocumentQueryService direct = new();

        AssertFieldSchemaResultEqual(direct.GetFieldSchema(snapshot, request), gateway.GetFieldSchema(snapshot, request));
        Ra2AutomationFieldSchemaQuery missing = new(Ra2SectionKind.Techno, "Missing");
        AssertFieldSchemaResultEqual(direct.GetFieldSchema(snapshot, missing), gateway.GetFieldSchema(snapshot, missing));

        using CancellationTokenSource source = new();
        source.Cancel();
        AssertFieldSchemaResultEqual(
            direct.GetFieldSchema(snapshot, request, source.Token),
            gateway.GetFieldSchema(snapshot, request, source.Token));
    }

    [Fact]
    public void FindReferences_MatchesDirectServiceForSuccessFailureAndCancellation()
    {
        const string text = "[E1]\nPrimary=Weapon\n[Weapon]\nDamage=90\n";
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(text);
        Ra2AutomationReferenceQuery request = new(text.IndexOf("Weapon", StringComparison.Ordinal) + 1);
        Ra2AutomationCapabilityGateway gateway = new();
        Ra2AutomationDocumentQueryService direct = new();

        AssertReferenceResultEqual(direct.FindReferences(snapshot, request), gateway.FindReferences(snapshot, request));
        Ra2AutomationReferenceQuery unresolved = new(0);
        AssertReferenceResultEqual(direct.FindReferences(snapshot, unresolved), gateway.FindReferences(snapshot, unresolved));

        using CancellationTokenSource source = new();
        source.Cancel();
        AssertReferenceResultEqual(
            direct.FindReferences(snapshot, request, source.Token),
            gateway.FindReferences(snapshot, request, source.Token));
    }

    [Fact]
    public void ResolveReference_MatchesDirectServiceForSuccessFailureAndCancellation()
    {
        const string text = "[E1]\nPrimary=Weapon\n[Weapon]\nDamage=90\n";
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(text);
        Ra2AutomationReferenceResolveQuery request = new("E1", "Primary");
        Ra2AutomationCapabilityGateway gateway = new();
        Ra2AutomationDocumentQueryService direct = new();

        AssertResolveResultEqual(direct.ResolveReference(snapshot, request), gateway.ResolveReference(snapshot, request));
        Ra2AutomationReferenceResolveQuery missing = new("Missing", "Primary");
        AssertResolveResultEqual(direct.ResolveReference(snapshot, missing), gateway.ResolveReference(snapshot, missing));

        using CancellationTokenSource source = new();
        source.Cancel();
        AssertResolveResultEqual(
            direct.ResolveReference(snapshot, request, source.Token),
            gateway.ResolveReference(snapshot, request, source.Token));
    }

    [Fact]
    public void Validate_MatchesDirectServiceForSuccessFailureAndCancellation()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot("[E1]\nStrength=100\n");
        Ra2AutomationCapabilityGateway gateway = new();
        Ra2AutomationDocumentQueryService direct = new();

        AssertDiagnosticsResultEqual(direct.Validate(snapshot), gateway.Validate(snapshot));
        Ra2AutomationDocumentSnapshot failure = AutomationTestSupport.Snapshot(
            "[E1]\nStrength=100\n",
            new AutomationTestSupport.ThrowingFieldDefinitionProvider(new InvalidOperationException("private")));
        AssertDiagnosticsResultEqual(direct.Validate(failure), gateway.Validate(failure));

        using CancellationTokenSource source = new();
        source.Cancel();
        AssertDiagnosticsResultEqual(
            direct.Validate(snapshot, source.Token),
            gateway.Validate(snapshot, source.Token));
    }

    [Fact]
    public void Preview_MatchesDirectServiceForSuccessFailureAndCancellation()
    {
        Ra2AutomationDocumentSnapshot snapshot = EditableSnapshot("[E1]\nStrength=100\n");
        Ra2AutomationEditPlan plan = Plan(snapshot);
        Ra2AutomationCapabilityGateway gateway = new();
        Ra2AutomationEditPreviewService direct = new();

        AssertPreviewResultEqual(direct.Preview(snapshot, plan), gateway.Preview(snapshot, plan));
        Ra2AutomationEditPlan stalePlan = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            snapshot.Version,
            snapshot.FieldRegistry.Revision,
            plan.Operations,
            plan.Summary,
            plan.Origin);
        AssertPreviewResultEqual(direct.Preview(snapshot, stalePlan), gateway.Preview(snapshot, stalePlan));

        using CancellationTokenSource source = new();
        source.Cancel();
        AssertPreviewResultEqual(
            direct.Preview(snapshot, plan, source.Token),
            gateway.Preview(snapshot, plan, source.Token));
    }

    [Fact]
    public void Gateway_PreservesExistingDocumentAndPreviewLimits()
    {
        string text = new(';', Ra2AutomationDocumentQueryService.MaximumDocumentCharacters + 1);
        Ra2AutomationDocumentSnapshot snapshot = EditableSnapshot(text);
        Ra2AutomationEditPlan plan = Plan(snapshot);
        Ra2AutomationCapabilityGateway gateway = new();

        Assert.Equal(
            Ra2AutomationSectionQueryFailureKind.DocumentTooLarge,
            gateway.GetSection(snapshot, new Ra2AutomationSectionQuery("E1")).FailureKind);
        Assert.Equal(
            Ra2AutomationFieldSchemaQueryFailureKind.DocumentTooLarge,
            gateway.GetFieldSchema(snapshot, new Ra2AutomationFieldSchemaQuery(Ra2SectionKind.Techno, "Strength")).FailureKind);
        Assert.Equal(
            Ra2AutomationReferenceQueryFailureKind.DocumentTooLarge,
            gateway.FindReferences(snapshot, new Ra2AutomationReferenceQuery(0)).FailureKind);
        Assert.Equal(
            Ra2AutomationReferenceResolveFailureKind.DocumentTooLarge,
            gateway.ResolveReference(snapshot, new Ra2AutomationReferenceResolveQuery("E1", "Primary")).FailureKind);
        Assert.Equal(Ra2AutomationDocumentDiagnosticsFailureKind.DocumentTooLarge, gateway.Validate(snapshot).FailureKind);
        Assert.Equal(Ra2AutomationEditPreviewFailureKind.DocumentTooLarge, gateway.Preview(snapshot, plan).FailureKind);
    }

    [Fact]
    public async Task OneGatewayInstance_IsSafeForConcurrentTypedCalls()
    {
        Ra2AutomationCapabilityGateway gateway = new();
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            "[E1]\nStrength=100\n",
            version: 31);

        Ra2AutomationSectionQueryResult[] results = await Task.WhenAll(
            Enumerable.Range(0, 16)
                .Select(_ => Task.Run(() => gateway.GetSection(snapshot, new Ra2AutomationSectionQuery("E1")))));

        Assert.All(results, result =>
        {
            Assert.True(result.Succeeded);
            Assert.Equal(snapshot.DocumentId, result.DocumentId);
            Assert.Equal(snapshot.Version, result.Version);
            Assert.Equal("E1", result.Section!.Name);
            Assert.Equal("100", Assert.Single(result.Section.Fields).EffectiveValue);
        });
    }

    [Fact]
    public void GatewayPublicSurface_HasNoGenericRoutingOrHostAuthorityMembers()
    {
        Type[] gatewayTypes =
        [
            typeof(IRa2AutomationCapabilityGateway),
            typeof(Ra2AutomationCapabilityGateway),
            typeof(Ra2AutomationCapabilityDescriptor),
            typeof(Ra2AutomationCapabilityIds),
            typeof(Ra2AutomationCapabilityRisk),
            typeof(Ra2AutomationCapabilityStability)
        ];
        string[] forbiddenFragments =
        [
            "Invoke", "Apply", "Save", "Store", "Session", "Transaction", "Job", "Event",
            "Artifact", "File", "Process", "Dynamic", "Reflection", "Serialize"
        ];

        foreach (Type type in gatewayTypes)
        {
            MemberInfo[] members = type.GetMembers(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            Assert.DoesNotContain(
                members,
                member => forbiddenFragments.Any(fragment => member.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));

            foreach (MethodInfo method in members.OfType<MethodInfo>())
            {
                Assert.NotEqual(typeof(object), method.ReturnType);
                Assert.DoesNotContain(method.GetParameters(), parameter => parameter.ParameterType == typeof(object));
            }
        }
    }

    private static void AssertMethod(Type type, string name, Type returnType, params Type[] parameterTypes)
    {
        MethodInfo method = Assert.Single(type.GetMethods(), candidate => candidate.Name == name);
        Assert.Equal(returnType, method.ReturnType);
        Assert.Equal(parameterTypes, method.GetParameters().Select(parameter => parameter.ParameterType));
        if (parameterTypes.LastOrDefault() == typeof(CancellationToken))
            Assert.True(method.GetParameters()[^1].IsOptional);
    }

    private static void AssertSectionResultEqual(
        Ra2AutomationSectionQueryResult expected,
        Ra2AutomationSectionQueryResult actual)
    {
        Assert.Equal(expected.Succeeded, actual.Succeeded);
        Assert.Equal(expected.FailureKind, actual.FailureKind);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.DocumentId, actual.DocumentId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.FilePath, actual.FilePath);
        Assert.Equal(expected.FieldRegistryRevision, actual.FieldRegistryRevision);
        if (expected.Section is null)
        {
            Assert.Null(actual.Section);
            return;
        }

        Assert.NotNull(actual.Section);
        Assert.Equal(expected.Section.Name, actual.Section!.Name);
        Assert.Equal(expected.Section.Kind, actual.Section.Kind);
        Assert.Equal(expected.Section.Occurrence, actual.Section.Occurrence);
        Assert.Equal(expected.Section.HeaderLineNumber, actual.Section.HeaderLineNumber);
        Assert.Equal(expected.Section.HeaderSpan, actual.Section.HeaderSpan);
        Assert.Equal(expected.Section.BodySpan, actual.Section.BodySpan);
        Assert.Equal(expected.Section.FullSpan, actual.Section.FullSpan);
        Assert.Equal(expected.Section.Fields.Count, actual.Section.Fields.Count);
        for (int index = 0; index < expected.Section.Fields.Count; index++)
        {
            Ra2AutomationFieldFact left = expected.Section.Fields[index];
            Ra2AutomationFieldFact right = actual.Section.Fields[index];
            Assert.Equal(left.Key, right.Key);
            Assert.Equal(left.EffectiveValue, right.EffectiveValue);
            Assert.Equal(left.LineNumber, right.LineNumber);
            Assert.Equal(left.LineSpan, right.LineSpan);
            Assert.Equal(left.KeySpan, right.KeySpan);
            Assert.Equal(left.ValueSpan, right.ValueSpan);
        }
    }

    private static void AssertFieldSchemaResultEqual(
        Ra2AutomationFieldSchemaQueryResult expected,
        Ra2AutomationFieldSchemaQueryResult actual)
    {
        Assert.Equal(expected.Succeeded, actual.Succeeded);
        Assert.Equal(expected.FailureKind, actual.FailureKind);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.DocumentId, actual.DocumentId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.FilePath, actual.FilePath);
        Assert.Equal(expected.FieldRegistryRevision, actual.FieldRegistryRevision);
        if (expected.Fact is null)
        {
            Assert.Null(actual.Fact);
            return;
        }

        Assert.NotNull(actual.Fact);
        Assert.Equal(expected.Fact.Key, actual.Fact!.Key);
        Assert.Equal(expected.Fact.SectionKind, actual.Fact.SectionKind);
        Assert.Equal(expected.Fact.AppliesTo, actual.Fact.AppliesTo);
        Assert.Equal(expected.Fact.EditorKind, actual.Fact.EditorKind);
        Assert.Equal(expected.Fact.ValueKind, actual.Fact.ValueKind);
        Assert.Equal(expected.Fact.BooleanStyle, actual.Fact.BooleanStyle);
        Assert.Equal(expected.Fact.AllowedValues, actual.Fact.AllowedValues);
        Assert.Equal(expected.Fact.EnumName, actual.Fact.EnumName);
        Assert.Equal(expected.Fact.Separator, actual.Fact.Separator);
        Assert.Equal(expected.Fact.DisplayName, actual.Fact.DisplayName);
        Assert.Equal(expected.Fact.Description, actual.Fact.Description);
        Assert.Equal(expected.Fact.Aliases, actual.Fact.Aliases);
        Assert.Equal(expected.Fact.SourceKind, actual.Fact.SourceKind);
        Assert.Equal(expected.Fact.TrustLevel, actual.Fact.TrustLevel);
        Assert.Equal(expected.Fact.AuthoringDisposition, actual.Fact.AuthoringDisposition);
    }

    private static void AssertReferenceResultEqual(
        Ra2AutomationReferenceQueryResult expected,
        Ra2AutomationReferenceQueryResult actual)
    {
        Assert.Equal(expected.Succeeded, actual.Succeeded);
        Assert.Equal(expected.FailureKind, actual.FailureKind);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.DocumentId, actual.DocumentId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.FilePath, actual.FilePath);
        Assert.Equal(expected.FieldRegistryRevision, actual.FieldRegistryRevision);
        Assert.Equal(expected.HasReferences, actual.HasReferences);
        if (expected.Target is null)
            Assert.Null(actual.Target);
        else
        {
            Assert.NotNull(actual.Target);
            Assert.Equal(expected.Target.Name, actual.Target!.Name);
            Assert.Equal(expected.Target.Kind, actual.Target.Kind);
        }

        Assert.Equal(expected.References.Count, actual.References.Count);
        for (int index = 0; index < expected.References.Count; index++)
        {
            Ra2AutomationReferenceFact left = expected.References[index];
            Ra2AutomationReferenceFact right = actual.References[index];
            Assert.Equal(left.SourceSectionName, right.SourceSectionName);
            Assert.Equal(left.SourceKey, right.SourceKey);
            Assert.Equal(left.LineNumber, right.LineNumber);
            Assert.Equal(left.LineSpan, right.LineSpan);
            Assert.Equal(left.ValueSpan, right.ValueSpan);
        }
    }

    private static void AssertResolveResultEqual(
        Ra2AutomationReferenceResolveResult expected,
        Ra2AutomationReferenceResolveResult actual)
    {
        Assert.Equal(expected.Succeeded, actual.Succeeded);
        Assert.Equal(expected.FailureKind, actual.FailureKind);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.DocumentId, actual.DocumentId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.FilePath, actual.FilePath);
        Assert.Equal(expected.FieldRegistryRevision, actual.FieldRegistryRevision);
        if (expected.Fact is null)
        {
            Assert.Null(actual.Fact);
            return;
        }

        Assert.NotNull(actual.Fact);
        Assert.Equal(expected.Fact.SourceSectionName, actual.Fact!.SourceSectionName);
        Assert.Equal(expected.Fact.SourceSectionOccurrence, actual.Fact.SourceSectionOccurrence);
        Assert.Equal(expected.Fact.SourceKey, actual.Fact.SourceKey);
        Assert.Equal(expected.Fact.SourceFieldOccurrence, actual.Fact.SourceFieldOccurrence);
        Assert.Equal(expected.Fact.SourceLineNumber, actual.Fact.SourceLineNumber);
        Assert.Equal(expected.Fact.SourceSpan, actual.Fact.SourceSpan);
        Assert.Equal(expected.Fact.RawEffectiveToken, actual.Fact.RawEffectiveToken);
        Assert.Equal(expected.Fact.ReferenceIndex, actual.Fact.ReferenceIndex);
        Assert.Equal(expected.Fact.TargetSectionName, actual.Fact.TargetSectionName);
        Assert.Equal(expected.Fact.TargetSectionKind, actual.Fact.TargetSectionKind);
        Assert.Equal(expected.Fact.Basis, actual.Fact.Basis);
        Assert.Equal(expected.Fact.IsTargetDefined, actual.Fact.IsTargetDefined);
        Assert.Equal(expected.Fact.TargetDefinitionCount, actual.Fact.TargetDefinitionCount);
        Assert.Equal(expected.Fact.IsSchemaDeclaredReference, actual.Fact.IsSchemaDeclaredReference);
    }

    private static void AssertDiagnosticsResultEqual(
        Ra2AutomationDocumentDiagnosticsResult expected,
        Ra2AutomationDocumentDiagnosticsResult actual)
    {
        Assert.Equal(expected.Succeeded, actual.Succeeded);
        Assert.Equal(expected.FailureKind, actual.FailureKind);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.DocumentId, actual.DocumentId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.FilePath, actual.FilePath);
        Assert.Equal(expected.FieldRegistryRevision, actual.FieldRegistryRevision);
        Assert.Equal(expected.Diagnostics.Count, actual.Diagnostics.Count);
        for (int index = 0; index < expected.Diagnostics.Count; index++)
        {
            Ra2AutomationDiagnosticFact left = expected.Diagnostics[index];
            Ra2AutomationDiagnosticFact right = actual.Diagnostics[index];
            Assert.Equal(left.Code, right.Code);
            Assert.Equal(left.SourceKind, right.SourceKind);
            Assert.Equal(left.Severity, right.Severity);
            Assert.Equal(left.Message, right.Message);
            Assert.Equal(left.FilePath, right.FilePath);
            Assert.Equal(left.LineNumber, right.LineNumber);
            Assert.Equal(left.ColumnNumber, right.ColumnNumber);
            Assert.Equal(left.SectionId, right.SectionId);
            Assert.Equal(left.Key, right.Key);
            Assert.Equal(left.AnalysisVersion, right.AnalysisVersion);
        }
    }

    private static void AssertPreviewResultEqual(
        Ra2AutomationEditPreviewResult expected,
        Ra2AutomationEditPreviewResult actual)
    {
        Assert.Equal(expected.Succeeded, actual.Succeeded);
        Assert.Equal(expected.FailureKind, actual.FailureKind);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.DocumentId, actual.DocumentId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.FilePath, actual.FilePath);
        Assert.Equal(expected.FieldRegistryRevision, actual.FieldRegistryRevision);
        Assert.Equal(expected.PlanId, actual.PlanId);
        Assert.Equal(expected.CandidateText, actual.CandidateText);
        Assert.Equal(expected.Changes.Count, actual.Changes.Count);
        Assert.Equal(expected.OperationPreviews.Count, actual.OperationPreviews.Count);
        Assert.Equal(expected.SectionCreationPreviews.Count, actual.SectionCreationPreviews.Count);
        Assert.Equal(expected.AddedDiagnostics.Count, actual.AddedDiagnostics.Count);
        Assert.Equal(expected.RemovedDiagnostics.Count, actual.RemovedDiagnostics.Count);
        Assert.Equal(expected.AddedErrorCount, actual.AddedErrorCount);
        Assert.Equal(expected.AddedWarningCount, actual.AddedWarningCount);
        Assert.Equal(expected.RequiresExplicitConfirmation, actual.RequiresExplicitConfirmation);
        if (expected.Succeeded)
        {
            Assert.NotEqual(Guid.Empty, expected.PreviewId);
            Assert.NotEqual(Guid.Empty, actual.PreviewId);
        }
        else
        {
            Assert.Equal(Guid.Empty, expected.PreviewId);
            Assert.Equal(Guid.Empty, actual.PreviewId);
        }

        for (int index = 0; index < expected.Changes.Count; index++)
        {
            Assert.Equal(expected.Changes[index].Span, actual.Changes[index].Span);
            Assert.Equal(expected.Changes[index].NewText, actual.Changes[index].NewText);
            Assert.Equal(expected.Changes[index].Reason, actual.Changes[index].Reason);
        }
        for (int index = 0; index < expected.OperationPreviews.Count; index++)
        {
            Ra2AutomationEditOperationPreview left = expected.OperationPreviews[index];
            Ra2AutomationEditOperationPreview right = actual.OperationPreviews[index];
            Assert.Equal(left.OperationIndex, right.OperationIndex);
            Assert.Equal(left.Operation.Kind, right.Operation.Kind);
            Assert.Equal(left.Operation.SectionName, right.Operation.SectionName);
            Assert.Equal(left.Operation.Key, right.Operation.Key);
            Assert.Equal(left.Operation.Value, right.Operation.Value);
            Assert.Equal(left.OutcomeKind, right.OutcomeKind);
            Assert.Equal(left.ResolvedSectionKind, right.ResolvedSectionKind);
            Assert.Equal(left.IsKnownField, right.IsKnownField);
            Assert.Equal(left.FieldTrustLevel, right.FieldTrustLevel);
            Assert.Equal(left.AffectedOriginalSpan, right.AffectedOriginalSpan);
            Assert.Equal(left.Summary, right.Summary);
        }
    }

    private static Ra2AutomationDocumentSnapshot EditableSnapshot(string text)
        => new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            1,
            "rulesmd.ini",
            text,
            isEditable: true,
            new Ra2AutomationFieldRegistrySnapshot(
                new AutomationTestSupport.EmptyFieldDefinitionProvider(),
                7));

    private static Ra2AutomationEditPlan Plan(Ra2AutomationDocumentSnapshot snapshot)
        => new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            snapshot.DocumentId,
            snapshot.Version,
            snapshot.FieldRegistry.Revision,
            [new Ra2AutomationEditOperation(
                Ra2AutomationEditOperationKind.UpsertField,
                "E1",
                "Armor",
                "steel")],
            "test",
            "tests");
}
