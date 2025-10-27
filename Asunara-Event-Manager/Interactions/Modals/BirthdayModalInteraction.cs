using EventManager.Events.BirthdayCreated;
using MediatR;
using NetCord;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Interactions.Modals;

public class BirthdayModalInteraction(ISender sender) : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction(Konst.Modal.Birthday.Id)]
    public async Task CreateBirthday()
    {
        ulong userId = Context.User.Id;

        var dayInputString = Context.Components.OfType<TextInput>().Single(x => x.CustomId == Konst.Modal.Birthday.DayInputId).Value;
        if (!int.TryParse(dayInputString, out int dayInput))
        {
            await Context.Interaction.SendFollowupMessageAsync("Bitte gib einen gültigen Tag ein!");
            return;
        }

        var monthInputString = Context.Components.OfType<TextInput>().Single(x => x.CustomId == Konst.Modal.Birthday.MonthInputId).Value;
        if (!int.TryParse(monthInputString, out int monthInput))
        {
            await Context.Interaction.SendFollowupMessageAsync("Bitte gib einen gültigen Monat ein!");
            return;
        }

        var yearInputString = Context.Components.OfType<TextInput>().Single(x => x.CustomId == Konst.Modal.Birthday.YearInputId).Value;
        int? yearInput = null;
        if (!string.IsNullOrEmpty(yearInputString))
        {
            if (!int.TryParse(yearInputString, out var year))
            {
                await Context.Interaction.SendFollowupMessageAsync("Bitte gib ein gültiges Jahr ein!");

                return;
            }
            
            yearInput = year;
        } 

        var success = await sender.Send(new BirthdayCreatedEvent()
        {
            Day = dayInput, Month = monthInput, Year = yearInput, DiscordUserId = userId
        });

        if (success)
        {
            await Context.Interaction.SendFollowupMessageAsync("Dein Geburtstag wurde erfolgreich erstellt!");
        }
        else
        {
            await Context.Interaction.SendFollowupMessageAsync("Dein Geburtstag konnte nicht erstellt werden. Bitte versuche es mit korrekten Eingaben erneut!");
        }
    }
}