using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using EventManager.Events.CheckFskRestrictionOnUser;
using EventManager.Events.CheckVoiceActivityForChannel;
using EventManager.Events.EventDeleted;
using EventManager.Events.StartTrackingVoice;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;

namespace EventManager.Events.MemberJoinedChannel;

public class MemberJoinedChannelEventHandler : IRequestHandler<MemberJoinedChannelEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ISender _sender;
    private readonly EventParticipantService _eventParticipantService;
    private readonly ILogger<MemberJoinedChannelEventHandler> _log;
    private readonly GatewayClient  _gatewayClient;
    
    public MemberJoinedChannelEventHandler(DiscordEventRepository discordEventRepository, ISender sender, EventParticipantService eventParticipantService, ILogger<MemberJoinedChannelEventHandler> log, GatewayClient gatewayClient)
    {
        _discordEventRepository = discordEventRepository;
        _sender = sender;
        _eventParticipantService = eventParticipantService;
        _log = log;
        _gatewayClient = gatewayClient;
    }

    public async Task Handle(MemberJoinedChannelEvent request, CancellationToken cancellationToken)
    {
        await _sender.Send(new StartTrackingVoiceEvent()
        {
            DiscordUser = request.User, DiscordChannel = request.Channel,
        }, cancellationToken);
        
        await _sender.Send(new CheckVoiceActivityForChannelEvent()
        {
            ChannelId = request.Channel.Id,
        }, cancellationToken);
        
        var guildId = request.Channel.GuildId;
        var guild = _gatewayClient.Cache.Guilds[guildId];
        var events = await _discordEventRepository.GetAllUncompleted();

        foreach (var deletedEvent in events.Where(x => !guild.ScheduledEvents.ContainsKey(x.DiscordId)).ToList())
        {
            await _sender.Send(new EventDeletedEvent()
            {
                DiscordId = deletedEvent.DiscordId
            }, cancellationToken);
            
            events.Remove(deletedEvent);
        }
        
        DiscordEvent? @event = events.SingleOrDefault(x => guild.ScheduledEvents[x.DiscordId].Status == GuildScheduledEventStatus.Active);

        if (@event is null)
        {
            DiscordEvent? futureEvent = @events.FirstOrDefault(x => x.Date - DateTime.UtcNow < TimeSpan.FromMinutes(30) && guild.ScheduledEvents[x.DiscordId].ChannelId == request.Channel.Id);
            if (futureEvent is not null)
            {
                _log.LogInformation("Adding user {UserId} to future event {DiscordEventId} he's {Time} to early", request.User.Id, futureEvent.DiscordId, futureEvent.Date - DateTime.UtcNow);;
                _eventParticipantService.AddParticipant(futureEvent.Id, request.User.Id);
            }
            
            return;
        }

        var eventChannelId = guild.ScheduledEvents[@event.DiscordId].ChannelId;

        if (eventChannelId is null)
        {
            return;
        }

        if (request.Channel.Id != eventChannelId)
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
        
        await request.User.ModifyAsync(x => x.ChannelId = null, cancellationToken: cancellationToken);
        DMChannel dmChannel = await request.User.GetDMChannelAsync(cancellationToken: cancellationToken);
        await dmChannel.SendMessageAsync($"Hallöchen, leider entsprichst du nicht den Altersregeln für dieses Event: \"{checkResult.ErrorMessage}\"", cancellationToken: cancellationToken);
    }
}