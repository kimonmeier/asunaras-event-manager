using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.MessageReceived;

public class MessageReceivedEventHandler : IRequestHandler<MessageReceivedEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly RootConfig _rootConfig;
    private readonly ILogger<MessageReceivedEventHandler> _logger;

    public MessageReceivedEventHandler(DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository, RootConfig rootConfig, ILogger<MessageReceivedEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
        _rootConfig = rootConfig;
        _logger = logger;
    }

    public async Task Handle(MessageReceivedEvent request, CancellationToken cancellationToken)
    {
        if (_rootConfig.Discord.Activity.ExcludedChannelsId.Contains(request.Message.Channel.Id))
        {
            _logger.LogDebug($"Die Activity in dem Channel {request.Message.Channel.Id} ist deaktiviert!");
            return;
        }
        
        var transaction = await _dbTransactionFactory.CreateTransaction();
        
        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Date = DateTime.UtcNow, ChannelId = request.Message.Channel.Id, DiscordUserId = request.Message.Author.Id,
        });
        
        await transaction.Commit(cancellationToken);
    }
}