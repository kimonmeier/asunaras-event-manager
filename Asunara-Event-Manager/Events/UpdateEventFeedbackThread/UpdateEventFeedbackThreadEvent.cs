using MediatR;

namespace EventManager.Events.UpdateEventFeedbackThread;

public class UpdateEventFeedbackThreadEvent : IRequest
{
    public required ulong DiscordEventId { get; init; }
}