public sealed class ResiliencePipeline
{
    private readonly CircuitBreaker _circuitBreaker;
    private readonly ConcurrencyLimiter _concurrencyLimiter;
    private readonly SlidingWindowRateLimiter _rateLimiter;

    public ResiliencePipeline(CircuitBreaker circuitBreaker, ConcurrencyLimiter concurrencyLimiter, SlidingWindowRateLimiter rateLimiter)
    {
        _circuitBreaker = circuitBreaker;
        _concurrencyLimiter = concurrencyLimiter;
        _rateLimiter = rateLimiter;
    }

    public async Task<TResponse> ExecuteAsync<TResponse>(Func<Task<TResponse>> action, CancellationToken cancellationToken = default)
    {
        if (_circuitBreaker.IsOpen && !_circuitBreaker.IsOpenStateResumed())
        {
            throw new CircuitBreakerOpenException("Circuit is open; requests are temporarily blocked.");
        }

        await _rateLimiter.WaitAsync(cancellationToken);
        await _concurrencyLimiter.AcquireAsync(cancellationToken);
        try
        {
            var result = await action();
            _circuitBreaker.SetClosedState();
            return result;
        }
        catch (Exception ex)
        {
            if (IsInfrastructureFailure(ex))
            {
                _circuitBreaker.SetOpenState();
            }
            throw;
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    // Only genuine connectivity/server-health problems should trip the circuit breaker.
    // A 4xx response (bad request, unknown model, etc.) is a problem with that specific
    // request, not a signal that Ollama itself is unhealthy, so it shouldn't count as a failure.
    private static bool IsInfrastructureFailure(Exception exception)
    {
        return exception is HttpRequestException { StatusCode: null or >= System.Net.HttpStatusCode.InternalServerError };
    }
}
