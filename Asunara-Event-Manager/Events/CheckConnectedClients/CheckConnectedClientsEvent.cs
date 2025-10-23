using MediatR;
using NetCord;

namespace EventManager.Events.CheckConnectedClients;

public class CheckConnectedClientsEvent : IRequest
{
    public required List<GuildUser> ConnectedUsers { get; init; }
}