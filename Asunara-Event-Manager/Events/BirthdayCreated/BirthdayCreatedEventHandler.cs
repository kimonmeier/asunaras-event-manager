using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Birthday;
using EventManager.Data.Repositories;
using MediatR;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.BirthdayCreated;

public class BirthdayCreatedEventHandler : IRequestHandler<BirthdayCreatedEvent, bool>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly UserBirthdayRepository _birthdayRepository;
    private readonly GatewayClient _client;
    private readonly RootConfig _config;

    public BirthdayCreatedEventHandler(DbTransactionFactory dbTransactionFactory, UserBirthdayRepository birthdayRepository, GatewayClient client, RootConfig config)
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
            var birthDay = new DateOnly(request.Year ?? 1, request.Month, request.Day);

            var age = birthDay.GetAge();
            if (request.Year is not null)
            {
                if (age > 60)
                {
                    await SendMessageToEventChat(
                        $"Der User <@{request.DiscordUserId}> ({request.DiscordUserId}) gibt bei seinem Geburtstag an über 60 zu sein! Alter: {age}, Geburtstag: {birthDay:dd.MM.yyyy}");
                }

                if (age < 16)
                {
                    await SendMessageToEventChat(
                        $"Der User <@{request.DiscordUserId}> ({request.DiscordUserId}) gibt bei seinem Geburtstag an unter 16 zu sein! Alter: {age}, Geburtstag: {birthDay:dd.MM.yyyy}");
                }
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
        var channel = _client.Cache.Guilds[_config.Discord.TeamDiscordServerId].Channels[_config.Discord.EventChatId] as TextGuildChannel;
        await channel!.SendMessageAsync(message);
    }

    private async Task SendHistoryToTeam(ulong discordUserId)
    {
        EmbedProperties embedBuilder = new EmbedProperties();
        embedBuilder.WithAuthor(new EmbedAuthorProperties().WithName("Event-Manager"));
        embedBuilder.WithColor(new Color(128, 0, 128));
        embedBuilder.WithTitle("Geburtstagsliste");
        embedBuilder.WithDescription($"Es gab eine merkwürdige Änderung an dem Geburtstag von dem User <@{discordUserId}>");

        var historyByUserId = await _birthdayRepository.GetHistoryByUserId(discordUserId);

        // Wenn die Leute den gleichen Geburtstag eintragen einfach ignorieren!
        if (historyByUserId.GroupBy(x => x.Birthday).Count() <= 1)
        {
            return;
        }

        foreach (UserBirthday birthday in historyByUserId)
        {
            embedBuilder.AddFields(new EmbedFieldProperties()
                .WithName(birthday.CreationDate.ToString("dd.MM.yyyy HH:mm 'Uhr'"))
                .WithValue(birthday.Birthday.ToString("dd.MM.yyyy")));
        }

        var channel = _client.Cache.Guilds[_config.Discord.TeamDiscordServerId].Channels[_config.Discord.Birthday.BirthdayTeamNotificationChannelId] as TextGuildChannel;
        await channel!.SendMessageAsync(new MessageProperties().AddEmbeds(embedBuilder));
    }
}