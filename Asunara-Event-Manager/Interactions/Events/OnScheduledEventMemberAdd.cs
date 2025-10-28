using EventManager.Events.MemberAddedEvent;
using MediatR;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnScheduledEventMemberAdd(ISender sender) : IGuildScheduledEventUserAddGatewayHandler
{
    public async ValueTask HandleAsync(GuildScheduledEventUserEventArgs eventArgs)
    {
        await sender.Send(new MemberAddedEventEvent()
        {
            EventId = eventArgs.GuildScheduledEventId,
            UserId = eventArgs.UserId,
            GuildId = eventArgs.GuildId,
        });
    }
}