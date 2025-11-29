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

        var task = _semaphore.WaitAsync(5000);

        var count = 0;
        do
        {
            switch (count)
            {
                case 1:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 100ms");

                    continue;
                case 3:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 300ms");

                    continue;

                case 10:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 1s");

                    continue;


                case 50:
                    _logger.LogWarning($"Could not acquire lock for {Thread.CurrentThread.ManagedThreadId}, waiting for 5s");

                    continue;
            }

            count++;
            await Task.Delay(100);
        } while (!task.IsCompleted);

        if (task.Exception != null)
        {
            throw task.Exception;
        }
        
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