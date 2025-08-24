using Discord;
using Discord.Interactions;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace EventManager.Commands;

public class EventUncompletedAutocompleteHandler : AutocompleteHandler
{
    private readonly DiscordEventRepository _repository;

    public EventUncompletedAutocompleteHandler(DiscordEventRepository eventRepository)
    {
        _repository = eventRepository;
    }

    public async override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        List<DiscordEvent> discordEvents = await _repository.GetAllUncompleted();

        List<AutocompleteResult> autocompleteResults = new List<AutocompleteResult>();
        foreach (DiscordEvent discordEvent in discordEvents.Where(x => x.Name.Contains(autocompleteInteraction.Data.Current.Value as string ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)))
        {
            autocompleteResults.Add(new AutocompleteResult(discordEvent.Name.WithMaxLength(100), discordEvent.Id.ToString()));
        }
        
        return AutocompletionResult.FromSuccess(autocompleteResults.Take(25));
    }
}