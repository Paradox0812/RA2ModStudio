using Xunit;

namespace RA2IniEditor.UiAutomationTests;

public sealed class UiAutomationFactAttribute : FactAttribute
{
    public UiAutomationFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RA2INIEDITOR_RUN_UI_AUTOMATION"), "1", StringComparison.Ordinal))
            Skip = "UI automation smoke is disabled. Set RA2INIEDITOR_RUN_UI_AUTOMATION=1 in an interactive desktop session to run it.";
    }
}
