using Discord;
using MediatR;

namespace EventManager.Events.SendMessageInChannel;

public class SendMessageInChannelCommand : IRequest
{
    public required ulong DiscordGuildId { get; init; }
    
    public required ulong DiscordChannelId { get; init; }
    
    public string? Message { get; init; }
    
    public Embed? Embed { get; init; }
}