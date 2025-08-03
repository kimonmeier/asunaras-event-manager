using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class DiscordEventRepository : GenericRepository<DiscordEvent>
{
    public DiscordEventRepository(DbContext dbContext) : base(dbContext)
    {
    }


    public Task<DiscordEvent?> FindByDiscordId(ulong discordId)
    {
        return Entities.SingleOrDefaultAsync(@event => @event.DiscordId == discordId);
    }

    public Task<DiscordEvent?> FindWithRestrictionsByDiscordId(ulong discordId)
    {
        return Entities.Include(x => x.Restrictions).SingleOrDefaultAsync(@event => @event.DiscordId == discordId);
    }

    public Task<List<DiscordEvent>> GetAllUncompleted()
    {
        return Entities.Where(x => !x.IsCompleted).ToListAsync();
    }
}