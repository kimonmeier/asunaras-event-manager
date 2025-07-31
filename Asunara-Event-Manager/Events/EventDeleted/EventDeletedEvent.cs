using MediatR;

namespace EventManager.Events.EventDeleted;

public class EventDeletedEvent : IRequest
{
    public required ulong DiscordId { get;set; }
}