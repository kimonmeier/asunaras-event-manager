using System.Globalization;
using System.Security.Cryptography;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz.Core;

namespace EventManager.Events.CheckReminder;

public class CheckReminderEventHandler : IRequestHandler<CheckReminderEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly RootConfig _config;
    private readonly ILogger<CheckReminderEventHandler> _logger;
    
    public CheckReminderEventHandler(DiscordEventRepository discordEventRepository, DiscordSocketClient discordSocketClient, RootConfig config, ILogger<CheckReminderEventHandler> logger)
    {
        _discordEventRepository = discordEventRepository;
        _discordSocketClient = discordSocketClient;
        _config = config;
        _logger = logger;
    }

    public async Task Handle(CheckReminderEvent request, CancellationToken cancellationToken)
    {
        var allUncompleted = await _discordEventRepository.GetAllUncompleted();
        
        foreach (var discordEvent in allUncompleted)
        {
            SocketGuild socketGuild = _discordSocketClient.GetGuild(_config.Discord.MainDiscordServerId);
            SocketGuildEvent guildEvent = socketGuild.GetEvent(discordEvent.DiscordId);
            
            _logger.LogDebug("Checking reminder for event {DiscordEventName}:{DiscordEventId}", discordEvent.Name, discordEvent.DiscordId);
            _logger.LogDebug("Event start time: {EventStartTime}", (guildEvent?.StartTime.UtcDateTime) ?? DateTime.MinValue);
            _logger.LogDebug("Current time: {CurrentTime}", DateTime.UtcNow);
            
            if (guildEvent.StartTime.UtcDateTime > DateTime.UtcNow.AddHours(1))
            {
                continue;
            }

            IEnumerable<RestUser> interestedUsers = await guildEvent.GetUsersAsync(new RequestOptions() { CancelToken = cancellationToken }).FlattenAsync();
            foreach (var user in interestedUsers)
            {
                RestDMChannel? dmChannel = await user.CreateDMChannelAsync();

                if (dmChannel is null)
                {
                    _logger.LogError("Could not open DM Channel with: <@" + user.Id + ">");
                    continue;
                }

                await dmChannel.SendMessageAsync($"Hallöchen {socketGuild.GetUser(user.Id).DisplayName},\n\n Du hast dich für das Event {guildEvent.Name} angemeldet! Dieses findet **heute** um **{discordEvent.Date.ToShortTimeString()} Uhr** statt!\n\nSei dabei und joine dem Channel, oder melde dich ab in dem du dich nicht mehr für das Event interessierst!\n\n|| https://discord.gg/asunara?event={discordEvent.DiscordId} ||");
            }
        }
    }
}