namespace EventManager.Data;

public class DbTransactionLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Guid? _currentSemaphoreOwner = null;
    
    public async Task<Guid> LockAsync()
    {
        await _semaphore.WaitAsync();

        _currentSemaphoreOwner = Guid.NewGuid();
        return _currentSemaphoreOwner.Value;
    }
    
    public void Release(Guid ownerId)
    {
        if (_currentSemaphoreOwner != ownerId)
        {
            throw new InvalidOperationException("Invalid owner, the current owner is: " + _currentSemaphoreOwner + "but the owner requested to release is: " + ownerId + "");
        }
        
        _currentSemaphoreOwner = null;
        _semaphore.Release();
    }
}