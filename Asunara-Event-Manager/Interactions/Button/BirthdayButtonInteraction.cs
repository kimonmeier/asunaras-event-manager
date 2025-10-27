using EventManager.Events.BirthdayDelete;
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
            new LabelProperties("Monat", new TextInputProperties(Konst.Modal.Birthday.DayInputId, TextInputStyle.Short).WithRequired()),
            new LabelProperties("Jahr (optional)", new TextInputProperties(Konst.Modal.Birthday.DayInputId, TextInputStyle.Short))
        ]);

        await Context.Interaction.SendResponseAsync(InteractionCallback.Modal(modalProperties));
    }

    [ComponentInteraction(Konst.ButtonBirthdayDelete)]
    public async Task DeleteBirthday()
    {
        await sender.Send(new BirthdayDeleteEvent()
        {
            DiscordUserId = Context.User.Id
        });

        await SendResponses();
    }

    private async Task SendResponses()
    {
        await Context.Interaction.SendResponseAsync(InteractionCallback.Message("Dein Wunsch wurde registriert!"));
    }
}