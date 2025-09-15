using MediatR;

namespace EventManager.Events.BirthdayCreated;

public class BirthdayCreatedEvent : IRequest<bool>
{
    public required ulong DiscordUserId { get; init; }
    
    public required int Day { get; init; }

    public required int Month { get; init; }
    
    public required int? Year { get; init; }
}