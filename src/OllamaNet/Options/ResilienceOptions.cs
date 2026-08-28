public sealed class ResilienceOptions
{
    public int FailureThreshold { get; set; } = 3;
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);

    public int MaxConcurrency { get; set; } = 1;

    public int PermitLimit { get; set; } = 5;
    public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(20);
    public TimeSpan MaxWait { get; set; } = TimeSpan.FromSeconds(30);
}
//10 requests
/* 
Rate limiter -5 requests at 10:00
will be there till 10:01
10:00:30 Max wait
*/