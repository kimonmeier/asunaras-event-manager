using EventManager.Data.Repositories;
using EventManager.Helper;
using MediatR;

namespace EventManager.Events.QotdSimilarQuestions;

public class QotdSimilarQuestionsEventHandler : IRequestHandler<QotdSimilarQuestionsEvent, KeyValuePair<string, double>?>
{
    private readonly QotdQuestionRepository _qotdQuestionRepository;

    public QotdSimilarQuestionsEventHandler(QotdQuestionRepository qotdQuestionRepository)
    {
        _qotdQuestionRepository = qotdQuestionRepository;
    }

    public async Task<KeyValuePair<string, double>?> Handle(QotdSimilarQuestionsEvent request, CancellationToken cancellationToken)
    {
        List<string> questions = await _qotdQuestionRepository.GetQuestionsText();
        
        var similarity = FuzzyComparer.GetSimilarities(request.Question, questions);

        return similarity;
    }
}