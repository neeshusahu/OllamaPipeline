using System.Net;
using System.Net.Http.Json;



public sealed class OllamaHttpClient : IOllamaHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IExceptionHandler _exceptionHandler;

    public OllamaHttpClient(HttpClient httpClient, IExceptionHandler exceptionHandler)
    {
        _httpClient=httpClient;
        _exceptionHandler=exceptionHandler;
    }
    public async Task<HttpContent> PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken ct)
    {
        try
        {
         var response= await _httpClient.PostAsJsonAsync(endpoint,request, ct);
        if(response.IsSuccessStatusCode)
        {
            return response.Content;
        }

       

        throw new HttpRequestException($"Ollama request failed: {(int)response.StatusCode} {response.ReasonPhrase}", null, response.StatusCode);
        }
        catch(Exception ex)
        {
            _exceptionHandler.Handle(ex);
            throw;
        }
    }
}
