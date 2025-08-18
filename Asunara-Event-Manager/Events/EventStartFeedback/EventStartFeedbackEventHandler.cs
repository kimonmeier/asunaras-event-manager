using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using EventManager.Events.UpdateEventFeedbackThread;
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

    public EventStartFeedbackEventHandler(DiscordSocketClient client, UserPreferenceRepository userPreferenceRepository, RootConfig config, ILogger<EventStartFeedbackEventHandler> logger, ISender sender)
    {
        _client = client;
        _userPreferenceRepository = userPreferenceRepository;
        _config = config;
        _logger = logger;
        _sender = sender;
    }

    public async Task Handle(EventStartFeedbackEvent request, CancellationToken cancellationToken)
    {
        SocketGuild guild = _client.GetGuild(_config.Discord.MainDiscordServerId);
        var users = await guild.GetEvent(request.Event.DiscordId).GetUsersAsync(new RequestOptions()
        {
            CancelToken = cancellationToken
        }).FlattenAsync();

        await _sender.Send(new UpdateEventFeedbackThreadEvent()
        {
            DiscordEventId = request.Event.DiscordId,
        }, cancellationToken);
        
        foreach (var eventUser in users)
        {
            // Remove Participant Role
            await guild.GetUser(eventUser.Id).RemoveRoleAsync(_config.Discord.Event.EventParticipantRoleId);

            var userPreference = await _userPreferenceRepository.GetByDiscordAsync(eventUser.Id);
            if (userPreference is null)
            {
                continue;
            }

            if (!userPreference.AllowReminderInPrivateMessage)
            {
                continue;
            }

            await StartFeedbackLoopUser(request.Event, eventUser.Id);
        }
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


        await dmChannel.SendMessageAsync(
            $"Hallöchen du hast gerade beim Event \"{discordEvent.Name}\" teilgenommen. Wir hoffen es hat dir gefallen und wir würden uns über eine Bewertung freuen!",
            components: feedbackComponent.Build());
    }
}