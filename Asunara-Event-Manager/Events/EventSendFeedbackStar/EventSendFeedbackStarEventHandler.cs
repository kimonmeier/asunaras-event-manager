using EventManager.Data;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.EventSendFeedbackStar;

public class EventSendFeedbackStarEventHandler : IRequestHandler<EventSendFeedbackStarEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly EventFeedbackRepository _eventFeedbackRepository;

    public EventSendFeedbackStarEventHandler(DbTransactionFactory dbTransactionFactory, EventFeedbackRepository eventFeedbackRepository, DiscordEventRepository discordEventRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _eventFeedbackRepository = eventFeedbackRepository;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(EventSendFeedbackStarEvent request, CancellationToken cancellationToken)
    {
        DiscordEvent? @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);

        if (@event is null)
        {
            throw new Exception("Event not found");
        }
        
        var transaction = _dbTransactionFactory.CreateTransaction();

        EventFeedback eventFeedback = await _eventFeedbackRepository.GetOrCreateByDiscordEventAndUser(@event.Id, request.DiscordUserId);
        eventFeedback.Score = request.StarCount;
        
        await transaction.Commit(cancellationToken);
    }
}