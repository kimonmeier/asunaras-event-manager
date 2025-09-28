using Discord;
using Discord.Interactions;
using EventManager.Commands.Event;
using EventManager.Events.CheckBirthday;
using EventManager.Events.ResetUserPreference;
using EventManager.Events.SendMessageToAll;
using EventManager.Events.SendMessageToEvent;
using MediatR;

namespace EventManager.Commands.Admin;

[Group("admin", "Admin Commands für den ")]
public class AdminInteraction : InteractionModuleBase
{
    private readonly ISender _sender;

    public AdminInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SlashCommand("reset-user-preference", "Entfernt die User Preference")]
    public async Task ResetUserPreference(IUser user)
    {
        await _sender.Send(new ResetUserPreferenceEvent()
        {
            DiscordUserId = user.Id
        });
    }

    [SlashCommand("send-message-to-interest", "Sendet eine Nachricht an alle Interessierten für ein Event")]
    public async Task SendMessageToInterest([Autocomplete(typeof(EventUncompletedAutocompleteHandler))] string eventId, string message)
    {
        await _sender.Send(new SendMessageToEventEvent()
        {
            Author = Context.User,
            DiscordEventId = Guid.Parse(eventId),
            Message = message
        });
    }
    
    [SlashCommand("send-message-to-all", "Sendet eine Nachricht an alle die in der Datenbank sind")]
    public async Task SendMessageToAll(string message)
    {
        await _sender.Send(new SendMessageToAllEvent()
        {
            Author = Context.User,
            Message = message
        });
    }

    [SlashCommand("check-birthdays", "Führt die Logik für die Geburtstage aus")]
    public async Task CheckBirthdays()
    {
        await _sender.Send(new CheckBirthdayEvent());
    }
}