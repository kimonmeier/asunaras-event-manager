using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using MediatR;

namespace EventManager.Events.ResetUserPreference;

public class ResetUserPreferenceEventHandler : IRequestHandler<ResetUserPreferenceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly RootConfig _config;
    private readonly DiscordSocketClient _client;
    private readonly ISender _sender;

    public ResetUserPreferenceEventHandler(DbTransactionFactory dbTransactionFactory, UserPreferenceRepository userPreferenceRepository, ISender sender, DiscordSocketClient client, RootConfig config)
    {
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
        
        await _sender.Send(new CheckForUserPreferenceOnEventInterestedEvent()
        {
            DiscordUser = _client.GetGuild(_config.Discord.MainDiscordServerId).GetUser(request.DiscordUserId),
        }, cancellationToken);
    }
}