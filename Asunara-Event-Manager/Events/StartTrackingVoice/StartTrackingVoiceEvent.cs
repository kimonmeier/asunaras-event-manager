using MediatR;

namespace EventManager.Events.StartTrackingVoice;

public class StartTrackingVoiceEvent : IRequest
{
    public required ulong DiscordUserId { get; init; }
    
    public required ulong DiscordChannelId { get; init; }
}