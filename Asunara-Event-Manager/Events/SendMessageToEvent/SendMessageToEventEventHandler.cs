using EventManager.Configuration;
using EventManager.Data.Entities.Events;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using MediatR;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.SendMessageToEvent;

public class SendMessageToEventEventHandler : IRequestHandler<SendMessageToEventEvent>
{
    private readonly GatewayClient _client;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly RootConfig _config;
    
    public SendMessageToEventEventHandler(GatewayClient client, UserPreferenceRepository userPreferenceRepository, RootConfig config, DiscordEventRepository discordEventRepository)
    {
        _client = client;
        _userPreferenceRepository = userPreferenceRepository;
        _config = config;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(SendMessageToEventEvent request, CancellationToken cancellationToken)
    {
        List<UserPreference> userPreferences = await _userPreferenceRepository.ListAllAsync();
        var cacheGuild = _client.Cache.Guilds[_config.Discord.MainDiscordServerId];
        GuildUser guildUser = cacheGuild.Users[request.Author.Id];

        DiscordEvent? discordEvent = await _discordEventRepository.FindByEntityAsync(request.DiscordEventId);
        if (discordEvent is null)
        {
            throw new Exception("Event not found");
        }

        GuildScheduledEvent guildEvent = cacheGuild.ScheduledEvents[discordEvent.DiscordId];

        EmbedProperties embed = new EmbedProperties();        
        embed.WithTitle("Private Nachricht");
        embed.WithAuthor(new EmbedAuthorProperties().WithName(guildUser.GlobalName).WithIconUrl(guildUser.GetGuildAvatarUrl(ImageFormat.Png)!.ToString(512)));
        embed.WithDescription(request.Message);
        embed.WithColor(new Color(0, 0, 255));

        await foreach (GuildScheduledEventUser scheduledEventUser in guildEvent.GetUsersAsync().WithCancellation(cancellationToken))
        {
            UserPreference preference = (await _userPreferenceRepository.GetByDiscordAsync(scheduledEventUser.User.Id))!;

            if (!(preference.AllowReminderForEvent || preference.AllowReminderForFeedback))
            {
                continue;
            }

            DMChannel dmChannel = await scheduledEventUser.User.GetDMChannelAsync(cancellationToken: cancellationToken);
            
            await dmChannel.SendMessageAsync(new MessageProperties().AddEmbeds(embed), cancellationToken: cancellationToken);
        }
    }
}