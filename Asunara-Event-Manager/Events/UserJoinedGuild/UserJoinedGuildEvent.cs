using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.UserJoinedGuild;

public class UserJoinedGuildEvent : IRequest
{
    public required SocketGuildUser GuildUser { get; set; }
}