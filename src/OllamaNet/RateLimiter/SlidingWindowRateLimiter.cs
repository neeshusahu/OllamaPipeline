using System.Net.Cache;

public sealed class SlidingWindowRateLimiter
{
    private readonly TimeSpan _window;
    private readonly TimeSpan  _maxWait;
    private readonly int _permitLimit;
    
  private readonly object _lock = new();

    private readonly Queue<DateTimeOffset> _requests = new();

      public SlidingWindowRateLimiter(
        int permitLimit,
        TimeSpan window,
        TimeSpan maxWait)
    {
        _permitLimit = permitLimit;
        _window = window;
        _maxWait = maxWait;
    }

    public  async Task WaitAsync( CancellationToken cancellationToken = default)
    {
        var deadline= DateTimeOffset.UtcNow + _maxWait;
        while(true)
        {
            TimeSpan _waitTime;
            lock(_lock)
            {
                while(_requests.Count>0 && (DateTimeOffset.UtcNow- _requests.Peek())>=_window)
                {
                    _requests.Dequeue();

                }
                if(_requests.Count< _permitLimit)
                {
                    _requests.Enqueue(DateTimeOffset.UtcNow);
                    return;
                }
                _waitTime= _requests.Peek()+_window - DateTimeOffset.UtcNow;
            }
            var remaining=deadline-DateTimeOffset.UtcNow;
            if (remaining <= TimeSpan.Zero) 
            { throw new TimeoutException( "Rate limit wait time exceeded."); }
            _waitTime = _waitTime < remaining
                                ? _waitTime
                           : remaining;
            
          await Task.Delay(_waitTime, cancellationToken);
        }
    }

}