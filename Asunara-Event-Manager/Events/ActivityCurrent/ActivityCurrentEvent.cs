using MediatR;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Events.ActivityCurrent;

public class ActivityCurrentEvent : IRequest
{
    public required ApplicationCommandContext Context { get; init; }
    
    public required GuildUser User { get; init; }
}