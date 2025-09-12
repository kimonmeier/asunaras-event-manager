using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.AddReminderPreference;

public class AddReminderPreferenceEventHandler : IRequestHandler<AddReminderPreferenceEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly UserPreferenceRepository _userPreferenceRepository;

    public AddReminderPreferenceEventHandler(DbTransactionFactory dbTransactionFactory, UserPreferenceRepository userPreferenceRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _userPreferenceRepository = userPreferenceRepository;
    }

    public async Task Handle(AddReminderPreferenceEvent request, CancellationToken cancellationToken)
    {
        var transaction = await _dbTransactionFactory.CreateTransaction();;
        
        var user = await _userPreferenceRepository.GetOrCreateByDiscordAsync(request.DiscordUserId);

        user.AllowReminderForEvent = request.Preference;

        await transaction.Commit(cancellationToken);
    }
}