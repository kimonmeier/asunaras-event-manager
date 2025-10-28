using MediatR;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Events.ActivityTop;

public class ActivityTopEvent : IRequest
{
    public required ApplicationCommandContext Context { get; init; }
    
    public required DateOnly Since { get; init; }
    
    public bool IgnoreAfk { get; init; }
    
    public bool IgnoreTeamMember { get; init; }
}