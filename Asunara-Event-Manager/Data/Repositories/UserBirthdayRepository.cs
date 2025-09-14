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
        return Entities.SingleOrDefaultAsync(x => x.DiscordId == discordId && !x.IsDeleted);
    }

    public Task<bool> HasByDiscord(ulong discordId)
    {
        return Entities.AnyAsync(x => x.DiscordId == discordId);
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

    public Task<List<UserBirthday>> GetCurrentBirthday(int month, int day)
    {
        return Entities.Where(x => x.Birthday.Month == month && x.Birthday.Day == day).Where(x => !x.IsDeleted).ToListAsync();
    }

    public async Task<List<UserBirthday>> GetHistoryByUserId(ulong discordId)
    {
        return await Entities.Where(x => x.DiscordId == discordId).OrderBy(x => x.CreationDate).ToListAsync();
    }

    public async Task DeleteByDiscordAsync(ulong discordId)
    {
        var userBirthday = await Entities.SingleOrDefaultAsync(x => x.DiscordId == discordId && !x.IsDeleted);

        if (userBirthday is null)
        {
            return;
        }

        await RemoveAsync(userBirthday);
    }
}