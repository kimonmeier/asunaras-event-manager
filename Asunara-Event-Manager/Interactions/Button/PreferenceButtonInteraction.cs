using EventManager.Events.AddFeedbackPreference;
using EventManager.Events.AddReminderPreference;
using EventManager.Extensions;
using MediatR;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Interactions.Button;

public class PreferenceButtonInteraction(ISender sender) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(Konst.ButtonFeedbackNo)]
    public async Task UserDoesntWantFeedback()
    {
        await this.Deferred();
        
        await sender.Send(new AddFeedbackPreferenceEvent
        {
            DiscordUserId = Context.User.Id, Preference = false
        });

        await this.Answer("Du erhälst nun keine Frage nach Feedback mehr!");
    }

    [ComponentInteraction(Konst.ButtonFeedbackYes)]
    public async Task UserWantsFeedback()
    {
        await this.Deferred();
        
        await sender.Send(new AddFeedbackPreferenceEvent
        {
            DiscordUserId = Context.User.Id, Preference = true
        });
        
        await this.Answer("Du erhälst nun wieder Fragen nach Feedback!");
    }

    [ComponentInteraction(Konst.ButtonReminderNo)]
    public async Task UserDoesntWantsReminder()
    {
        await this.Deferred();
        
        await sender.Send(new AddReminderPreferenceEvent()
        {
            DiscordUserId = Context.User.Id, Preference = false
        });
        
        await this.Answer("Du erhälst nun keine Reminder mehr!");
    }

    [ComponentInteraction(Konst.ButtonReminderYes)]
    public async Task UserWantsReminder()
    {
        await this.Deferred();
        
        await sender.Send(new AddReminderPreferenceEvent()
        {
            DiscordUserId = Context.User.Id, Preference = true
        });
        
        await this.Answer("Du erhälst nun wieder Reminder für ein anstehendes Event!");
    }
}