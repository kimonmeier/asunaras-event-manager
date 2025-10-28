using EventManager.Events.EventCreated;
using MediatR;
using NetCord;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnScheduledEventCreated(ISender sender) : IGuildScheduledEventCreateGatewayHandler
{
    public async ValueTask HandleAsync(GuildScheduledEvent scheduledEvent)
    {
        await sender.Send(new EventCreatedEvent()
        {
            DiscordId = scheduledEvent.Id,
            EventName = scheduledEvent.Name,
            UtcDatum = scheduledEvent.ScheduledStartTime.UtcDateTime
        });
    }
}