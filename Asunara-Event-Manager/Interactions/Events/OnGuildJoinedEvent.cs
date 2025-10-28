using EventManager.Configuration;
using EventManager.Events.CheckConnectedClients;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnGuildJoinedEvent(
    RootConfig config,
    ILogger<OnGuildJoinedEvent> logger,
    GatewayClient gatewayClient,
    VoiceStateHistoryService voiceStateHistoryService,
    ISender sender) : IGuildCreateGatewayHandler
{
    public async ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        if (arg.GuildId != config.Discord.MainDiscordServerId && arg.GuildId != config.Discord.MainDiscordServerId)
        {
            await HandleLeave(arg.Guild);
            return;
        }
        
        var guild = arg.Guild;

        if (guild is null)
        {
            return;
        }
        
        await gatewayClient.RequestGuildUsersAsync(new GuildUsersRequestProperties(guild.Id));
        foreach (var guildVoiceState in guild.VoiceStates)
        {
            voiceStateHistoryService.AddLastVoiceState(guildVoiceState.Value);
        }

        var connectedClients = guild.VoiceStates.Keys.Select(x => guild.Users[x]).ToList();
        await sender.Send(new CheckConnectedClientsEvent()
        {
            ConnectedUsers = connectedClients
        });
    }

    private async Task HandleLeave(Guild guild)
    {
        SentrySdk.AddBreadcrumb("Guild ID", $"{guild.Id}");
        SentrySdk.AddBreadcrumb("Guild Name", $"{guild.Name}");
        SentrySdk.AddBreadcrumb("Guild Owner", guild.OwnerId.ToString());


        if (guild.Users is null)
        {
            SentrySdk.AddBreadcrumb("Guild Users", "Is somehow null");
        }
        else
        {
            SentrySdk.AddBreadcrumb("Guild Users", $"{string.Join(", ", guild.Users?.Select(x => x.Key) ?? [])}");
        }

        try
        {
            var dmChannelAsync = await guild.Users[guild.OwnerId].GetDMChannelAsync();
            await dmChannelAsync.SendMessageAsync(
                $"Sorry aber ich bin ein Teambot für den Midnight-Café Discord und kann deswegen nicht joinen!");
        }
        catch (Exception e)
        {
            logger.LogInformation("The owner of the guild could not be contacted!");
        }

        SentrySdk.CaptureMessage($"Guild {guild.Name} ({guild.Id}) is not in the config.json. Leaving it...",
            SentryLevel.Warning);

        await guild.LeaveAsync();

    }
}