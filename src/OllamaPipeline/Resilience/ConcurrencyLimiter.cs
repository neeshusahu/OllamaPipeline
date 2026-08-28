public sealed class ConcurrencyLimiter
{
    private readonly SemaphoreSlim _semaphoreSlim;
    public ConcurrencyLimiter(int maxConcurrency)
    {
        _semaphoreSlim=new SemaphoreSlim(maxConcurrency);
    }
    public async Task AcquireAsync(CancellationToken cancellationToken=default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
    }
    public void Release()
    {
        _semaphoreSlim.Release();
    }
}