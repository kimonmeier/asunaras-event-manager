using EventManager.Data;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace EventManager.Events.EventDeleted;

public class EventDeletedEventHandler : IRequestHandler<EventDeletedEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ILogger<EventDeletedEventHandler> _logger;

    public EventDeletedEventHandler(DbTransactionFactory dbTransactionFactory, DiscordEventRepository discordEventRepository, ILogger<EventDeletedEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _discordEventRepository = discordEventRepository;
        _logger = logger;
    }

    public async Task Handle(EventDeletedEvent request, CancellationToken cancellationToken)
    {
        DiscordEvent? discordEvent = await _discordEventRepository.FindByDiscordId(request.DiscordId);

        if (discordEvent is null)
        {
            _logger.LogError("Event not found for DiscordId: {DiscordId}", request.DiscordId);
            return;
        }

        using var dbTransaction = _dbTransactionFactory.CreateTransaction();
        await _discordEventRepository.RemoveAsync(discordEvent);
        await dbTransaction.Commit(cancellationToken);
    }
}