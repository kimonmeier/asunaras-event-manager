using MediatR;

namespace EventManager.Events.ThrowException;

public class ThrowExceptionEventHandler : IRequestHandler<ThrowExceptionEvent>
{
    public Task Handle(ThrowExceptionEvent request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}