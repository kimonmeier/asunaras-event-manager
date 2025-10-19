using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Entities.QOTD;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.QotdCheckQuestions;

public class QotdCheckQuestionsEventHandler : IRequestHandler<QotdCheckQuestionsEvent>
{
    private readonly ILogger<QotdCheckQuestionsEventHandler> _logger;
    private readonly DiscordSocketClient _client;
    private readonly QotdQuestionRepository _questionRepository;
    private readonly RootConfig _config;

    public QotdCheckQuestionsEventHandler(ILogger<QotdCheckQuestionsEventHandler> logger, DiscordSocketClient client, QotdQuestionRepository questionRepository, RootConfig config)
    {
        _logger = logger;
        _client = client;
        _questionRepository = questionRepository;
        _config = config;
    }

    public async Task Handle(QotdCheckQuestionsEvent request, CancellationToken cancellationToken)
    {
        QotdQuestion? unusedQuestion = await _questionRepository.GetUnusedQuestion();

        if (unusedQuestion is not null)
        {
            _logger.LogInformation("Checked QOTD-Question and there are unused questions");
            return;
        }

        SocketTextChannel textChannel = _client.GetGuild(_config.Discord.TeamDiscordServerId).GetTextChannel(_config.Discord.EventChatId);

        await textChannel.SendMessageAsync(
            "Achtung für den heutigen Tag gibt es keine QOTD-Fragen. Wenn bis zum Zeitpunkt vom Posten keine Fragen nachgereicht sind, wird eine vorhandene Frage gepostet!");
    }
}