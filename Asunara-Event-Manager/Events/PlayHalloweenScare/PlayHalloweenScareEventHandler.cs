using EventManager.Background;
using EventManager.Configuration;
using EventManager.Services;
using MediatR;
using Quartz;

namespace EventManager.Events.PlayHalloweenScare;

public class PlayHalloweenScareEventHandler : IRequestHandler<PlayHalloweenScareEvent>
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly AudioService _audioService;
    private readonly HalloweenService _halloweenService;
    private readonly RootConfig _config;

    public PlayHalloweenScareEventHandler(ISchedulerFactory schedulerFactory, AudioService audioService, HalloweenService halloweenService, RootConfig config)
    {
        _schedulerFactory = schedulerFactory;
        _audioService = audioService;
        _halloweenService = halloweenService;
        _config = config;
    }

    public async Task Handle(PlayHalloweenScareEvent request, CancellationToken cancellationToken)
    {
        var halloweenAudioFiles = _config.Discord.Halloween.AudioFiles;
        var halloweenAudioFile = halloweenAudioFiles[Random.Shared.Next(halloweenAudioFiles.Length - 1)];

        var connectedChannel = _audioService.GetConnectedVoiceChannel();

        if (connectedChannel is null)
        {
            await ScheduleCheck(cancellationToken);
            return;
        }

        var userIds = connectedChannel
            .ConnectedUsers
            .Select(x => x.Id)
            .ToArray();
        _halloweenService.Scared(connectedChannel.Id, userIds);
        
        await _audioService.PlayAudioAsync(halloweenAudioFile);
        await _audioService.DisconnectFromVoiceChannelAsync();
        
        await ScheduleCheck(cancellationToken);
    }

    private async Task ScheduleCheck(CancellationToken cancellationToken)
    {
        ITrigger trigger = TriggerBuilder
            .Create()
            .ForJob(JobKey.Create(nameof(SelectHalloweenChannelJob)))
            .StartAt(DateTimeOffset.Now.AddMinutes(30))
            .Build();

        IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await scheduler
            .ScheduleJob(trigger, cancellationToken);
    }
}