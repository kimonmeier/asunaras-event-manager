using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.EventExtendedFeedback;

public class EventExtendedFeedbackEventHandler : IRequestHandler<EventExtendedFeedbackEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly EventFeedbackRepository _eventFeedbackRepository;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly DiscordSocketClient _client;
    private readonly RootConfig _config;

    public EventExtendedFeedbackEventHandler(DbTransactionFactory dbTransactionFactory, EventFeedbackRepository eventFeedbackRepository, DiscordEventRepository discordEventRepository, DiscordSocketClient client, RootConfig config)
    {
        _dbTransactionFactory = dbTransactionFactory;
        _eventFeedbackRepository = eventFeedbackRepository;
        _discordEventRepository = discordEventRepository;
        _client = client;
        _config = config;
    }

    public async Task Handle(EventExtendedFeedbackEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);

        if (@event is null)
        {
            throw new Exception("Event not found");
        }
        
        var transaction = _dbTransactionFactory.CreateTransaction();
        
        var eventFeedback = await _eventFeedbackRepository.GetOrCreateByDiscordEventAndUser(@event.Id, request.DiscordUserId);
        eventFeedback.Critic = request.Critic;
        eventFeedback.Good = request.Good;
        eventFeedback.Suggestion = request.Suggestion;
        
        await transaction.Commit(cancellationToken);

        IMessage message = await _client.GetGuild(_config.Discord.TeamDiscordServerId).GetTextChannel(_config.Discord.Event.FeedbackChannelId).GetMessageAsync(@event.FeedbackMessage!.Value);
        await message.Thread.SendMessageAsync(embed:
            new EmbedBuilder()
            .WithAuthor("Event-Manager")
            .WithColor(Color.Purple)
            .AddField("Ersteller", eventFeedback.Anonymous ? "Anonym!" : _client.GetGuild(_config.Discord.MainDiscordServerId).GetUser(request.DiscordUserId).DisplayName)
            .AddField("Sterne", $"{eventFeedback.Score} / 5 Sternen")
            .AddField("Was dem User gefallen hat", FormatField(eventFeedback.Good))
            .AddField("Was dem User nicht gefallen hat", FormatField(eventFeedback.Critic))
            .AddField("Was der User verbessern würde", FormatField(eventFeedback.Suggestion))
            .Build()
        );
    }

    private string FormatField(string? field)
    {
        return string.IsNullOrWhiteSpace(field) ? "_Keine Angabe_" : field;
    }
}