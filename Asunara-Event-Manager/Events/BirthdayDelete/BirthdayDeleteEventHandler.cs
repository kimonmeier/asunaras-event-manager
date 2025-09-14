using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.BirthdayDelete;

public class BirthdayDeleteEventHandler : IRequestHandler<BirthdayDeleteEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly UserBirthdayRepository _birthdayRepository;

    public BirthdayDeleteEventHandler(DbTransactionFactory dbTransactionFactory, UserBirthdayRepository birthdayRepository)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _birthdayRepository = birthdayRepository;
    }

    public async Task Handle(BirthdayDeleteEvent request, CancellationToken cancellationToken)
    {
        using var transaction = await _dbTransactionFactory.CreateTransaction();
        await _birthdayRepository.DeleteByDiscordAsync(request.DiscordUserId);
        await transaction.Commit(cancellationToken);
    }
}