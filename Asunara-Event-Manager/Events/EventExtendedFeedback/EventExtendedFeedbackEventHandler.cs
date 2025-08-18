using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.EventExtendedFeedback;

public class EventExtendedFeedbackEventHandler : IRequestHandler<EventExtendedFeedbackEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly EventFeedbackRepository _eventFeedbackRepository;
    private readonly DiscordEventRepository _discordEventRepository;

    public EventExtendedFeedbackEventHandler(DbTransactionFactory dbTransactionFactory, EventFeedbackRepository eventFeedbackRepository, DiscordEventRepository discordEventRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _eventFeedbackRepository = eventFeedbackRepository;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(EventExtendedFeedbackEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);

        if (@event is null)
        {
            throw new Exception("Event not found");
        }
        
        var transaction = _dbTransactionFactory.CreateTransaction();
        
        var eventFeedback = await _eventFeedbackRepository.GetOrCreateByDiscordEventAndUser(@event.Id, request.DiscordUserId);
        eventFeedback.Critic = request.Critic;
        eventFeedback.Good = request.Good;
        eventFeedback.Suggestion = request.Suggestion;
        
        await transaction.Commit(cancellationToken);
    }
}