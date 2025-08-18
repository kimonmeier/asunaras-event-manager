using MediatR;

namespace EventManager.Events.EventExtendedFeedback;

public class EventExtendedFeedbackEvent : IRequest
{
    public required ulong DiscordEventId { get; init; }
 
    public required ulong DiscordUserId { get; init; }
    
    public string? Good { get; init; }
    
    public string? Critic { get; init; }
    
    public string? Suggestion { get; init; }
}