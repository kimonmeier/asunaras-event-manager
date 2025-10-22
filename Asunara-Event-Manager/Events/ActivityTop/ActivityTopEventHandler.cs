using EventManager.Configuration;
using EventManager.Data.Repositories;
using MediatR;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.ActivityTop;

public class ActivityTopEventHandler : IRequestHandler<ActivityTopEvent>
{
    private readonly ActivityEventRepository _activityEventRepository;
    private readonly RootConfig _config;
    private readonly GatewayClient _client;

    public ActivityTopEventHandler(ActivityEventRepository activityEventRepository, RootConfig config, GatewayClient client)
    {
        _activityEventRepository = activityEventRepository;
        _config = config;
        _client = client;
    }

    public async Task Handle(ActivityTopEvent request, CancellationToken cancellationToken)
    {
        var guild = _client.Cache.Guilds[_config.Discord.MainDiscordServerId];

        var topMessages = await _activityEventRepository.GetTopMessagesSince(request.Since);
        var topVoices = await _activityEventRepository.GetTopVoiceSince(request.Since, request.IgnoreAfk);

        if (request.IgnoreTeamMember)
        {
            Guild socketGuild = _client.Cache.Guilds[_config.Discord.MainDiscordServerId];

            topMessages.RemoveAll(x => socketGuild.Users.ContainsKey(x.DiscordUserId));
            topVoices.RemoveAll(x => socketGuild.Users.ContainsKey(x.DiscordUserId));
        }

        EmbedProperties builder = new EmbedProperties();
        builder.WithAuthor(new EmbedAuthorProperties()
                .WithName("Midnight-Café Event Manager"))
            .WithColor(new Color(0, 0, 255))
            .WithTitle("Top Aktivität auf dem Server!")
            .WithDescription("Folgende Top-Aktivitäten gibt es auf dem Server!");


        builder.AddFields(new EmbedFieldProperties() { Name = "------------------------------------------", Value = "**Top Nachrichten**" });
        int index = 1;
        foreach (var topMessage in topMessages.Take(10))
        {
            builder.AddFields(new EmbedFieldProperties() { Name = $"**Top {index}** - {guild.Users[topMessage.DiscordUserId].Username}", Value = $"{topMessage.Count} Nachrichten"});
            index++;
        }
        builder.AddFields(new EmbedFieldProperties() { Name = "------------------------------------------", Value = "**Top Voice**" });
        index = 1;
        foreach (var topVoice in topVoices.Take(10))
        {
            builder.AddFields(new EmbedFieldProperties() { Name = $"**Top {index}** - {guild.Users[topVoice.DiscordUserId].Username}", Value = $"{TimeSpan.FromMilliseconds(topVoice.Count).TotalHours:F2} Stunden"});
            index++;
        }

        await request.Context.Interaction.ModifyResponseAsync(x => x.AddEmbeds(builder), cancellationToken: cancellationToken);
    }
}