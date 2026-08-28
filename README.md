# OllamaNet

A .NET 9 client library for locally running [Ollama](https://ollama.com/) with built-in resilience.

## Features

* Generate and Embed support
* Dependency Injection and Options configuration
* Concurrency limiting
* Sliding-window rate limiting
* Circuit breaking
* Independent resilience pipelines for Generate and Embed

## Requirements

* .NET 9
* Ollama running locally
* Required models pulled

```bash
ollama serve
ollama pull phi4-mini
ollama pull nomic-embed-text
```

## Install

Build the package locally:

```bash
dotnet pack src/OllamaNet/OllamaNet.csproj -c Release
```

Install the package:

```bash
dotnet add package OllamaNet --source src/OllamaNet/bin/Release
```

## Usage

```csharp
services.AddOllamaNetConfig(options =>
{
    options.BaseAddress = "http://localhost:11434";

    options.Models.Add(
        new OllamaModel
        {
            ModelName = "phi4-mini"
        });

    options.Models.Add(
        new OllamaModel
        {
            ModelName = "nomic-embed-text",
            type = ModelOperation.Embed
        });
});
```

Inject `IOllamaModelClient` and make calls:

```csharp
var response = await client.GenerateAsync(
    "phi4-mini",
    "You are a helpful assistant.",
    "Say hello in one sentence.");
```

For embeddings:

```csharp
var response = await client.EmbedAsync(
    "nomic-embed-text",
    "Say hello in one sentence.");
```

Models must be configured before they can be used.

## Resilience

Every request passes through:

```text
Circuit Breaker
      ↓
Rate Limiter
      ↓
Concurrency Limiter
      ↓
Ollama
```

`Generate` and `Embed` use independent resilience pipelines.

## Resilience Configuration

```csharp
services.AddOllamaNetConfig(options =>
{
    options.Resilience[ModelOperation.Generate] =
        new ResilienceOptions
        {
            MaxConcurrency = 1,
            PermitLimit = 5,
            Window = TimeSpan.FromSeconds(20),
            MaxWait = TimeSpan.FromSeconds(30),
            FailureThreshold = 3,
            BreakDuration = TimeSpan.FromSeconds(30)
        };
});
```

| Setting            | Description                                  |
| ------------------ | -------------------------------------------- |
| `MaxConcurrency`   | Maximum concurrent Ollama calls              |
| `PermitLimit`      | Maximum requests allowed within the window   |
| `Window`           | Sliding rate-limit window                    |
| `MaxWait`          | Maximum time waiting for a rate-limit permit |
| `FailureThreshold` | Failures before opening the circuit          |
| `BreakDuration`    | Time the circuit remains open                |

> **Note:** The resilience values shown are for demonstration and benchmarking. 

## Project Structure

```text
src/OllamaNet/
├── Options/
├── HttpClient/
├── ModelClient/
├── RateLimiter/
├── Request/
├── Response/
├── ExceptionHandler/
└── Registrations/
```

## Status
 Actively developed and not yet published to NuGet.org.
