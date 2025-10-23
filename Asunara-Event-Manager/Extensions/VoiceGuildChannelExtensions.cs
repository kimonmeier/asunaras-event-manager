using NetCord;
using NetCord.Gateway;

namespace EventManager.Extensions;

public static class VoiceGuildChannelExtensions
{
    public static List<VoiceState> GetConnectedUsers(this IVoiceGuildChannel channel, GatewayClient client)
    {
        var cacheGuild = client.Cache.Guilds[channel.GuildId];
        
        return cacheGuild.VoiceStates
            .Where(x => x.Value.ChannelId == channel.Id)
            .Select(x => x.Value)
            .ToList();
    }
}