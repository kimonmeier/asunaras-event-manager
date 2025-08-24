using EventManager.Events.CheckReminder;
using MediatR;
using Quartz;

namespace EventManager.Background;

public class CheckEventReminderJob : IJob
{
    private readonly ISender _sender;

    public CheckEventReminderJob(ISender sender)
    {
        _sender = sender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _sender.Send(new CheckReminderEvent());
    }
}