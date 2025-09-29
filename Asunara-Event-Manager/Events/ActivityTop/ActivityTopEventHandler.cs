using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.ActivityTop;

public class ActivityTopEventHandler : IRequestHandler<ActivityTopEvent>
{
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly RootConfig _config;
    private readonly DiscordSocketClient _client;

    public ActivityTopEventHandler(ActivityEventRepository activityEventRepository, RootConfig config, DiscordSocketClient client)
    {
        _activityEventRepository = activityEventRepository;
        _config = config;
        _client = client;
    }

    public async Task Handle(ActivityTopEvent request, CancellationToken cancellationToken)
    {
        var guild = _client.GetGuild(_config.Discord.MainDiscordServerId);
        
        var topMessages = await _activityEventRepository.GetTopMessagesSince(request.Since);
        var topVoices = await _activityEventRepository.GetTopVoiceSince(request.Since, request.IgnoreAfk ?? true);

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithAuthor(x =>
        {
            x.WithName("Midnight-Café Event Manager");
        })
        .WithColor(Color.Blue)
        .WithCurrentTimestamp()
        .WithTitle("Top Aktivität auf dem Server!")
        .WithDescription("Folgende Top-Aktivitäten gibt es auf dem Server!");


        builder.AddField("------------------------------------------", "**Top Nachrichten**");
        int index = 1;
        foreach (var topMessage in topMessages)
        {
            builder.AddField($"**Top {index}** - {guild.GetUser(topMessage.DiscordUserId).Username}", $"{topMessage.Count} Nachrichten");
            index++;
        }
        
        builder.AddField("------------------------------------------", "**Top Voice**");
        index = 1;
        foreach (var topVoice in topVoices)
        {
            builder.AddField($"**Top {index}** - {guild.GetUser(topVoice.DiscordUserId).Username}", $"{TimeSpan.FromMilliseconds(topVoice.Count).TotalHours:F2} Stunden");
            index++;
        }
        
        await request.Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = builder.Build());
    }
}