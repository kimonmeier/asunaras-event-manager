using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using EventManager.Events.StartTrackingVoice;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.StopTrackingVoice;

public class StopTrackingVoiceEventHandler : IRequestHandler<StopTrackingVoiceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly RootConfig _rootConfig;
    private readonly ILogger<StopTrackingVoiceEventHandler> _logger;

    public StopTrackingVoiceEventHandler(DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository, RootConfig rootConfig, ILogger<StopTrackingVoiceEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
        _rootConfig = rootConfig;
        _logger = logger;
    }

    public async Task Handle(StopTrackingVoiceEvent request, CancellationToken cancellationToken)
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
        
        var lastVoiceActivity = await _activityEventRepository.GetLastVoiceActivityByDiscordId(request.DiscordUser.Id);

        if (lastVoiceActivity is null || lastVoiceActivity.Type is ActivityType.VoiceChannelLeft)
        {
            _logger.LogDebug($"Es gibt keine Start-Aktivität für den Nutzer \"{request.DiscordUser.Username}\"!");
            return;
        }
        
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Type = ActivityType.VoiceChannelLeft, Date = DateTime.UtcNow, DiscordUserId = request.DiscordUser.Id, ChannelId = request.DiscordChannel.Id
        });
        
        await transaction.Commit(cancellationToken);
    }
}