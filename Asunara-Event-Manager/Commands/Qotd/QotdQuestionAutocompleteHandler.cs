using EventManager.Data.Entities.Events.QOTD;
using EventManager.Data.Repositories;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Commands.Qotd;

public class QotdQuestionAutocompleteHandler : IAutocompleteProvider<AutocompleteInteractionContext>
{
    private readonly QotdQuestionRepository _repository;

    public QotdQuestionAutocompleteHandler(QotdQuestionRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        List<QotdQuestion> qotdQuestions = await _repository.ListAllAsync();

        List<ApplicationCommandOptionChoiceProperties> autocompleteResults = new List<ApplicationCommandOptionChoiceProperties>();
        foreach (QotdQuestion qotdQuestion in qotdQuestions.Where(x => x.Question.Contains(option.Value ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)))
        {
            autocompleteResults.Add(new ApplicationCommandOptionChoiceProperties(qotdQuestion.Question.WithMaxLength(100), qotdQuestion.Id.ToString()));
        }
        
        return autocompleteResults.Take(25);
    }
}