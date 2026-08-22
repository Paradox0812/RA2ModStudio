namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;

internal interface IRa2IniProjectFieldHarvester
{
    Ra2IniFieldHarvestResult HarvestProject(Ra2IniProjectFieldHarvestRequest request);
}
