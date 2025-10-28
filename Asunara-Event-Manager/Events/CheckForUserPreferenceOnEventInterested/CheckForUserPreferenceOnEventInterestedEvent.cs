using MediatR;
using NetCord;

namespace EventManager.Events.CheckForUserPreferenceOnEventInterested;

public class CheckForUserPreferenceOnEventInterestedEvent : IRequest
{
    public required GuildUser DiscordUser { get; init; }
}