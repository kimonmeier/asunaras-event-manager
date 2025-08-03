using Discord;
using Discord.WebSocket;
using EventManager.Data.Entities.Events;
using EventManager.Models.Restrictions;
using MediatR;

namespace EventManager.Events.CheckFskRestrictionOnUser;

public class CheckFskRestrictionOnUserEvent : IRequest<RestrictionCheckResult>
{
    public required DiscordEvent Event { get; set; }
    
    public required SocketGuildUser User { get; set; }
}