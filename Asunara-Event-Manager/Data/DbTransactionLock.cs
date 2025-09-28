using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace EventManager.Data;

public class DbTransactionLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<DbTransactionLock> _logger;
    private Guid? _currentSemaphoreOwner = null;

    public DbTransactionLock(ILogger<DbTransactionLock> logger)
    {
        _logger = logger;
    }

    public async Task<Guid> LockAsync()
    {
        _logger.LogDebug($"Trying to acquire lock from {Thread.CurrentThread.ManagedThreadId}");

        var task = _semaphore.WaitAsync(1000);

        var count = 0;
        do
        {
            switch (count)
            {
                case 10:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 100ms");

                    break;
                case 25:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 250ms");

                    break;

                case 100:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 1s");

                    break;


                case 500:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 5s");

                    break;
            }

            count++;
            await Task.Delay(100);
        } while (!task.IsCompleted);

        _currentSemaphoreOwner = Guid.NewGuid();

        _logger.LogDebug($"Acquired lock from {Thread.CurrentThread.ManagedThreadId} with owner: {_currentSemaphoreOwner}");

        return _currentSemaphoreOwner.Value;
    }

    public void Release(Guid ownerId)
    {
        if (_currentSemaphoreOwner != ownerId)
        {
            throw new InvalidOperationException("Invalid owner, the current owner is: " + _currentSemaphoreOwner + "but the owner requested to release is: " + ownerId + "");
        }

        _logger.LogDebug($"Releasing lock from {Thread.CurrentThread.ManagedThreadId} with owner: {_currentSemaphoreOwner}");

        _currentSemaphoreOwner = null;
        _semaphore.Release();
    }
}