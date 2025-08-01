using MediatR;

namespace EventManager.Events.QotdCreated;

public class QotdCreatedEvent : IRequest
{
    public required string Question { get; set; }
    
    public required ulong AuthorId { get; set; }
}