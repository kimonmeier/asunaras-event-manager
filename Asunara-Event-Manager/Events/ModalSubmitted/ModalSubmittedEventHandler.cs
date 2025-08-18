using Discord.WebSocket;
using EventManager.Events.EventExtendedFeedback;
using MediatR;

namespace EventManager.Events.ModalSubmitted;

public class ModalSubmittedEventHandler : IRequestHandler<ModalSubmittedEvent>
{
    private readonly ISender _sender;

    public ModalSubmittedEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(ModalSubmittedEvent request, CancellationToken cancellationToken)
    {
        await request.ModalData.DeferLoadingAsync(ephemeral: true);

        if (request.ModalData.Data.CustomId.StartsWith(Konst.Modal.Feedback.Id))
        {
            await HandleFeedbackModal(request.ModalData);
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
            x.Content = "Dein Feedback wurde erfolgreich gesendet!";
        });
    }
}