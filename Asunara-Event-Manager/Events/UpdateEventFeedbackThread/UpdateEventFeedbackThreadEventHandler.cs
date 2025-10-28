using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Repositories;
using EventManager.Extensions;
using MediatR;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.UpdateEventFeedbackThread;

public class UpdateEventFeedbackThreadEventHandler : IRequestHandler<UpdateEventFeedbackThreadEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly EventFeedbackRepository _eventFeedbackRepository;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly RootConfig _config;
    private readonly GatewayClient _client;

    public UpdateEventFeedbackThreadEventHandler(DiscordEventRepository discordEventRepository,
        EventFeedbackRepository eventFeedbackRepository,
        DbTransactionFactory dbTransactionFactory, RootConfig config, GatewayClient client)
    {
        _discordEventRepository = discordEventRepository;
        _eventFeedbackRepository = eventFeedbackRepository;
        _dbTransactionFactory = dbTransactionFactory;
        _config = config;
        _client = client;
    }

    public async Task Handle(UpdateEventFeedbackThreadEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByDiscordId(request.DiscordEventId);

        if (@event is null)
        {
            return;
        }

        var feedbacks = await _eventFeedbackRepository.GetByDiscordEvent(@event.Id);

        TextChannel feedbackTextChannel = _client.Cache.Guilds[_config.Discord.TeamDiscordServerId]
            .GetTextChannel(_config.Discord.Event.FeedbackChannelId);
        EmbedProperties embedBuilder = new EmbedProperties()
            .WithAuthor(new EmbedAuthorProperties().WithName("Event-Manager"))
            .WithColor(new Color(0, 255, 0))
            .WithTitle($"{@event.Name} vom {@event.Date.ToShortDateString()}")
            .WithDescription(
                $"Dies ist das Feedback für das {@event.Name} Event was am {@event.Date.ToShortDateString()} stattgefunden hat!"
            )
            .WithFields([
                new EmbedFieldProperties()
                {
                    Name = "Bewertung",
                    Value = $"{(feedbacks.Any() ? feedbacks.Average(x => x.Score).ToString("F2") : "?")} / 5"
                },
                new EmbedFieldProperties() { Name = "Anzahl Bewertungen", Value = feedbacks.Count.ToString() }
            ]);

        var messageProperties = new MessageProperties();
        messageProperties.WithEmbeds([embedBuilder]);
        if (@event.FeedbackMessage.HasValue)
        {
            await feedbackTextChannel.ModifyMessageAsync(@event.FeedbackMessage.Value,
                x => { x.WithEmbeds([embedBuilder]); }, cancellationToken: cancellationToken);

            return;
        }

        GuildThreadFromMessageProperties guildThreadFromMessageProperties =
                new GuildThreadFromMessageProperties($"Feedbacks für {@event.Name}")
                {
                    AutoArchiveDuration = ThreadArchiveDuration.OneWeek,
                }
            ;
        var message =
            await feedbackTextChannel.SendMessageAsync(messageProperties, cancellationToken: cancellationToken);
        await message.CreateGuildThreadAsync(guildThreadFromMessageProperties, cancellationToken: cancellationToken);

        var transaction = await _dbTransactionFactory.CreateTransaction();

        @event.FeedbackMessage = message.Id;

        await _discordEventRepository.UpdateAsync(@event);
        await transaction.Commit(cancellationToken);
    }
}