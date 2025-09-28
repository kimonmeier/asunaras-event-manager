using Discord;
using Discord.Interactions;
using EventManager.Events.AskFeedback;
using EventManager.Events.ForceFeedback;
using MediatR;

namespace EventManager.Commands.Event;

[Group("events", "Diese Gruppe hat alle Befehle um mit Events zu arbeiten")]
public class EventInteraction : InteractionModuleBase
{
    private readonly ISender _sender;

    public EventInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SlashCommand("ask-feedback", "Startet den Feedback-Loop manuell!")]
    public async Task AskForFeedback([Autocomplete(typeof(EventAutocompleteHandler))] string eventId)
    {
        await _sender.Send(new AskFeedbackEvent()
        {
            EventId = Guid.Parse(eventId)
        });
    }

    [SlashCommand("force-feedback", "Forced Feedback von einem User")]
    public async Task ForceFeedback([Autocomplete(typeof(EventAutocompleteHandler))] string eventId, IUser user)
    {
        await _sender.Send(new ForceFeedbackEvent()
        {
            EventId = Guid.Parse(eventId), User = user
        });
    }
}