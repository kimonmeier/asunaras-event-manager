using EventManager.Data;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using EventManager.Events.UpdateEventFeedbackThread;
using MediatR;

namespace EventManager.Events.EventSendFeedbackStar;

public class EventSendFeedbackStarEventHandler : IRequestHandler<EventSendFeedbackStarEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly EventFeedbackRepository _eventFeedbackRepository;
    private readonly ISender _sender;

    public EventSendFeedbackStarEventHandler(DbTransactionFactory dbTransactionFactory, EventFeedbackRepository eventFeedbackRepository, DiscordEventRepository discordEventRepository, ISender sender)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _eventFeedbackRepository = eventFeedbackRepository;
        _discordEventRepository = discordEventRepository;
        _sender = sender;
    }

    public async Task Handle(EventSendFeedbackStarEvent request, CancellationToken cancellationToken)
    {
        DiscordEvent? @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);

        if (@event is null)
        {
            throw new Exception("Event not found");
        }

        if (request.StarCount > 5)
        {
            request.StarCount = 5;
        }

        if (request.StarCount < 0)
        {
            request.StarCount = 0;
        }
        
        var transaction = await _dbTransactionFactory.CreateTransaction();;

        EventFeedback eventFeedback = await _eventFeedbackRepository.GetOrCreateByDiscordEventAndUser(@event.Id, request.DiscordUserId);
        eventFeedback.Score = request.StarCount;
        
        await transaction.Commit(cancellationToken);
        
        await _sender.Send(new UpdateEventFeedbackThreadEvent()
        {
            DiscordEventId = request.DiscordEventId
        }, cancellationToken);
    }
}