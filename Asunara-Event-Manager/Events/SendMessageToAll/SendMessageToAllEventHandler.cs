using EventManager.Configuration;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using MediatR;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.SendMessageToAll;

public class SendMessageToAllEventHandler : IRequestHandler<SendMessageToAllEvent>
{
    private readonly GatewayClient _client;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly RootConfig _config;
    
    public SendMessageToAllEventHandler(GatewayClient client, UserPreferenceRepository userPreferenceRepository, RootConfig config)
    {
        _client = client;
        _userPreferenceRepository = userPreferenceRepository;
        _config = config;
    }

    public async Task Handle(SendMessageToAllEvent request, CancellationToken cancellationToken)
    {
        List<UserPreference> userPreferences = await _userPreferenceRepository.ListAllAsync();
        var cacheGuild = _client.Cache.Guilds[_config.Discord.MainDiscordServerId];
        GuildUser guildUser = cacheGuild.Users[request.Author.Id];

        EmbedProperties embedBuilder = new EmbedProperties();
        embedBuilder.WithTitle("Private Nachricht");
        embedBuilder.WithAuthor(new EmbedAuthorProperties().WithName(guildUser.GlobalName).WithIconUrl(guildUser.GetGuildAvatarUrl(ImageFormat.Png)!.ToString(512)));
        embedBuilder.WithDescription(request.Message);
        embedBuilder.WithColor(new Color(0, 0, 255));

        foreach (UserPreference userPreference in userPreferences)
        {
            if (!(userPreference.AllowReminderForEvent || userPreference.AllowReminderForFeedback))
            {
                continue;
            }

            DMChannel dmChannel = await cacheGuild.Users[userPreference.DiscordUserId].GetDMChannelAsync(cancellationToken: cancellationToken);
            
            await dmChannel.SendMessageAsync(new MessageProperties().AddEmbeds(embedBuilder), cancellationToken: cancellationToken);
        }
    }
}