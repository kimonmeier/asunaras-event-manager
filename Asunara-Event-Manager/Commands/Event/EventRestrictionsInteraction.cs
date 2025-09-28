using Discord.Interactions;
using EventManager.Events.AddFSKRestriction;
using MediatR;

namespace EventManager.Commands.Event;

[Group("event-restrictions", "Commands die das Event beeinflussen")]
public class EventRestrictionsInteraction : InteractionModuleBase
{
    private readonly ISender _sender;

    public EventRestrictionsInteraction(ISender sender)
    {
        _sender = sender;
    }

    [SlashCommand("fsk", "Fügt eine FSK Restriktion auf ein Event ein")]
    public async Task AddFskRestriction([Autocomplete(typeof(EventUncompletedAutocompleteHandler))] string eventId, int? maxAge = null, int? minAge =  null)
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