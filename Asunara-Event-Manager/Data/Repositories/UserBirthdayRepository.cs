using EventManager.Data.Entities.Birthday;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class UserBirthdayRepository : GenericRepository<UserBirthday>
{
    public UserBirthdayRepository(DbContext dbContext) : base(dbContext)
    {
    }
    
    public Task<UserBirthday?> GetByDiscordAsync(ulong discordId)
    {
        return Entities.SingleOrDefaultAsync(x => x.DiscordId == discordId);
    }

    public async Task<UserBirthday> GetNewUserBirthday(ulong discordId)
    {
        UserBirthday? userBirthday = await GetByDiscordAsync(discordId);
        if (userBirthday is not null)
        {
            userBirthday.IsDeleted = true;

            await UpdateAsync(userBirthday);
        }
        
        return await AddAsync(new UserBirthday()
        {
            DiscordId = discordId,
        });
    }
}