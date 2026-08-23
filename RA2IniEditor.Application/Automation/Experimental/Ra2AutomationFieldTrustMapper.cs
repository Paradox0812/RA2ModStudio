using RA2IniEditor.Application.FieldTrust;

namespace RA2IniEditor.Application.Automation.Experimental;

internal static class Ra2AutomationFieldTrustMapper
{
    public static Ra2AutomationFieldTrustLevel ToAutomationLevel(Ra2FieldTrustLevel level)
        => level switch
        {
            Ra2FieldTrustLevel.Verified => Ra2AutomationFieldTrustLevel.Verified,
            Ra2FieldTrustLevel.VerifiedGuardrail => Ra2AutomationFieldTrustLevel.VerifiedGuardrail,
            Ra2FieldTrustLevel.Inferred => Ra2AutomationFieldTrustLevel.Inferred,
            Ra2FieldTrustLevel.ManualCurated => Ra2AutomationFieldTrustLevel.ManualCurated,
            Ra2FieldTrustLevel.AutoExtracted => Ra2AutomationFieldTrustLevel.AutoExtracted,
            Ra2FieldTrustLevel.Obsolete => Ra2AutomationFieldTrustLevel.Obsolete,
            Ra2FieldTrustLevel.NonExistent => Ra2AutomationFieldTrustLevel.NonExistent,
            Ra2FieldTrustLevel.PseudoField => Ra2AutomationFieldTrustLevel.PseudoField,
            _ => Ra2AutomationFieldTrustLevel.Unknown
        };

    public static Ra2AutomationFieldAuthoringDisposition ToAuthoringDisposition(
        Ra2AutomationFieldTrustLevel level)
        => level switch
        {
            Ra2AutomationFieldTrustLevel.Verified or
            Ra2AutomationFieldTrustLevel.ManualCurated => Ra2AutomationFieldAuthoringDisposition.Normal,
            Ra2AutomationFieldTrustLevel.VerifiedGuardrail or
            Ra2AutomationFieldTrustLevel.Obsolete or
            Ra2AutomationFieldTrustLevel.NonExistent or
            Ra2AutomationFieldTrustLevel.PseudoField => Ra2AutomationFieldAuthoringDisposition.Blocked,
            _ => Ra2AutomationFieldAuthoringDisposition.Caution
        };
}
