using Discord;
using MediatR;

namespace EventManager.Events.SendMessageToAll;

public class SendMessageToAllEvent : IRequest
{
    public required IUser Author { get; init; }
    
    public required string Message { get; init; }
}