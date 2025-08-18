using EventManager.Data.Entities.Events;
using MediatR;

namespace EventManager.Events.EventStartFeedback;

public class EventStartFeedbackEvent : IRequest
{
    public required DiscordEvent Event { get; set; }
}