using MediatR;

namespace EventManager.Events.QotdSimilarQuestions;

public class QotdSimilarQuestionsEvent : IRequest<Dictionary<string, double>>
{
    public required string Question { get; init; }
}