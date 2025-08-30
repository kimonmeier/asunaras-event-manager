using MediatR;
using Sentry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManager.Behaviour;

public class SentryTracingBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Get the current span if one exists
        var parentSpan = SentrySdk.GetSpan();
        
        if (parentSpan != null)
        {
            return await ExecuteWithChildSpan(next, cancellationToken, parentSpan);
        }
        
        return await ExecuteRequestWithTransaction(next, cancellationToken);
    }

    private async static Task<TResponse> ExecuteRequestWithTransaction(RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var transaction = SentrySdk.StartTransaction(
            "mediatr.handle",
            typeof(TRequest).Name
        );

        // Set the transaction on the current scope
        SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);
        SentrySdk.AddBreadcrumb(typeof(TRequest).Name, "mediatr");;

        try
        {
            // Continue the pipeline
            var response = await next(cancellationToken);
            
            // If everything went well, finish the transaction with an OK status
            transaction.Finish(SpanStatus.Ok);
            
            return response;
        }
        catch (Exception e)
        {
            // If an exception occurs, finish the transaction with an error status
            transaction.Finish(e);
            
            // Re-throw the exception to not swallow it
            throw;
        }
    }

    private async static Task<TResponse> ExecuteWithChildSpan(RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken, ISpan parentSpan)
    {
        var childSpan = parentSpan.StartChild(
            "mediatr.handle",
            typeof(TRequest).Name
        );
            
        SentrySdk.ConfigureScope(scope => scope.Span = childSpan);
        SentrySdk.AddBreadcrumb(typeof(TRequest).Name, "mediatr");
        
        try
        {
            // Continue the pipeline with the child span active
            var response = await next(cancellationToken);
                
            // If everything went well, finish the child span with an OK status
            childSpan.Finish(SpanStatus.Ok);
                
            return response;
        }
        catch (Exception e)
        {
            // If an exception occurs, finish the child span with an error status
            childSpan.Finish(e);
                
            // Re-throw the exception to not swallow it
            throw;
        }
    }
}