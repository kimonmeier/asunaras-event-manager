using EventManager.Events.AskFeedback;
using EventManager.Events.ForceFeedback;
using EventManager.Extensions;
using MediatR;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Sentry.Protocol;

namespace EventManager.Commands.Event;

[SlashCommand("events", "Diese Gruppe hat alle Befehle um mit Events zu arbeiten",
    DefaultGuildPermissions = Permissions.SendPolls, Contexts = [InteractionContextType.Guild])]
public class EventInteraction : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ISender _sender;

    public EventInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SubSlashCommand("ask-feedback", "Startet den Feedback-Loop manuell!")]
    public async Task AskForFeedback([SlashCommandParameter(AutocompleteProviderType = typeof(EventUncompletedAutocompleteHandler))] string eventId)
    {
        await this.Deferred(true);
        
        await _sender.Send(new AskFeedbackEvent()
        {
            EventId = Guid.Parse(eventId)
        });
        
        await this.Answer("Feedback was asked!");
    }

    [SubSlashCommand("force-feedback", "Forced Feedback von einem User")]
    public async Task ForceFeedback([SlashCommandParameter(AutocompleteProviderType = typeof(EventUncompletedAutocompleteHandler))] string eventId, GuildUser user)
    {
        await this.Deferred();
        
        await _sender.Send(new ForceFeedbackEvent()
        {
            EventId = Guid.Parse(eventId), User = user
        });

        await this.Answer("Feedback was forced!");
    }
}