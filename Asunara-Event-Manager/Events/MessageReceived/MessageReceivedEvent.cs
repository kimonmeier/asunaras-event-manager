using MediatR;
using NetCord.Gateway;

namespace EventManager.Events.MessageReceived;

public class MessageReceivedEvent : IRequest
{
    public required Message Message { get; init; }
    
}