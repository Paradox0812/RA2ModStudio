namespace RA2IniEditor.IDE.AI;

internal interface IRa2AiContextProvider
{
    Ra2AiContext BuildContext(Ra2AiContextRequest request);
}
