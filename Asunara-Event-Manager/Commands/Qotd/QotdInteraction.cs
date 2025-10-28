using System.Text;
using EventManager.Events.QotdCreated;
using EventManager.Events.QotdDeleted;
using EventManager.Events.QotdPost;
using EventManager.Events.QotdSimilarQuestions;
using EventManager.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Commands.Qotd;

[SlashCommand("qotd", "Commands für die Question of the Day",
    DefaultGuildPermissions = Permissions.SendPolls, Contexts = [InteractionContextType.Guild])]
public class QotdInteraction : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ISender _sender;
    private readonly ILogger<QotdInteraction> _logger;

    public QotdInteraction(ISender sender, ILogger<QotdInteraction> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [SubSlashCommand("add", "Adds a new question")]
    public async Task AddQuestion(string question)
    {
        await this.Deferred();
        
        await _sender.Send(new QotdCreatedEvent()
        {
            Question = question, AuthorId = Context.User.Id,
        });

        await ModifyResponseAsync(x =>
        {
            x.Content = "Die Frage wurde erfolgreich erstellt";
        });
    }

    [SubSlashCommand("remove", "Removes a question")]
    public async Task RemoveQuestion([SlashCommandParameter(AutocompleteProviderType = typeof(QotdQuestionAutocompleteHandler))] string questionId)
    {
        await this.Deferred();
        
        await _sender.Send(new QotdDeletedEvent()
        {
            QuestionId = Guid.Parse(questionId)
        });

        await ModifyResponseAsync(x =>
        {
            x.Content = "Die Frage wurde erfolgreich gelöscht";
        });
    }

    [SubSlashCommand("post", "Posts a QOTD")]
    public async Task PostQuestion()
    {
        await this.Deferred();
        
        await _sender.Send(new QotdPostEvent());

        await ModifyResponseAsync(x =>
        {
            x.Content = "Es wurde eine QOTD gepostet!";
        });
    }

    [SubSlashCommand("similar", "Checkt die Übereinstimmung zu vorhandenen Fragen")]
    public async Task CheckSimilarQuestion(string question)
    {
        await this.Deferred();
        
        var similarity = await _sender.Send(new QotdSimilarQuestionsEvent()
        {
            Question = question
        });


        if (!similarity.Any())
        {
            await ModifyResponseAsync(x =>
            {
                x.Content = $"Es wurde keine ähnliche Frage gefunden";
            });

            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Folgende Fragen wurde gefunden: ");

        foreach (var (key, value) in similarity)
        {
            builder.AppendLine($"{key} - {(value * 100):F}%");       
        }
        
        await ModifyResponseAsync(x =>
        {
            x.Content = builder.ToString();
        });
    }
}