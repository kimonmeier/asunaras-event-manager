using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.MemberAddedEvent;

public class MemberAddedEventEvent : IRequest
{
    public required IUser User { get; init; }
    
    public required SocketGuildEvent Event { get; init; }
    
}