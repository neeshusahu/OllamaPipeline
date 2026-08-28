public sealed class OllamaOptions
{
    public required string  BaseAddress {get;set;}

    public IList<OllamaModel> Models {get;set;}=new List<OllamaModel>();

    public Dictionary<ModelOperation, ResilienceOptions> Resilience { get; set; } =
        new()
        {
            [ModelOperation.Generate] = new ResilienceOptions(),
            [ModelOperation.Embed] = new ResilienceOptions()
        };
}