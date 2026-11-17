using EventManager.Configuration;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using EventManager.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.SendMessageToAll;

public class SendMessageToAllEventHandler : IRequestHandler<SendMessageToAllEvent>
{
    private readonly GatewayClient _client;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly RootConfig _config;
    private readonly ILogger<SendMessageToAllEventHandler> _logger;
    
    public SendMessageToAllEventHandler(GatewayClient client, UserPreferenceRepository userPreferenceRepository, RootConfig config, ILogger<SendMessageToAllEventHandler> logger)
    {
        _client = client;
        _userPreferenceRepository = userPreferenceRepository;
        _config = config;
        _logger = logger;
    }

    public async Task Handle(SendMessageToAllEvent request, CancellationToken cancellationToken)
    {
        List<UserPreference> userPreferences = await _userPreferenceRepository.ListAllAsync();
        var cacheGuild = _client.Cache.Guilds[_config.Discord.MainDiscordServerId];
        GuildUser? guildUser = await cacheGuild.GetOrFetchUserAsync(request.Author.Id, cancellationToken);
        ImageUrl? authorAvatarUrl = guildUser?.GetGuildAvatarUrl(ImageFormat.Png) ?? request.Author.GetAvatarUrl(ImageFormat.Png);

        EmbedProperties embedBuilder = new EmbedProperties();
        embedBuilder.WithTitle("Private Nachricht");
        embedBuilder.WithAuthor(new EmbedAuthorProperties().WithName(request.Author.GlobalName).WithIconUrl(authorAvatarUrl?.ToString(512)));
        embedBuilder.WithDescription(request.Message);
        embedBuilder.WithColor(new Color(0, 0, 255));

        foreach (UserPreference userPreference in userPreferences)
        {
            if (!(userPreference.AllowReminderForEvent || userPreference.AllowReminderForFeedback))
            {
                continue;
            }

            GuildUser? recipient = await cacheGuild.GetOrFetchUserAsync(userPreference.DiscordUserId, cancellationToken);

            if (recipient is null)
            {
                _logger.LogWarning("User {UserId} is no longer a member of the guild, skipping the message", userPreference.DiscordUserId);
                continue;
            }

            DMChannel dmChannel = await recipient.GetDMChannelAsync(cancellationToken: cancellationToken);
            
            await dmChannel.SendMessageAsync(new MessageProperties().AddEmbeds(embedBuilder), cancellationToken: cancellationToken);
        }
    }
}