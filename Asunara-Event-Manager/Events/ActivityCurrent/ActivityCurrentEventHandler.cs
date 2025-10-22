using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using MediatR;
using NetCord;
using NetCord.Rest;

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
        
        
        EmbedProperties embed = new EmbedProperties();
        embed.WithAuthor(new EmbedAuthorProperties()
        {
            Name = request.User.GlobalName,
            IconUrl = request.User.GetAvatarUrl(ImageFormat.Png)!.ToString()
        });
        embed.WithTitle("Aktivität");
        embed.WithDescription($"Der User {request.User.GlobalName} hat folgende Aktivität. {request.User.Id}");
        embed.WithColor(new Color(0, 255, 0));
        embed.AddFields(new EmbedFieldProperties()
        {
            Name = "**Nachrichten**", Value = messageCount.ToString()
        });
        embed.AddFields(new EmbedFieldProperties()
        {
            Name = "**Letzter Voice-Status**", Value = lastVoiceActivity?.Type.ToString() ?? "Unbekannt"
        });
        
        await request.Context.Interaction.ModifyResponseAsync(x => x.AddEmbeds(embed), cancellationToken: cancellationToken);
    }
}