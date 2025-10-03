using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories.Base;
using EventManager.Data.Repositories.Models;
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
            .AsNoTracking()
            .Where(x => x.Type != ActivityType.MessageCreated)
            .Where(x => x.DiscordUserId == discordId)
            .OrderByDescending(x => x.Date).FirstOrDefaultAsync();
    }

    public Task<int> GetMessageCountByDiscordId(ulong discordId, DateTime since)
    {
        return Entities
            .AsNoTracking()
            .Where(x => x.Type == ActivityType.MessageCreated)
            .Where(x => x.DiscordUserId == discordId)
            .Where(x => x.Date >= since)
            .CountAsync();
    }

    public async Task<int> GetVoiceCountByDiscordId(ulong discordId, DateTime since, bool ignoreAfk)
    {
        await Task.CompletedTask;

        var events = Entities
            .AsNoTracking()
            .Where(x => x.Type != ActivityType.MessageCreated)
            .Where(x => x.Date >= since)
            .Where(x => x.DiscordUserId == discordId)
            .OrderByDescending(x => x.Date)
            .ToList();
        
        NormalizeVoiceActivityEvents(events, discordId, since);

        List<ActivityTopResult> result = new List<ActivityTopResult>();
        List<ActivityEvent> activityEvents = new List<ActivityEvent>();
        while (events.Count > 0)
        {
            var currentActivity = events.Last();

            activityEvents.Add(currentActivity);

            ActivityTopResult? voiceActivity = ProcessVoiceActivity(currentActivity, activityEvents, ignoreAfk);
            if (voiceActivity is not null)
            {
                result.Add(voiceActivity);
            }

            events.Remove(currentActivity);
        }

        return result.GroupBy(x => x.DiscordUserId).Select(x => x.Sum(y => y.Count)).SingleOrDefault();
    }

    public Task<List<ActivityTopResult>> GetTopMessagesSince(DateTime since)
    {
        return Entities
            .AsNoTracking()
            .Where(x => x.Date >= since)
            .Where(x => x.Type == ActivityType.MessageCreated)
            .GroupBy(x => x.DiscordUserId)
            .Select(x => new ActivityTopResult()
            {
                DiscordUserId = x.Key, Count = x.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    public async Task<List<ActivityTopResult>> GetTopVoiceSince(DateTime since, bool ignoreAfk)
    {
        await Task.CompletedTask;

        var groups = Entities
            .AsNoTracking()
            .Where(x => x.Type != ActivityType.MessageCreated)
            .Where(x => x.Date >= since)
            .OrderByDescending(x => x.Date)
            .GroupBy(x => x.DiscordUserId);

        var result = new List<ActivityTopResult>();
        foreach (var group in groups)
        {
            List<ActivityEvent> activityEvents = new List<ActivityEvent>();
            var events = group.ToList();
            
            NormalizeVoiceActivityEvents(events, group.Key, since);

            while (events.Count > 0)
            {
                var currentActivity = events.First();

                activityEvents.Add(currentActivity);

                ActivityTopResult? voiceActivity = ProcessVoiceActivity(currentActivity, activityEvents, ignoreAfk);
                if (voiceActivity is not null)
                {
                    result.Add(voiceActivity);
                }

                events.Remove(currentActivity);
            }
        }

        return result
            .GroupBy(x => x.DiscordUserId).Select(x => new ActivityTopResult()
            {
                DiscordUserId = x.Key, Count = x.Sum(y => y.Count)
            })
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    public async Task<List<ulong>> GetUserIdsCurrentlyConnectedToVoiceChannels()
    {
        await Task.CompletedTask;
        
        var list = Entities
            .AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Where(x => x.Type != ActivityType.MessageCreated)
            .GroupBy(x => x.DiscordUserId);

        List<ulong> userIds = new List<ulong>();
        foreach (var group in list)
        {
            var activityEvents = group.OrderByDescending(x => x.Date);

            if (activityEvents.First().Type != ActivityType.VoiceChannelLeft)
            {
                userIds.Add(group.Key);
            }
        }

        return userIds;  
    }

    private ActivityTopResult? ProcessVoiceActivity(ActivityEvent currentActivity, List<ActivityEvent> activityEvents, bool ignoreAfk)
    {
        if (activityEvents.Count <= 0 || currentActivity.Type != ActivityType.VoiceChannelLeft)
        {
            return null;
        }

        if (activityEvents.All(x => x.Type != ActivityType.VoiceChannelJoined))
        {
            return null;
        }

        var topResult = new ActivityTopResult()
        {
            DiscordUserId = currentActivity.DiscordUserId,
            Count = (int)currentActivity.Date.Subtract(activityEvents.Last(x => x.Type == ActivityType.VoiceChannelJoined).Date).TotalMilliseconds
        };

        if (ignoreAfk)
        {
            while (activityEvents.Any(x => x.Type == ActivityType.VoiceChannelAfk))
            {
                ActivityEvent firstAfkEvent = activityEvents.Last(x => x.Type == ActivityType.VoiceChannelAfk);
                ActivityEvent? lastAfkEvent = activityEvents.LastOrDefault(x => x.Type == ActivityType.VoiceChannelNonAfk);

                activityEvents.Remove(firstAfkEvent);

                if (lastAfkEvent is null)
                {
                    topResult.Count -= (int)currentActivity.Date.Subtract(firstAfkEvent.Date).TotalMilliseconds;

                    break;
                }

                activityEvents.Remove(lastAfkEvent);

                topResult.Count -= (int)lastAfkEvent.Date.Subtract(firstAfkEvent.Date).TotalMilliseconds;
            }
        }

        activityEvents.Clear();

        return topResult;
    }
    
    private static void NormalizeVoiceActivityEvents(List<ActivityEvent> events, ulong discordUserId, DateTime since)
    {
        ActivityEvent? beforeEvent = events
            .Where(x => x.DiscordUserId == discordUserId)
            .OrderByDescending(x => x.Date)
            .FirstOrDefault(x => x.Date <= since);
        
        ActivityEvent firstEvent = events.First();
        ActivityEvent lastEvent = events.Last();

        if (beforeEvent is not null)
        {
            if (beforeEvent.Type == ActivityType.VoiceChannelAfk)
            {
                switch (lastEvent.Type)
                {
                    case ActivityType.VoiceChannelNonAfk:
                        lastEvent.Type = ActivityType.VoiceChannelJoined;
                        break;
                    case ActivityType.VoiceChannelLeft:
                        events.Remove(lastEvent);
                        break;
                }
            }
            else if (beforeEvent.Type != ActivityType.VoiceChannelLeft)
            {
                events.Add(new ActivityEvent
                {
                    DiscordUserId = discordUserId,
                    Date = since,
                    Type = ActivityType.VoiceChannelJoined
                });
            }
        }

        // To account for current voice time!
        if (firstEvent.Type != ActivityType.VoiceChannelLeft)
        {
            events.Insert(0, new ActivityEvent
            {
                DiscordUserId = discordUserId,
                Type = ActivityType.VoiceChannelLeft,
                Date = DateTime.UtcNow
            });
        }
    }
}