using EventManager.Configuration;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using EventManager.Events.UpdateEventFeedbackThread;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.EventStartFeedback;

public class EventStartFeedbackEventHandler : IRequestHandler<EventStartFeedbackEvent>
{
    private readonly GatewayClient _client;
    private readonly RootConfig _config;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly ILogger<EventStartFeedbackEventHandler> _logger;
    private readonly ISender _sender;
    private readonly EventParticipantService _eventParticipantService;

    public EventStartFeedbackEventHandler(GatewayClient client, UserPreferenceRepository userPreferenceRepository,
        RootConfig config,
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
        Guild guild = _client.Cache.Guilds[_config.Discord.MainDiscordServerId];
        GuildScheduledEvent? socketGuildEvent = guild.ScheduledEvents[request.Event.DiscordId];

        List<GuildScheduledEventUser> users;

        if (socketGuildEvent is not null)
        {
            users = socketGuildEvent.GetUsersAsync().ToBlockingEnumerable().ToList();
        }
        else
        {
            users = new List<GuildScheduledEventUser>();
        }

        await _sender.Send(new UpdateEventFeedbackThreadEvent()
        {
            DiscordEventId = request.Event.DiscordId,
        }, cancellationToken);

        Thread thread = new(async void () =>
        {
            try
            {
                foreach (var eventUser in users)
                {
                    // Remove Participant Role
                    await guild.RemoveUserRoleAsync(eventUser.User.Id, _config.Discord.Event.EventParticipantRoleId,
                        cancellationToken: cancellationToken);

                    if (!_eventParticipantService.HasParticipant(request.Event.Id, eventUser.User.Id))
                    {
                        _logger.LogInformation(
                            "User {UserId} is not a participant of event but was interested {DiscordEventName}{DiscordEventId}",
                            eventUser.User.Id, request.Event.Name,
                            request.Event.DiscordId);

                        continue;
                    }

                    await CheckUser(request.Event, eventUser.User.Id);
                }

                // Remove already sent
                List<ulong> participants = _eventParticipantService.GetParticipants(request.Event.Id);
                participants.RemoveAll(x => users.Any(z => z.User.Id == x));

                foreach (var eventUserId in participants)
                {
                    await CheckUser(request.Event, eventUserId);
                }
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
            }
        });

        thread.Start();
    }

    private async Task CheckUser(DiscordEvent discordEvent, ulong userId)
    {
        _logger.LogInformation("Checking for User: {0}", userId);
        var userPreference = await _userPreferenceRepository.GetByDiscordAsync(userId);
        if (userPreference is null)
        {
            _logger.LogInformation("User {UserId} has no preference", userId);
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
        DMChannel dmChannel = await _client.Cache.Guilds[_config.Discord.MainDiscordServerId].Users[userId]
            .GetDMChannelAsync();

        if (dmChannel is null)
        {
            _logger.LogError("Could not find DMChannel for user {UserId}", userId);
            return;
        }

        MessageProperties messageProperties = new();
        messageProperties.Content =
            $"Hallöchen du hast gerade beim Event \"{discordEvent.Name}\" teilgenommen. Wir hoffen es hat dir gefallen und wir würden uns über eine Bewertung freuen!";
        messageProperties.AddComponents(new ActionRowProperties()
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{discordEvent.DiscordId}", "⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{discordEvent.DiscordId}", "⭐⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{discordEvent.DiscordId}", "⭐⭐⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{discordEvent.DiscordId}", "⭐⭐⭐⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{discordEvent.DiscordId}", "⭐⭐⭐⭐⭐",
                ButtonStyle.Primary)));

        try
        {
            await dmChannel.SendMessageAsync(messageProperties);
        }
        catch (Exception exception)
        {
            SentrySdk.CaptureException(exception);
            _logger.LogError("The User {0} has DM's disabled!", userId);
        }
    }
}