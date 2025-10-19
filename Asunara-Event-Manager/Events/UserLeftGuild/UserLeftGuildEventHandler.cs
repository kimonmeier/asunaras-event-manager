using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.UserLeftGuild;

public class UserLeftGuildEventHandler : IRequestHandler<UserLeftGuildEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ILogger<UserLeftGuildEventHandler> _logger;
    private readonly DiscordUserRepository  _discordUserRepository;

    public UserLeftGuildEventHandler(DbTransactionFactory dbTransactionFactory, DiscordUserRepository discordUserRepository, ILogger<UserLeftGuildEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _discordUserRepository = discordUserRepository;
        _logger = logger;
    }

    public async Task Handle(UserLeftGuildEvent request, CancellationToken cancellationToken)
    {
        var user = await _discordUserRepository.GetByDiscordId(request.DiscordUserId);

        if (user is null)
        {
            _logger.LogError($"User {request.DiscordUserId} does not exist");
            return;
        }

        var transaction = await _dbTransactionFactory.CreateTransaction();
        await _discordUserRepository.RemoveAsync(user);
        await transaction.Commit(cancellationToken);
    }
}