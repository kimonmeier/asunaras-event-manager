using MediatR;
using NetCord;

namespace EventManager.Events.ForceFeedback;

public class ForceFeedbackEvent : IRequest
{
    public required Guid EventId { get; init; }
    
    public required GuildUser User { get; init; }
}