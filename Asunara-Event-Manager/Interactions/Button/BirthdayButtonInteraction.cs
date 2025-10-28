using EventManager.Events.BirthdayDelete;
using EventManager.Extensions;
using MediatR;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Interactions.Button;

public class BirthdayButtonInteraction(ISender sender) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(Konst.ButtonBirthdayRegister)]
    public async Task RegisterBirthday()
    {
        ModalProperties modalProperties = new ModalProperties(Konst.Modal.Birthday.Id, "Geburstag erfassen/ändern");
        modalProperties.AddComponents([
            new LabelProperties("Tag", new TextInputProperties(Konst.Modal.Birthday.DayInputId, TextInputStyle.Short).WithRequired()),
            new LabelProperties("Monat", new TextInputProperties(Konst.Modal.Birthday.MonthInputId, TextInputStyle.Short).WithRequired()),
            new LabelProperties("Jahr (optional)", new TextInputProperties(Konst.Modal.Birthday.YearInputId, TextInputStyle.Short).WithRequired(false))
        ]);

        await Context.Interaction.SendResponseAsync(InteractionCallback.Modal(modalProperties));
    }

    [ComponentInteraction(Konst.ButtonBirthdayDelete)]
    public async Task DeleteBirthday()
    {
        await this.Deferred(true);
        
        await sender.Send(new BirthdayDeleteEvent()
        {
            DiscordUserId = Context.User.Id
        });

        await this.Answer("Dein Geburtstag wurde erfolgreich gelöscht!");
    }
}