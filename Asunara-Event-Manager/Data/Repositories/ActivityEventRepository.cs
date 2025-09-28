using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class ActivityEventRepository : GenericRepository<ActivityEvent>
{
    public ActivityEventRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public Task<ActivityEvent?> GetLastVoiceActivityByDiscordId(ulong discordId)
    {
        return Entities
            .Where(x => x.Type != ActivityType.MessageCreated)
            .Where(x => x.DiscordUserId == discordId)
            .OrderBy(x => x.Date).FirstOrDefaultAsync();
    }
}