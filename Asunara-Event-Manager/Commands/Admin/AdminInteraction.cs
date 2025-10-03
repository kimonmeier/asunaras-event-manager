using Discord;
using Discord.Interactions;
using EventManager.Commands.Event;
using EventManager.Events.CheckBirthday;
using EventManager.Events.CheckConnectedClients;
using EventManager.Events.CheckVoiceActivityForChannel;
using EventManager.Events.ResetUserPreference;
using EventManager.Events.SendMessageToAll;
using EventManager.Events.SendMessageToEvent;
using EventManager.Events.ThrowException;
using MediatR;

namespace EventManager.Commands.Admin;

[RequireUserPermission(GuildPermission.ViewAuditLog)]
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
            Author = Context.User, DiscordEventId = Guid.Parse(eventId), Message = message
        });
    }

    [SlashCommand("send-message-to-all", "Sendet eine Nachricht an alle die in der Datenbank sind")]
    public async Task SendMessageToAll(string message)
    {
        await _sender.Send(new SendMessageToAllEvent()
        {
            Author = Context.User, Message = message
        });
    }

    [SlashCommand("check-birthdays", "Führt die Logik für die Geburtstage aus")]
    public async Task CheckBirthdays()
    {
        await _sender.Send(new CheckBirthdayEvent());
    }

    [SlashCommand("force-check-activity-channel", "Führt die Logik aus um einen Channel zu checken")]
    public async Task ForceCheckChannel(IAudioChannel channel)
    {
        await _sender.Send(new CheckVoiceActivityForChannelEvent()
        {
            ChannelId = channel.Id
        });
    }

    [SlashCommand("force-exception", "Forced eine Exception für Sentry")]
    public async Task ForceException(bool withMediator)
    {
        if (withMediator)
        {
            await _sender.Send(new ThrowExceptionEvent());
        }
        else
        {
            throw new Exception("Test Exception");
        }
        
    }
}