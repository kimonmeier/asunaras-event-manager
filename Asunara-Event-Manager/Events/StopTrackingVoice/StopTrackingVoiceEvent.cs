using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.StopTrackingVoice;

public class StopTrackingVoiceEvent : IRequest
{
    public required SocketGuildUser DiscordUser { get; init; }
    
    public required SocketGuildChannel DiscordChannel { get; init; }
}