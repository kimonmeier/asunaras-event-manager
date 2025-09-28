using EventManager.Data;
using EventManager.Data.Entities.Activity;
using EventManager.Data.Repositories;
using EventManager.Events.StartTrackingVoice;
using MediatR;

namespace EventManager.Events.StopTrackingVoice;

public class StopTrackingVoiceEventHandler : IRequestHandler<StopTrackingVoiceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ActivityEventRepository _activityEventRepository;

    public StopTrackingVoiceEventHandler(DbTransactionFactory dbTransactionFactory, ActivityEventRepository activityEventRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _activityEventRepository = activityEventRepository;
    }

    public async Task Handle(StopTrackingVoiceEvent request, CancellationToken cancellationToken)
    {
        Transaction transaction = await _dbTransactionFactory.CreateTransaction();

        await _activityEventRepository.AddAsync(new ActivityEvent()
        {
            Type = ActivityType.VoiceChannelLeft, Date = DateTime.UtcNow, DiscordUserId = request.DiscordUserId, ChannelId = request.DiscordChannelId
        });
        
        await transaction.Commit(cancellationToken);
    }
}