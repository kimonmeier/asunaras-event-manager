using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using EventManager.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;

namespace EventManager.Events.ResetUserPreference;

public class ResetUserPreferenceEventHandler : IRequestHandler<ResetUserPreferenceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly RootConfig _config;
    private readonly GatewayClient _client;
    private readonly ISender _sender;
    private readonly ILogger<ResetUserPreferenceEventHandler> _logger;

    public ResetUserPreferenceEventHandler(DbTransactionFactory dbTransactionFactory, UserPreferenceRepository userPreferenceRepository, ISender sender, GatewayClient client, RootConfig config, ILogger<ResetUserPreferenceEventHandler> logger)
    {
        _logger = logger;
        _dbTransactionFactory = dbTransactionFactory;
        _userPreferenceRepository = userPreferenceRepository;
        _sender = sender;
        _client = client;
        _config = config;
    }

    public async Task Handle(ResetUserPreferenceEvent request, CancellationToken cancellationToken)
    {
        UserPreference? preference = await _userPreferenceRepository.GetByDiscordAsync(request.DiscordUserId);

        if (preference is null)
        {
            return;
        }

        Transaction transaction = await _dbTransactionFactory.CreateTransaction();;

        await _userPreferenceRepository.RemoveAsync(preference);
        
        await transaction.Commit(cancellationToken);

        GuildUser? guildUser = await _client.Cache.Guilds[_config.Discord.MainDiscordServerId]
            .GetOrFetchUserAsync(request.DiscordUserId, cancellationToken);

        if (guildUser is null)
        {
            _logger.LogWarning("User {UserId} is no longer a member of the guild, skipping the preference check",
                request.DiscordUserId);

            return;
        }
        
        await _sender.Send(new CheckForUserPreferenceOnEventInterestedEvent()
        {
            DiscordUser = guildUser,
        }, cancellationToken);
    }
}