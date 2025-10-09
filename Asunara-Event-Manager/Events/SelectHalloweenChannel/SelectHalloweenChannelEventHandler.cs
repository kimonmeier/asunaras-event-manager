using Discord.WebSocket;
using EventManager.Background;
using EventManager.Configuration;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EventManager.Events.SelectHalloweenChannel;

public class SelectHalloweenChannelEventHandler : IRequestHandler<SelectHalloweenChannelEvent>
{
    private readonly HalloweenService _halloweenService;
    private readonly AudioService _audioService;
    private readonly DiscordSocketClient _client;
    private readonly RootConfig _config;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<SelectHalloweenChannelEventHandler> _logger;

    public SelectHalloweenChannelEventHandler(HalloweenService halloweenService, AudioService audioService, DiscordSocketClient client, RootConfig config, ISchedulerFactory schedulerFactory, ILogger<SelectHalloweenChannelEventHandler> logger)
    {
        _halloweenService = halloweenService;
        _audioService = audioService;
        _client = client;
        _config = config;
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task Handle(SelectHalloweenChannelEvent request, CancellationToken cancellationToken)
    {
        var voiceChannels = _client.GetGuild(_config.Discord.MainDiscordServerId).VoiceChannels.Where(x => x.ConnectedUsers.Count > 0).ToList();

        // Remove channels that were recently scared
        voiceChannels
            .RemoveAll(channel => _halloweenService.GetTimedDifferenceBetweenScaredChannel(channel.Id).TotalMinutes <= _config.Discord.Halloween.MinTimeBetweenScaresPerChannel);
        
        // Remove channels where a user was recently scared
        voiceChannels
            .RemoveAll(channel =>
                channel.ConnectedUsers.Any(user => _halloweenService.GetTimedDifferenceBetweenScaredUser(user.Id).TotalMinutes <= _config.Discord.Halloween.MinTimeBetweenScaresPerUser));

        var channelToScare = voiceChannels
            .Select(x => new
            {
                Channel = x,
                x.ConnectedUsers,
                AverageTimeScared = x.ConnectedUsers.Average(user => _halloweenService.GetTimedDifferenceBetweenScaredUser(user.Id).TotalMinutes)
            })
            .OrderBy(x => x.AverageTimeScared)
            .FirstOrDefault();
        
        IScheduler scheduler = await _schedulerFactory
            .GetScheduler(cancellationToken);
        if (channelToScare is null)
        {
            ITrigger trigger = TriggerBuilder
                .Create()
                .ForJob(JobKey.Create(nameof(SelectHalloweenChannelJob)))
                .StartAt(DateTimeOffset.Now.AddMinutes(30))
                .Build();

            await scheduler
                .ScheduleJob(trigger, cancellationToken);

            _logger.LogDebug("No channel to scare found");
            
            return;
        }

        _logger.LogDebug("Scaring channel {ChannelId} trying to connect", channelToScare.Channel.Id);
        await _audioService.ConnectToVoiceChannelAsync(channelToScare.Channel);

        var timeToWait = Random.Shared.Next(_config.Discord.Halloween.MinWaitTimeForScare, _config.Discord.Halloween.MaxWaitTimeForScare);
        
        _logger.LogDebug("Waiting {Time} minutes before playing scare", timeToWait);
        ITrigger triggerPlayHalloween = TriggerBuilder
            .Create()
            .ForJob(JobKey.Create(nameof(PlayHalloweenScareJob)))
            .StartAt(DateTimeOffset.Now.AddMinutes(timeToWait))
            .Build();
        
        await scheduler
            .ScheduleJob(triggerPlayHalloween, cancellationToken);
    }
}