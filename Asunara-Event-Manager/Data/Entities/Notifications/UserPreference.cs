using EventManager.Data.Entities.Events.Base;

namespace EventManager.Data.Entities.Notifications;

public class UserPreference : IEntity
{
    public Guid Id { get; set; }
    
    public ulong DiscordUserId { get; set; }
    
    public bool AllowReminderInPrivateMessage { get; set; }
    
    public bool AllowReminderForEvent { get; set; }
}