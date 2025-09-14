using System.Text;
using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.CheckBirthday;

public class CheckBirthdayEventHandler : IRequestHandler<CheckBirthdayEvent>
{
    private readonly UserBirthdayRepository _userBirthdayRepository;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly RootConfig _config;
    
    public CheckBirthdayEventHandler(UserBirthdayRepository userBirthdayRepository, DiscordSocketClient discordSocketClient, RootConfig config)
    {
        _userBirthdayRepository = userBirthdayRepository;
        _discordSocketClient = discordSocketClient;
        _config = config;
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

        foreach (var birthday in currentBirthdays)
        {
            SocketGuildUser socketGuildUser = guild.GetUser(birthday.DiscordId);
            await socketGuildUser.AddRoleAsync(birthdayRole);
        }

        var birthdayMessages = _config.Discord.Birthday.Messages;
        var messageIndex = Random.Shared.Next(0, birthdayMessages.Length - 1);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(string.Format(birthdayMessages[messageIndex], _config.Discord.Birthday.BirthdayChildRoleId));
        builder.AppendLine($"|| <@&{_config.Discord.Birthday.BirthdayNotificationRoleId}> ||");
        
        await birthdayChannel.SendMessageAsync(builder.ToString());
        await guild.GetTextChannel(_config.Discord.HauptchatChannelId).SendMessageAsync(builder.ToString());
    }
}