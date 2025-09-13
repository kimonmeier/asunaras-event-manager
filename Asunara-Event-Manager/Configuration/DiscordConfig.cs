using System.ComponentModel;
using System.Text.Json.Serialization;

namespace EventManager.Configuration;

public class DiscordConfig
{
    public string Token { get; set; }
    
    public string Activity { get; set; }
    
    public ulong MainDiscordServerId { get; set; }
    
    public ulong TeamDiscordServerId { get; set; }
    
    public ulong EventChatId { get; set; }
    
    public QotdConfig Qotd { get; set; } = new QotdConfig();
    
    public FskConfig Fsk { get; set; } = new FskConfig();
    
    public Emotes Emote { get; set; } = new Emotes();
    
    public Events Event { get; set; } = new Events();
    
    public BirthdayConfig Birthday { get; set; } = new BirthdayConfig();
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
    public List<FskRange> Range { get; set; } = new List<FskRange>();
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
    
    public ulong RoleId { get; set; }
}