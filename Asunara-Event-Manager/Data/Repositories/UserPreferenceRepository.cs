using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class UserPreferenceRepository : GenericRepository<UserPreference>
{
    public UserPreferenceRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public Task<bool> HasByDiscordAsync(ulong discordUserId)
    {
        return Entities.AnyAsync(x => x.DiscordUserId == discordUserId);
    }

    public async Task<UserPreference> GetOrCreateByDiscordAsync(ulong discordId)
    {
        var userPreference = await Entities.SingleOrDefaultAsync(x => x.DiscordUserId == discordId);

        if (userPreference is not null)
        {
        
            return userPreference;
        }
        
        return await AddAsync(new UserPreference()
        {
            DiscordUserId = discordId,
        });
    }
}