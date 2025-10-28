using MediatR;
using NetCord;

namespace EventManager.Events.SendMessageToEvent;

public class SendMessageToEventEvent : IRequest
{
    public required User Author { get; init; }
    
    public required Guid DiscordEventId { get; init; }
    
    public required string Message { get; init; }
}