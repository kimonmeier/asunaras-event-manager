using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.MemberLeftChannel;

public class MemberLeftChannelEvent : IRequest
{
    public required SocketGuildUser User { get; init; }
    
    public required SocketGuildChannel Channel { get; init; }
}