using MediatR;

namespace EventManager.Events.QotdSimilarQuestions;

public class QotdSimilarQuestionsEvent : IRequest<KeyValuePair<string, double>?>
{
    public required string Question { get; init; }
}