public enum CircuitState
{
    Open,
    HalfOpen,
    Closed
}

public sealed class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message)
    {
    }
}

public sealed class CircuitBreaker
{
    private  int _failureCount;
    private readonly int _consecutiveFailureLimt;
    private readonly TimeSpan _coolDown;
    private  DateTimeOffset _stateAcquiredAt;
    private CircuitState _state = CircuitState.Closed;
    private readonly object _lock = new();

    public CircuitBreaker(int failureThreshold = 5, TimeSpan? breakDuration = null)
    {
        _consecutiveFailureLimt=failureThreshold;
        _coolDown=breakDuration ?? TimeSpan.FromSeconds(10);
    }

    public bool IsOpen
    {
        get
        {
            lock (_lock)
            {
                return _state == CircuitState.Open;
            }
        }
    }

    public bool IsOpenStateResumed()
    {
       lock(_lock)
        {
            if(_state==CircuitState.Open)
            {
                if(DateTimeOffset.UtcNow-_stateAcquiredAt>=_coolDown)
                {
                    _state=CircuitState.HalfOpen;
                    return true;
                }

            }
            return false;
        }
    }

    public void SetClosedState()
    {
       lock(_lock)
        {
            _failureCount=0;
            _state=CircuitState.Closed;
        }

    }
    public void SetOpenState()
    {
        lock(_lock)
        {
            _failureCount++;
            if(_state==CircuitState.HalfOpen || _failureCount>=_consecutiveFailureLimt)
            {
                _state=CircuitState.Open;
                _stateAcquiredAt=DateTimeOffset.UtcNow;
            }
        }
    }

}