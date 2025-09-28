using MediatR;

namespace EventManager.Events.CheckVoiceActivityForChannel;

public class CheckVoiceActivityForChannelEvent : IRequest
{
    public required ulong ChannelId { get; init; }
}