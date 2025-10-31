using MediatR;
using NetCord;

namespace EventManager.Events.MemberJoinedChannel;

public class MemberJoinedChannelEvent : IRequest
{
    public required GuildUser User { get; init; }
    
    public required IVoiceGuildChannel Channel { get; init; }
}