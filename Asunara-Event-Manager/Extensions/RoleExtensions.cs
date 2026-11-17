using NetCord;
using NetCord.Gateway;

namespace EventManager.Extensions;

public static class RoleExtensions
{
    /// <summary>
    /// Loads every guild user that currently has the given role.
    /// The gateway cache is intentionally not used here: it only contains the users
    /// Discord sent with GUILD_CREATE plus the ones seen in gateway events since,
    /// so it is never a complete view of the guild.
    /// </summary>
    public async static Task<IList<GuildUser>> GetUsersAsync(this Role role, GatewayClient client,
        CancellationToken cancellationToken = default)
    {
        List<GuildUser> users = new List<GuildUser>();

        await foreach (GuildUser user in client.Rest.GetGuildUsersAsync(role.GuildId)
                           .WithCancellation(cancellationToken))
        {
            if (user.RoleIds.Any(roleId => roleId == role.Id))
            {
                users.Add(user);
            }
        }

        return users;
    }
}
