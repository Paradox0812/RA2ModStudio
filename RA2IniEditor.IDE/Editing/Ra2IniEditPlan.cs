namespace RA2IniEditor.IDE.Editing;

/// <summary>
/// 表示绑定文档、编辑修订和字段库修订的不可变编辑计划。
/// </summary>
internal sealed class Ra2IniEditPlan
{
    public const int MaximumOperationCount = 128;
    public const int MaximumSummaryLength = 512;
    public const int MaximumOriginLength = 128;

    public Ra2IniEditPlan(
        Guid planId,
        Guid expectedDocumentId,
        int expectedEditRevision,
        long expectedFieldRegistryRevision,
        IEnumerable<Ra2IniEditOperation> operations,
        string summary,
        string origin)
    {
        if (planId == Guid.Empty)
            throw new ArgumentException("Plan identity cannot be empty.", nameof(planId));
        if (expectedDocumentId == Guid.Empty)
            throw new ArgumentException("Expected document identity cannot be empty.", nameof(expectedDocumentId));
        if (expectedEditRevision < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedEditRevision));
        if (expectedFieldRegistryRevision <= 0)
            throw new ArgumentOutOfRangeException(nameof(expectedFieldRegistryRevision));

        ArgumentNullException.ThrowIfNull(operations);
        Ra2IniEditOperation[] operationArray = operations.ToArray();
        if (operationArray.Length is < 1 or > MaximumOperationCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operations),
                $"A plan must contain between 1 and {MaximumOperationCount} operations.");
        }

        if (operationArray.Any(operation => operation is null))
            throw new ArgumentException("Plan operations cannot contain null entries.", nameof(operations));

        PlanId = planId;
        ExpectedDocumentId = expectedDocumentId;
        ExpectedEditRevision = expectedEditRevision;
        ExpectedFieldRegistryRevision = expectedFieldRegistryRevision;
        Operations = Array.AsReadOnly(operationArray);
        Summary = ValidateDisplayText(summary, MaximumSummaryLength, nameof(summary));
        Origin = ValidateDisplayText(origin, MaximumOriginLength, nameof(origin));
    }

    public Guid PlanId { get; }

    public Guid ExpectedDocumentId { get; }

    public int ExpectedEditRevision { get; }

    public long ExpectedFieldRegistryRevision { get; }

    public IReadOnlyList<Ra2IniEditOperation> Operations { get; }

    public string Summary { get; }

    public string Origin { get; }

    private static string ValidateDisplayText(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Plan display text cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength ||
            normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
        {
            throw new ArgumentException("Plan display text is too long or contains control characters.", parameterName);
        }

        return normalized;
    }
}
