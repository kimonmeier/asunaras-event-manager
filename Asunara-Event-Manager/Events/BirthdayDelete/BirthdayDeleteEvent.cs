using MediatR;

namespace EventManager.Events.BirthdayDelete;

public class BirthdayDeleteEvent : IRequest
{
    public required ulong DiscordUserId { get; init; }
    
}