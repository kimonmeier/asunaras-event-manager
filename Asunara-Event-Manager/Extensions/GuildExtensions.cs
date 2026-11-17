using System.Net;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

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

    /// <summary>
    /// Resolves a guild user. The gateway cache only holds a part of the guild members
    /// (the ones Discord sent with GUILD_CREATE plus the ones seen in gateway events since),
    /// so a cache miss is resolved through the REST api. Only a 404 means the user is no longer
    /// a member of the guild, in that case <c>null</c> is returned. Every other REST error is rethrown.
    /// </summary>
    public static async Task<GuildUser?> GetOrFetchUserAsync(this Guild guild, ulong userId,
        CancellationToken cancellationToken = default)
    {
        if (guild.Users.TryGetValue(userId, out GuildUser? cachedUser))
        {
            return cachedUser;
        }

        try
        {
            return await guild.GetUserAsync(userId, cancellationToken: cancellationToken);
        }
        catch (RestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves every user connected to the given voice channel. The guild user attached to the
    /// voice state and the gateway cache are used first, the REST api is only called on a real miss.
    /// Users that are no longer members of the guild are skipped.
    /// </summary>
    public static async Task<List<GuildUser>> GetConnectedUsersAsync(this Guild guild, ulong voiceChannelId,
        CancellationToken cancellationToken = default)
    {
        List<GuildUser> users = new List<GuildUser>();

        foreach (VoiceState voiceState in guild.VoiceStates.Values.Where(x => x.ChannelId == voiceChannelId))
        {
            GuildUser? user = voiceState.User ?? await guild.GetOrFetchUserAsync(voiceState.UserId, cancellationToken);

            if (user is not null)
            {
                users.Add(user);
            }
        }

        return users;
    }
}
