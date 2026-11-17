using System.Net;
using System.Text;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Birthday;
using EventManager.Data.Repositories;
using EventManager.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.CheckBirthday;

public class CheckBirthdayEventHandler : IRequestHandler<CheckBirthdayEvent>
{
    private readonly UserBirthdayRepository _userBirthdayRepository;
    private readonly GatewayClient _discordClient;
    private readonly RootConfig _config;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ILogger<CheckBirthdayEventHandler> _logger;

    public CheckBirthdayEventHandler(UserBirthdayRepository userBirthdayRepository, GatewayClient discordClient,
        RootConfig config,
        DbTransactionFactory dbTransactionFactory, ILogger<CheckBirthdayEventHandler> logger)
    {
        _userBirthdayRepository = userBirthdayRepository;
        _discordClient = discordClient;
        _config = config;
        _dbTransactionFactory = dbTransactionFactory;
        _logger = logger;
    }

    public async Task Handle(CheckBirthdayEvent request, CancellationToken cancellationToken)
    {
        var guild = _discordClient.Cache.Guilds[_config.Discord.MainDiscordServerId];
        var birthdayRole = guild.Roles[_config.Discord.Birthday.BirthdayChildRoleId];
        var birthdayChannel = guild.GetTextChannel(_config.Discord.Birthday.ChannelId);

        await ClearBirthdayChannel(birthdayChannel);
        await RemoveBirthdayRoleOnOldUsers(birthdayRole, cancellationToken);

        var currentBirthdays = await _userBirthdayRepository.GetCurrentBirthday(DateTime.Now.Month, DateTime.Now.Day);

        var birthdayUsers = await CheckBirthdaysAndExistingUser(guild, currentBirthdays, cancellationToken);

        if (currentBirthdays.Count == 0)
        {
            await birthdayChannel.SendMessageAsync("Leider gibt es heute keine Geburtstage!",
                cancellationToken: cancellationToken);

            return;
        }

        var birthdayMessages = _config.Discord.Birthday.Messages;
        var messageIndex = Random.Shared.Next(0, birthdayMessages.Length - 1);

        StringBuilder comfortBuilder = new StringBuilder();
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(string.Format(birthdayMessages[messageIndex], _config.Discord.Birthday.BirthdayChildRoleId));
        builder.AppendLine("Geburtstag haben:");
        comfortBuilder.Append(builder);
        bool hasComfort = false;

        foreach (var birthday in currentBirthdays)
        {
            var ageString = birthday.Birthday.Year == 1
                ? "?? Jahre alt"
                : $"{birthday.Birthday.GetAge()} Jahre alt";
            var line = $"- <@{birthday.DiscordId}> - {ageString}";

            GuildUser guildUser = birthdayUsers[birthday.DiscordId];

            if (guildUser.RoleIds.Any(x => x == _config.Discord.Comfort.ComfortRoleId))
            {
                comfortBuilder.AppendLine(line);
                hasComfort = true;
            }

            builder.AppendLine(line);
        }

        comfortBuilder.AppendLine($"Lasst den Krümeln eine schöne Nachricht da!");
        comfortBuilder.AppendLine($"|| <@&{_config.Discord.Comfort.ComfortRoleId}> ||");
        builder.AppendLine($"|| <@&{_config.Discord.Birthday.BirthdayNotificationRoleId}> ||");

        await birthdayChannel.SendMessageAsync(builder.ToString(), cancellationToken: cancellationToken);

        await guild.GetTextChannel(_config.Discord.HauptchatChannelId)
            .SendMessageAsync(builder.ToString(), cancellationToken: cancellationToken);
        if (hasComfort)
        {
            await guild.GetTextChannel(_config.Discord.Comfort.ChannelId)
                .SendMessageAsync(comfortBuilder.ToString(), cancellationToken: cancellationToken);
        }
    }

    private async Task RemoveBirthdayRoleOnOldUsers(Role birthdayRole, CancellationToken cancellationToken)
    {
        IList<GuildUser> roleMembers = await birthdayRole.GetUsersAsync(_discordClient, cancellationToken);

        _logger.LogInformation("Found {Count} user(s) with the birthday role {RoleId}", roleMembers.Count,
            birthdayRole.Id);

        foreach (GuildUser roleMember in roleMembers)
        {
            try
            {
                await roleMember.RemoveRoleAsync(birthdayRole.Id, cancellationToken: cancellationToken);
            }
            catch (RestException exception)
            {
                _logger.LogError(exception, "Could not remove the birthday role from user {UserId}", roleMember.Id);
            }
        }
    }

    private async Task ClearBirthdayChannel(TextChannel birthdayChannel)
    {
        var messages = await birthdayChannel.GetMessagesAroundAsync(_config.Discord.Birthday.AnnouncementMessageId);

        foreach (var message in messages)
        {
            if (message.Id == _config.Discord.Birthday.AnnouncementMessageId)
            {
                continue;
            }

            await message.DeleteAsync();
        }
    }

    private async Task<Dictionary<ulong, GuildUser>> CheckBirthdaysAndExistingUser(Guild guild,
        List<UserBirthday> birthdays,
        CancellationToken cancellationToken)
    {
        Role birthdayRole = guild.Roles[_config.Discord.Birthday.BirthdayChildRoleId];
        Dictionary<ulong, GuildUser> birthdayUsers = new Dictionary<ulong, GuildUser>();

        for (int i = 0; i < birthdays.Count; i++)
        {
            var birthday = birthdays[i];

            GuildUser? guildUser = await GetGuildUser(guild, birthday.DiscordId, cancellationToken);

            if (guildUser is not null)
            {
                birthdayUsers[birthday.DiscordId] = guildUser;

                await guildUser.AddRoleAsync(birthdayRole.Id, cancellationToken: cancellationToken);

                continue;
            }

            _logger.LogWarning("User {UserId} is no longer a member of the guild, removing the birthday entry",
                birthday.DiscordId);

            using var transaction = await _dbTransactionFactory.CreateTransaction();
            await _userBirthdayRepository.DeleteByDiscordAsync(birthday.DiscordId);
            await transaction.Commit(cancellationToken);

            birthdays.RemoveAt(i);
            i--;
        }

        return birthdayUsers;
    }

    /// <summary>
    /// Resolves a guild user. The gateway cache only holds a part of the guild members,
    /// so a cache miss is resolved through the REST api and only a 404 means the user really left.
    /// </summary>
    private async Task<GuildUser?> GetGuildUser(Guild guild, ulong userId, CancellationToken cancellationToken)
    {
        if (guild.Users.TryGetValue(userId, out GuildUser? cachedUser))
        {
            return cachedUser;
        }

        try
        {
            return await _discordClient.Rest.GetGuildUserAsync(guild.Id, userId,
                cancellationToken: cancellationToken);
        }
        catch (RestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
