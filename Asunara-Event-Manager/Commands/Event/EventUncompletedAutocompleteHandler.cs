using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Commands.Event;

public class EventUncompletedAutocompleteHandler : IAutocompleteProvider<AutocompleteInteractionContext>
{
    private readonly DiscordEventRepository _repository;

    public EventUncompletedAutocompleteHandler(DiscordEventRepository eventRepository)
    {
        _repository = eventRepository;
    }

    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        List<DiscordEvent> discordEvents = await _repository.GetAllUncompleted();

        List<ApplicationCommandOptionChoiceProperties> autocompleteResults = new List<ApplicationCommandOptionChoiceProperties>();
        foreach (DiscordEvent discordEvent in discordEvents.Where(x => x.Name.Contains(option.Value ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)))
        {
            autocompleteResults.Add(new ApplicationCommandOptionChoiceProperties(discordEvent.Name.WithMaxLength(100), discordEvent.Id.ToString()));
        }
        
        return autocompleteResults.Take(25);
    }
}