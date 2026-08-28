using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddOllamaNetConfig(options =>
{
    options.BaseAddress = "http://localhost:11434";
    options.Models.Add(new OllamaModel { ModelName = "phi4-mini" });
    options.Models.Add(new OllamaModel { ModelName = "nomic-embed-text", type= ModelOperation.Embed });
});

var provider = services.BuildServiceProvider();
var modelClient = provider.GetRequiredService<IOllamaModelClient>();

var generateResponse = await modelClient.GenerateAsync(
    modelName: "phi4-mini",
    systemPrompt: "You are a helpful assistant.",
    userPrompt: "Say hello in one sentence.");

Console.WriteLine($"Generate response: {generateResponse.OllamaGenerateResponse}");

var embedResponse = await modelClient.EmbedAsync(
    modelName: "nomic-embed-text",
    input: "Say hello in one sentence.");

Console.WriteLine($"Embed response dimensions: {embedResponse.OllamaEmbedResponse?.FirstOrDefault()?.Length ?? 0}");

Console.WriteLine();
Console.WriteLine("Firing 10 parallel GenerateAsync calls to exercise the rate limiter and concurrency limiter...");

var batchStart = DateTime.UtcNow;

var parallelCalls = Enumerable.Range(1, 10).Select(async i =>
{
    var callStart = DateTime.UtcNow;
    try
    {
        var response = await modelClient.GenerateAsync(
            modelName: "phi4-mini",
            systemPrompt: "You are a helpful assistant.",
            userPrompt: $"Call #{i}: say hello in one sentence.");

        var elapsed = DateTime.UtcNow - callStart;
        return $"Call #{i,2}: succeeded in {elapsed.TotalMilliseconds,7:F0}ms — {response.OllamaGenerateResponse}";
    }
    catch (Exception ex)
    {
        var elapsed = DateTime.UtcNow - callStart;
        return $"Call #{i,2}: failed    in {elapsed.TotalMilliseconds,7:F0}ms — {ex.GetType().Name}: {ex.Message}";
    }
});

var results = await Task.WhenAll(parallelCalls);

var batchElapsed = DateTime.UtcNow - batchStart;
Console.WriteLine($"All 10 calls finished in {batchElapsed.TotalMilliseconds:F0}ms total.");
foreach (var result in results.OrderBy(r => r))
{
    Console.WriteLine(result);
}
