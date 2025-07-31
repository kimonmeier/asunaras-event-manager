using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.QotdDeleted;

public class QotdDeletedEventHandler : IRequestHandler<QotdDeletedEvent>
{
    private readonly QotdQuestionRepository _repository;
    private readonly DbTransactionFactory _transactionFactory;

    public QotdDeletedEventHandler(QotdQuestionRepository repository, DbTransactionFactory transactionFactory)
    {
        _repository = repository;
        _transactionFactory = transactionFactory;
    }

    public async Task Handle(QotdDeletedEvent request, CancellationToken cancellationToken)
    {
        var transaction = _transactionFactory.CreateTransaction();

        await _repository.RemoveAsync(request.QuestionId);

        await transaction.Commit(cancellationToken);
    }
}