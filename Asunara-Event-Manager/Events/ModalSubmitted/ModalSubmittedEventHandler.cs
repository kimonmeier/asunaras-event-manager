using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Events.BirthdayCreated;
using EventManager.Events.EventExtendedFeedback;
using EventManager.Events.SendMessageInChannel;
using MediatR;

namespace EventManager.Events.ModalSubmitted;

public class ModalSubmittedEventHandler : IRequestHandler<ModalSubmittedEvent>
{
    private readonly ISender _sender;
    private readonly RootConfig _config;

    public ModalSubmittedEventHandler(ISender sender, RootConfig config)
    {
        _sender = sender;
        _config = config;
    }

    public async Task Handle(ModalSubmittedEvent request, CancellationToken cancellationToken)
    {
        await request.ModalData.DeferLoadingAsync(ephemeral: true);

        if (request.ModalData.Data.CustomId.StartsWith(Konst.Modal.Feedback.Id))
        {
            await HandleFeedbackModal(request.ModalData);
        }

        if (request.ModalData.Data.CustomId.StartsWith(Konst.Modal.Birthday.Id))
        {
            await HandleBirthdayModal(request.ModalData);
        }
    }

    private async Task HandleFeedbackModal(SocketModal modal)
    {
        ulong eventId = ulong.Parse(modal.Data.CustomId.Split(Konst.PayloadDelimiter)[1]);

        var goodInput = modal.Data.Components.Single(x => x.CustomId == Konst.Modal.Feedback.GoodInputId).Value;
        var criticInput = modal.Data.Components.Single(x => x.CustomId == Konst.Modal.Feedback.CriticInputId).Value;
        var suggestionInput = modal.Data.Components.Single(x => x.CustomId == Konst.Modal.Feedback.SuggestionInputId).Value;

        await _sender.Send(new EventExtendedFeedbackEvent()
        {
            DiscordEventId = eventId,
            DiscordUserId = modal.User.Id,
            Good = goodInput,
            Critic = criticInput,
            Suggestion = suggestionInput
        });

        await modal.ModifyOriginalResponseAsync(x =>
        {
            x.Content = "Dein Feedback wurde erfolgreich dem Team übermittelt. Vielen Dank für deine Zeit!";
        });
    }

    private async Task HandleBirthdayModal(SocketModal modal)
    {
        ulong userId = ulong.Parse(modal.Data.CustomId.Split(Konst.PayloadDelimiter)[1]);

        var dayInputString = modal.Data.Components.Single(x => x.CustomId == Konst.Modal.Birthday.DayInputId).Value;
        if (!int.TryParse(dayInputString, out int dayInput))
        {
            await SendSuccessResponse(modal, "Bitte gib einen gültigen Tag ein!");
            return;
        }

        var monthInputString = modal.Data.Components.Single(x => x.CustomId == Konst.Modal.Birthday.MonthInputId).Value;
        if (!int.TryParse(monthInputString, out int monthInput))
        {
            await SendSuccessResponse(modal, "Bitte gib einen gültigen Monat ein!");
            return;
        }

        var yearInputString = modal.Data.Components.Single(x => x.CustomId == Konst.Modal.Birthday.YearInputId).Value;
        if (!int.TryParse(yearInputString, out int yearInput))
        {
            await SendSuccessResponse(modal, "Bitte gib ein gültiges Jahr ein!");
            return;
        }
        
        var success = await _sender.Send(new BirthdayCreatedEvent()
        {
            Day = dayInput, Month = monthInput, Year = yearInput, DiscordUserId = userId
        });

        if (success)
        {
            await SendSuccessResponse(modal, "Dein Geburtstag wurde erfolgreich erstellt!");
        }
        else
        {
            await SendSuccessResponse(modal, "Dein Geburtstag konnte nicht erstellt werden. Bitte versuche es mit korrekten Eingaben erneut!");
        }
    }
    
    private async Task SendSuccessResponse(SocketModal arg, string message)
    {
        if (arg.HasResponded)
        {

            await arg.ModifyOriginalResponseAsync(x => x.Content = message);
        }
        else
        {
            await arg.RespondAsync(message);
        }
    }
}