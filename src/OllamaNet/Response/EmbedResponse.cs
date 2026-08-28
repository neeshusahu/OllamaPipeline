using System.Text.Json.Serialization;

public record class EmbedResponse
{
    [JsonPropertyName("embeddings")]
    public float[][]? OllamaEmbedResponse { get; init; }
}
