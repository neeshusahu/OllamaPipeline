# OllamaNet

A .NET client library for a locally running [Ollama](https://ollama.com/) instance — `Generate` and `Embed` calls, wired into `Microsoft.Extensions.DependencyInjection`, with built-in rate limiting, concurrency limiting, and circuit breaking so a burst of calls can't overwhelm a local Ollama server that only serves one request at a time.

## Requirements

- .NET 9
- A running local Ollama instance (`ollama serve`), with whatever models you configure already pulled (`ollama pull <model>`)

## Install

Not published to nuget.org yet. Build it locally and add it as a local package source:

```bash
dotnet pack src/OllamaNet/OllamaNet.csproj -c Release
dotnet add package OllamaNet --source src/OllamaNet/bin/Release
```

Or reference the project directly with a `ProjectReference` if you're working inside this repo.

## Quick start

Register it against your `IServiceCollection`:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddOllamaNetConfig(options =>
{
    options.BaseAddress = "http://localhost:11434";
    options.Models.Add(new OllamaModel { ModelName = "phi4-mini" });                                      // type defaults to Generate
    options.Models.Add(new OllamaModel { ModelName = "nomic-embed-text", type = ModelOperation.Embed });
});

var provider = services.BuildServiceProvider();
var modelClient = provider.GetRequiredService<IOllamaModelClient>();
```

Then call it:

```csharp
var generateResponse = await modelClient.GenerateAsync(
    modelName: "phi4-mini",
    systemPrompt: "You are a helpful assistant.",
    userPrompt: "Say hello in one sentence.");

Console.WriteLine(generateResponse.OllamaGenerateResponse);

var embedResponse = await modelClient.EmbedAsync(
    modelName: "nomic-embed-text",
    input: "Say hello in one sentence.");

Console.WriteLine(embedResponse.OllamaEmbedResponse?.FirstOrDefault()?.Length); // 768 for nomic-embed-text
```

Every `OllamaModel` must be declared up front via `options.Models` — calling `GenerateAsync`/`EmbedAsync` with a model name that wasn't configured throws `ArgumentNullException` before any HTTP call is made.

## Built-in resilience

Every call goes through a `ResiliencePipeline` before it reaches Ollama:

```
circuit breaker check → rate limiter (wait for a permit) → concurrency limiter (acquire a slot) → the actual call
```

- **Circuit breaker** — opens after `FailureThreshold` consecutive failures and blocks calls for `BreakDuration`, then allows one trial call. Only genuine server-health problems trip it (a 5xx from Ollama, or not being able to reach it at all) — a 4xx like "model not found" is treated as a problem with that specific request, not a sign Ollama itself is unhealthy, so it doesn't count as a failure.
- **Sliding-window rate limiter** — allows at most `PermitLimit` requests per rolling `Window`; a call that can't get a permit within `MaxWait` throws `TimeoutException`.
- **Concurrency limiter** — caps how many calls can be in flight against Ollama at once (`MaxConcurrency`). Ollama itself serves one generation request at a time, so this defaults to `1` — running more than that in parallel doesn't get you real concurrency, it just produces queued requests that are more likely to time out.

`Generate` and `Embed` each get their **own, independent** circuit breaker, rate limiter, and concurrency limiter — heavy Generate traffic won't throttle or trip the breaker for Embed calls, and vice versa.

## Configuration reference

```csharp
services.AddOllamaNetConfig(options =>
{
    options.BaseAddress = "http://localhost:11434";
    options.Models.Add(new OllamaModel { ModelName = "phi4-mini" });

    options.Resilience[ModelOperation.Generate] = new ResilienceOptions
    {
        FailureThreshold = 3,               // consecutive failures before the circuit opens
        BreakDuration = TimeSpan.FromSeconds(30),
        MaxConcurrency = 1,                 // max in-flight calls to Ollama at once
        PermitLimit = 5,                    // max calls allowed per Window
        Window = TimeSpan.FromSeconds(20),  // the rolling window PermitLimit applies to
        MaxWait = TimeSpan.FromSeconds(30), // how long a call will wait for a rate-limit permit before throwing
    };
});
```

`options.Resilience` is a `Dictionary<ModelOperation, ResilienceOptions>` pre-populated with defaults for both `Generate` and `Embed` — overwrite either entry to customize just that operation; anything left unset keeps the defaults shown above.

| `OllamaModel` | |
|---|---|
| `ModelName` | required — must match a model already pulled in Ollama |
| `type` | `ModelOperation.Generate` (default) or `ModelOperation.Embed` — informational; doesn't currently gate which method you can call it with |

## Project layout

```
src/OllamaNet/
├── Options/           OllamaOptions, OllamaModel, ResilienceOptions, ModelOperation
├── HttpClient/         IOllamaHttpClient / OllamaHttpClient — raw HTTP transport, returns HttpContent
├── ModelClient/         IOllamaModelClient / OllamaModelClient — builds requests, deserializes responses
├── RateLimiter/         CircuitBreaker, ConcurrencyLimiter, SlidingWindowRateLimiter, ResiliencePipeline
├── Request/ Response/   Wire-format DTOs for Ollama's /api/generate and /api/embed
├── ExceptionHandler/     IExceptionHandler / ConsoleExceptionHandler — logs exceptions as they pass through
└── Registrations/        ServiceCollectionExtension.AddOllamaNetConfig
```

## Status

Actively developed, not yet published to nuget.org — no license or repository URL is set on the package yet.
