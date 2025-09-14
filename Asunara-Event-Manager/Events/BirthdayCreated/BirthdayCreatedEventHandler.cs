using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Birthday;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.BirthdayCreated;

public class BirthdayCreatedEventHandler : IRequestHandler<BirthdayCreatedEvent, bool>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly UserBirthdayRepository _birthdayRepository;
    private readonly DiscordSocketClient _client;
    private readonly RootConfig _config;

    public BirthdayCreatedEventHandler(DbTransactionFactory dbTransactionFactory, UserBirthdayRepository birthdayRepository, DiscordSocketClient client, RootConfig config)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _birthdayRepository = birthdayRepository;
        _client = client;
        _config = config;
    }

    public async Task<bool> Handle(BirthdayCreatedEvent request, CancellationToken cancellationToken)
    {
        try
        {
            var birthDay = new DateOnly(request.Year, request.Month, request.Day);

            var age = birthDay.GetAge();
            if (age > 60)
            {
                await SendMessageToEventChat($"Der User <@{request.DiscordUserId}> gibt bei seinem Geburtstag an über 60 zu sein! Alter: {age}, Geburtstag: {birthDay:dd.MM.yyyy}");

                return false;
            }

            if (age < 16)
            {
                await SendMessageToEventChat($"Der User <@{request.DiscordUserId}> gibt bei seinem Geburtstag an unter 16 zu sein! Alter: {age}, Geburtstag: {birthDay:dd.MM.yyyy}");

                return false;
            }

            bool sendHistory = await _birthdayRepository.HasByDiscord(request.DiscordUserId);


            using var transaction = await _dbTransactionFactory.CreateTransaction();

            UserBirthday userBirthday = await _birthdayRepository.GetNewUserBirthday(request.DiscordUserId);
            userBirthday.Birthday = birthDay;

            await transaction.Commit(cancellationToken);

            if (sendHistory)
            {
                await SendHistoryToTeam(request.DiscordUserId);
            }
            
            return true;
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return false;
        }
    }

    private async Task SendMessageToEventChat(string message)
    {
        var channel = _client.GetGuild(_config.Discord.TeamDiscordServerId).GetTextChannel(_config.Discord.EventChatId);
        await channel.SendMessageAsync(message);
    }

    private async Task SendHistoryToTeam(ulong discordUserId)
    {
        EmbedBuilder embedBuilder = new EmbedBuilder();
        embedBuilder.WithAuthor("Event-Manager");
        embedBuilder.WithColor(Color.Purple);
        embedBuilder.WithTitle("Geburtstagsliste");
        embedBuilder.WithDescription($"Es gab eine merkwürdige Änderung an dem Geburtstag von dem User <@{discordUserId}>");
        
        var historyByUserId = await _birthdayRepository.GetHistoryByUserId(discordUserId);
        foreach (UserBirthday birthday in historyByUserId)
        {
            embedBuilder.AddField(birthday.CreationDate.ToString("dd.MM.yyyy HH:mm 'Uhr'"), birthday.Birthday.ToString("dd.MM.yyyy"));
        }
        
        var channel = _client.GetGuild(_config.Discord.TeamDiscordServerId).GetTextChannel(_config.Discord.EventChatId);
        await channel.SendMessageAsync(embed: embedBuilder.Build());
    }
}