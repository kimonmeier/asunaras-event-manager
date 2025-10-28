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

    public static VoiceGuildChannel GetVoiceGuildChannel(this Guild guild, ulong channelId)
    {
        IGuildChannel channel = guild.Channels[channelId];

        if (channel is not VoiceGuildChannel voiceGuildChannel)
        {
            throw new InvalidCastException();
        }

        return voiceGuildChannel;
    }
}