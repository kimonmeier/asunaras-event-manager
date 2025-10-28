using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using EventManager.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;

namespace EventManager.Events.CheckVoiceActivityForChannel;

public class CheckVoiceActivityForChannelEventHandler : IRequestHandler<CheckVoiceActivityForChannelEvent>
{
    private readonly GatewayClient _discordClient;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly RootConfig _rootConfig;
    private readonly ILogger<CheckVoiceActivityForChannelEventHandler> _logger;

    public CheckVoiceActivityForChannelEventHandler(GatewayClient discordClient, DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository,
        RootConfig rootConfig, ILogger<CheckVoiceActivityForChannelEventHandler> logger)
    {
        _discordClient = discordClient;
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

        Guild guild = _discordClient.Cache.Guilds[_rootConfig.Discord.MainDiscordServerId];

        IGuildChannel guildChannel = guild.Channels[request.ChannelId];

        if (guildChannel is null)
        {
            _logger.LogWarning("Could not find channel with id {ChannelId}", request.ChannelId);
            return;
        }
        
        List<VoiceState> users;
        if (guildChannel is VoiceGuildChannel stageChannel)
        {
            users = stageChannel.GetConnectedUsers(_discordClient);
        }
        else if (guildChannel is StageGuildChannel voiceChannel)
        {
            users = voiceChannel.GetConnectedUsers(_discordClient);
        } 
        else
        {
            throw new Exception($"Unknown Channel Type for Channel {request.ChannelId} with type {guildChannel.GetType().FullName}");
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
            _logger.LogDebug("All Users are Muted and no activity is allowed");
            await MarkUserInactive(guildChannel.Id, users, cancellationToken);
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

    private async Task MarkUserInactive(ulong channelId, IList<VoiceState> voiceStates, CancellationToken cancellationToken = default)
    {
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        foreach (var voiceState in voiceStates)
        {
            var lastEntry = await _activityEventRepository.GetLastVoiceActivityByDiscordId(voiceState.UserId);

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
                Type = ActivityType.VoiceChannelAfk, Date = DateTime.UtcNow, DiscordUserId = voiceState.UserId, ChannelId = channelId
            });
        }

        await transaction.Commit(cancellationToken);
    }

    private async Task MarkUserActive(ulong channelId, IList<VoiceState> voiceStates, CancellationToken cancellationToken = default)
    {
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        foreach (var voiceState in voiceStates)
        {
            var lastEntry = await _activityEventRepository.GetLastVoiceActivityByDiscordId(voiceState.UserId);

            if (lastEntry?.Type is not ActivityType.VoiceChannelAfk)
            {
                continue;
            }

            await _activityEventRepository.AddAsync(new ActivityEvent()
            {
                Type = ActivityType.VoiceChannelNonAfk, Date = DateTime.UtcNow, DiscordUserId = voiceState.UserId, ChannelId = channelId
            });
        }

        await transaction.Commit(cancellationToken);
    }
}