using Discord;
using MediatR;

namespace EventManager.Events.ForceFeedback;

public class ForceFeedbackEvent : IRequest
{
    public required Guid EventId { get; init; }
    
    public required IUser User { get; init; }
}