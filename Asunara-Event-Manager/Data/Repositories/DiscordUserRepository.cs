using EventManager.Data.Entities.Users;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class DiscordUserRepository : GenericRepository<DiscordUser>
{
    public DiscordUserRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public Task<DiscordUser?> GetByDiscordId(ulong discordId)
    {
        return Entities.SingleOrDefaultAsync(x => x.DiscordUserId == discordId);
    }
}