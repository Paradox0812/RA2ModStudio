namespace RA2IniEditor.IDE.FieldAnnotations;

internal interface IRa2FieldAnnotationStore
{
    Ra2FieldAnnotationLoadResult Load(string path);

    Ra2FieldAnnotationSaveResult Save(string path, Ra2FieldAnnotationPack pack);
}
