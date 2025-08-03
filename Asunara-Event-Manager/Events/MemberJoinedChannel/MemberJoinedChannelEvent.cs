using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.MemberJoinedChannel;

public class MemberJoinedChannelEvent : IRequest
{
    public required SocketGuildUser User { get; init; }
    
    public required SocketGuildChannel Channel { get; init; }
}