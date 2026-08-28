public interface IOllamaHttpClient
{
    Task<HttpContent> PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken ct);
}
