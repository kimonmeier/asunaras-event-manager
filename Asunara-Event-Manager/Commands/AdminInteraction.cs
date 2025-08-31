using Discord;
using Discord.Interactions;
using EventManager.Events.ResetUserPreference;
using MediatR;

namespace EventManager.Commands;

[Group("admin", "Admin Commands für den ")]
public class AdminInteraction : InteractionModuleBase
{
    private readonly ISender _sender;

    public AdminInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SlashCommand("reset-user-preference", "Entfernt die User Preference")]
    public async Task ResetUserPreference(IUser user)
    {
        await _sender.Send(new ResetUserPreferenceEvent()
        {
            DiscordUserId = user.Id
        });
    }
    
}