using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.StartTrackingVoice;

public class StartTrackingVoiceEvent : IRequest
{
    public required SocketGuildUser DiscordUser { get; init; }
    
    public required SocketGuildChannel DiscordChannel { get; init; }
}