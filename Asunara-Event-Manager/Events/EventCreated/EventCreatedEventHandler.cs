using EventManager.Data;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Events.EventCreated;

public class EventCreatedEventHandler : IRequestHandler<EventCreatedEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly DiscordEventRepository _discordEventRepository;

    public EventCreatedEventHandler(DbTransactionFactory dbTransactionFactory, DiscordEventRepository discordEventRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(EventCreatedEvent request, CancellationToken cancellationToken)
    {
        using var transaction = _dbTransactionFactory.CreateTransaction();

        await _discordEventRepository.AddAsync(new DiscordEvent()
        {
            DiscordId = request.DiscordId, Date = request.Datum
        });

        await transaction.Commit(cancellationToken);
    }
}