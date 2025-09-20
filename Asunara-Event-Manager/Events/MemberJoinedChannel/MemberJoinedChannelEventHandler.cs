using Discord;
using Discord.WebSocket;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using EventManager.Events.CheckFskRestrictionOnUser;
using EventManager.Models.Restrictions;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.MemberJoinedChannel;

public class MemberJoinedChannelEventHandler : IRequestHandler<MemberJoinedChannelEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ISender _sender;
    private readonly EventParticipantService _eventParticipantService;
    private readonly ILogger<MemberJoinedChannelEventHandler> _log;

    public MemberJoinedChannelEventHandler(DiscordEventRepository discordEventRepository, ISender sender, EventParticipantService eventParticipantService, ILogger<MemberJoinedChannelEventHandler> log)
    {
        _discordEventRepository = discordEventRepository;
        _sender = sender;
        _eventParticipantService = eventParticipantService;
        _log = log;
    }

    public async Task Handle(MemberJoinedChannelEvent request, CancellationToken cancellationToken)
    {
        var guild = request.Channel.Guild;
        var events = await _discordEventRepository.GetAllUncompleted();

        DiscordEvent? @event = events.SingleOrDefault(x => guild.GetEvent(x.DiscordId)?.Status == GuildScheduledEventStatus.Active);

        if (@event is null)
        {
            DiscordEvent? futureEvent = @events.FirstOrDefault(x => x.Date - DateTime.UtcNow < TimeSpan.FromMinutes(30) && guild.GetEvent(x.DiscordId).Channel.Id == request.Channel.Id);
            if (futureEvent is not null)
            {
                _log.LogInformation("Adding user {UserId} to future event {DiscordEventId} he's {Time} to early", request.User.Id, futureEvent.DiscordId, futureEvent.Date - DateTime.UtcNow);;
                _eventParticipantService.AddParticipant(futureEvent.Id, request.User.Id);
            }
            
            return;
        }

        var eventChannel = guild.GetEvent(@event.DiscordId).Channel;

        if (eventChannel is null)
        {
            return;
        }

        if (request.Channel.Id != eventChannel.Id)
        {
            return;
        }
        
        _eventParticipantService.AddParticipant(@event.Id, request.User.Id);
        
        await _sender.Send(new CheckForUserPreferenceOnEventInterestedEvent()
        {
            DiscordUser = request.User,
        }, cancellationToken);
        
        var checkResult = await _sender.Send(new CheckFskRestrictionOnUserEvent()
        {
            User = request.User,
            Event = (await _discordEventRepository.FindWithRestrictionsByDiscordId(@event.DiscordId))!
        }, cancellationToken);

        if (checkResult.Success)
        {
            return;
        }
        
        await request.User.ModifyAsync(x => x.Channel = null);
        IDMChannel dmChannel = await request.User.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync($"Hallöchen, leider entsprichst du nicht den Altersregeln für dieses Event: \"{checkResult.ErrorMessage}\"");
    }
}