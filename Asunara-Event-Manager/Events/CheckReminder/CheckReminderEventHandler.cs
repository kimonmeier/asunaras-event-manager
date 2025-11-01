using EventManager.Configuration;
using EventManager.Data.Entities.Events;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.CheckReminder;

public class CheckReminderEventHandler : IRequestHandler<CheckReminderEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly GatewayClient _discordClient;
    private readonly UserPreferenceRepository _userPreferenceRepository;
    private readonly RootConfig _config;
    private readonly EventReminderService _eventReminderService;
    private readonly ILogger<CheckReminderEventHandler> _logger;
    
    public CheckReminderEventHandler(DiscordEventRepository discordEventRepository, GatewayClient discordClient, RootConfig config, ILogger<CheckReminderEventHandler> logger, UserPreferenceRepository userPreferenceRepository, EventReminderService eventReminderService)
    {
        _discordEventRepository = discordEventRepository;
        _discordClient = discordClient;
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
            if (!_discordClient.Cache.Guilds.ContainsKey(_config.Discord.MainDiscordServerId))
            {
                _logger.LogWarning("No guilds found, the bot is not ready yet");
                return;
            }
            
            Guild socketGuild = _discordClient.Cache.Guilds[_config.Discord.MainDiscordServerId];
            GuildScheduledEvent guildEvent = socketGuild.ScheduledEvents[discordEvent.DiscordId];

            if (guildEvent is null)
            {
                _logger.LogError("Could not find guild event for event {DiscordEventName}:{DiscordEventId}", discordEvent.Name, discordEvent.DiscordId);
                continue;
            }
            
            _logger.LogDebug("Checking reminder for event {DiscordEventName}:{DiscordEventId}", discordEvent.Name, discordEvent.DiscordId);
            _logger.LogDebug("Event start time: {EventStartTime}", guildEvent!.ScheduledStartTime.UtcDateTime);
            _logger.LogDebug("Current time: {CurrentTime}", DateTime.UtcNow);
            
            if (guildEvent.ScheduledStartTime.UtcDateTime > DateTime.UtcNow.AddMinutes(50))
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

            var interestedUsers = guildEvent.GetUsersAsync().ToBlockingEnumerable();
            foreach (var user in interestedUsers)
            {
                await SendEventReminderMessage(user, discordEvent, socketGuild, guildEvent);
            }
            
            _eventReminderService.AnnounceEvent(discordEvent.Id);
        }
    }

    private async Task SendEventReminderMessage(GuildScheduledEventUser user, DiscordEvent discordEvent, Guild socketGuild, GuildScheduledEvent guildEvent)
    {
        UserPreference? preference = await _userPreferenceRepository.GetByDiscordAsync(user.User.Id);

        if (!(preference?.AllowReminderForEvent ?? false))
        {
            _logger.LogInformation("User {UserId} is not allowed to receive reminders for event {DiscordEventName}:{DiscordEventId}", user.User.Id, discordEvent.Name, discordEvent.DiscordId);

            return;
        }

        DMChannel? dmChannel = await user.User.GetDMChannelAsync();

        if (dmChannel is null)
        {
            _logger.LogError("Could not open DM Channel with: <@" + user.User.Id + ">");
            return;
        }
        
        await dmChannel.SendMessageAsync($"Hallöchen {socketGuild.Users[user.User.Id].Nickname} <:MilkAndMocha_Heart:1368600961693651154>,\n\n Du hast dich für das Event **{guildEvent.Name}** angemeldet! Dieses findet **heute** um **{discordEvent.Date.FromUtc().ToShortTimeString()} Uhr** statt!\n\nSei dabei und joine dem Channel, oder melde dich ab in dem du dich nicht mehr für das Event interessierst!\n\n|| https://discord.gg/cafemidnight?event={discordEvent.DiscordId} ||");
    }
}