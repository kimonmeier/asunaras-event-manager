using EventManager.Data.Repositories;
using EventManager.Helper;
using MediatR;

namespace EventManager.Events.QotdSimilarQuestions;

public class QotdSimilarQuestionsEventHandler : IRequestHandler<QotdSimilarQuestionsEvent, Dictionary<string, double>>
{
    private readonly QotdQuestionRepository _qotdQuestionRepository;

    public QotdSimilarQuestionsEventHandler(QotdQuestionRepository qotdQuestionRepository)
    {
        _qotdQuestionRepository = qotdQuestionRepository;
    }

    public async Task<Dictionary<string, double>> Handle(QotdSimilarQuestionsEvent request, CancellationToken cancellationToken)
    {
        List<string> questions = await _qotdQuestionRepository.GetQuestionsText();
        
        return FuzzyComparer.GetSimilarities(request.Question, questions);
    }
}