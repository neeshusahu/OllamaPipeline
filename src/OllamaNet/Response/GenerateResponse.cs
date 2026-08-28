using System.Text.Json.Serialization;

public record class GenerateResponse
{
    [JsonPropertyName("response")]
    public string? OllamaGenerateResponse { get; init; }
}
