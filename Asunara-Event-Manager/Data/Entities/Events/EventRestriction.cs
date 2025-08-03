using EventManager.Data.Entities.Events.Base;
using EventManager.Data.Enum;

namespace EventManager.Data.Entities.Events;

public abstract class EventRestriction : IEntity
{
    public Guid Id { get; set; }
    
    public Guid DiscordEventId { get; set; }
    
    public DiscordEvent DiscordEvent { get; set; }
    
    public abstract RestrictionType Type { get; set; }
}