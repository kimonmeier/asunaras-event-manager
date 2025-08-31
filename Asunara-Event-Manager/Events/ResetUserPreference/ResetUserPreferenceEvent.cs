using MediatR;

namespace EventManager.Events.ResetUserPreference;

public class ResetUserPreferenceEvent : IRequest
{
    public required ulong DiscordUserId { get; init; }
}