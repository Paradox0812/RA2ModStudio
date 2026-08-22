namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

internal interface IFieldRegistryRollbackService
{
    FieldRegistryRollbackResult Rollback(FieldRegistryRollbackRequest request);
}
