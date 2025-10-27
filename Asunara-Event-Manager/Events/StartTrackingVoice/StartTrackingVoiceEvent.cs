using MediatR;
using NetCord;

namespace EventManager.Events.StartTrackingVoice;

public class StartTrackingVoiceEvent : IRequest
{
    public required GuildUser DiscordUser { get; init; }
    
    public required IVoiceGuildChannel DiscordChannel { get; init; }
}