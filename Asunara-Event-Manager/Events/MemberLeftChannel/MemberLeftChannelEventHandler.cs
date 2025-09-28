using EventManager.Events.CheckVoiceActivityForChannel;
using EventManager.Events.StopTrackingVoice;
using MediatR;

namespace EventManager.Events.MemberLeftChannel;

public class MemberLeftChannelEventHandler : IRequestHandler<MemberLeftChannelEvent>
{
    private readonly ISender _sender;

    public MemberLeftChannelEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(MemberLeftChannelEvent request, CancellationToken cancellationToken)
    {
        await _sender.Send(new StopTrackingVoiceEvent()
        {
            DiscordUserId = request.User.Id, DiscordChannelId = request.Channel.Id,
        }, cancellationToken);
        
        await _sender.Send(new CheckVoiceActivityForChannelEvent()
        {
            ChannelId = request.Channel.Id,
        }, cancellationToken);
    }
}