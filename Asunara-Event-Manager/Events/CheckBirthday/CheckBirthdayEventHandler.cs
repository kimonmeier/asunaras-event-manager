using System.Text;
using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.CheckBirthday;

public class CheckBirthdayEventHandler : IRequestHandler<CheckBirthdayEvent>
{
    private readonly UserBirthdayRepository _userBirthdayRepository;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly RootConfig _config;
    private readonly DbTransactionFactory _dbTransactionFactory;
    
    public CheckBirthdayEventHandler(UserBirthdayRepository userBirthdayRepository, DiscordSocketClient discordSocketClient, RootConfig config, DbTransactionFactory dbTransactionFactory)
    {
        _userBirthdayRepository = userBirthdayRepository;
        _discordSocketClient = discordSocketClient;
        _config = config;
        _dbTransactionFactory = dbTransactionFactory;
    }

    public async Task Handle(CheckBirthdayEvent request, CancellationToken cancellationToken)
    {
        var guild = _discordSocketClient.GetGuild(_config.Discord.MainDiscordServerId);
        var birthdayRole = guild.GetRole(_config.Discord.Birthday.BirthdayChildRoleId);
        var birthdayChannel = guild.GetTextChannel(_config.Discord.Birthday.ChannelId);

        IEnumerable<IMessage> messages = await birthdayChannel.GetMessagesAsync(_config.Discord.Birthday.AnnouncementMessageId, Direction.After, 1000).FlattenAsync();

        foreach (var message in messages)
        {
            await message.DeleteAsync();
        }

        foreach (SocketGuildUser roleMember in birthdayRole.Members)
        {
            await roleMember.RemoveRoleAsync(birthdayRole);
        }
        
        var currentBirthdays = await _userBirthdayRepository.GetCurrentBirthday(DateTime.Now.Month, DateTime.Now.Day);

        if (currentBirthdays.Count == 0)
        {
            await birthdayChannel.SendMessageAsync("Leider gibt es heute keine Geburtstage!");
            return;
        }

        foreach (var birthday in currentBirthdays)
        {
            SocketGuildUser socketGuildUser = guild.GetUser(birthday.DiscordId);

            if (socketGuildUser is null)
            {
                using var transaction = await _dbTransactionFactory.CreateTransaction();
                await _userBirthdayRepository.DeleteByDiscordAsync(birthday.DiscordId);
                await transaction.Commit(cancellationToken);
                continue;
            }
            
            await socketGuildUser.AddRoleAsync(birthdayRole);
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

            if (guild.GetUser(birthday.DiscordId).Roles.Any(x => x.Id == _config.Discord.Comfort.ComfortRoleId))
            {
                comfortBuilder.AppendLine(line);
                hasComfort = true;
            }

            builder.AppendLine(line);
        }
        comfortBuilder.AppendLine($"Lasst den Krümeln eine schöne Nachricht da!");
        comfortBuilder.AppendLine($"|| <@&{_config.Discord.Comfort.ComfortRoleId}> ||");
        builder.AppendLine($"|| <@&{_config.Discord.Birthday.BirthdayNotificationRoleId}> ||");
        
        await birthdayChannel.SendMessageAsync(builder.ToString());
        
        await guild.GetTextChannel(_config.Discord.HauptchatChannelId).SendMessageAsync(builder.ToString());
        if (hasComfort)
        {
            await guild.GetTextChannel(_config.Discord.Comfort.ChannelId).SendMessageAsync(comfortBuilder.ToString());
        }
    }
}