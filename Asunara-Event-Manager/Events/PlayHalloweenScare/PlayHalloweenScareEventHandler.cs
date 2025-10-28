using EventManager.Background;
using EventManager.Configuration;
using EventManager.Extensions;
using EventManager.Services;
using MediatR;
using NetCord.Gateway;
using Quartz;

namespace EventManager.Events.PlayHalloweenScare;

public class PlayHalloweenScareEventHandler : IRequestHandler<PlayHalloweenScareEvent>
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly AudioService _audioService;
    private readonly HalloweenService _halloweenService;
    private readonly RootConfig _config;
    private readonly GatewayClient _client;

    public PlayHalloweenScareEventHandler(ISchedulerFactory schedulerFactory, AudioService audioService,
        HalloweenService halloweenService, RootConfig config, GatewayClient client)
    {
        _schedulerFactory = schedulerFactory;
        _audioService = audioService;
        _halloweenService = halloweenService;
        _config = config;
        _client = client;
    }

    public async Task Handle(PlayHalloweenScareEvent request, CancellationToken cancellationToken)
    {
        var halloweenAudioFiles = _config.Discord.Halloween.AudioFiles;
        var halloweenAudioFile = halloweenAudioFiles[Random.Shared.Next(halloweenAudioFiles.Length - 1)];

        var connectedChannelId = _audioService.GetConnectedVoiceChannelId();

        if (connectedChannelId is null)
        {
            await ScheduleCheck(cancellationToken);
            return;
        }

        var userIds = _client.Cache.Guilds[_config.Discord.MainDiscordServerId]
            .GetConnectedUsers(connectedChannelId.Value)
            .Select(x => x.Id)
            .ToArray();
        _halloweenService.Scared(connectedChannelId.Value, userIds);

        await _audioService.PlayAudioAsync(halloweenAudioFile);
        await _audioService.DisconnectFromVoiceChannelAsync();

        await ScheduleCheck(cancellationToken);
    }

    private async Task ScheduleCheck(CancellationToken cancellationToken)
    {
        ITrigger trigger = TriggerBuilder
            .Create()
            .ForJob(JobKey.Create(nameof(SelectHalloweenChannelJob)))
            .StartAt(DateTimeOffset.Now.AddMinutes(Random.Shared.Next(10, 300)))
            .Build();

        IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await scheduler
            .ScheduleJob(trigger, cancellationToken);
    }
}