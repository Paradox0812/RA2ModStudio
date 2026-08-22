namespace RA2IniEditor.IDE.AI;

internal interface IRa2AiPromptBuilder
{
    Ra2AiRequest Build(Ra2AiPromptBuildRequest request);
}
