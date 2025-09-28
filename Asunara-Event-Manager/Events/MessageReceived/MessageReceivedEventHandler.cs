using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.MessageReceived;

public class MessageReceivedEventHandler : IRequestHandler<MessageReceivedEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;

    public MessageReceivedEventHandler(DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
    }

    public async Task Handle(MessageReceivedEvent request, CancellationToken cancellationToken)
    {
        var transaction = await _dbTransactionFactory.CreateTransaction();

        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Date = DateTime.UtcNow, ChannelId = request.Message.Channel.Id, DiscordUserId = request.Message.Author.Id,
        });
        
        await transaction.Commit(cancellationToken);
    }
}