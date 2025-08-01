using EventManager.Events.QotdCheckQuestions;
using MediatR;
using Quartz;

namespace EventManager.Background;

public class QotdCheckQuestionsJob : IJob
{
    private readonly ISender _sender;

    public QotdCheckQuestionsJob(ISender sender)
    {
        _sender = sender;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return _sender.Send(new QotdCheckQuestionsEvent());
    }
}