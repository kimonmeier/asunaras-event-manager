using Discord.Interactions;
using EventManager.Events.AskFeedback;
using MediatR;

namespace EventManager.Commands;

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
}