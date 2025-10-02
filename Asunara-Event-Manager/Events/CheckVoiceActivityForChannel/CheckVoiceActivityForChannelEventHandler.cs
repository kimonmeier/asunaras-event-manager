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
        }
        else if (guildChannel is SocketVoiceChannel voiceChannel)
        {
            users = voiceChannel.ConnectedUsers.ToList();
        }
        else
        {
            throw new Exception("Unknown Channel Type");
        }

        _logger.LogDebug("Found {UsersCount} Users in Channel {ChannelId}", users.Count, request.ChannelId);
        _logger.LogDebug("The users found are: {Muted} muted, {SelfMuted} self muted", users.All(x => x.IsMuted), users.All(x => x.IsSelfMuted));
        _logger.LogDebug("The users found are: {Muted} deafened, {SelfMuted} self deafened", users.All(x => x.IsDeafened), users.All(x => x.IsSelfDeafened));

        // If all Users are Deafened
        if (users.All(x => x.IsSelfDeafened || x.IsDeafened))
        {
            _logger.LogDebug("All Users are Deafened");
            await MarkUserInactive(guildChannel.Id, users, cancellationToken);

            return;
        }

        // If all Users are Muted
        if (users.All(x => x.IsMuted || x.IsSelfMuted))
        {
            _logger.LogDebug("All Users are Muted");

            if (!users.Any(x => x.Activities.Any(activity => _rootConfig.Discord.Activity.AllowedActivities.Contains(activity.Name)))) {
                _logger.LogDebug("All Users are Muted and no activity is allowed");
                await MarkUserInactive(guildChannel.Id, users, cancellationToken);

                return;
            }
            
            _logger.LogDebug("There is at least one User that is allowed to be active");
        }

        // If only one User is in the Channel
        if (users.Count <= 1)
        {
            _logger.LogDebug("Only one User is in the Channel");
            await MarkUserInactive(guildChannel.Id, users, cancellationToken);

            return;
        }
        
        _logger.LogDebug("There are more than one User in the Channel");
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