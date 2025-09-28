using EventManager.Data.Entities.Events.Base;

namespace EventManager.Data.Entities.Activity;

public class ActivityEvent : IEntity
{
    public Guid Id { get; set; }
    
    public ActivityType Type { get; set; }
    
    public ulong DiscordUserId { get; set; }
    
    public DateTime Date { get; set; }
    
    public ulong ChannelId { get; set; }
}