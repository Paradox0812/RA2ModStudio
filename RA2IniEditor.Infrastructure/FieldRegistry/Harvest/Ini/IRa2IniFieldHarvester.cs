namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;

internal interface IRa2IniFieldHarvester
{
    Ra2IniFieldHarvestResult HarvestCurrentText(Ra2IniFieldHarvestRequest request);
}
