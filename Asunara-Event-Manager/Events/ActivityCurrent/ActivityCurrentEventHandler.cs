using Discord;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.ActivityCurrent;

public class ActivityCurrentEventHandler : IRequestHandler<ActivityCurrentEvent>
{
    private readonly ActivityEventRepository _activityEventRepository;

    public ActivityCurrentEventHandler(ActivityEventRepository activityEventRepository)
    {
        _activityEventRepository = activityEventRepository;
    }

    public async Task Handle(ActivityCurrentEvent request, CancellationToken cancellationToken)
    {
        ActivityEvent? lastVoiceActivity = await _activityEventRepository.GetLastVoiceActivityByDiscordId(request.User.Id);
        int messageCount = await _activityEventRepository.GetMessageCountByDiscordId(request.User.Id, DateTime.MinValue);
        
        EmbedBuilder builder = new EmbedBuilder();
        builder.WithAuthor(x =>
        {
            x.WithName(request.User.GlobalName);
            x.WithIconUrl(request.User.GetAvatarUrl());
        });
        builder.WithTitle("Aktivität");
        builder.WithDescription($"Der User {request.User.GlobalName} hat folgende Aktivität. {request.User.Mention}");
        builder.WithColor(Color.Green);
        builder.WithCurrentTimestamp();
        builder.AddField("**Nachrichten**", messageCount);
        builder.AddField("**Letzter Voice-Status**", lastVoiceActivity?.Type.ToString() ?? "Unbekannt");
        
        await request.Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = builder.Build());
    }
}