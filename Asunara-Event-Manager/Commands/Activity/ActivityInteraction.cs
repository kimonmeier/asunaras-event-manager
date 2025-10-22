using EventManager.Events.ActivityCurrent;
using EventManager.Events.ActivityTop;
using EventManager.Events.ActivityUser;
using MediatR;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Commands.Activity;

[SlashCommand("activity", "Die Commands für die Aktivität der Nutzer",
    DefaultGuildPermissions = Permissions.SendPolls)]
public class ActivityInteraction : ApplicationCommandModule<SlashCommandContext>
{
    private readonly ISender _sender;

    public ActivityInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SubSlashCommand("top", "Die Top-Aktivität der User")]
    public async Task Top(bool ignoreAfk = true, bool ignoreTeamMembers = true, DateTime? since = null)
    {
        await _sender.Send(new ActivityTopEvent()
        {
            Context = Context,
            IgnoreAfk = ignoreAfk,
            IgnoreTeamMember = ignoreTeamMembers,
            Since = since ?? DateTime.MinValue,
        });
    }

    [SubSlashCommand("current", "Zeigt den aktuellen Status laut Aktivität an!")]
    public async Task Current(GuildUser user)
    {
        await _sender.Send(new ActivityCurrentEvent()
        {
            Context = Context, User = user,
        });
    }

    [SubSlashCommand("user", "Zeigt die Aktivität von einem User an")]
    public async Task User(GuildUser user, bool ignoreAfk = true, DateTime? since = null)
    {
        await _sender.Send(new ActivityUserEvent()
        {
            Context = Context, User = user, Since = since, IgnoreAfk = ignoreAfk
        });
    }
    
    [SubSlashCommand("me", "Zeigt die Aktivität von dir selber")]
    public async Task Me(bool ignoreAfk = true, DateTime? since = null)
    {
        await _sender.Send(new ActivityUserEvent()
        {
            Context = Context, User = Context.Guild.Users[Context.User.Id], Since = since, IgnoreAfk = ignoreAfk
        });
    }
}