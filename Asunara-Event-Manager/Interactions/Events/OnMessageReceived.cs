using EventManager.Events.MessageReceived;
using MediatR;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnMessageReceived(ISender sender) : IMessageCreateGatewayHandler
{
    public async ValueTask HandleAsync(Message message)
    {
        await sender.Send(new MessageReceivedEvent()
        {
            Message = message
        });
    }
}