using MediatR;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Events.ActivityUser;

public class ActivityUserEvent : IRequest
{
    public required ApplicationCommandContext Context { get; init; }
    
    public required GuildUser User { get; init; }
    
    public DateOnly? Since { get; init; }
    
    public required bool IgnoreAfk { get; init; }
}