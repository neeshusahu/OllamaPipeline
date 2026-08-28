public sealed class ResilienceOptions
{
    public int FailureThreshold { get; set; } = 3;
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);

    public int MaxConcurrency { get; set; } = 1;

    public int PermitLimit { get; set; } = 5;
    public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(20);
    public TimeSpan MaxWait { get; set; } = TimeSpan.FromSeconds(30);
}
