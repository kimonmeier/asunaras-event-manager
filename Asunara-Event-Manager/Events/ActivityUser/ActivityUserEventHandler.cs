using Discord;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

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

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithAuthor(x =>
            {
                x.WithName("Midnight-Café Event Manager");
            })
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithTitle("Aktivität auf dem Server!")
            .WithDescription($"Folgende Aktivität hat der User <@{request.User.Id}> gibt es auf dem Server!")
            .AddField("------------------------------------------", "**Top Nachrichten**")
            .AddField("**1 Tag:**", messageCountOneDay.ToString())
            .AddField("**7 Tage:**", messageCountOneWeek.ToString())
            .AddField("**14 Tage:**", messageCountTwoWeeks.ToString())
            .AddField("------------------------------------------", "**Top Voice**")
            .AddField($"**1 Tag**", $"{TimeSpan.FromMilliseconds(voiceCountOneDay).TotalHours:F2} Stunden")
            .AddField($"**7 Tage**", $"{TimeSpan.FromMilliseconds(voiceCountOneWeek).TotalHours:F2} Stunden")
            .AddField($"**14 Tage**", $"{TimeSpan.FromMilliseconds(voiceCountTwoWeeks).TotalHours:F2} Stunden");


        await request.Context.Interaction.ModifyOriginalResponseAsync(x =>  x.Embed = builder.Build());
    }
}