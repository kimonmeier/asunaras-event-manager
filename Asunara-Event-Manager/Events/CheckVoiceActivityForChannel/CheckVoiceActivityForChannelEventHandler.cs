using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.CheckVoiceActivityForChannel;

public class CheckVoiceActivityForChannelEventHandler : IRequestHandler<CheckVoiceActivityForChannelEvent>
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly RootConfig _rootConfig;
    private readonly ILogger<CheckVoiceActivityForChannelEventHandler> _logger;

    public CheckVoiceActivityForChannelEventHandler(DiscordSocketClient discordSocketClient, DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository,
        RootConfig rootConfig, ILogger<CheckVoiceActivityForChannelEventHandler> logger)
    {
        _discordSocketClient = discordSocketClient;
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
        _rootConfig = rootConfig;
        _logger = logger;
    }

    public async Task Handle(CheckVoiceActivityForChannelEvent request, CancellationToken cancellationToken)
    {
        if (_rootConfig.Discord.Activity.ExcludedChannelsId.Contains(request.ChannelId))
        {
            _logger.LogInformation("Channel {ChannelId} is excluded from activity tracking", request.ChannelId);
            return;
        }
        
        SocketGuild guild = _discordSocketClient.GetGuild(_rootConfig.Discord.MainDiscordServerId);

        SocketGuildChannel guildChannel = guild.GetChannel(request.ChannelId);

        List<SocketGuildUser> users;
        if (guildChannel is SocketStageChannel stageChannel)
        {
            users = stageChannel.ConnectedUsers.ToList();
        } else if (guildChannel is SocketVoiceChannel voiceChannel)
        {
            users = voiceChannel.ConnectedUsers.ToList();
        }
        else
        {
            throw new Exception("Unknown Channel Type");
        }

        // If all Users are Deafened
        if (users.All(x => x.IsDeafened))
        {
            await MarkUserInactive(guildChannel.Id, users, cancellationToken);
            return;
        }

        // If all Users are Muted
        if (users.All(x =>
                x.IsMuted && x.Activities.All(activity => !_rootConfig.Discord.Activity.AllowedActivities.Contains(activity.Name, StringComparer.InvariantCultureIgnoreCase))))
        {
            await MarkUserInactive(guildChannel.Id, users, cancellationToken);
            return;
        }
        
        // If only one User is in the Channel
        if (users.Count == 1)
        {
            await MarkUserInactive(guildChannel.Id, users, cancellationToken);
            return;
        }
        
        // Otherwise mark Users as active!
        await MarkUserActive(guildChannel.Id, users, cancellationToken);
    }
    
    private async Task MarkUserInactive(ulong channelId, IList<SocketGuildUser> users, CancellationToken cancellationToken = default)
    {
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        foreach (var user in users)
        {
            var lastEntry = await _activityEventRepository.GetLastVoiceActivityByDiscordId(user.Id);

            if (lastEntry is null)
            {
                continue;
            }

            if (lastEntry.Type is ActivityType.VoiceChannelAfk)
            {
                continue;
            }

            await _activityEventRepository.AddAsync(new ActivityEvent()
            {
                Type = ActivityType.VoiceChannelAfk, Date = DateTime.UtcNow, DiscordUserId = user.Id, ChannelId = channelId
            });
        }
        
        await transaction.Commit(cancellationToken);
    }

    private async Task MarkUserActive(ulong channelId, IList<SocketGuildUser> users, CancellationToken cancellationToken = default)
    {
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();
        
        foreach (var user in users)
        {
            var lastEntry = await _activityEventRepository.GetLastVoiceActivityByDiscordId(user.Id);

            if (lastEntry?.Type is not ActivityType.VoiceChannelAfk)
            {
                continue;
            }
            
            await _activityEventRepository.AddAsync(new ActivityEvent()
            {
                Type = ActivityType.VoiceChannelNonAfk, Date = DateTime.UtcNow, DiscordUserId = user.Id, ChannelId = channelId
            });
        }
        
        await transaction.Commit(cancellationToken);
    }
}