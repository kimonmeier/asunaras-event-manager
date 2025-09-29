using Discord;
using MediatR;

namespace EventManager.Events.ActivityTop;

public class ActivityTopEvent : IRequest
{
    public required IInteractionContext Context { get; init; }
    
    public required DateTime Since { get; init; }
    
    public bool? IgnoreAfk { get; init; }
}