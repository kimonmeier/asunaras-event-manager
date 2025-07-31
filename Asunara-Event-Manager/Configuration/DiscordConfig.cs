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