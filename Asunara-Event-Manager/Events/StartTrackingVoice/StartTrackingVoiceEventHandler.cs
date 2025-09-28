using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.StartTrackingVoice;

public class StartTrackingVoiceEventHandler : IRequestHandler<StartTrackingVoiceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly RootConfig _rootConfig;
    private readonly ILogger<StartTrackingVoiceEventHandler> _logger;

    public StartTrackingVoiceEventHandler(DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository, RootConfig rootConfig, ILogger<StartTrackingVoiceEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
        _rootConfig = rootConfig;
        _logger = logger;
    }

    public async Task Handle(StartTrackingVoiceEvent request, CancellationToken cancellationToken)
    {
        if (_rootConfig.Discord.Activity.ExcludedChannelsId.Contains(request.DiscordChannelId))
        {
            _logger.LogDebug($"Die Aktivität in dem Voice-Channel {request.DiscordChannelId} ist deaktiviert!");
            return;
        }
        
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Type = ActivityType.VoiceChannelJoined, Date = DateTime.UtcNow, DiscordUserId = request.DiscordUserId, ChannelId = request.DiscordChannelId
        });
        
        await transaction.Commit(cancellationToken);
    }
}