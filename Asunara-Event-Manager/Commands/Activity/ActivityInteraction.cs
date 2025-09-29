using Discord;
using Discord.Interactions;
using EventManager.Events.ActivityCurrent;
using EventManager.Events.ActivityTop;
using EventManager.Events.ActivityUser;
using MediatR;

namespace EventManager.Commands.Activity;

[RequireUserPermission(GuildPermission.SendPolls)]
[Group("activity", "Die Commands für die Aktivität der Nutzer")]
public class ActivityInteraction : InteractionModuleBase
{
    private readonly ISender _sender;

    public ActivityInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SlashCommand("top", "Die Top-Aktivität der User")]
    public async Task Top([Summary(description: "Ignoriere die Zeit die User AFK im Channel verbracht haben!")]bool ignoreAfk = true, DateTime? since = null)
    {
        await _sender.Send(new ActivityTopEvent()
        {
            Context = Context,
            IgnoreAfk = ignoreAfk,
            Since = since ?? DateTime.MinValue,
        });
    }

    [SlashCommand("current", "Zeigt den aktuellen Status laut Aktivität an!")]
    public async Task Current(IUser user)
    {
        await _sender.Send(new ActivityCurrentEvent()
        {
            Context = Context, User = user,
        });
    }

    [SlashCommand("user", "Zeigt die Aktivität von einem User an")]
    public async Task User(IUser user, bool ignoreAfk = true, DateTime? since = null)
    {
        await _sender.Send(new ActivityUserEvent()
        {
            Context = Context, User = user, Since = since, IgnoreAfk = ignoreAfk
        });
    }
    
    [SlashCommand("me", "Zeigt die Aktivität von dir selber")]
    public async Task Me(bool ignoreAfk = true, DateTime? since = null)
    {
        await _sender.Send(new ActivityUserEvent()
        {
            Context = Context, User = Context.User, Since = since, IgnoreAfk = ignoreAfk
        });
    }
}