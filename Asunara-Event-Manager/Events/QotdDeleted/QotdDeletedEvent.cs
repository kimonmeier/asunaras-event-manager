using MediatR;

namespace EventManager.Events.QotdDeleted;

public class QotdDeletedEvent : IRequest
{
    public required Guid QuestionId { get; set; }
}