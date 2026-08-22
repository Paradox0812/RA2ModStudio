namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal interface IFieldRegistryHarvestParser
{
    FieldRegistryHarvestParseResult Parse(FieldRegistryHarvestDocument document);
}

