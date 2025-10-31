using MediatR;
using NetCord;

namespace EventManager.Events.MemberLeftChannel;

public class MemberLeftChannelEvent : IRequest
{
    public required GuildUser User { get; init; }
    
    public required IVoiceGuildChannel Channel { get; init; }
}