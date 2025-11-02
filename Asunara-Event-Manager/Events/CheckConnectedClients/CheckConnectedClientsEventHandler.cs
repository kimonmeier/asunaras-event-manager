using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using EventManager.Events.CheckVoiceActivityForChannel;
using MediatR;
using NetCord;
using NetCord.Gateway;

namespace EventManager.Events.CheckConnectedClients;

public class CheckConnectedClientsEventHandler : IRequestHandler<CheckConnectedClientsEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly ISender _sender;

    public CheckConnectedClientsEventHandler(DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository, ISender sender)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
        _sender = sender;
    }


    public async Task Handle(CheckConnectedClientsEvent request, CancellationToken cancellationToken)
    {
        var channelIds = new HashSet<ulong>();
        var wronglyConnectedUserIds = (await _activityEventRepository.GetUserIdsCurrentlyConnectedToVoiceChannels()).ToHashSet();

        foreach (GuildUser connectedUser in request.ConnectedUsers)
        {
            await ProcessConnectedUser(connectedUser, channelIds, wronglyConnectedUserIds, cancellationToken);
        }

        await CheckChannelsForActivity(channelIds, cancellationToken);
        await MarkDisconnectedClients(wronglyConnectedUserIds, cancellationToken);
    }

    private async Task ProcessConnectedUser(GuildUser connectedUser, HashSet<ulong> channelIds, HashSet<ulong> wronglyConnectedUserIds, CancellationToken cancellationToken)
    {
        var activity = await _activityEventRepository.GetLastVoiceActivityByDiscordId(connectedUser.Id);
        VoiceState? voiceState = null;
        try
        {
            voiceState = await connectedUser.GetVoiceStateAsync(cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            return;
        }

        if (voiceState?.ChannelId is null)
        {
            return;
        }
        
        var voiceChannelId = voiceState.ChannelId.Value;
        
        
        channelIds.Add(voiceState.ChannelId.Value);
        wronglyConnectedUserIds.Remove(connectedUser.Id);

        if (activity is null)
        {
            await CreateEntity(connectedUser.Id, voiceChannelId, cancellationToken);

            return;
        }

        if (activity.Type != ActivityType.VoiceChannelLeft && activity.ChannelId == voiceChannelId)
        {
            return;
        }

        await CreateEntity(connectedUser.Id, voiceChannelId, cancellationToken);
    }

    private async Task CreateEntity(ulong discordUserId, ulong channelId, CancellationToken cancellationToken)
    {
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Date = DateTime.UtcNow, Type = ActivityType.VoiceChannelJoined, ChannelId = channelId, DiscordUserId = discordUserId
        });

        await transaction.Commit(cancellationToken);
    }

    private async Task MarkDisconnectedClients(HashSet<ulong> users, CancellationToken cancellationToken = default)
    {
        Transaction transactionConnected = await _dbTransactionFactory.CreateTransaction();
        foreach (var connectedClient in users)
        {
            ActivityEvent lastVoiceActivityByDiscordId = (await _activityEventRepository.GetLastVoiceActivityByDiscordId(connectedClient))!;
            await _activityEventRepository.AddAsync(new ActivityEvent()
            {
                Date = DateTime.UtcNow, Type = ActivityType.VoiceChannelLeft, ChannelId = lastVoiceActivityByDiscordId.ChannelId, DiscordUserId = connectedClient
            });
        }

        await transactionConnected.Commit(cancellationToken);
    }

    private async Task CheckChannelsForActivity(HashSet<ulong> channelIds, CancellationToken cancellationToken = default)
    {
        foreach (ulong channelId in channelIds)
        {
            await _sender.Send(new CheckVoiceActivityForChannelEvent()
            {
                ChannelId = channelId
            }, cancellationToken);
        }
    }
}