using MediatR;
using NetCord;

namespace EventManager.Events.StopTrackingVoice;

public class StopTrackingVoiceEvent : IRequest
{
    public required GuildUser DiscordUser { get; init; }
    
    public required IVoiceGuildChannel DiscordChannel { get; init; }
}