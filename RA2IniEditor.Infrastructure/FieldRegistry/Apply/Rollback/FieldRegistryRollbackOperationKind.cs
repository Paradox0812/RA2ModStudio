namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

internal enum FieldRegistryRollbackOperationKind
{
    RestoreBackup = 0,
    DeleteCreatedTarget = 1,
    NoOp = 2
}
