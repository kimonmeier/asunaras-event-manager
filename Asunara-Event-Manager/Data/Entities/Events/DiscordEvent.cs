using EventManager.Data.Entities.Events.Base;

namespace EventManager.Data.Entities.Events;

public class DiscordEvent : IEntity
{
    public Guid Id { get; set; }
    
    public ulong DiscordId { get; set; }
    
    public DateTime Date { get; set; }
}