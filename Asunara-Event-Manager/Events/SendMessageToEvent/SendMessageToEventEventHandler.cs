using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Entities.Events;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using EventManager.Events.SendMessageToAll;
using MediatR;

namespace EventManager.Events.SendMessageToEvent;

public class SendMessageToEventEventHandler : IRequestHandler<SendMessageToEventEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly RootConfig _config;
    
    public SendMessageToEventEventHandler(DiscordSocketClient client, UserPreferenceRepository userPreferenceRepository, RootConfig config, DiscordEventRepository discordEventRepository)
    {
        _client = client;
        _userPreferenceRepository = userPreferenceRepository;
        _config = config;
        _discordEventRepository = discordEventRepository;
    }

    public async Task Handle(SendMessageToEventEvent request, CancellationToken cancellationToken)
    {
        List<UserPreference> userPreferences = await _userPreferenceRepository.ListAllAsync();
        SocketGuildUser guildUser = _client.GetGuild(_config.Discord.MainDiscordServerId).GetUser(request.Author.Id);

        DiscordEvent? discordEvent = await _discordEventRepository.FindByEntityAsync(request.DiscordEventId);
        if (discordEvent is null)
        {
            throw new Exception("Event not found");
        }

        SocketGuildEvent guildEvent = _client.GetGuild(_config.Discord.MainDiscordServerId).GetEvent(discordEvent.DiscordId);

        EmbedBuilder embedBuilder = new EmbedBuilder();
        embedBuilder.WithTitle("Private Nachricht");
        embedBuilder.WithAuthor(guildUser.DisplayName, guildUser.GetGuildAvatarUrl());
        embedBuilder.WithDescription(request.Message);
        embedBuilder.AddField("Event", guildEvent.Name);
        embedBuilder.WithColor(Color.Blue);

        foreach (RestUser user in await guildEvent.GetUsersAsync(new RequestOptions()).FlattenAsync())
        {
            UserPreference preference = (await _userPreferenceRepository.GetByDiscordAsync(user.Id))!;

            if (!(preference.AllowReminderForEvent || preference.AllowReminderForFeedback))
            {
                continue;
            }

            IDMChannel dmChannel = await user.CreateDMChannelAsync();
            
            await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}