using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using MediatR;
using NetCord;
using NetCord.Rest;

namespace EventManager.Events.CheckForUserPreferenceOnEventInterested;

public class
    CheckForUserPreferenceOnEventInterestedEventHandler : IRequestHandler<CheckForUserPreferenceOnEventInterestedEvent>
{
    private readonly UserPreferenceRepository _repository;
    private readonly DbTransactionFactory _transactionFactory;
    private readonly RootConfig _rootConfig;

    public CheckForUserPreferenceOnEventInterestedEventHandler(UserPreferenceRepository repository,
        DbTransactionFactory transactionFactory, RootConfig rootConfig)
    {
        _repository = repository;
        _transactionFactory = transactionFactory;
        _rootConfig = rootConfig;
    }

    public async Task Handle(CheckForUserPreferenceOnEventInterestedEvent request, CancellationToken cancellationToken)
    {
        var hasSaved = await _repository.HasByDiscordAsync(request.DiscordUser.Id);

        if (hasSaved)
        {
            return;
        }

        DMChannel dmChannel = await request.DiscordUser.GetDMChannelAsync(cancellationToken: cancellationToken);

        await CreateEntity(request.DiscordUser.Id, cancellationToken);
        await SendMessagesAsync(dmChannel);
    }

    private async Task CreateEntity(ulong discordUserId, CancellationToken cancellationToken)
    {
        var transaction = await _transactionFactory.CreateTransaction();

        await _repository.AddAsync(new UserPreference()
        {
            DiscordUserId = discordUserId
        });

        await transaction.Commit(cancellationToken);
    }

    private async Task SendMessagesAsync(DMChannel dmChannel)
    {
        await dmChannel.SendMessageAsync(
            "Hallöchen Freunde!\nIch darf doch Freunde sagen?\n\nIch habe gesehen, dass du dich für ein Event auf dem Midnight Café Discord interessierst. Um dich bei zukünftigen Events optimal zu unterstützen habe ich zwei Fragen an dich und wäre froh wenn du diese beantworten könntest!");


        MessageProperties reminderMessage = new MessageProperties();
        reminderMessage.Content =
            "Darf ich dich für Events, bei welchen du dich als interessiert eingetragen hast, im vorhinein per Privat-Nachricht auf das Event hinweisen?";

        reminderMessage.AddComponents(new ActionRowProperties()
            .AddComponents(new ButtonProperties(Konst.ButtonReminderYes,
                EmojiProperties.Custom(_rootConfig.Discord.Emote.Yes), ButtonStyle.Secondary))
            .AddComponents(new ButtonProperties(Konst.ButtonReminderNo,
                EmojiProperties.Custom(_rootConfig.Discord.Emote.No), ButtonStyle.Secondary)));

        MessageProperties eventFeedbackMessage = new MessageProperties();
        eventFeedbackMessage.Content =
            "Darf ich dich nach Events bei welchen du Teilgenommen hast, nach Feedback fragen?";

        reminderMessage.AddComponents(new ActionRowProperties()
            .AddComponents(new ButtonProperties(Konst.ButtonFeedbackYes,
                EmojiProperties.Custom(_rootConfig.Discord.Emote.Yes), ButtonStyle.Secondary))
            .AddComponents(new ButtonProperties(Konst.ButtonFeedbackNo,
                EmojiProperties.Custom(_rootConfig.Discord.Emote.No), ButtonStyle.Secondary)));

        await dmChannel.SendMessageAsync(reminderMessage);
        await dmChannel.SendMessageAsync(eventFeedbackMessage);
    }
}