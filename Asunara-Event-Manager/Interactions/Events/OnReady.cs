using EventManager.Configuration;
using EventManager.Events.CheckConnectedClients;
using EventManager.Services;
using MediatR;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnReady(ISender sender, RootConfig rootConfig, GatewayClient gatewayClient, VoiceStateHistoryService voiceStateHistoryService) : IReadyGatewayHandler
{
    public async ValueTask HandleAsync(ReadyEventArgs arg)
    {
        foreach (var guildId in arg.GuildIds)
        {
            await gatewayClient.RequestGuildUsersAsync(new GuildUsersRequestProperties(guildId));
            
            var guild = gatewayClient.Cache.Guilds[guildId];
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
    }
}