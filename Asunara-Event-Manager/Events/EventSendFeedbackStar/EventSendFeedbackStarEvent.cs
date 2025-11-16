using MediatR;

namespace EventManager.Events.EventSendFeedbackStar;

public class EventSendFeedbackStarEvent : IRequest
{
    public required ulong DiscordEventId { get; init; }
    
    public required ulong DiscordUserId { get; init; }
    
    public required int StarCount { get; set; }
}