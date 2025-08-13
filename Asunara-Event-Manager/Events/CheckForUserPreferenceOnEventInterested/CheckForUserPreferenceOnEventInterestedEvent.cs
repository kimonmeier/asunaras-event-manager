using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.CheckForUserPreferenceOnEventInterested;

public class CheckForUserPreferenceOnEventInterestedEvent : IRequest
{
    public required SocketGuildUser DiscordUser { get; init; }
}