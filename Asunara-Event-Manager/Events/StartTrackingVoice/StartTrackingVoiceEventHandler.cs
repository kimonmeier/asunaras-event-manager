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
        if (_rootConfig.Discord.Activity.ExcludedChannelsId.Contains(request.DiscordChannel.Id))
        {
            _logger.LogDebug($"Die Aktivität in dem Voice-Channel \"{request.DiscordChannel.Name}\" ist deaktiviert!");
            return;
        }
        
        if (request.DiscordUser.IsBot)
        {
            _logger.LogDebug($"Der Nutzer \"{request.DiscordUser.Username}\" ist ein Bot!");
            return;
        }
        
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Type = ActivityType.VoiceChannelJoined, Date = DateTime.UtcNow, DiscordUserId = request.DiscordUser.Id, ChannelId = request.DiscordChannel.Id
        });
        
        await transaction.Commit(cancellationToken);
    }
}