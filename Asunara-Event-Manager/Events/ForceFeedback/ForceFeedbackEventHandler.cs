using EventManager.Data.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.ForceFeedback;

public class ForceFeedbackEventHandler : IRequestHandler<ForceFeedbackEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ILogger<ForceFeedbackEventHandler> _logger;

    public ForceFeedbackEventHandler(DiscordEventRepository discordEventRepository, GatewayClient client, ILogger<ForceFeedbackEventHandler> logger)
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
        
        DMChannel dmChannel = await request.User.GetDMChannelAsync();

        if (dmChannel is null)
        {
            _logger.LogError("Could not find DMChannel for user {UserId}", request.User.Id);;

            return;
        }


        MessageProperties messageProperties = new();
        messageProperties.Content =
            $"Hallöchen du hast gerade beim Event \"{@event.Name}\" teilgenommen. Wir hoffen es hat dir gefallen und wir würden uns über eine Bewertung freuen!";
        messageProperties.AddComponents(new ActionRowProperties()
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{@event.DiscordId}", "⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{@event.DiscordId}", "⭐⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{@event.DiscordId}", "⭐⭐⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{@event.DiscordId}", "⭐⭐⭐⭐",
                ButtonStyle.Primary))
            .AddComponents(new ButtonProperties(
                $"{Konst.ButtonFeedbackStarGroup}{Konst.PayloadDelimiter}1{Konst.PayloadDelimiter}{@event.DiscordId}", "⭐⭐⭐⭐⭐",
                ButtonStyle.Primary)));

        try
        {
            await dmChannel.SendMessageAsync(messageProperties);
        }
        catch (Exception exception)
        {
            SentrySdk.CaptureException(exception);
            _logger.LogError("The User {0} has DM's disabled!", request.User.Id);
        }
    }
}