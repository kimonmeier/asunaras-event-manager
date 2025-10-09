using EventManager.Events.SelectHalloweenChannel;
using MediatR;
using Quartz;

namespace EventManager.Background;

public class SelectHalloweenChannelJob : IJob
{
    private readonly ISender _sender;

    public SelectHalloweenChannelJob(ISender sender)
    {
        _sender = sender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _sender.Send(new SelectHalloweenChannelEvent(), context.CancellationToken);
    }
}