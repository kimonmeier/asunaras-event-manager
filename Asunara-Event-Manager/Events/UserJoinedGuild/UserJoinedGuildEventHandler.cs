using Discord;
using Discord.WebSocket;
using EventManager.Data;
using EventManager.Data.Entities.Users;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.UserJoinedGuild;

public class UserJoinedGuildEventHandler : IRequestHandler<UserJoinedGuildEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly DiscordUserRepository _discordUserRepository;
    private readonly ILogger<UserJoinedGuildEventHandler> _logger;

    public UserJoinedGuildEventHandler(DbTransactionFactory dbTransactionFactory,
        DiscordUserRepository discordUserRepository, ILogger<UserJoinedGuildEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _discordUserRepository = discordUserRepository;
        _logger = logger;
    }

    public async Task Handle(UserJoinedGuildEvent request, CancellationToken cancellationToken)
    {
        var user = await _discordUserRepository.GetByDiscordId(request.GuildUser.Id);

        if (user is not null)
        {
            if (user.IsDeleted)
            {
                await UndeleteUser(user, cancellationToken);
            }

            return;
        }
        
        await CreateUser(request.GuildUser, cancellationToken);
    }

    private async Task UndeleteUser(DiscordUser user, CancellationToken cancellationToken)
    {
        var transaction = await _dbTransactionFactory.CreateTransaction();

        user.IsDeleted = true;

        await _discordUserRepository.UpdateAsync(user);
        await transaction.Commit(cancellationToken);
    }

    private async Task CreateUser(SocketGuildUser user, CancellationToken cancellationToken)
    {
        var transaction = await _dbTransactionFactory.CreateTransaction();

        await _discordUserRepository.AddAsync(new DiscordUser()
        {
            AvatarUrl = user.GetGuildAvatarUrl(ImageFormat.Auto, 256),
            DiscordUserId = user.Id,
            DisplayName = user.DisplayName,
            Username = user.Username,
        });
        
        await transaction.Commit(cancellationToken);
    }

}