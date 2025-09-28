using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.MessageReceived;

public class MessageReceivedEvent : IRequest
{
    public required SocketMessage Message { get; init; }
    
}