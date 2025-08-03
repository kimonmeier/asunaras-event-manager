using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;
using Discord.Interactions;
using EventManager.Data.Repositories;
using EventManager.Events.AddFSKRestriction;
using MediatR;

namespace EventManager.Commands;

[Group("event-restrictions", "Commands die das Event beeinflussen")]
public class EventInteraction : InteractionModuleBase
{
    private readonly ISender _sender;

    public EventInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SlashCommand("fsk", "Fügt eine FSK Restriktion auf ein Event ein")]
    public async Task AddFskRestriction([Autocomplete(typeof(EventAutocompleteHandler))] string eventId, int? maxAge = null, int? minAge =  null)
    {
        await _sender.Send(new AddFSKRestrictionEvent()
        {
            DiscordEventId = Guid.Parse(eventId), MaxAge = maxAge, MinAge = minAge
        });
        
        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "Die Restriktion wurde erstellt";
        });
    }
}