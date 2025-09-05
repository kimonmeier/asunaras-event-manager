using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using EventManager.Events.UpdateEventFeedbackThread;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.EventStartFeedback;

public class EventStartFeedbackEventHandler : IRequestHandler<EventStartFeedbackEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly RootConfig _config;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly ILogger<EventStartFeedbackEventHandler> _logger;
    private readonly ISender _sender;
    private readonly EventParticipantService _eventParticipantService;

    public EventStartFeedbackEventHandler(DiscordSocketClient client, UserPreferenceRepository userPreferenceRepository, RootConfig config,
        ILogger<EventStartFeedbackEventHandler> logger, ISender sender, EventParticipantService eventParticipantService)
    {
        _client = client;
        _userPreferenceRepository = userPreferenceRepository;
        _config = config;
        _logger = logger;
        _sender = sender;
        _eventParticipantService = eventParticipantService;
    }

    public async Task Handle(EventStartFeedbackEvent request, CancellationToken cancellationToken)
    {
        SocketGuild guild = _client.GetGuild(_config.Discord.MainDiscordServerId);
        SocketGuildEvent? socketGuildEvent = guild.GetEvent(request.Event.DiscordId);

        List<RestUser> users;

        if (socketGuildEvent is not null)
        {
            users = (await socketGuildEvent.GetUsersAsync(new RequestOptions()
            {
                CancelToken = cancellationToken
            }).FlattenAsync()).ToList();
            
        }
        else
        {
            users = new List<RestUser>();
        }

        await _sender.Send(new UpdateEventFeedbackThreadEvent()
        {
            DiscordEventId = request.Event.DiscordId,
        }, cancellationToken);

        List<Task> tasks = new List<Task>();
        
        foreach (var eventUser in users)
        {
            // Remove Participant Role
            await guild.GetUser(eventUser.Id).RemoveRoleAsync(_config.Discord.Event.EventParticipantRoleId);

            if (!_eventParticipantService.HasParticipant(request.Event.Id, eventUser.Id))
            {
                _logger.LogInformation("User {UserId} is not a participant of event but was interested {DiscordEventName}{DiscordEventId}", eventUser.Id, request.Event.Name,
                    request.Event.DiscordId);

                continue;
            }

            tasks.Add(CheckUser(request.Event, eventUser.Id));
        }

        // Remove already sent
        List<ulong> participants = _eventParticipantService.GetParticipants(request.Event.Id);
        participants.RemoveAll(x => users.Any(z => z.Id == x));

        foreach (var eventUserId in participants)
        {
            tasks.Add(CheckUser(request.Event, eventUserId));
        }
        
        _logger.LogInformation("Started all Feedback Loops for the current Event");
        
        await Task.WhenAll(tasks);
        
        _logger.LogInformation("Finished all Feedback Loops for the current Event");
    }

    private async Task CheckUser(DiscordEvent discordEvent, ulong userId)
    {
        _logger.LogInformation("Checking for User: {0}", userId);
        var userPreference = await _userPreferenceRepository.GetByDiscordAsync(userId);
        if (userPreference is null)
        {
            _logger.LogInformation("User {UserId} has no preference", userId);
            await _sender.Send(new CheckForUserPreferenceOnEventInterestedEvent()
            {
                DiscordUser = _client.GetGuild(_config.Discord.MainDiscordServerId).GetUser(userId),
            });

            return;
        }

        if (!userPreference.AllowReminderForFeedback)
        {
            _logger.LogInformation("User {UserId} is not allowed to receive feedback reminders", userId);
            return;
        }
        
        _logger.LogInformation("Starting Feedback Loop for User {UserId}", userId);
        await StartFeedbackLoopUser(discordEvent, userId);
    }

    private async Task StartFeedbackLoopUser(DiscordEvent discordEvent, ulong userId)
    {
        IDMChannel dmChannel = await _client.GetGuild(_config.Discord.MainDiscordServerId).GetUser(userId).CreateDMChannelAsync();

        if (dmChannel is null)
        {
            _logger.LogError("Could not find DMChannel for user {UserId}", userId);
            return;
        }

        ComponentBuilder feedbackComponent = new ComponentBuilder()
                .AddRow(new ActionRowBuilder()
                    .WithButton("⭐", $"{Konst.ButtonFeedback1Star}{Konst.PayloadDelimiter}{discordEvent.DiscordId}")
                    .WithButton("⭐⭐", $"{Konst.ButtonFeedback2Star}{Konst.PayloadDelimiter}{discordEvent.DiscordId}")
                    .WithButton("⭐⭐⭐", $"{Konst.ButtonFeedback3Star}{Konst.PayloadDelimiter}{discordEvent.DiscordId}")
                    .WithButton("⭐⭐⭐⭐", $"{Konst.ButtonFeedback4Star}{Konst.PayloadDelimiter}{discordEvent.DiscordId}")
                    .WithButton("⭐⭐⭐⭐⭐", $"{Konst.ButtonFeedback5Star}{Konst.PayloadDelimiter}{discordEvent.DiscordId}")
                )
            ;


        try
        {
            await dmChannel.SendMessageAsync(
                $"Hallöchen du hast gerade beim Event \"{discordEvent.Name}\" teilgenommen. Wir hoffen es hat dir gefallen und wir würden uns über eine Bewertung freuen!",
                components: feedbackComponent.Build());
        } catch (Exception _)
        {
            _logger.LogError("The User {0} has DM's disabled!", userId);
        }
    }
}