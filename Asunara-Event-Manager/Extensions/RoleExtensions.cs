using NetCord;
using NetCord.Gateway;

namespace EventManager.Extensions;

public static class RoleExtensions
{
    public static IList<GuildUser> GetUser(this Role role, GatewayClient client)
    {
        var guild = client.Cache.Guilds[role.GuildId];

        return guild.Users.Values.Where(user => user.RoleIds.Any(roleId => roleId == role.Id)).ToList();
    }
    
}