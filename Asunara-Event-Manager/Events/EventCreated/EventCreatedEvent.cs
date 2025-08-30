using MediatR;

namespace EventManager.Events.EventCreated;

public class EventCreatedEvent : IRequest
{
    public required ulong DiscordId { get; init; }
    
    public required DateTime UtcDatum { get; init; }
    
    public required string EventName { get; init; }
}