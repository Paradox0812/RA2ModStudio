using System.Globalization;
using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation;

internal enum Ra2ContentTemplateParameterKind
{
    Identifier = 0,
    String,
    Integer,
    Float,
    Boolean,
    Reference
}

internal enum Ra2ContentTemplateValueSourceKind
{
    Literal = 0,
    Parameter
}

internal sealed class Ra2ContentTemplateValueSource
{
    private Ra2ContentTemplateValueSource(Ra2ContentTemplateValueSourceKind kind, string value)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (value is null || value.Length > Ra2AutomationEditOperation.MaximumValueLength ||
            value.IndexOfAny(['\r', '\n', '\0']) >= 0)
        {
            throw new ArgumentException("A template value source is invalid or exceeds the supported limit.", nameof(value));
        }

        Kind = kind;
        Value = kind == Ra2ContentTemplateValueSourceKind.Parameter
            ? Ra2ContentTemplateValidation.ValidateName(value, nameof(value))
            : value;
    }

    public Ra2ContentTemplateValueSourceKind Kind { get; }
    public string Value { get; }

    public static Ra2ContentTemplateValueSource Literal(string value) => new(Ra2ContentTemplateValueSourceKind.Literal, value);
    public static Ra2ContentTemplateValueSource Parameter(string name) => new(Ra2ContentTemplateValueSourceKind.Parameter, name);
}

internal sealed class Ra2ContentTemplateParameter
{
    public Ra2ContentTemplateParameter(
        string name,
        Ra2ContentTemplateParameterKind kind,
        bool required,
        string? defaultValue = null)
    {
        Name = Ra2ContentTemplateValidation.ValidateName(name, nameof(name));
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (required && defaultValue is not null)
            throw new ArgumentException("A required template parameter cannot declare a default value.", nameof(defaultValue));
        if (defaultValue is not null && !Ra2ContentTemplateValidation.IsValidParameterValue(kind, defaultValue))
            throw new ArgumentException("The template parameter default is invalid for its declared kind.", nameof(defaultValue));

        Kind = kind;
        Required = required;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public Ra2ContentTemplateParameterKind Kind { get; }
    public bool Required { get; }
    public string? DefaultValue { get; }
}

internal sealed class Ra2ContentTemplateFieldSpec
{
    public Ra2ContentTemplateFieldSpec(string key, Ra2ContentTemplateValueSource valueSource)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("A template field key is required.", nameof(key));

        string normalized = key.Trim();
        if (normalized.Length > Ra2AutomationEditOperation.MaximumKeyLength ||
            normalized.IndexOfAny(['\r', '\n', '\0', '=']) >= 0)
        {
            throw new ArgumentException("The template field key is invalid or exceeds the supported limit.", nameof(key));
        }

        Key = normalized;
        ValueSource = valueSource ?? throw new ArgumentNullException(nameof(valueSource));
    }

    public string Key { get; }
    public Ra2ContentTemplateValueSource ValueSource { get; }
}

internal sealed class Ra2ContentTemplateSectionSpec
{
    public Ra2ContentTemplateSectionSpec(
        Ra2ContentTemplateValueSource sectionNameSource,
        Ra2SectionKind expectedKind,
        IEnumerable<Ra2ContentTemplateFieldSpec> fields,
        Ra2ContentTemplateSectionTargetMode targetMode = Ra2ContentTemplateSectionTargetMode.CreateNew)
    {
        SectionNameSource = sectionNameSource ?? throw new ArgumentNullException(nameof(sectionNameSource));
        if (!Enum.IsDefined(expectedKind) || expectedKind == Ra2SectionKind.Unknown)
            throw new ArgumentOutOfRangeException(nameof(expectedKind));
        if (!Enum.IsDefined(targetMode))
            throw new ArgumentOutOfRangeException(nameof(targetMode));
        ArgumentNullException.ThrowIfNull(fields);

        Ra2ContentTemplateFieldSpec[] fieldArray = fields.ToArray();
        if (fieldArray.Any(field => field is null))
            throw new ArgumentException("Template fields cannot contain null entries.", nameof(fields));
        if (fieldArray.GroupBy(field => field.Key, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new ArgumentException("A template section cannot contain duplicate field keys.", nameof(fields));

        ExpectedKind = expectedKind;
        TargetMode = targetMode;
        Fields = Array.AsReadOnly(fieldArray);
    }

    public Ra2ContentTemplateValueSource SectionNameSource { get; }
    public Ra2SectionKind ExpectedKind { get; }
    public Ra2ContentTemplateSectionTargetMode TargetMode { get; }
    public IReadOnlyList<Ra2ContentTemplateFieldSpec> Fields { get; }
}

internal enum Ra2ContentTemplateSectionTargetMode
{
    CreateNew = 0,
    RequireExisting
}

internal sealed class Ra2ContentTemplateDefinition
{
    public const int MaximumParameterCount = 64;

    public Ra2ContentTemplateDefinition(
        string id,
        int version,
        string displayName,
        IEnumerable<Ra2ContentTemplateParameter> parameters,
        IEnumerable<Ra2ContentTemplateSectionSpec> sections,
        IEnumerable<Ra2ContentTemplateRegistrationSpec>? registrations = null)
    {
        Id = Ra2ContentTemplateValidation.ValidateDisplay(id, 128, nameof(id));
        if (version <= 0)
            throw new ArgumentOutOfRangeException(nameof(version));
        DisplayName = Ra2ContentTemplateValidation.ValidateDisplay(displayName, 256, nameof(displayName));
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(sections);

        Ra2ContentTemplateParameter[] parameterArray = parameters.ToArray();
        Ra2ContentTemplateSectionSpec[] sectionArray = sections.ToArray();
        Ra2ContentTemplateRegistrationSpec[] registrationArray = (registrations ?? []).ToArray();
        if (parameterArray.Length > MaximumParameterCount)
            throw new ArgumentOutOfRangeException(nameof(parameters));
        if (parameterArray.Any(parameter => parameter is null) ||
            sectionArray.Any(section => section is null) ||
            registrationArray.Any(registration => registration is null))
            throw new ArgumentException("Template definitions cannot contain null entries.");
        if (parameterArray.GroupBy(parameter => parameter.Name, StringComparer.Ordinal).Any(group => group.Count() > 1))
            throw new ArgumentException("Template parameter names must be unique using ordinal comparison.", nameof(parameters));
        if (sectionArray.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(sections), "A template must contain at least one section.");

        int totalWork = checked(
            sectionArray.Length +
            sectionArray.Sum(section => section.Fields.Count) +
            registrationArray.Length);
        if (totalWork > Ra2AutomationEditPlan.MaximumOperationCount)
            throw new ArgumentOutOfRangeException(nameof(sections), "The template exceeds the edit-plan work limit.");

        HashSet<string> parameterNames = parameterArray.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        string[] references = sectionArray
            .Select(section => section.SectionNameSource)
            .Concat(sectionArray.SelectMany(section => section.Fields.Select(field => field.ValueSource)))
            .Concat(registrationArray.Select(registration => registration.ObjectIdSource))
            .Where(source => source.Kind == Ra2ContentTemplateValueSourceKind.Parameter)
            .Select(source => source.Value)
            .ToArray();
        if (references.Any(reference => !parameterNames.Contains(reference)))
            throw new ArgumentException("A template value source references an undeclared parameter.", nameof(sections));
        if (parameterArray.Any(parameter =>
                references.Contains(parameter.Name, StringComparer.Ordinal) &&
                !parameter.Required && parameter.DefaultValue is null))
        {
            throw new ArgumentException("Every optional parameter used by a value source must declare a default.", nameof(parameters));
        }

        Version = version;
        Parameters = Array.AsReadOnly(parameterArray);
        Sections = Array.AsReadOnly(sectionArray);
        Registrations = Array.AsReadOnly(registrationArray);
    }

    public string Id { get; }
    public int Version { get; }
    public string DisplayName { get; }
    public IReadOnlyList<Ra2ContentTemplateParameter> Parameters { get; }
    public IReadOnlyList<Ra2ContentTemplateSectionSpec> Sections { get; }
    public IReadOnlyList<Ra2ContentTemplateRegistrationSpec> Registrations { get; }
}

internal enum Ra2ContentTemplateCompilationFailureKind
{
    None = 0,
    MissingArgument,
    UnknownArgument,
    DuplicateArgument,
    InvalidArgumentValue,
    ConflictingSections,
    SectionAlreadyExists,
    RequiredSectionNotFound,
    RequiredSectionKindMismatch,
    FieldSchemaNotFound,
    FieldSchemaUnavailable,
    InvalidFieldValue,
    BlockedFieldTrust,
    DocumentTooLarge,
    OperationLimitExceeded,
    RegistrationTargetNotDeclared,
    RegistrationSectionNotFound,
    RegistrationSectionKindMismatch,
    InvalidRegistrationList,
    DuplicateRegistration,
    RegistrationIndexOverflow,
    Canceled,
    UnexpectedFailure
}

internal sealed record Ra2ContentTemplateCompilationWarning(
    string SectionName,
    string Key,
    Ra2AutomationFieldTrustLevel TrustLevel,
    string Message);

internal sealed class Ra2ContentTemplateCompilationResult
{
    public Ra2ContentTemplateCompilationResult(
        Ra2ContentTemplateCompilationFailureKind failureKind,
        string message,
        Ra2AutomationEditPlan? plan,
        IEnumerable<Ra2ContentTemplateCompilationWarning>? warnings = null)
    {
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A compilation result message is required.", nameof(message));
        if ((failureKind == Ra2ContentTemplateCompilationFailureKind.None) != (plan is not null))
            throw new ArgumentException("The compilation result payload does not match its failure state.", nameof(plan));

        Ra2ContentTemplateCompilationWarning[] warningArray = (warnings ?? []).ToArray();
        if (failureKind != Ra2ContentTemplateCompilationFailureKind.None && warningArray.Length != 0)
            throw new ArgumentException("A failed compilation result cannot contain partial warnings.", nameof(warnings));

        Succeeded = failureKind == Ra2ContentTemplateCompilationFailureKind.None;
        FailureKind = failureKind;
        Message = message;
        Plan = plan;
        Warnings = Array.AsReadOnly(warningArray);
    }

    public bool Succeeded { get; }
    public Ra2ContentTemplateCompilationFailureKind FailureKind { get; }
    public string Message { get; }
    public Ra2AutomationEditPlan? Plan { get; }
    public IReadOnlyList<Ra2ContentTemplateCompilationWarning> Warnings { get; }
}

internal sealed class Ra2ContentTemplateCompiler
{
    private readonly IRa2AutomationDocumentQueryService _queryService;

    public Ra2ContentTemplateCompiler(IRa2AutomationDocumentQueryService? queryService = null)
    {
        _queryService = queryService ?? new Ra2AutomationDocumentQueryService();
    }

    public Ra2ContentTemplateCompilationResult Compile(
        Ra2ContentTemplateDefinition definition,
        IEnumerable<KeyValuePair<string, string>> arguments,
        Ra2AutomationDocumentSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(snapshot);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            KeyValuePair<string, string>[] argumentArray = arguments.ToArray();
            if (argumentArray.Length > Ra2ContentTemplateDefinition.MaximumParameterCount)
                return Failure(Ra2ContentTemplateCompilationFailureKind.UnknownArgument, "The template argument limit was exceeded.");
            if (argumentArray.Any(argument => argument.Key is null || argument.Value is null))
                return Failure(Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue, "Template arguments cannot contain null names or values.");
            if (argumentArray.GroupBy(argument => argument.Key, StringComparer.Ordinal).Any(group => group.Count() > 1))
                return Failure(Ra2ContentTemplateCompilationFailureKind.DuplicateArgument, "A template argument was supplied more than once.");

            Dictionary<string, Ra2ContentTemplateParameter> parameterMap = definition.Parameters.ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);
            if (argumentArray.Any(argument => !parameterMap.ContainsKey(argument.Key)))
                return Failure(Ra2ContentTemplateCompilationFailureKind.UnknownArgument, "An unknown template argument was supplied.");

            Dictionary<string, string> boundValues = new(StringComparer.Ordinal);
            foreach (Ra2ContentTemplateParameter parameter in definition.Parameters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string? value = argumentArray.FirstOrDefault(argument => string.Equals(argument.Key, parameter.Name, StringComparison.Ordinal)).Value;
                bool supplied = argumentArray.Any(argument => string.Equals(argument.Key, parameter.Name, StringComparison.Ordinal));
                if (!supplied)
                    value = parameter.DefaultValue;
                if (value is null)
                {
                    if (parameter.Required)
                        return Failure(Ra2ContentTemplateCompilationFailureKind.MissingArgument, $"Required template argument '{parameter.Name}' was not supplied.");
                    continue;
                }
                if (!Ra2ContentTemplateValidation.IsValidParameterValue(parameter.Kind, value))
                    return Failure(Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue, $"Template argument '{parameter.Name}' is invalid for its declared kind.");

                boundValues.Add(parameter.Name, value);
            }

            List<(Ra2ContentTemplateSectionSpec Spec, string Name)> sections = new(definition.Sections.Count);
            foreach (Ra2ContentTemplateSectionSpec section in definition.Sections)
            {
                string name = Resolve(section.SectionNameSource, boundValues);
                if (!Ra2ContentTemplateValidation.IsValidIdentifier(name))
                    return Failure(Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue, "A resolved template section name is invalid.");
                sections.Add((section, name.Trim()));
            }

            if (sections.GroupBy(section => section.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
                return Failure(Ra2ContentTemplateCompilationFailureKind.ConflictingSections, "Template sections resolve to duplicate names.");

            List<(Ra2ContentTemplateRegistrationSpec Spec, string ObjectId)> registrations =
                new(definition.Registrations.Count);
            foreach (Ra2ContentTemplateRegistrationSpec registration in definition.Registrations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (registration.Policy != Ra2ContentRegistrationPolicy.ExplicitNumberedList)
                {
                    return Failure(
                        Ra2ContentTemplateCompilationFailureKind.InvalidRegistrationList,
                        "The template uses a registration policy that is not supported by the current-document compiler.");
                }

                string objectId = Resolve(registration.ObjectIdSource, boundValues);
                if (!Ra2ContentTemplateValidation.IsValidIdentifier(objectId))
                    return Failure(Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue, "A resolved registration object identifier is invalid.");

                (Ra2ContentTemplateSectionSpec Spec, string Name)[] targets = sections
                    .Where(section =>
                        string.Equals(section.Name, objectId.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        IsCompatibleSectionKind(registration.ExpectedObjectKind, section.Spec.ExpectedKind))
                    .ToArray();
                if (targets.Length != 1)
                {
                    return Failure(
                        Ra2ContentTemplateCompilationFailureKind.RegistrationTargetNotDeclared,
                        $"Registration target '{objectId.Trim()}' is not declared exactly once by the template.");
                }

                registrations.Add((registration, objectId.Trim()));
            }

            List<Ra2AutomationSectionCreateOperation> sectionCreations = new(sections.Count);
            List<Ra2AutomationEditOperation> operations = [];
            List<Ra2ContentTemplateCompilationWarning> warnings = [];

            foreach ((Ra2ContentTemplateSectionSpec spec, string sectionName) in sections)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2AutomationSectionQueryResult sectionQuery = _queryService.GetSection(
                    snapshot,
                    new Ra2AutomationSectionQuery(sectionName),
                    cancellationToken);
                if (sectionQuery.FailureKind == Ra2AutomationSectionQueryFailureKind.AmbiguousSection)
                {
                    return Failure(
                        spec.TargetMode == Ra2ContentTemplateSectionTargetMode.RequireExisting
                            ? Ra2ContentTemplateCompilationFailureKind.RequiredSectionKindMismatch
                            : Ra2ContentTemplateCompilationFailureKind.SectionAlreadyExists,
                        $"Section '{sectionName}' is ambiguous.");
                }
                if (sectionQuery.FailureKind == Ra2AutomationSectionQueryFailureKind.Canceled)
                    return Failure(Ra2ContentTemplateCompilationFailureKind.Canceled, "Template compilation was canceled.");

                Ra2SectionKind effectiveSectionKind = spec.ExpectedKind;
                if (spec.TargetMode == Ra2ContentTemplateSectionTargetMode.CreateNew)
                {
                    if (sectionQuery.Succeeded)
                        return Failure(Ra2ContentTemplateCompilationFailureKind.SectionAlreadyExists, $"Section '{sectionName}' already exists.");
                    if (sectionQuery.FailureKind != Ra2AutomationSectionQueryFailureKind.NotFound)
                    {
                        return Failure(
                            sectionQuery.FailureKind == Ra2AutomationSectionQueryFailureKind.DocumentTooLarge
                                ? Ra2ContentTemplateCompilationFailureKind.DocumentTooLarge
                                : Ra2ContentTemplateCompilationFailureKind.UnexpectedFailure,
                            "The document could not be checked for section conflicts.");
                    }

                    sectionCreations.Add(new Ra2AutomationSectionCreateOperation(sectionName, spec.ExpectedKind));
                }
                else
                {
                    if (!sectionQuery.Succeeded || sectionQuery.Section is null)
                    {
                        return Failure(
                            sectionQuery.FailureKind == Ra2AutomationSectionQueryFailureKind.DocumentTooLarge
                                ? Ra2ContentTemplateCompilationFailureKind.DocumentTooLarge
                                : Ra2ContentTemplateCompilationFailureKind.RequiredSectionNotFound,
                            $"Required section '{sectionName}' was not found.");
                    }
                    if (!IsCompatibleSectionKind(spec.ExpectedKind, sectionQuery.Section.Kind))
                    {
                        return Failure(
                            Ra2ContentTemplateCompilationFailureKind.RequiredSectionKindMismatch,
                            $"Required section '{sectionName}' is not compatible with {spec.ExpectedKind}.");
                    }

                    effectiveSectionKind = sectionQuery.Section.Kind;
                }
                foreach (Ra2ContentTemplateFieldSpec field in spec.Fields)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string value = Resolve(field.ValueSource, boundValues);
                    Ra2AutomationFieldSchemaQueryResult schema = _queryService.GetFieldSchema(
                        snapshot,
                        new Ra2AutomationFieldSchemaQuery(effectiveSectionKind, field.Key),
                        cancellationToken);
                    if (!schema.Succeeded)
                    {
                        Ra2ContentTemplateCompilationFailureKind failure = schema.FailureKind switch
                        {
                            Ra2AutomationFieldSchemaQueryFailureKind.NotFound => Ra2ContentTemplateCompilationFailureKind.FieldSchemaNotFound,
                            Ra2AutomationFieldSchemaQueryFailureKind.Canceled => Ra2ContentTemplateCompilationFailureKind.Canceled,
                            Ra2AutomationFieldSchemaQueryFailureKind.DocumentTooLarge => Ra2ContentTemplateCompilationFailureKind.DocumentTooLarge,
                            _ => Ra2ContentTemplateCompilationFailureKind.FieldSchemaUnavailable
                        };
                        return Failure(failure, $"Field schema '{effectiveSectionKind}.{field.Key}' is unavailable.");
                    }

                    Ra2AutomationFieldSchemaFact fact = schema.Fact!;
                    if (fact.AuthoringDisposition == Ra2AutomationFieldAuthoringDisposition.Blocked)
                        return Failure(Ra2ContentTemplateCompilationFailureKind.BlockedFieldTrust, $"Field '{field.Key}' is blocked for automated authoring.");
                    if (!Ra2ContentTemplateValidation.IsValidSchemaValue(fact, value))
                        return Failure(Ra2ContentTemplateCompilationFailureKind.InvalidFieldValue, $"Value for field '{field.Key}' does not satisfy its effective schema.");
                    if (fact.AuthoringDisposition == Ra2AutomationFieldAuthoringDisposition.Caution)
                    {
                        warnings.Add(new Ra2ContentTemplateCompilationWarning(
                            sectionName,
                            field.Key,
                            fact.TrustLevel,
                            $"Field '{field.Key}' requires authoring caution."));
                    }

                    operations.Add(new Ra2AutomationEditOperation(Ra2AutomationEditOperationKind.UpsertField, sectionName, field.Key, value));
                }
            }

            Dictionary<string, Ra2ContentRegistrationAllocationState> registrationStates =
                new(StringComparer.OrdinalIgnoreCase);
            foreach ((Ra2ContentTemplateRegistrationSpec spec, string objectId) in registrations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Ra2ContentRegistryKindCatalog.TryGetObjectKind(spec.RegistrySectionName, out Ra2SectionKind registryObjectKind) ||
                    !IsCompatibleSectionKind(spec.ExpectedObjectKind, registryObjectKind))
                {
                    return Failure(
                        Ra2ContentTemplateCompilationFailureKind.RegistrationSectionKindMismatch,
                        $"Registration section '{spec.RegistrySectionName}' is not compatible with {spec.ExpectedObjectKind}.");
                }

                if (!registrationStates.TryGetValue(spec.RegistrySectionName, out Ra2ContentRegistrationAllocationState? state))
                {
                    Ra2AutomationSectionQueryResult registryQuery = _queryService.GetSection(
                        snapshot,
                        new Ra2AutomationSectionQuery(spec.RegistrySectionName),
                        cancellationToken);
                    if (!registryQuery.Succeeded || registryQuery.Section is null)
                    {
                        Ra2ContentTemplateCompilationFailureKind failure = registryQuery.FailureKind switch
                        {
                            Ra2AutomationSectionQueryFailureKind.NotFound => Ra2ContentTemplateCompilationFailureKind.RegistrationSectionNotFound,
                            Ra2AutomationSectionQueryFailureKind.Canceled => Ra2ContentTemplateCompilationFailureKind.Canceled,
                            Ra2AutomationSectionQueryFailureKind.DocumentTooLarge => Ra2ContentTemplateCompilationFailureKind.DocumentTooLarge,
                            _ => Ra2ContentTemplateCompilationFailureKind.InvalidRegistrationList
                        };
                        return Failure(failure, $"Registration section '{spec.RegistrySectionName}' is unavailable or ambiguous.");
                    }
                    if (registryQuery.Section.Kind != Ra2SectionKind.Global)
                    {
                        return Failure(
                            Ra2ContentTemplateCompilationFailureKind.RegistrationSectionKindMismatch,
                            $"Registration section '{spec.RegistrySectionName}' is not classified as a registry/global section.");
                    }

                    if (!Ra2ContentRegistrationAllocationState.TryCreate(
                            registryQuery.Section.Fields,
                            out state,
                            out Ra2ContentTemplateCompilationFailureKind failureKind,
                            out string failureMessage))
                    {
                        return Failure(failureKind, failureMessage);
                    }

                    registrationStates.Add(spec.RegistrySectionName, state!);
                }

                if (state!.ContainsObject(objectId))
                    continue;
                if (!state.TryReserve(objectId, out int index))
                {
                    return Failure(
                        Ra2ContentTemplateCompilationFailureKind.RegistrationIndexOverflow,
                        $"Registration section '{spec.RegistrySectionName}' has no representable next index.");
                }

                operations.Add(new Ra2AutomationEditOperation(
                    Ra2AutomationEditOperationKind.UpsertField,
                    spec.RegistrySectionName,
                    index.ToString(CultureInfo.InvariantCulture),
                    objectId));
            }

            cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationEditPlan plan = new(
                Guid.NewGuid(),
                snapshot.DocumentId,
                snapshot.Version,
                snapshot.FieldRegistry.Revision,
                sectionCreations,
                operations,
                $"Expand template {definition.DisplayName}",
                $"ContentTemplate/{definition.Id}@{definition.Version}");
            return new Ra2ContentTemplateCompilationResult(
                Ra2ContentTemplateCompilationFailureKind.None,
                "The template compiled successfully.",
                plan,
                warnings);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure(Ra2ContentTemplateCompilationFailureKind.Canceled, "Template compilation was canceled.");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            return Failure(Ra2ContentTemplateCompilationFailureKind.UnexpectedFailure, "Template compilation failed unexpectedly.");
        }
    }

    private static string Resolve(Ra2ContentTemplateValueSource source, IReadOnlyDictionary<string, string> values)
        => source.Kind == Ra2ContentTemplateValueSourceKind.Literal
            ? source.Value
            : values[source.Value];

    private static bool IsCompatibleSectionKind(Ra2SectionKind expected, Ra2SectionKind actual)
        => expected == actual ||
           (expected == Ra2SectionKind.Techno && actual is
               Ra2SectionKind.Techno or
               Ra2SectionKind.Unit or
               Ra2SectionKind.Infantry or
               Ra2SectionKind.Vehicle or
               Ra2SectionKind.Aircraft or
               Ra2SectionKind.Building);

    private static Ra2ContentTemplateCompilationResult Failure(Ra2ContentTemplateCompilationFailureKind kind, string message)
        => new(kind, message, null);
}

internal static class Ra2ContentTemplateValidation
{
    public static string ValidateName(string value, string parameterName)
    {
        if (!IsValidIdentifier(value))
            throw new ArgumentException("A template name is invalid or exceeds the supported limit.", parameterName);
        return value.Trim();
    }

    public static string ValidateDisplay(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Template display text is required.", parameterName);
        string normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("Template display text is invalid or exceeds the supported limit.", parameterName);
        return normalized;
    }

    public static bool IsValidIdentifier(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Trim().Length <= Ra2AutomationEditOperation.MaximumSectionNameLength &&
           value.Trim().IndexOfAny(['\r', '\n', '\0', '=', '[', ']']) < 0;

    public static bool IsValidParameterValue(Ra2ContentTemplateParameterKind kind, string value)
    {
        if (value is null || value.Length > Ra2AutomationEditOperation.MaximumValueLength || value.IndexOfAny(['\r', '\n', '\0']) >= 0)
            return false;
        return kind switch
        {
            Ra2ContentTemplateParameterKind.Identifier or Ra2ContentTemplateParameterKind.Reference => IsValidIdentifier(value),
            Ra2ContentTemplateParameterKind.Integer => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            Ra2ContentTemplateParameterKind.Float => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) && double.IsFinite(parsed),
            Ra2ContentTemplateParameterKind.Boolean => IsBoolean(value, Ra2FieldBooleanValueStyle.Unknown, []),
            Ra2ContentTemplateParameterKind.String => true,
            _ => false
        };
    }

    public static bool IsValidSchemaValue(Ra2AutomationFieldSchemaFact fact, string value)
    {
        if (value.Length > Ra2AutomationEditOperation.MaximumValueLength || value.IndexOfAny(['\r', '\n', '\0']) >= 0)
            return false;

        return fact.ValueKind switch
        {
            Ra2FieldValueKind.Integer => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            Ra2FieldValueKind.Float => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) && double.IsFinite(parsed),
            Ra2FieldValueKind.Boolean => IsBoolean(value, fact.BooleanStyle, fact.AllowedValues),
            Ra2FieldValueKind.Enum => fact.AllowedValues.Count == 0 || ContainsOrdinalIgnoreCase(fact.AllowedValues, value),
            Ra2FieldValueKind.EnumList => ValidateList(value, fact.Separator, token => fact.AllowedValues.Count == 0 || ContainsOrdinalIgnoreCase(fact.AllowedValues, token)),
            Ra2FieldValueKind.Reference => IsValidIdentifier(value),
            Ra2FieldValueKind.ReferenceList => ValidateList(value, fact.Separator, IsValidIdentifier),
            _ => true
        };
    }

    private static bool IsBoolean(string value, Ra2FieldBooleanValueStyle style, IReadOnlyList<string> allowedValues)
    {
        if (allowedValues.Count > 0)
            return ContainsOrdinalIgnoreCase(allowedValues, value);
        return style switch
        {
            Ra2FieldBooleanValueStyle.YesNo => value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("no", StringComparison.OrdinalIgnoreCase),
            Ra2FieldBooleanValueStyle.TrueFalse => value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("false", StringComparison.OrdinalIgnoreCase),
            _ => value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                 value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("false", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static bool ValidateList(string value, string separator, Func<string, bool> predicate)
    {
        string actualSeparator = string.IsNullOrEmpty(separator) ? "," : separator;
        string[] tokens = value.Split(actualSeparator, StringSplitOptions.None).Select(token => token.Trim()).ToArray();
        return tokens.Length > 0 && tokens.All(token => token.Length > 0 && predicate(token));
    }

    private static bool ContainsOrdinalIgnoreCase(IEnumerable<string> values, string candidate)
        => values.Any(value => string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase));
}
