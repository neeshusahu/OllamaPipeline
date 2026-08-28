using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


public static class ServiceCollectionExtension
{
    public static IServiceCollection AddOllamaNetConfig(this IServiceCollection services, Action<OllamaOptions> options )
    {
      services.Configure<OllamaOptions>(options);
      services.AddHttpClient<IOllamaHttpClient, OllamaHttpClient>(
    (serviceProvider, client) =>
    {
        var options =
            serviceProvider.GetRequiredService<IOptions<OllamaOptions>>();

        client.BaseAddress = new Uri(options.Value.BaseAddress);
    });
      services.AddResiliencePipeline(ModelOperation.Embed);
      services.AddResiliencePipeline(ModelOperation.Generate);
      services.AddTransient<IOllamaModelClient, OllamaModelClient>();
      services.AddTransient<IExceptionHandler, ConsoleExceptionHandler>();
      return services;
    }

   
      private static void AddResiliencePipeline(
        this IServiceCollection services,
      ModelOperation type)
    {
        services.AddKeyedSingleton<ResiliencePipeline>(
            type,
            (serviceProvider, _) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<
                            IOptions<OllamaOptions>>();

                var resilience =
                    options.Value.Resilience.TryGetValue(
                        type,
                        out var configured)
                        ? configured
                        : new ResilienceOptions();

                var circuitBreaker =
                    new CircuitBreaker(
                        failureThreshold:
                            resilience.FailureThreshold,
                        breakDuration:
                            resilience.BreakDuration);

                var concurrencyLimiter =
                    new ConcurrencyLimiter(
                        maxConcurrency:
                            resilience.MaxConcurrency);

                var rateLimiter =
                    new SlidingWindowRateLimiter(
                        permitLimit:
                            resilience.PermitLimit,
                        window:
                            resilience.Window,
                        maxWait:
                            resilience.MaxWait);

                return new ResiliencePipeline(
                    circuitBreaker,
                    concurrencyLimiter,
                    rateLimiter);
            });
    }
    
}