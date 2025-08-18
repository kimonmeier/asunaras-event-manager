using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.SendMessageInChannel;

public class SendMessageInChannelCommandHandler : IRequestHandler<SendMessageInChannelCommand>
{
    private readonly DiscordSocketClient _discordSocketClient;

    public SendMessageInChannelCommandHandler(DiscordSocketClient discordSocketClient)
    {
        _discordSocketClient = discordSocketClient;
    }

    public Task Handle(SendMessageInChannelCommand request, CancellationToken cancellationToken)
    {
        if (request.Embed is null && string.IsNullOrWhiteSpace(request.Message))
        {
            throw new Exception("Message or Embed is required");
        }
        
        SocketGuild socketGuild = _discordSocketClient.GetGuild(request.DiscordGuildId);
        if (socketGuild is null)
        {
            throw new Exception("Guild not found");
        }
        
        SocketTextChannel socketTextChannel = socketGuild.GetTextChannel(request.DiscordChannelId);
        if (socketTextChannel is null)
        {
            throw new Exception("Channel not found");
        }

        if (request.Embed is not null)
        {
            return socketTextChannel.SendMessageAsync(embed: request.Embed);
        }
        
        return socketTextChannel.SendMessageAsync(request.Message);
    }
}