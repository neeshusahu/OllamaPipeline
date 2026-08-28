using System.Text.Json.Serialization;

internal sealed record GenerateRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("system")]
    public required string SystemPrompt { get; init; }

    [JsonPropertyName("prompt")]
    public required string UserPrompt { get; init; }

    [JsonPropertyName("format")]
    public string Type { get; init; } = "json";

    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = false;

    [JsonPropertyName("options")]
    public ModelOptions? Options { get; init; }
}

internal sealed record ModelOptions
{
    [JsonPropertyName("temperature")]
    public int Temperature { get; init; } = 0;
}
