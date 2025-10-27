using EventManager.Events.AddFeedbackPreference;
using EventManager.Events.AddReminderPreference;
using MediatR;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Interactions.Button;

public class PreferenceButtonInteraction(ISender sender) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(Konst.ButtonFeedbackNo)]
    public async Task UserDoesntWantFeedback()
    {
        await sender.Send(new AddFeedbackPreferenceEvent
        {
            DiscordUserId = Context.User.Id, Preference = false
        });
    }

    [ComponentInteraction(Konst.ButtonFeedbackYes)]
    public async Task UserWantsFeedback()
    {
        await sender.Send(new AddFeedbackPreferenceEvent
        {
            DiscordUserId = Context.User.Id, Preference = true
        });
    }

    [ComponentInteraction(Konst.ButtonReminderNo)]
    public async Task UserDoesntWantsReminder()
    {
        await sender.Send(new AddReminderPreferenceEvent()
        {
            DiscordUserId = Context.User.Id, Preference = false
        });
    }

    [ComponentInteraction(Konst.ButtonReminderYes)]
    public async Task UserWantsReminder()
    {
        await sender.Send(new AddReminderPreferenceEvent()
        {
            DiscordUserId = Context.User.Id, Preference = true
        });
    }
}