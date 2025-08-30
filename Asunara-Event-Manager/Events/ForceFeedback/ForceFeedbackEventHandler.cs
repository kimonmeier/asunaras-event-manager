using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.ForceFeedback;

public class ForceFeedbackEventHandler : IRequestHandler<ForceFeedbackEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ILogger<ForceFeedbackEventHandler> _logger;

    public ForceFeedbackEventHandler(DiscordEventRepository discordEventRepository, DiscordSocketClient client, ILogger<ForceFeedbackEventHandler> logger)
    {
        _discordEventRepository = discordEventRepository;
        _logger = logger;
    }

    public async Task Handle(ForceFeedbackEvent request, CancellationToken cancellationToken)
    {
        var @event = await _discordEventRepository.FindByEntityAsync(request.EventId);

        if (@event is null)
        {
            throw new Exception("Event not found");
        }
        
        IDMChannel dmChannel = await request.User.CreateDMChannelAsync();

        if (dmChannel is null)
        {
            _logger.LogError("Could not find DMChannel for user {UserId}", request.User.Id);;

            return;
        }

        ComponentBuilder feedbackComponent = new ComponentBuilder()
                .AddRow(new ActionRowBuilder()
                    .WithButton("⭐", $"{Konst.ButtonFeedback1Star}{Konst.PayloadDelimiter}{@event.DiscordId}")
                    .WithButton("⭐⭐", $"{Konst.ButtonFeedback2Star}{Konst.PayloadDelimiter}{@event.DiscordId}")
                    .WithButton("⭐⭐⭐", $"{Konst.ButtonFeedback3Star}{Konst.PayloadDelimiter}{@event.DiscordId}")
                    .WithButton("⭐⭐⭐⭐", $"{Konst.ButtonFeedback4Star}{Konst.PayloadDelimiter}{@event.DiscordId}")
                    .WithButton("⭐⭐⭐⭐⭐", $"{Konst.ButtonFeedback5Star}{Konst.PayloadDelimiter}{@event.DiscordId}")
                )
            ;


        await dmChannel.SendMessageAsync(
            $"Hallöchen du hast gerade beim Event \"{@event.Name}\" teilgenommen. Wir hoffen es hat dir gefallen und wir würden uns über eine Bewertung freuen!",
            components: feedbackComponent.Build());
    }
}