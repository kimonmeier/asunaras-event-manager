using Discord;
using MediatR;

namespace EventManager.Events.ActivityCurrent;

public class ActivityCurrentEvent : IRequest
{
    public required IInteractionContext Context { get; init; }
    
    public required IUser User { get; init; }
}