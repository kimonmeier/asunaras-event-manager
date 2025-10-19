using Discord;
using Discord.WebSocket;
using EventManager.Data;
using EventManager.Data.Entities.Users;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.CheckExistingUsers;

public class CheckExistingUsersEventHandler : IRequestHandler<CheckExistingUsersEvent>
{
    private readonly ILogger<CheckExistingUsersEventHandler> _logger;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly DiscordUserRepository _discordUserRepository;

    public CheckExistingUsersEventHandler(ILogger<CheckExistingUsersEventHandler> logger,
        DbTransactionFactory dbTransactionFactory, DiscordUserRepository discordUserRepository)
    {
        _logger = logger;
        _dbTransactionFactory = dbTransactionFactory;
        _discordUserRepository = discordUserRepository;
    }

    public async Task Handle(CheckExistingUsersEvent request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CheckExistingUsers event");

        var existingUsers = await _discordUserRepository.ListAllAsync();

        foreach (var socketGuildUser in request.Users)
        {
            var existingUser = existingUsers.SingleOrDefault(x => x.DiscordUserId == socketGuildUser.Id);

            if (existingUser is not null)
            {
                existingUsers.Remove(existingUser);
                _logger.LogDebug("User {UserId} already exists", socketGuildUser.Id);
                continue;
            }

            _logger.LogDebug("User {UserId} doesn't exist", socketGuildUser.Id);
            await CreateUser(socketGuildUser, cancellationToken);
        }

        foreach (var existingUser in existingUsers)
        {
            _logger.LogDebug("User {UserId} exists but isn't on the discord", existingUser.Id);
            await DeleteUser(existingUser, cancellationToken);
        }
    }

    private async Task CreateUser(SocketGuildUser socketGuildUser, CancellationToken cancellationToken)
    {
        var transaction = await _dbTransactionFactory.CreateTransaction();
        await _discordUserRepository.AddAsync(new DiscordUser()
        {
            AvatarUrl = socketGuildUser.GetGuildAvatarUrl(ImageFormat.Auto, 256),
            DiscordUserId = socketGuildUser.Id,
            DisplayName = socketGuildUser.DisplayName,
            Username = socketGuildUser.Username,
        });

        await transaction.Commit(cancellationToken);
    }

    private async Task DeleteUser(DiscordUser discordUser, CancellationToken cancellationToken)
    {
        var transaction = await _dbTransactionFactory.CreateTransaction();
        await _discordUserRepository.RemoveAsync(discordUser);

        await transaction.Commit(cancellationToken);
    }
}