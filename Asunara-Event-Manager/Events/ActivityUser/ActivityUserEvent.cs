using Discord;
using MediatR;

namespace EventManager.Events.ActivityUser;

public class ActivityUserEvent : IRequest
{
    public required IInteractionContext Context { get; init; }
    
    public required IUser User { get; init; }
    
    public DateTime? Since { get; init; }
    
    public required bool IgnoreAfk { get; init; }
}