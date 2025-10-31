using Microsoft.Extensions.DependencyInjection;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Quartz;

namespace EventManager.Interactions.Events;

public class OnReadyEvent(IServiceProvider provider) : IReadyGatewayHandler
{
    public async ValueTask HandleAsync(ReadyEventArgs arg)
    {
        ISchedulerFactory factory = provider.GetRequiredService<ISchedulerFactory>();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.Start();
    }
}