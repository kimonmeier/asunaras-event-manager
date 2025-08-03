using MediatR;

namespace EventManager.Events.AddFSKRestriction;

public class AddFSKRestrictionEvent : IRequest
{
    public required Guid DiscordEventId { get; init; }
    
    public int? MinAge { get; set; }
    
    public int? MaxAge { get; set; }
}