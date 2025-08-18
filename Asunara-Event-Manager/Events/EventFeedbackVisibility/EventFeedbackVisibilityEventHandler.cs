using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.EventFeedbackVisibility;

public class EventFeedbackVisibilityEventHandler : IRequestHandler<EventFeedbackVisibilityEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly EventFeedbackRepository _eventFeedbackRepository;
    private readonly DiscordEventRepository _discordEventRepository;

    public EventFeedbackVisibilityEventHandler(EventFeedbackRepository eventFeedbackRepository, DbTransactionFactory dbTransactionFactory,
        DiscordEventRepository discordEventRepository)
    {
        _eventFeedbackRepository = eventFeedbackRepository;
        _dbTransactionFactory = dbTransactionFactory;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(EventFeedbackVisibilityEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);
        if (@event is null)
        {
            throw new Exception("Event not found");
        }
        
        var transaction = _dbTransactionFactory.CreateTransaction();
        
        var eventFeedback = await _eventFeedbackRepository.GetOrCreateByDiscordEventAndUser(@event.Id, request.DiscordUserId);
        eventFeedback.Anonymous = request.Anonymous;
        
        await transaction.Commit(cancellationToken);
    }
}