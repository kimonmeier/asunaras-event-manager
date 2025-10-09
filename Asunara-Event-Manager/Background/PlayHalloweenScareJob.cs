using EventManager.Events.PlayHalloweenScare;
using MediatR;
using Quartz;

namespace EventManager.Background;

public class PlayHalloweenScareJob : IJob
{
    private readonly ISender _sender;

    public PlayHalloweenScareJob(ISender sender)
    {
        _sender = sender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _sender.Send(new PlayHalloweenScareEvent(), context.CancellationToken);
    }
}