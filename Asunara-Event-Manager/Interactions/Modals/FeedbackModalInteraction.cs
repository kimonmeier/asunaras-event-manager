using EventManager.Events.EventExtendedFeedback;
using EventManager.Extensions;
using MediatR;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Interactions.Modals;

public class FeedbackModalInteraction(ISender sender) : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction(Konst.Modal.Feedback.Id)]
    public async Task CreateFeedback(ulong eventId)
    {
        await this.Deferred();
        
        var goodInput = Context.Components.OfType<Label>().Select(x => x.Component).Cast<TextInput>().Single(x => x.CustomId == Konst.Modal.Feedback.GoodInputId).Value;
        var criticInput = Context.Components.OfType<Label>().Select(x => x.Component).Cast<TextInput>().Single(x => x.CustomId == Konst.Modal.Feedback.CriticInputId).Value;
        var suggestionInput = Context.Components.OfType<Label>().Select(x => x.Component).Cast<TextInput>().Single(x => x.CustomId == Konst.Modal.Feedback.SuggestionInputId).Value;

        await sender.Send(new EventExtendedFeedbackEvent()
        {
            DiscordEventId = eventId,
            DiscordUserId = Context.User.Id,
            Good = goodInput,
            Critic = criticInput,
            Suggestion = suggestionInput
        });

        await this.Answer("Dein Feedback wurde erfolgreich dem Team übermittelt. Vielen Dank für deine Zeit!");
    }
    
    
}