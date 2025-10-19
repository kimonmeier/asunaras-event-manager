using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.CheckExistingUsers;

public class CheckExistingUsersEvent : IRequest
{
    public required List<SocketGuildUser> Users { get; set; }
}