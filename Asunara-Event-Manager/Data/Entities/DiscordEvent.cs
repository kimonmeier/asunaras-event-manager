using EventManager.Data.Entities.Base;

namespace EventManager.Data.Entities;

public class DiscordEvent : IEntity
{
    public Guid Id { get; set; }
    
    public ulong DiscordId { get; set; }
    
    public DateTime Date { get; set; }
}