using MediatR;

namespace EventManager.Events.AddReminderPreference;

public class AddReminderPreferenceEvent : IRequest
{
    public required ulong DiscordUserId { get; init; }

    public required bool Preference { get; init; }
}