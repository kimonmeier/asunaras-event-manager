using MediatR;

namespace EventManager.Events.UserLeftGuild;

public class UserLeftGuildEvent : IRequest
{
    public required ulong DiscordUserId { get; set; }

}