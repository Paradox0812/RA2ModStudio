namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

internal interface IFieldRegistryApplyWriter
{
    FieldRegistryApplyWriteResult Write(FieldRegistryApplyWriteRequest request);
}
