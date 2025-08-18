using EventManager.Data.Entities.Events.Base;

namespace EventManager.Data.Entities.Events;

public class DiscordEvent : IEntity
{
    public Guid Id { get; set; }
    
    public ulong DiscordId { get; set; }
    
    public string Name { get; set; }
    
    public DateTime Date { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public List<EventRestriction> Restrictions { get; set; } = new List<EventRestriction>();
    
    public ulong? FeedbackMessage { get; set; }
}