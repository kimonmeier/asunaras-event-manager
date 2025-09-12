using EventManager.Data;
using EventManager.Data.Entities.Events.QOTD;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.QotdCreated;

public class QotdCreatedEventHandler : IRequestHandler<QotdCreatedEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly QotdQuestionRepository _repository;
    private readonly ILogger<QotdCreatedEventHandler> _logger;
    
    public QotdCreatedEventHandler(DbTransactionFactory dbTransactionFactory, QotdQuestionRepository repository, ILogger<QotdCreatedEventHandler> logger)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(QotdCreatedEvent request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            _logger.LogError("Invalid question: {Question}", request.Question);
            return;
        }
        
        _logger.LogInformation("Starting request");

        using var transaction = await _dbTransactionFactory.CreateTransaction();;

        await _repository.AddAsync(new QotdQuestion()
        {
            Question = request.Question,
            AuthorId = request.AuthorId,
        });
        
        await transaction.Commit(cancellationToken);
        
        _logger.LogInformation("Ending request");
    }
}