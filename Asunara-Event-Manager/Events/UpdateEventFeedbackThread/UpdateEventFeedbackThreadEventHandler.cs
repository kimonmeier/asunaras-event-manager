using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Repositories;
using MediatR;

namespace EventManager.Events.UpdateEventFeedbackThread;

public class UpdateEventFeedbackThreadEventHandler : IRequestHandler<UpdateEventFeedbackThreadEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly EventFeedbackRepository _eventFeedbackRepository;
    private readonly DbTransactionFactory _dbTransactionFactory;
    private readonly RootConfig _config;
    private readonly DiscordSocketClient _client;

    public UpdateEventFeedbackThreadEventHandler(DiscordEventRepository discordEventRepository, EventFeedbackRepository eventFeedbackRepository,
        DbTransactionFactory dbTransactionFactory, RootConfig config, DiscordSocketClient client)
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

        SocketTextChannel feedbackTextChannel = _client.GetGuild(_config.Discord.TeamDiscordServerId).GetTextChannel(_config.Discord.Event.FeedbackChannelId);
        EmbedBuilder embedBuilder = new EmbedBuilder()
            .WithAuthor("Event-Manager")
            .WithColor(Color.Green)
            .WithTitle($"{@event.Name} vom {@event.Date.ToShortDateString()}")
            .WithDescription($"Dies ist das Feedback für das {@event.Name} Event was am {@event.Date.ToShortDateString()} stattgefunden hat!")
            .AddField("Bewertung", $"{(feedbacks.Any() ? feedbacks.Average(x => x.Score) : "?")} / 5");

        if (@event.FeedbackMessage.HasValue)
        {
            await feedbackTextChannel.ModifyMessageAsync(@event.FeedbackMessage.Value, x =>
            {
                x.Embed = embedBuilder.Build();
            });

            return;
        }

        var message = await feedbackTextChannel.SendMessageAsync(embed: embedBuilder.Build());
        await feedbackTextChannel.CreateThreadAsync($"Feedbacks für {@event.Name}", ThreadType.PublicThread, ThreadArchiveDuration.OneWeek, message);

        var transaction = _dbTransactionFactory.CreateTransaction();
        @event.FeedbackMessage = message.Id;

        await _discordEventRepository.UpdateAsync(@event);
        await transaction.Commit(cancellationToken);
    }
}