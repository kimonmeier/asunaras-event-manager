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
        List<ulong> channelIds = new List<ulong>();
        foreach (SocketGuildUser connectedUser in request.ConnectedUsers)
        {
            if (!connectedUser.VoiceState.HasValue)
            {
                continue;
            }

            var voiceState = connectedUser.VoiceState.Value;

            if (!channelIds.Contains(voiceState.VoiceChannel.Id))
            {
                channelIds.Add(voiceState.VoiceChannel.Id);
            }
            
            var activity = await _activityEventRepository.GetLastVoiceActivityByDiscordId(connectedUser.Id);

            if (activity is not null && activity.Type == ActivityType.VoiceChannelLeft)
            {
                continue;
            }
            
            Transaction transaction = await _dbTransactionFactory.CreateTransaction();

            await _activityEventRepository.AddAsync(new ActivityEvent()
            {
                Date = DateTime.UtcNow, Type = ActivityType.VoiceChannelJoined, ChannelId = voiceState.VoiceChannel.Id, DiscordUserId = connectedUser.Id
            });

            await transaction.Commit(cancellationToken);
        }

        foreach (ulong channelId in channelIds)
        {
            await _sender.Send(new CheckVoiceActivityForChannelEvent()
            {
                ChannelId = channelId
            }, cancellationToken);
        }
    }
}