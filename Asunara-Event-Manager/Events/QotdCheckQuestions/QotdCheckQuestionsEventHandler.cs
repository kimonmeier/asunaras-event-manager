using EventManager.Configuration;
using EventManager.Data.Entities.Events.QOTD;
using EventManager.Data.Repositories;
using EventManager.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;

namespace EventManager.Events.QotdCheckQuestions;

public class QotdCheckQuestionsEventHandler : IRequestHandler<QotdCheckQuestionsEvent>
{
    private readonly ILogger<QotdCheckQuestionsEventHandler> _logger;
    private readonly GatewayClient _client;
    private readonly QotdQuestionRepository _questionRepository;
    private readonly RootConfig _config;

    public QotdCheckQuestionsEventHandler(ILogger<QotdCheckQuestionsEventHandler> logger, GatewayClient client, QotdQuestionRepository questionRepository, RootConfig config)
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

        TextChannel textChannel = _client.Cache.Guilds[_config.Discord.TeamDiscordServerId].GetTextChannel(_config.Discord.EventChatId);

        await textChannel.SendMessageAsync(
            "Achtung für den heutigen Tag gibt es keine QOTD-Fragen. Wenn bis zum Zeitpunkt vom Posten keine Fragen nachgereicht sind, wird eine vorhandene Frage gepostet!", cancellationToken: cancellationToken);
    }
}