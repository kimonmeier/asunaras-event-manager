using Discord;
using Discord.Interactions;
using EventManager.Events.QotdCreated;
using EventManager.Events.QotdDeleted;
using EventManager.Events.QotdPost;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Commands;

[Group("qotd", "Commands für die Question of the Day")]
[RequireRole(Konst.RoleTeamTeamDiscordId)]
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
        await DeferAsync();

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
        await DeferAsync();
        
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
        await DeferAsync();

        await _sender.Send(new QotdPostEvent());

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "Es wurde eine QOTD gepostet!";
        });
    }
    
}