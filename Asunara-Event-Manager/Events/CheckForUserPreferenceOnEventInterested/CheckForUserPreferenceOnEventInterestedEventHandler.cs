using Discord;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Entities.Notifications;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.CheckForUserPreferenceOnEventInterested;

public class CheckForUserPreferenceOnEventInterestedEventHandler : IRequestHandler<CheckForUserPreferenceOnEventInterestedEvent>
{
    private readonly UserPreferenceRepository _repository;
    private readonly DbTransactionFactory _transactionFactory;
    private readonly RootConfig _rootConfig;

    public CheckForUserPreferenceOnEventInterestedEventHandler(UserPreferenceRepository repository, DbTransactionFactory transactionFactory, RootConfig rootConfig)
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

        IDMChannel? dmChannel = await request.DiscordUser.CreateDMChannelAsync();
        if (dmChannel is null)
        {
            return;
        }

        await CreateEntity(request.DiscordUser.Id, cancellationToken);
        await SendMessagesAsync(dmChannel);
    }
    
    private async Task CreateEntity(ulong discordUserId, CancellationToken cancellationToken)
    {
        var transaction = _transactionFactory.CreateTransaction();

        await _repository.AddAsync(new UserPreference()
        {
            DiscordUserId = discordUserId
        });
        
        await transaction.Commit(cancellationToken);
    }

    private async Task SendMessagesAsync(IDMChannel dmChannel)
    {
        await dmChannel.SendMessageAsync(
            "Hallöchen Freunde!\nIch darf doch Freunde sagen?\n\nIch habe gesehen, dass du dich für ein Event auf dem Asunara Discord interessierst. Um dich bei zukünftigen Events optimal zu unterstützen habe ich zwei Fragen an dich und wäre froh wenn du diese beantworten könntest!");

        var reminderComponent = new ComponentBuilder()
            .WithButton(ButtonBuilder.CreatePrimaryButton("Ja", Konst.ButtonReminderYes, Emote.Parse(_rootConfig.Discord.Emote.Yes)))
            .WithButton(ButtonBuilder.CreateSecondaryButton("Nein", Konst.ButtonReminderNo, Emote.Parse(_rootConfig.Discord.Emote.No)))
            .Build();

        var eventFeedbackComponent = new ComponentBuilder()
            .WithButton(ButtonBuilder.CreatePrimaryButton("Ja", Konst.ButtonFeedbackYes, Emote.Parse(_rootConfig.Discord.Emote.Yes)))
            .WithButton(ButtonBuilder.CreateSecondaryButton("Nein", Konst.ButtonFeedbackNo, Emote.Parse(_rootConfig.Discord.Emote.No)))
            .Build();

        await dmChannel.SendMessageAsync("Darf ich dich für Events, bei welchen du dich als interessiert eingetragen hast, im vorhinein per Privat-Nachricht auf das Event hinweisen?",
            components: reminderComponent);

        await dmChannel.SendMessageAsync("Darf ich dich nach Events bei welchen du Teilgenommen hast, nach Feedback fragen?",
            components: eventFeedbackComponent);
    }
}