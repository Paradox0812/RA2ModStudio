namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2BooleanValueSet
{
    public Ra2BooleanValueSet(Ra2BooleanValueStyle style, string trueValue, string falseValue)
    {
        Style = style;
        TrueValue = string.IsNullOrWhiteSpace(trueValue)
            ? throw new ArgumentException("True value cannot be empty.", nameof(trueValue))
            : trueValue;
        FalseValue = string.IsNullOrWhiteSpace(falseValue)
            ? throw new ArgumentException("False value cannot be empty.", nameof(falseValue))
            : falseValue;
    }

    public Ra2BooleanValueStyle Style { get; }

    public string TrueValue { get; }

    public string FalseValue { get; }

    public static Ra2BooleanValueSet YesNo { get; } = new(Ra2BooleanValueStyle.YesNo, "yes", "no");

    public static Ra2BooleanValueSet TrueFalse { get; } = new(Ra2BooleanValueStyle.TrueFalse, "true", "false");
}
