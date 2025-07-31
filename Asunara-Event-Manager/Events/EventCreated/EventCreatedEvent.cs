using MediatR;

namespace EventManager.Events.EventCreated;

public class EventCreatedEvent : IRequest
{
    public required ulong DiscordId { get; init; }
    
    public required DateTime Datum { get; init; }
}