using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class EventRestrictionRepository : GenericRepository<EventRestriction>
{
    public EventRestrictionRepository(DbContext dbContext) : base(dbContext)
    {
    }
}