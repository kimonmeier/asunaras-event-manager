using EventManager.Data.Entities.Events;
using EventManager.Models.Restrictions;
using MediatR;
using NetCord;

namespace EventManager.Events.CheckFskRestrictionOnUser;

public class CheckFskRestrictionOnUserEvent : IRequest<RestrictionCheckResult>
{
    public required DiscordEvent Event { get; set; }
    
    public required GuildUser User { get; set; }
}