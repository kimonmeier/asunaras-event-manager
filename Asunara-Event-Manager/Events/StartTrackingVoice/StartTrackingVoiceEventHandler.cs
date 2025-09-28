using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.StartTrackingVoice;

public class StartTrackingVoiceEventHandler : IRequestHandler<StartTrackingVoiceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;

    public StartTrackingVoiceEventHandler(DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
    }

    public async Task Handle(StartTrackingVoiceEvent request, CancellationToken cancellationToken)
    {
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Type = ActivityType.VoiceChannelJoined, Date = DateTime.UtcNow, DiscordUserId = request.DiscordUserId, ChannelId = request.DiscordChannelId
        });
        
        await transaction.Commit(cancellationToken);
    }
}