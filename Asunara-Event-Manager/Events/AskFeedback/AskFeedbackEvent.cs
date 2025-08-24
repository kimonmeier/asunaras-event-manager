using MediatR;

namespace EventManager.Events.AskFeedback;

public class AskFeedbackEvent : IRequest
{
    public required Guid EventId { get; init; }
}