using MediatR;

namespace EventManager.Events.EventFeedbackVisibility;

public class EventFeedbackVisibilityEvent : IRequest
{
    public required ulong DiscordEventId { get; init; }
    
    public required ulong DiscordUserId { get; init; }
    
    public required bool Anonymous { get; init; }
}