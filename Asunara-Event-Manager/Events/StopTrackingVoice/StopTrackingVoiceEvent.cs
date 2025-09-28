using MediatR;

namespace EventManager.Events.StopTrackingVoice;

public class StopTrackingVoiceEvent : IRequest
{
    public required ulong DiscordUserId { get; init; }
    
    public required ulong DiscordChannelId { get; init; }
}