using MediatR;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Events.ActivityTop;

public class ActivityTopEvent : IRequest
{
    public required SlashCommandContext Context { get; init; }
    
    public required DateTime Since { get; init; }
    
    public bool IgnoreAfk { get; init; }
    
    public bool IgnoreTeamMember { get; init; }
}