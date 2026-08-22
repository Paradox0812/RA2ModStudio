namespace RA2IniEditor.Application.Classification;

internal interface IRa2SectionClassifier
{
    Ra2SectionClassificationResult Classify(string text);
}
