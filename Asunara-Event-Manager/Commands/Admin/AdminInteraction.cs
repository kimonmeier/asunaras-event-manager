using EventManager.Commands.Event;
using EventManager.Events.CheckBirthday;
using EventManager.Events.CheckConnectedClients;
using EventManager.Events.CheckVoiceActivityForChannel;
using EventManager.Events.ResetUserPreference;
using EventManager.Events.SelectHalloweenChannel;
using EventManager.Events.SendMessageToAll;
using EventManager.Events.SendMessageToEvent;
using EventManager.Events.ThrowException;
using EventManager.Extensions;
using EventManager.Services;
using MediatR;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Commands.Admin;

[SlashCommand("admin", "Admin Commands für den Bot",
    DefaultGuildPermissions = Permissions.ViewAuditLog, Contexts = [InteractionContextType.Guild])]
public class AdminInteraction : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ISender _sender;
    private readonly AudioService _audioService;

    public AdminInteraction(ISender sender, AudioService audioService)
    {
        _sender = sender;
        _audioService = audioService;
    }

    [SubSlashCommand("reset-user-preference", "Entfernt die User Preference")]
    public async Task ResetUserPreference(User user)
    {
        await this.Deferred();
        
        await _sender.Send(new ResetUserPreferenceEvent()
        {
            DiscordUserId = user.Id
        });

        await ModifyResponseAsync(x => x.Content = "Die Präferenzen für einen Benutzer wurden zurückgestellt");
    }

    [SubSlashCommand("send-message-to-interest", "Sendet eine Nachricht an alle Interessierten für ein Event")]
    public async Task SendMessageToInterest([SlashCommandParameter(AutocompleteProviderType = typeof(EventUncompletedAutocompleteHandler))] string eventId, string message)
    {
        await this.Deferred();
        
        await _sender.Send(new SendMessageToEventEvent()
        {
            Author = Context.User, DiscordEventId = Guid.Parse(eventId), Message = message
        });
        
        
        await ModifyResponseAsync(x => x.Content = "Die Nachricht wurde an alle Interessierten gesendet");
    }

    [SubSlashCommand("send-message-to-all", "Sendet eine Nachricht an alle die in der Datenbank sind")]
    public async Task SendMessageToAll(string message)
    {
        await this.Deferred();
        
        await _sender.Send(new SendMessageToAllEvent()
        {
            Author = Context.User, Message = message
        });
        
        await ModifyResponseAsync(x => x.Content = "Es wurde erfolgreich eine Nachricht an alle User gesendet!");
    }

    [SubSlashCommand("check-birthdays", "Führt die Logik für die Geburtstage aus")]
    public async Task CheckBirthdays()
    {
        await this.Deferred();
        
        await _sender.Send(new CheckBirthdayEvent());
        await this.Answer("Die Geburstage wurden überprüft");
    }

    [SubSlashCommand("force-check-activity-channel", "Führt die Logik aus um einen Channel zu checken")]
    public async Task ForceCheckChannel(IVoiceGuildChannel channel)
    {
        await this.Deferred();
        
        await _sender.Send(new CheckVoiceActivityForChannelEvent()
        {
            ChannelId = channel.Id
        });

        await this.Answer("Der Channel {0} wurde erfolgreich auf die Aktivität überprüft", channel.Name);
    }

    [SubSlashCommand("force-exception", "Forced eine Exception für Sentry")]
    public async Task ForceException(bool withMediator)
    {
        await this.Deferred();
        if (withMediator)
        {
            await _sender.Send(new ThrowExceptionEvent());
        }
        else
        {
            throw new Exception("Test Exception");
        }
    }

    [SubSlashCommand("connect-to-voice", "Connects to a voice channel")]
    public async Task ConnectToVoice(IVoiceGuildChannel channel)
    {
        await this.Deferred(true);
        
        await _audioService.ConnectToVoiceChannelAsync(channel.GuildId, channel.Id);
        await ModifyResponseAsync(x => x.Content = "Client hat sich mit dem Voice-Channel verbunden");
    }
    
    [SubSlashCommand("disconnect-from-voice", "Disconnects from a voice channel")]
    public async Task DisconnectFromVoice()
    {
        await this.Deferred(true);
        
        await _audioService.DisconnectFromVoiceChannelAsync();
        
        await ModifyResponseAsync(x => x.Content = "Client wurde vom Voice-Channel getrennt");
    }

    [SubSlashCommand("play-sound", "Plays a sound")]
    public async Task PlaySound(string url)
    {
        await this.Deferred(true);
        
        await ModifyResponseAsync(x => x.Content = "Audio-File wird abgespielt");
        
        await _audioService.PlayAudioAsync(url);
        
        await ModifyResponseAsync(x => x.Content = "Audio-File wurde abgespielt");
    }

    [SubSlashCommand("start-halloween", "Starts the Halloween Event")]
    public async Task StartHalloween()
    {
        await this.Deferred();
        
        await _sender.Send(new SelectHalloweenChannelEvent());

        await ModifyResponseAsync(x => x.Content = "Halloween Event gestartet");
    }
}