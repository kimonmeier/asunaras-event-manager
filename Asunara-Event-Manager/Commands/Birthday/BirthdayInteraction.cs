using EventManager.Events.PostBirthdayMessage;
using MediatR;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Commands.Birthday;

[SlashCommand("birthday", "Gibt die Möglichkeit die Geburtstag einzusehen",
    DefaultGuildPermissions = Permissions.SendPolls)]
public class BirthdayInteraction : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ISender _sender;

    public BirthdayInteraction(ISender sender)
    {
        _sender = sender;
    }

    
    [SubSlashCommand("post-message", "Postet die Nachricht für die Verwaltung der Geburtstage")]
    public async Task PostBirthdayMessage(string channelId)
    {
        await _sender.Send(new PostBirthdayMessageEvent()
        {
            TextChannelId = ulong.Parse(channelId)
        });
    }
}