public sealed class OllamaModel
{
    public required string ModelName {get;set;}
    public  ModelOperation type{get; set;}=ModelOperation.Generate;
}
public enum ModelOperation
{
    Generate,
    Embed,
}