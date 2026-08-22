namespace RA2IniEditor.IDE.Language;

internal interface IRa2CompletionProvider
{
    Ra2CompletionResult GetCompletions(Ra2CompletionRequest request);
}
