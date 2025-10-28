using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Repositories;
using EventManager.Extensions;
using MediatR;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.EventExtendedFeedback;

public class EventExtendedFeedbackEventHandler : IRequestHandler<EventExtendedFeedbackEvent>
{
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly EventFeedbackRepository _eventFeedbackRepository;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly GatewayClient _client;
    private readonly RootConfig _config;

    public EventExtendedFeedbackEventHandler(DbTransactionFactory dbTransactionFactory,
        EventFeedbackRepository eventFeedbackRepository, DiscordEventRepository discordEventRepository,
        GatewayClient client, RootConfig config)
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

        var transaction = await _dbTransactionFactory.CreateTransaction();
        ;

        var eventFeedback =
            await _eventFeedbackRepository.GetOrCreateByDiscordEventAndUser(@event.Id, request.DiscordUserId);
        eventFeedback.Critic = request.Critic;
        eventFeedback.Good = request.Good;
        eventFeedback.Suggestion = request.Suggestion;


        EmbedProperties feedbackEmbed = new EmbedProperties()
            .WithAuthor(new EmbedAuthorProperties() { Name = "Event-Manager" })
            .WithColor(new Color(124, 0, 124))
            .AddFields(
                new EmbedFieldProperties()
                {
                    Name = "Ersteller",
                    Value = eventFeedback.Anonymous
                        ? "Anonym!"
                        : _client.Cache.Guilds[_config.Discord.MainDiscordServerId].Users[request.DiscordUserId]
                            .Nickname
                },
                new EmbedFieldProperties() { Name = "Sterne", Value = $"{eventFeedback.Score:F2} / 5 Sterne" },
                new EmbedFieldProperties()
                    { Name = "Was dem User gefallen hat", Value = FormatField(eventFeedback.Good) },
                new EmbedFieldProperties()
                    { Name = "Was dem User nicht gefallen hat", Value = FormatField(eventFeedback.Critic) },
                new EmbedFieldProperties()
                    { Name = "Was der User verbessern würde", Value = FormatField(eventFeedback.Suggestion) }
            );

        var message = await _client
            .Cache
            .Guilds[_config.Discord.TeamDiscordServerId]
            .GetTextChannel(_config.Discord.Event.FeedbackChannelId)
            .GetMessageAsync(@event.FeedbackMessage!.Value, cancellationToken: cancellationToken);

        if (@eventFeedback.FeedbackMessageId.HasValue)
        {
            await message.StartedThread!.ModifyMessageAsync(@eventFeedback.FeedbackMessageId.Value,
                x => { x.AddEmbeds(feedbackEmbed); }, cancellationToken: cancellationToken);
        }
        else
        {
            var userMessage = await message.StartedThread!.SendMessageAsync(
                new MessageProperties().AddEmbeds(feedbackEmbed), cancellationToken: cancellationToken);

            eventFeedback.FeedbackMessageId = userMessage.Id;
        }

        await transaction.Commit(cancellationToken);
    }

    private string FormatField(string? field)
    {
        return string.IsNullOrWhiteSpace(field) ? "_Keine Angabe_" : field;
    }
}