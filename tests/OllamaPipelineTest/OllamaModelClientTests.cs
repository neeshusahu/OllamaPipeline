using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Moq;

namespace OllamaPipelineTest;

public class OllamaModelClientTests
{
    private Mock<IOllamaHttpClient> _httpClientMock = null!;
    private OllamaModelClient _modelClient = null!;

    [SetUp]
    public void Setup()
    {
        _httpClientMock = new Mock<IOllamaHttpClient>();

        var options = Options.Create(new OllamaOptions
        {
            BaseAddress = "http://localhost:11434",
            Models = new List<OllamaModel>
            {
                new OllamaModel { ModelName = "phi4-mini" }
            }
        });

        var embedResilience = CreatePermissiveResiliencePipeline();
        var generateResilience = CreatePermissiveResiliencePipeline();

        _modelClient = new OllamaModelClient(_httpClientMock.Object, options, embedResilience, generateResilience);
    }

    private static ResiliencePipeline CreatePermissiveResiliencePipeline()
    {
        return new ResiliencePipeline(
            new CircuitBreaker(failureThreshold: int.MaxValue, breakDuration: TimeSpan.Zero),
            new ConcurrencyLimiter(maxConcurrency: int.MaxValue),
            new SlidingWindowRateLimiter(permitLimit: int.MaxValue, window: TimeSpan.FromMinutes(1), maxWait: TimeSpan.FromSeconds(30)));
    }

    [Test]
    public async Task EmbedAsync_ReturnsResponse_WhenModelIsConfigured()
    {
        var expected = new EmbedResponse { OllamaEmbedResponse = new[] { new[] { 0.1f, 0.2f } } };
        _httpClientMock
            .Setup(c => c.PostAsync("api/embed", It.IsAny<EmbedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => JsonContent.Create(expected));

        var result = await _modelClient.EmbedAsync("phi4-mini", "hello");

        Assert.That(result.OllamaEmbedResponse, Is.EqualTo(expected.OllamaEmbedResponse));
        _httpClientMock.Verify(
            c => c.PostAsync("api/embed", It.IsAny<EmbedRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void EmbedAsync_Throws_WhenModelIsNotConfigured()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _modelClient.EmbedAsync("unknown-model", "hello"));

        _httpClientMock.Verify(
            c => c.PostAsync(It.IsAny<string>(), It.IsAny<EmbedRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GenerateAsync_ReturnsResponse_WhenModelIsConfigured()
    {
        var expected = new GenerateResponse { OllamaGenerateResponse = "hello back" };
        _httpClientMock
            .Setup(c => c.PostAsync("api/generate", It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => JsonContent.Create(expected));

        var result = await _modelClient.GenerateAsync("phi4-mini", "system prompt", "user prompt");

        Assert.That(result.OllamaGenerateResponse, Is.EqualTo(expected.OllamaGenerateResponse));
        _httpClientMock.Verify(
            c => c.PostAsync("api/generate", It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void GenerateAsync_Throws_WhenModelIsNotConfigured()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _modelClient.GenerateAsync("unknown-model", "system prompt", "user prompt"));

        _httpClientMock.Verify(
            c => c.PostAsync(It.IsAny<string>(), It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
