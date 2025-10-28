using EventManager.Events.EventDeleted;
using MediatR;
using NetCord;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnScheduledEventCancelled(ISender sender) : IGuildScheduledEventDeleteGatewayHandler
{
    public async ValueTask HandleAsync(GuildScheduledEvent scheduledEvent)
    {
        await sender.Send(new EventDeletedEvent()
        {
            DiscordId = scheduledEvent.Id
        });
    }
}