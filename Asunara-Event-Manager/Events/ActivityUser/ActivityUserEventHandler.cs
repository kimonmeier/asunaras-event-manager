using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;

namespace EventManager.Events.ActivityUser;

public class ActivityUserEventHandler : IRequestHandler<ActivityUserEvent>
{
    private readonly ILogger<ActivityUserEventHandler> _logger;
    private readonly ActivityEventRepository _activityEventRepository;

    public ActivityUserEventHandler(ILogger<ActivityUserEventHandler> logger, ActivityEventRepository activityEventRepository)
    {
        _logger = logger;
        _activityEventRepository = activityEventRepository;
    }

    public async Task Handle(ActivityUserEvent request, CancellationToken cancellationToken)
    {
        DateTime oneDay = DateTime.UtcNow.AddDays(-1);
        DateTime oneWeek = DateTime.UtcNow.AddDays(-7);
        DateTime twoWeeks = DateTime.UtcNow.AddDays(-14);

        var messageCountOneDay = await _activityEventRepository.GetMessageCountByDiscordId(request.User.Id, oneDay);
        var messageCountOneWeek = await _activityEventRepository.GetMessageCountByDiscordId(request.User.Id, oneWeek);
        var messageCountTwoWeeks = await _activityEventRepository.GetMessageCountByDiscordId(request.User.Id, twoWeeks);

        var voiceCountOneDay = await _activityEventRepository.GetVoiceCountByDiscordId(request.User.Id, oneDay, request.IgnoreAfk);
        var voiceCountOneWeek = await _activityEventRepository.GetVoiceCountByDiscordId(request.User.Id, oneWeek, request.IgnoreAfk);
        var voiceCountTwoWeeks = await _activityEventRepository.GetVoiceCountByDiscordId(request.User.Id, twoWeeks, request.IgnoreAfk);

        EmbedProperties builder = new EmbedProperties();
        builder.WithAuthor(new EmbedAuthorProperties()
                .WithName("Midnight-Café Event Manager")
            )
            .WithColor(new Color(0, 0, 255))
            .WithTitle("Aktivität auf dem Server!")
            .WithDescription($"Folgende Aktivität hat der User <@{request.User.Id}> auf dem Server!")
            .AddFields(new EmbedFieldProperties() { Name = "------------------------------------------", Value = "**Top Nachrichten**" })
            .AddFields(new EmbedFieldProperties() { Name = "**1 Tag:**", Value = messageCountOneDay.ToString() })
            .AddFields(new EmbedFieldProperties() { Name = "**7 Tage:**", Value = messageCountOneWeek.ToString() })
            .AddFields(new EmbedFieldProperties() { Name = "**14 Tage:**", Value = messageCountTwoWeeks.ToString() })
            .AddFields(new EmbedFieldProperties() { Name = "------------------------------------------", Value = "**Top Voice**" })
            .AddFields(new EmbedFieldProperties() { Name = "**1 Tag**", Value = $"{TimeSpan.FromMilliseconds(voiceCountOneDay).TotalHours:F2} Stunden" })
            .AddFields(new EmbedFieldProperties() { Name = "**7 Tage**", Value = $"{TimeSpan.FromMilliseconds(voiceCountOneWeek).TotalHours:F2} Stunden" })
            .AddFields(new EmbedFieldProperties() { Name = "**14 Tage**", Value = $"{TimeSpan.FromMilliseconds(voiceCountTwoWeeks).TotalHours:F2} Stunden" });


        await request.Context.Interaction.ModifyResponseAsync(x =>  x.AddEmbeds(builder), cancellationToken: cancellationToken);
    }
}