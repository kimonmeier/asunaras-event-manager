using Discord;
using MediatR;

namespace EventManager.Events.PostBirthdayMessage;

public class PostBirthdayMessageEvent : IRequest
{
    public required ITextChannel TextChannel { get; init; }
}