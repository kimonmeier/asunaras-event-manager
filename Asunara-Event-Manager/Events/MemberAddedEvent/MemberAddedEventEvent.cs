using MediatR;
using NetCord;

namespace EventManager.Events.MemberAddedEvent;

public class MemberAddedEventEvent : IRequest
{
    public required ulong GuildId { get; set; }
    
    public required ulong UserId { get; init; }
    
    public required ulong EventId { get; init; }
    
}