using EventManager.Events.CheckBirthday;
using MediatR;
using Quartz;

namespace EventManager.Background;

public class CheckBirthdayJob : IJob
{
    private readonly ISender _sender;

    public CheckBirthdayJob(ISender sender)
    {
        _sender = sender;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return _sender.Send(new CheckBirthdayEvent());
    }
}