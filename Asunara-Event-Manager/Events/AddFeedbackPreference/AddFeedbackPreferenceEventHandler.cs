using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.AddFeedbackPreference;

public class AddFeedbackPreferenceEventHandler : IRequestHandler<AddFeedbackPreferenceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly UserPreferenceRepository _userPreferenceRepository;

    public AddFeedbackPreferenceEventHandler(DbTransactionFactory dbTransactionFactory, UserPreferenceRepository userPreferenceRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _userPreferenceRepository = userPreferenceRepository;
    }

    public async Task Handle(AddFeedbackPreferenceEvent request, CancellationToken cancellationToken)
    {
        var transaction = _dbTransactionFactory.CreateTransaction();
        
        var user = await _userPreferenceRepository.GetOrCreateByDiscordAsync(request.DiscordUserId);

        user.AllowReminderInPrivateMessage = request.Preference;

        await transaction.Commit(cancellationToken);
    }
}