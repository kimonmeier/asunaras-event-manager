using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.SendMessageToAll;

public class SendMessageToAllEventHandler : IRequestHandler<SendMessageToAllEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly RootConfig _config;
    
    public SendMessageToAllEventHandler(DiscordSocketClient client, UserPreferenceRepository userPreferenceRepository, RootConfig config)
    {
        _client = client;
        _userPreferenceRepository = userPreferenceRepository;
        _config = config;
    }

    public async Task Handle(SendMessageToAllEvent request, CancellationToken cancellationToken)
    {
        List<UserPreference> userPreferences = await _userPreferenceRepository.ListAllAsync();
        SocketGuildUser guildUser = _client.GetGuild(_config.Discord.MainDiscordServerId).GetUser(request.Author.Id);

        EmbedBuilder embedBuilder = new EmbedBuilder();
        embedBuilder.WithTitle("Private Nachricht");
        embedBuilder.WithAuthor(guildUser.DisplayName, guildUser.GetGuildAvatarUrl());
        embedBuilder.WithDescription(request.Message);
        embedBuilder.WithColor(Color.DarkBlue);

        foreach (UserPreference userPreference in userPreferences)
        {
            if (!(userPreference.AllowReminderForEvent || userPreference.AllowReminderForFeedback))
            {
                continue;
            }

            IDMChannel dmChannel = await _client.GetUser(userPreference.DiscordUserId).CreateDMChannelAsync();
            
            await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
        }


    }
}