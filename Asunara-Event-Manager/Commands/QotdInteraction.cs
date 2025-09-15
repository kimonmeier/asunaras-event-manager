using Discord;
using Discord.Interactions;
using EventManager.Events.QotdCreated;
using EventManager.Events.QotdDeleted;
using EventManager.Events.QotdPost;
using EventManager.Events.QotdSimilarQuestions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Commands;

[Group("qotd", "Commands für die Question of the Day")]
public class QotdInteraction : InteractionModuleBase
{
    private readonly ISender _sender;
    private readonly ILogger<QotdInteraction> _logger;
    
    public QotdInteraction(ISender sender, ILogger<QotdInteraction> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [SlashCommand("add", "Adds a new question")]
    public async Task AddQuestion(string question)
    {
        await _sender.Send(new QotdCreatedEvent()
        {
            Question = question,
            AuthorId = Context.User.Id,
        });
        
        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "Die Frage wurde erfolgreich erstellt";
        });
    }

    [SlashCommand("remove", "Removes a question")]
    public async Task RemoveQuestion([Autocomplete(typeof(QotdQuestionAutocompleteHandler))] string questionId)
    {
        await _sender.Send(new QotdDeletedEvent()
        {
            QuestionId = Guid.Parse(questionId)
        });

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "Die Frage wurde erfolgreich gelöscht";
        });
    }

    [SlashCommand("post", "Posts a QOTD")]
    public async Task PostQuestion()
    {
        await _sender.Send(new QotdPostEvent());

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "Es wurde eine QOTD gepostet!";
        });
    }

    [SlashCommand("similar", "Checkt die Übereinstimmung zu vorhandenen Fragen")]
    public async Task CheckSimilarQuestion(string question)
    {
        var similarity = await _sender.Send(new QotdSimilarQuestionsEvent()
        {
            Question = question
        });

        if (similarity == null)
        {
            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = $"Es wurde keine ähnliche Frage gefunden";
            });
        }
        else
        {
            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = $"Die Frage \"{similarity.Value.Key}\" wurde mit einer Übereinstimmung von {(similarity.Value.Value * 100):F}% gefunden";
            });
        }
    }
}