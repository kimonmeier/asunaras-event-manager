using MediatR;
using NetCord;

namespace EventManager.Events.MemberAddedEvent;

public class MemberAddedEventEvent : IRequest
{
    public required GuildUser User { get; init; }
    
    public required GuildScheduledEvent Event { get; init; }
    
}