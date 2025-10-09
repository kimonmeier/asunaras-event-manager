using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventManager.Configuration;

public class DiscordConfig
{
    public string Token { get; set; }
    
    public string ActivityPresence { get; set; }
    
    public ulong MainDiscordServerId { get; set; }
    
    public ulong TeamDiscordServerId { get; set; }
    
    public ulong EventChatId { get; set; }
    
    public ulong HauptchatChannelId { get; set; }
    
    public QotdConfig Qotd { get; set; } = new QotdConfig();
    
    public FskConfig Fsk { get; set; } = new FskConfig();
    
    public Emotes Emote { get; set; } = new Emotes();
    
    public Events Event { get; set; } = new Events();
    
    public BirthdayConfig Birthday { get; set; } = new BirthdayConfig();
    
    public ComfortConfig Comfort { get; set; } = new ComfortConfig();
    
    public ActivityConfig Activity { get; set; } = new ActivityConfig();
    
    public HalloweenConfig Halloween { get; set; } = new HalloweenConfig();
}

public class Emotes
{
    public string Yes { get; set; }
    
    public string No { get; set; }
}

public class QotdConfig
{
    public ulong ChannelId { get; set; }
    
    [JsonConverter(typeof(TimeOnlyConverter))]
    public TimeOnly Time { get; set; }
    
    public bool AllowReposts { get; set; }
    
    public string Text { get; set; }
    
    public string ThreadTitle { get; set; }
}

public class Events
{
    public ulong EventParticipantRoleId { get; set; }
    
    public ulong FeedbackChannelId { get; set; }
}

public class FskConfig
{
    public FskRange[] Range { get; set; } = [];
}

public class FskRange
{
    public ulong RoleId { get; set; }
    
    public int? MinAge { get; set; }
    
    public int? MaxAge { get; set; }
}

public class BirthdayConfig
{
    public ulong ChannelId { get; set; }
    
    public ulong BirthdayTeamNotificationChannelId { get; set; }
    
    public ulong BirthdayChildRoleId { get; set; }
    
    public ulong BirthdayNotificationRoleId { get; set; }
    
    public ulong AnnouncementMessageId { get; set; }
    
    public string[] Messages { get; set; } = [];
}

public class ComfortConfig
{
    public ulong ChannelId { get; set; }
    
    public ulong ComfortRoleId { get; set; }
}

public class ActivityConfig
{
    public ulong[] ExcludedChannelsId { get; set; } = [];

    public string[] AllowedActivities { get; set; } = [];
}

public class HalloweenConfig
{
    public int MinTimeBetweenScaresPerChannel { get; set; }
    
    public int MinTimeBetweenScaresPerUser { get; set; }
    
    public int MinWaitTimeForScare { get; set; }
    
    public int MaxWaitTimeForScare { get; set; }
    
    public string[] AudioFiles { get; set; } = [];
}