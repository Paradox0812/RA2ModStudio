namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply;

internal interface IFieldRegistryApplyPlanBuilder
{
    FieldRegistryApplyPlan BuildPlan(FieldRegistryApplyPlanRequest request);
}
