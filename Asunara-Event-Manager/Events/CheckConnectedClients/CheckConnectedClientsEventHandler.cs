using Discord.WebSocket;
using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using EventManager.Events.CheckVoiceActivityForChannel;
using MediatR;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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

        foreach (SocketGuildUser connectedUser in request.ConnectedUsers)
        {
            await ProcessConnectedUser(connectedUser, channelIds, wronglyConnectedUserIds, cancellationToken);
        }

        await CheckChannelsForActivity(channelIds, cancellationToken);
        await MarkDisconnectedClients(wronglyConnectedUserIds, cancellationToken);
    }

    private async Task ProcessConnectedUser(SocketGuildUser connectedUser, HashSet<ulong> channelIds, HashSet<ulong> wronglyConnectedUserIds, CancellationToken cancellationToken)
    {
        var activity = await _activityEventRepository.GetLastVoiceActivityByDiscordId(connectedUser.Id);
        var voiceState = connectedUser.VoiceState!.Value;

        channelIds.Add(voiceState.VoiceChannel.Id);
        wronglyConnectedUserIds.Remove(connectedUser.Id);

        if (activity is null)
        {
            await CreateEntity(connectedUser.Id, voiceState.VoiceChannel.Id, cancellationToken);

            return;
        }

        if (activity.Type != ActivityType.VoiceChannelLeft && activity.ChannelId == voiceState.VoiceChannel.Id)
        {
            return;
        }

        await CreateEntity(connectedUser.Id, voiceState.VoiceChannel.Id, cancellationToken);
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