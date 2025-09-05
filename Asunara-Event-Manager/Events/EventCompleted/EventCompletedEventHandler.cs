using EventManager.Data;
using EventManager.Data.Repositories;
using EventManager.Events.EventStartFeedback;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.EventCompleted;

public class EventCompletedEventHandler : IRequestHandler<EventCompletedEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ISender _sender;
    private readonly ILogger<EventCompletedEventHandler> _logger;

    public EventCompletedEventHandler(DbTransactionFactory dbTransactionFactory, DiscordEventRepository discordEventRepository, ISender sender, ILogger<EventCompletedEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _discordEventRepository = discordEventRepository;
        _sender = sender;
        _logger = logger;
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

        Task eventStartFeedback = _sender.Send(new EventStartFeedbackEvent()
        {
            Event = @event,
        }, cancellationToken);

        eventStartFeedback.ConfigureAwait(false).GetAwaiter();
        
        _logger.LogInformation("Event completed: {0}", @event.Name);;
    }
}