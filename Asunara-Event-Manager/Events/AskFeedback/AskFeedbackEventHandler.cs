using EventManager.Data.Repositories;
using EventManager.Events.EventStartFeedback;
using MediatR;

namespace EventManager.Events.AskFeedback;

public class AskFeedbackEventHandler : IRequestHandler<AskFeedbackEvent>
{
    private readonly ISender _sender;
    private readonly DiscordEventRepository _discordEventRepository;

    public AskFeedbackEventHandler(ISender sender, DiscordEventRepository discordEventRepository)
    {
        _sender = sender;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(AskFeedbackEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByEntityAsync(request.EventId);
        if (@event is null)
        {
            throw new Exception("Event not found");
        }

        await _sender.Send(new EventStartFeedbackEvent()
        {
            Event = @event,
        });
    }
}