using EventManager.Background;
using EventManager.Configuration;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using Quartz;

namespace EventManager.Events.SelectHalloweenChannel;

public class SelectHalloweenChannelEventHandler : IRequestHandler<SelectHalloweenChannelEvent>
{
    private readonly HalloweenService _halloweenService;
    private readonly AudioService _audioService;
    private readonly GatewayClient _client;
    private readonly RootConfig _config;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<SelectHalloweenChannelEventHandler> _logger;

    public SelectHalloweenChannelEventHandler(HalloweenService halloweenService, AudioService audioService,
        GatewayClient client, RootConfig config, ISchedulerFactory schedulerFactory,
        ILogger<SelectHalloweenChannelEventHandler> logger)
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
        var guild = _client.Cache.Guilds[_config.Discord.MainDiscordServerId];
        var voiceChannels = guild.VoiceStates
            .GroupBy(x => x.Value.ChannelId).Where(x => x.Key != null).Select(x => new { Channel = x.Key.Value, ConnectedUsers = x.ToList() })
            .ToList();

        // Remove channels that were recently scared
        voiceChannels
            .RemoveAll(channel => _halloweenService.GetTimedDifferenceBetweenScaredChannel(channel.Channel).TotalMinutes <=
                                  _config.Discord.Halloween.MinTimeBetweenScaresPerChannel);

        // Remove channels where a user was recently scared
        voiceChannels
            .RemoveAll(channel =>
                channel.ConnectedUsers.Any(user =>
                    _halloweenService.GetTimedDifferenceBetweenScaredUser(user.Key).TotalMinutes <=
                    _config.Discord.Halloween.MinTimeBetweenScaresPerUser));

        var channelToScare = voiceChannels
            .Select(x => new
            {
                x.Channel,
                x.ConnectedUsers,
                AverageTimeScared = x.ConnectedUsers.Average(user =>
                    _halloweenService.GetTimedDifferenceBetweenScaredUser(user.Key).TotalMinutes)
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
                .StartAt(DateTimeOffset.Now.AddMinutes(15))
                .Build();

            await scheduler
                .ScheduleJob(trigger, cancellationToken);

            _logger.LogDebug("No channel to scare found");

            return;
        }

        _logger.LogDebug("Scaring channel {ChannelId} trying to connect", channelToScare.Channel);
        await _audioService.ConnectToVoiceChannelAsync(guild.Id, channelToScare.Channel);

        var timeToWait = Random.Shared.Next(_config.Discord.Halloween.MinWaitTimeForScare,
            _config.Discord.Halloween.MaxWaitTimeForScare);

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