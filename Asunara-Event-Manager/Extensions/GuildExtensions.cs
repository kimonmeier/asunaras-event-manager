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
}