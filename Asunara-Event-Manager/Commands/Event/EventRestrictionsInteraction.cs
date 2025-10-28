using EventManager.Events.AddFSKRestriction;
using EventManager.Extensions;
using MediatR;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Commands.Event;

[SlashCommand("event-restrictions", "Commands die das Event beeinflussen",
    DefaultGuildPermissions = Permissions.SendPolls, Contexts = [InteractionContextType.Guild])]
public class EventRestrictionsInteraction : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ISender _sender;

    public EventRestrictionsInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SubSlashCommand("fsk", "Fügt eine FSK Restriktion auf ein Event ein")]
    public async Task AddFskRestriction([SlashCommandParameter(AutocompleteProviderType = typeof(EventUncompletedAutocompleteHandler))] string eventId, int? maxAge = null,
        int? minAge = null)
    {
        await this.Deferred();
        
        await _sender.Send(new AddFSKRestrictionEvent()
        {
            DiscordEventId = Guid.Parse(eventId), MaxAge = maxAge, MinAge = minAge
        });

        await this.Answer("Die Restriktion wurde angewendet!");
    }
}