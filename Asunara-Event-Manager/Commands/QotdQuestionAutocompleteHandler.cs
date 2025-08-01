using Discord;
using Discord.Interactions;
using EventManager.Data.Entities.QOTD;
using EventManager.Data.Repositories;

namespace EventManager.Commands;

public class QotdQuestionAutocompleteHandler : AutocompleteHandler
{
    private readonly QotdQuestionRepository _repository;

    public QotdQuestionAutocompleteHandler(QotdQuestionRepository repository)
    {
        _repository = repository;
    }

    public async override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        List<QotdQuestion> qotdQuestions = await _repository.ListAllAsync();

        List<AutocompleteResult> autocompleteResults = new List<AutocompleteResult>();
        foreach (QotdQuestion qotdQuestion in qotdQuestions.Where(x => x.Question.Contains(autocompleteInteraction.Data.Current.Value as string ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)))
        {
            autocompleteResults.Add(new AutocompleteResult(qotdQuestion.Question.WithMaxLength(100), qotdQuestion.Id.ToString()));
        }
        
        return AutocompletionResult.FromSuccess(autocompleteResults.Take(25));
    }
}