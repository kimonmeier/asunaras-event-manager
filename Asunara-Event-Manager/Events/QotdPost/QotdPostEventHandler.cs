using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Events.QOTD;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.QotdPost;

public class QotdPostEventHandler : IRequestHandler<QotdPostEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly RootConfig _config;
    private readonly ILogger<QotdPostEventHandler> _logger;
    private readonly DbTransactionFactory _transactionFactory;
    private readonly QotdQuestionRepository _questionRepository;
    private readonly QotdMessageRepository _messageRepository;

    public QotdPostEventHandler(DiscordSocketClient client, RootConfig config, ILogger<QotdPostEventHandler> logger, DbTransactionFactory transactionFactory, QotdQuestionRepository questionRepository, QotdMessageRepository messageRepository)
    {
        _client = client;
        _config = config;
        _logger = logger;
        _transactionFactory = transactionFactory;
        _questionRepository = questionRepository;
        _messageRepository = messageRepository;
    }

    public async Task Handle(QotdPostEvent request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_config.Discord.Qotd.Text))
        {
            _logger.LogError("The qotd message is empty");
            return;
        }
        
        SocketTextChannel channel = _client.GetGuild(_config.Discord.MainDiscordServerId).GetTextChannel(_config.Discord.Qotd.ChannelId);

        if (channel is null)
        {
            _logger.LogError("Could not find qotd channel on server {ServerId} with id {ChannelId}", _config.Discord.MainDiscordServerId, _config.Discord.Qotd.ChannelId);
            return;
        }

        var qotd = await FindQuestionOfTheDay();

        var qotdMessage = await channel.SendMessageAsync(string.Format(_config.Discord.Qotd.Text, qotd.Question));
        await channel.CreateThreadAsync(string.Format(_config.Discord.Qotd.ThreadTitle, DateOnly.FromDateTime(DateTime.Now).ToString("dd.MM.yyyy")), ThreadType.PublicThread, ThreadArchiveDuration.OneWeek, qotdMessage);

        using var dbTransaction = _transactionFactory.CreateTransaction();

        await _messageRepository.AddAsync(new QotdMessage()
        {
            QuestionId = qotd.Id, PostedOn = DateTime.UtcNow, MessageId = qotdMessage.Id,
        });

        await dbTransaction.Commit(cancellationToken);
    }

    private async Task<QotdQuestion> FindQuestionOfTheDay()
    {
        QotdQuestion? unusedQuestion = await _questionRepository.GetUnusedQuestion();

        if (unusedQuestion is not null)
        {
            return unusedQuestion;
        }

        if (!_config.Discord.Qotd.AllowReposts)
        {
            throw new Exception("Qotd message doesn't allow reposts and no new Questions are available");
        }

        QotdQuestion? leastQuestions = await _questionRepository.GetLeastQuestions();

        if (leastQuestions is not null)
        {
            return leastQuestions;
        }
        
        throw new Exception("Could not find least questions");
    }
}