using Discord;
using Discord.Interactions;
using EventManager.Events.PostBirthdayMessage;
using MediatR;

namespace EventManager.Commands.Birthday;

[RequireUserPermission(GuildPermission.SendPolls)]
[Group("birthday", "Gibt die Möglichkeit die Geburtstag einzusehen")]
public class BirthdayInteraction : InteractionModuleBase
{
    private readonly ISender _sender;

    public BirthdayInteraction(ISender sender)
    {
        _sender = sender;
    }

    
    [SlashCommand("post-message", "Postet die Nachricht für die Verwaltung der Geburtstage")]
    public async Task PostBirthdayMessage(string channelId)
    {
        await _sender.Send(new PostBirthdayMessageEvent()
        {
            TextChannelId = ulong.Parse(channelId)
        });
    }
    
    
}