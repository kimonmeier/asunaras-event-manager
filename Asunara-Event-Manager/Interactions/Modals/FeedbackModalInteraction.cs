using EventManager.Events.EventExtendedFeedback;
using MediatR;
using NetCord;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Interactions.Modals;

public class FeedbackModalInteraction(ISender sender) : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction(Konst.Modal.Feedback.Id)]
    public async Task CreateFeedback(ulong eventId)
    {
        var goodInput = Context.Components.OfType<TextInput>().Single(x => x.CustomId == Konst.Modal.Feedback.GoodInputId).Value;
        var criticInput = Context.Components.OfType<TextInput>().Single(x => x.CustomId == Konst.Modal.Feedback.CriticInputId).Value;
        var suggestionInput = Context.Components.OfType<TextInput>().Single(x => x.CustomId == Konst.Modal.Feedback.SuggestionInputId).Value;

        await sender.Send(new EventExtendedFeedbackEvent()
        {
            DiscordEventId = eventId,
            DiscordUserId = Context.User.Id,
            Good = goodInput,
            Critic = criticInput,
            Suggestion = suggestionInput
        });

        await Context.Interaction.ModifyResponseAsync(x =>
            x.Content = "Dein Feedback wurde erfolgreich dem Team übermittelt. Vielen Dank für deine Zeit!");
    }
    
    
}