public interface IOllamaModelClient
{
    Task<GenerateResponse> GenerateAsync(string modelName, string systemPrompt, string userPrompt,  CancellationToken cancellationToken=default);
    Task <EmbedResponse> EmbedAsync(string modelName, string input, CancellationToken cancellationToken=default);
}