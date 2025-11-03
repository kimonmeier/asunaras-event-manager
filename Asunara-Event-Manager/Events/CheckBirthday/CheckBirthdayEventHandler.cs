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

namespace EventManager.Events.CheckBirthday;

public class CheckBirthdayEventHandler : IRequestHandler<CheckBirthdayEvent>
{
    private readonly UserBirthdayRepository _userBirthdayRepository;
    private readonly GatewayClient _discordClient;
    private readonly RootConfig _config;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly ILogger<CheckBirthdayEventHandler> _logger;

    public CheckBirthdayEventHandler(UserBirthdayRepository userBirthdayRepository, GatewayClient discordClient, RootConfig config,
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
        await RemoveBirthdayRoleOnOldUsers(birthdayRole);

        var currentBirthdays = await _userBirthdayRepository.GetCurrentBirthday(DateTime.Now.Month, DateTime.Now.Day);

        if (currentBirthdays.Count == 0)
        {
            await birthdayChannel.SendMessageAsync("Leider gibt es heute keine Geburtstage!", cancellationToken: cancellationToken);

            return;
        }

        await CheckBirthdaysAndExistingUser(guild, currentBirthdays, cancellationToken);

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

            GuildUser guildUser = guild.Users[birthday.DiscordId];

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

        await guild.GetTextChannel(_config.Discord.HauptchatChannelId).SendMessageAsync(builder.ToString(), cancellationToken: cancellationToken);
        if (hasComfort)
        {
            await guild.GetTextChannel(_config.Discord.Comfort.ChannelId).SendMessageAsync(comfortBuilder.ToString(), cancellationToken: cancellationToken);
        }
    }

    private async Task RemoveBirthdayRoleOnOldUsers(Role birthdayRole)
    {
        foreach (GuildUser roleMember in birthdayRole.GetUser(_discordClient))
        {
            await roleMember.RemoveRoleAsync(birthdayRole.Id);
        }
    }

    private async Task ClearBirthdayChannel(TextChannel birthdayChannel)
    {
        var messages = await birthdayChannel.GetMessagesAroundAsync( _config.Discord.Birthday.AnnouncementMessageId);

        foreach (var message in messages)
        {
            if (message.Id == _config.Discord.Birthday.AnnouncementMessageId)
            {
                continue;
            }
            await message.DeleteAsync();
        }
    }
    
    private async Task CheckBirthdaysAndExistingUser(Guild guild, List<UserBirthday> birthdays, CancellationToken cancellationToken)
    {
        Role birthdayRole = guild.Roles[_config.Discord.Birthday.BirthdayChildRoleId];

        for (int i = 0; i < birthdays.Count; i++)
        {
            var birthday = birthdays[i];
            
            GuildUser? socketGuildUser = guild.Users.GetValueOrDefault(birthday.DiscordId);

            if (socketGuildUser is not null)
            {
                await socketGuildUser.AddRoleAsync(birthdayRole.Id, cancellationToken: cancellationToken);
                
                continue;
            }
            
            using var transaction = await _dbTransactionFactory.CreateTransaction();
            await _userBirthdayRepository.DeleteByDiscordAsync(birthday.DiscordId);
            await transaction.Commit(cancellationToken);
                
            birthdays.RemoveAt(i);
            i--;
        }
    }
}