using MediatR;

namespace EventManager.Events.EventCompleted;

public class EventCompletedEvent : IRequest
{
    public required ulong DiscordEventId { get; init; }
}