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
}