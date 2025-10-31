using NetCord;
using NetCord.Gateway;

namespace EventManager.Extensions;

public static class GuildExtensions
{
    public static TextChannel GetTextChannel(this Guild guild, ulong channelId)
    {
        IGuildChannel channel = guild.Channels[channelId];

        if (channel is not TextChannel textChannel)
        {
            throw new InvalidCastException();
        }

        return textChannel;
    }

    public static IVoiceGuildChannel GetVoiceGuildChannel(this Guild guild, ulong channelId)
    {
        IGuildChannel channel = guild.Channels[channelId];

        if (channel is VoiceGuildChannel voiceGuildChannel)
        {
            return voiceGuildChannel;
        } else if (channel is StageGuildChannel stageGuildChannel)
        {
            return stageGuildChannel;
        }
        
        throw new InvalidCastException();
    }

    public static IEnumerable<GuildUser> GetConnectedUsers(this Guild guild, ulong voiceChannelId)
    {
        return guild.VoiceStates.Where(x => x.Value.ChannelId == voiceChannelId).Select(x => guild.Users[x.Key]);
    }
}