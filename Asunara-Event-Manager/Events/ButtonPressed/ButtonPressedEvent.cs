using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.ButtonPressed;

public class ButtonPressedEvent : IRequest
{
    public required SocketMessageComponent Context { get; init; }
}