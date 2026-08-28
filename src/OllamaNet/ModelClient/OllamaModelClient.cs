using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class OllamaModelClient : IOllamaModelClient
{
    private readonly IOllamaHttpClient _ollamaHttpClient;
    private readonly Dictionary<string, OllamaModel> _modelsByName;

    private readonly ResiliencePipeline _embedResilience;
    private readonly ResiliencePipeline _generateResilience;

    public OllamaModelClient(
        IOllamaHttpClient ollamaHttpClient,
        IOptions<OllamaOptions> options,
        [FromKeyedServices(ModelOperation.Embed)] ResiliencePipeline embedResilience,
        [FromKeyedServices(ModelOperation.Generate)] ResiliencePipeline generateResilience)
    {
     _ollamaHttpClient=ollamaHttpClient;
     _modelsByName=options.Value.Models.ToDictionary(model=> model.ModelName);
     _embedResilience=embedResilience;
     _generateResilience=generateResilience;
    }
    public async Task<EmbedResponse> EmbedAsync(string modelName, string input, CancellationToken cancellationToken=default)
    {
        if(!_modelsByName.TryGetValue(modelName, out _))
        {
            throw new ArgumentNullException("Model has not been configured");
        }
         var request = new EmbedRequest
        {
            Model = modelName,
            Input=input
        };

        return await _embedResilience.ExecuteAsync(
            async () =>
            {
                var content = await _ollamaHttpClient.PostAsync("api/embed", request, cancellationToken);
                return await content.ReadFromJsonAsync<EmbedResponse>(cancellationToken)
                    ?? throw new InvalidOperationException("Ollama returned an empty embed response.");
            },
            cancellationToken);
    }

    public async Task<GenerateResponse> GenerateAsync(string modelName, string systemPrompt, string userPrompt, CancellationToken cancellationToken=default)
    {
        if(!_modelsByName.TryGetValue(modelName, out _))
        {
            throw new ArgumentNullException("Model has not been configured");
        }
        var request = new GenerateRequest
        {
            Model = modelName,
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Options = new ModelOptions()
            {

            }
        };

        return await _generateResilience.ExecuteAsync(
            async () =>
            {
                var content = await _ollamaHttpClient.PostAsync("api/generate", request, cancellationToken);
                return await content.ReadFromJsonAsync<GenerateResponse>(cancellationToken)
                    ?? throw new InvalidOperationException("Ollama returned an empty generate response.");
            },
            cancellationToken);
    }
}
