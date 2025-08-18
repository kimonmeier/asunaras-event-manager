using EventManager.Data;
using EventManager.Data.Repositories;
using EventManager.Events.EventStartFeedback;
using MediatR;

namespace EventManager.Events.EventCompleted;

public class EventCompletedEventHandler : IRequestHandler<EventCompletedEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ISender _sender;

    public EventCompletedEventHandler(DbTransactionFactory dbTransactionFactory, DiscordEventRepository discordEventRepository, ISender sender)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _discordEventRepository = discordEventRepository;
        _sender = sender;
    }

    public async Task Handle(EventCompletedEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);
        if (@event is null)
        {
            throw new Exception("Event not found");
        }

        await _sender.Send(new EventStartFeedbackEvent()
        {
            Event = @event,
        }, cancellationToken);
        
        Transaction transaction = _dbTransactionFactory.CreateTransaction();

        @event.IsCompleted = true;
        await _discordEventRepository.UpdateAsync(@event);
        await transaction.Commit(cancellationToken);
    }
}