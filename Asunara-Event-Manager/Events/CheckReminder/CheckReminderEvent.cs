using MediatR;

namespace EventManager.Events.CheckReminder;

public class CheckReminderEvent : IRequest
{
    public DateTime? MinDate { get; init; }
}