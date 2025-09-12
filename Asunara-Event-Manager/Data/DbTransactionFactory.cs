using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventManager.Data;


public class DbTransactionFactory
{
    private readonly DbContext _dbContext;
    private readonly DbTransactionLock _dbTransactionLock;
    private readonly ILogger<Transaction> _logger;

    public DbTransactionFactory(ILogger<Transaction> logger, DbTransactionLock dbTransactionLock, DbContext dbContext)
    {
        _logger = logger;
        _dbTransactionLock = dbTransactionLock;
        _dbContext = dbContext;
    }

    public async Task<Transaction> CreateTransaction()
    {
        var ownerLock = await _dbTransactionLock.LockAsync();
        
        return new Transaction(_logger, _dbContext, ownerLock, _dbTransactionLock);
    }
}
