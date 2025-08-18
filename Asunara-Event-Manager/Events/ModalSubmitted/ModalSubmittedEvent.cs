using Discord.WebSocket;
using MediatR;

namespace EventManager.Events.ModalSubmitted;

public class ModalSubmittedEvent : IRequest
{
    public required SocketModal ModalData { get; init; }
}