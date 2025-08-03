using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.EventCompleted;

public class EventCompletedEventHandler : IRequestHandler<EventCompletedEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly DbTransactionFactory _dbTransactionFactory;

    public EventCompletedEventHandler(DbTransactionFactory dbTransactionFactory, DiscordEventRepository discordEventRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(EventCompletedEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);
        if (@event is null)
        {
            throw new Exception("Event not found");
        }
        
        Transaction transaction = _dbTransactionFactory.CreateTransaction();

        @event.IsCompleted = true;
        await _discordEventRepository.UpdateAsync(@event);
        await transaction.Commit(cancellationToken);
    }
}