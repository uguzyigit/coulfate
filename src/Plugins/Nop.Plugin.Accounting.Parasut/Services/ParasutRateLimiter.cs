using System.Collections.Concurrent;

namespace Nop.Plugin.Accounting.Parasut.Services;

/// <summary>
/// Rate limiter for Paraşüt API (max 10 requests per 10 seconds)
/// </summary>
public class ParasutRateLimiter
{
    private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private const int MaxRequests = 10;
    private const int TimeWindowSeconds = 10;

    /// <summary>
    /// Wait if necessary to respect rate limit
    /// </summary>
    public async Task WaitIfNeededAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddSeconds(-TimeWindowSeconds);

            // Remove old timestamps
            while (_requestTimestamps.TryPeek(out var timestamp) && timestamp < cutoff)
            {
                _requestTimestamps.TryDequeue(out _);
            }

            // Check if we're at the limit
            if (_requestTimestamps.Count >= MaxRequests)
            {
                // Get the oldest timestamp
                if (_requestTimestamps.TryPeek(out var oldest))
                {
                    var waitTime = oldest.AddSeconds(TimeWindowSeconds) - now;
                    if (waitTime.TotalMilliseconds > 0)
                    {
                        await Task.Delay(waitTime);
                    }
                }
            }

            // Add current request
            _requestTimestamps.Enqueue(now);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
