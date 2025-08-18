using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class EventFeedbackRepository : GenericRepository<EventFeedback>
{
    public EventFeedbackRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<EventFeedback> GetOrCreateByDiscordEventAndUser(Guid discordEventId, ulong userId)
    {
        var eventFeedback = await Entities.SingleOrDefaultAsync(x => x.DiscordEventId == discordEventId && x.UserId == userId);
        
        if (eventFeedback is not null)
        {
            return eventFeedback;
        }
        
        return await AddAsync(new EventFeedback()
        {
            DiscordEventId = discordEventId,
            UserId = userId,
        });
    }

    public async Task<List<EventFeedback>> GetByDiscordEvent(Guid discordEventId)
    {
        return await Entities.Where(x => x.DiscordEventId == discordEventId).ToListAsync();
    }
}
