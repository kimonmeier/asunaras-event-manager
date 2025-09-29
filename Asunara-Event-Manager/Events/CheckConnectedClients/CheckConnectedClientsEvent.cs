using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.CheckConnectedClients;

public class CheckConnectedClientsEvent : IRequest
{
    public required List<SocketGuildUser> ConnectedUsers { get; init; }
}