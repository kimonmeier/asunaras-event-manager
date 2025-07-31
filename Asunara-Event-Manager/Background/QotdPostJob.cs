using EventManager.Events.QotdPost;
using MediatR;
using Quartz;

namespace EventManager.Background;

public class QotdPostJob : IJob
{
    private readonly ISender _sender;

    public QotdPostJob(ISender sender)
    {
        _sender = sender;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return _sender.Send(new QotdPostEvent());
    }
}