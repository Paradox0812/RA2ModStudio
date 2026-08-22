namespace RA2IniEditor.IDE.Classification;

internal interface IRa2SectionClassifier
{
    Ra2SectionClassificationResult Classify(string text);
}
