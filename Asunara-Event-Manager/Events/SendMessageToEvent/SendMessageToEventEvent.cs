using Discord;
using MediatR;

namespace EventManager.Events.SendMessageToEvent;

public class SendMessageToEventEvent : IRequest
{
    public required IUser Author { get; init; }
    
    public required Guid DiscordEventId { get; init; }
    
    public required string Message { get; init; }
}