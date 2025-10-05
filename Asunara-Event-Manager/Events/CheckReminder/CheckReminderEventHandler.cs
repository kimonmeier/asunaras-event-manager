using System.Globalization;
using System.Security.Cryptography;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Entities.Events;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using EventManager.Services;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using Quartz.Core;

namespace EventManager.Events.CheckReminder;

public class CheckReminderEventHandler : IRequestHandler<CheckReminderEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly RootConfig _config;
    private readonly EventReminderService _eventReminderService;
    private readonly ILogger<CheckReminderEventHandler> _logger;
    
    public CheckReminderEventHandler(DiscordEventRepository discordEventRepository, DiscordSocketClient discordSocketClient, RootConfig config, ILogger<CheckReminderEventHandler> logger, UserPreferenceRepository userPreferenceRepository, EventReminderService eventReminderService)
    {
        _discordEventRepository = discordEventRepository;
        _discordSocketClient = discordSocketClient;
        _config = config;
        _logger = logger;
        _userPreferenceRepository = userPreferenceRepository;
        _eventReminderService = eventReminderService;
    }

    public async Task Handle(CheckReminderEvent request, CancellationToken cancellationToken)
    {
        var allUncompleted = await _discordEventRepository.GetAllUncompleted();
        
        foreach (var discordEvent in allUncompleted)
        {
            SocketGuild socketGuild = _discordSocketClient.GetGuild(_config.Discord.MainDiscordServerId);
            SocketGuildEvent guildEvent = socketGuild.GetEvent(discordEvent.DiscordId);

            if (guildEvent is null)
            {
                _logger.LogError("Could not find guild event for event {DiscordEventName}:{DiscordEventId}", discordEvent.Name, discordEvent.DiscordId);
                continue;
            }
            
            _logger.LogDebug("Checking reminder for event {DiscordEventName}:{DiscordEventId}", discordEvent.Name, discordEvent.DiscordId);
            _logger.LogDebug("Event start time: {EventStartTime}", guildEvent!.StartTime.UtcDateTime);
            _logger.LogDebug("Current time: {CurrentTime}", DateTime.UtcNow);
            
            if (guildEvent.StartTime.UtcDateTime > DateTime.UtcNow.AddMinutes(45))
            {
                continue;
            }

            if (guildEvent.Status == GuildScheduledEventStatus.Active)
            {
                continue;
            }

            if (_eventReminderService.HasAnnouncedEvent(discordEvent.Id))
            {
                continue;
            }

            IEnumerable<RestUser> interestedUsers = await guildEvent.GetUsersAsync(new RequestOptions() { CancelToken = cancellationToken }).FlattenAsync();
            foreach (var user in interestedUsers)
            {
                await SendEventReminderMessage(user, discordEvent, socketGuild, guildEvent);
            }
            
            _eventReminderService.AnnounceEvent(discordEvent.Id);
        }
    }

    private async Task SendEventReminderMessage(RestUser user, DiscordEvent discordEvent, SocketGuild socketGuild, SocketGuildEvent guildEvent)
    {
        UserPreference? preference = await _userPreferenceRepository.GetByDiscordAsync(user.Id);

        if (!(preference?.AllowReminderForEvent ?? false))
        {
            _logger.LogInformation("User {UserId} is not allowed to receive reminders for event {DiscordEventName}:{DiscordEventId}", user.Id, discordEvent.Name, discordEvent.DiscordId);

            return;
        }

        RestDMChannel? dmChannel = await user.CreateDMChannelAsync();

        if (dmChannel is null)
        {
            _logger.LogError("Could not open DM Channel with: <@" + user.Id + ">");
            return;
        }

        await dmChannel.SendMessageAsync($"Hallöchen {socketGuild.GetUser(user.Id).DisplayName} <:MilkAndMocha_Heart:1368600961693651154>,\n\n Du hast dich für das Event **{guildEvent.Name}** angemeldet! Dieses findet **heute** um **{discordEvent.Date.FromUtc().ToShortTimeString()} Uhr** statt!\n\nSei dabei und joine dem Channel, oder melde dich ab in dem du dich nicht mehr für das Event interessierst!\n\n|| https://discord.gg/cafemidnight?event={discordEvent.DiscordId} ||");
    }
}