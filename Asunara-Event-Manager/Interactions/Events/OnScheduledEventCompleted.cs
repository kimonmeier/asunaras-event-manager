using EventManager.Events.EventCompleted;
using MediatR;
using NetCord;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnScheduledEventCompleted(ISender sender) : IGuildScheduledEventUpdateGatewayHandler
{
    public async ValueTask HandleAsync(GuildScheduledEvent arg)
    {
        if (arg.Status == GuildScheduledEventStatus.Completed)
        {
            await sender.Send(new EventCompletedEvent()
            {
                DiscordEventId = arg.Id
            });
            return;
        }
        
        
    }
}