using MediatR;
using NetCord;

namespace EventManager.Events.SendMessageToAll;

public class SendMessageToAllEvent : IRequest
{
    public required User Author { get; init; }
    
    public required string Message { get; init; }
}