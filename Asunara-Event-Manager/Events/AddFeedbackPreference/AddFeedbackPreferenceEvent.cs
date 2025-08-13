using MediatR;

namespace EventManager.Events.AddFeedbackPreference;

public class AddFeedbackPreferenceEvent : IRequest
{
    public required ulong DiscordUserId { get; init; }

    public required bool Preference { get; init; }
}