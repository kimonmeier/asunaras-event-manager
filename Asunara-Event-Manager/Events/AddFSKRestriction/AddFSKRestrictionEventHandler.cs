using System.ComponentModel.DataAnnotations;
using EventManager.Data;
using EventManager.Data.Entities.Restrictions;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.AddFSKRestriction;

public class AddFSKRestrictionEventHandler : IRequestHandler<AddFSKRestrictionEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly EventRestrictionRepository _eventRestrictionRepository;

    public AddFSKRestrictionEventHandler(DbTransactionFactory dbTransactionFactory, EventRestrictionRepository eventRestrictionRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _eventRestrictionRepository = eventRestrictionRepository;
    }

    public async Task Handle(AddFSKRestrictionEvent request, CancellationToken cancellationToken)
    {
        if (!request.MaxAge.HasValue && !request.MinAge.HasValue)
        {
            throw new ValidationException("Either MaxAge or MinAge must be specified");
        }

        var transaction = _dbTransactionFactory.CreateTransaction();

        await _eventRestrictionRepository.AddAsync(new FskRestrictions()
        {
            DiscordEventId = request.DiscordEventId, MaxAlter = request.MaxAge, MinAlter = request.MinAge,
        });
        
        await transaction.Commit(cancellationToken);
    }
}