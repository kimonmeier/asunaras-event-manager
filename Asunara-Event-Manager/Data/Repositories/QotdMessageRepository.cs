using EventManager.Data.Entities.Events.QOTD;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class QotdMessageRepository : GenericRepository<QotdMessage>
{
    public QotdMessageRepository(DbContext dbContext) : base(dbContext)
    {
    }

}