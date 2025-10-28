using EventManager.Configuration;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using EventManager.Events.CheckFskRestrictionOnUser;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;

namespace EventManager.Events.MemberAddedEvent;

public class MemberAddedEventEventHandler : IRequestHandler<MemberAddedEventEvent>
{
    private readonly GatewayClient _client;
    private readonly RootConfig _config;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ISender _sender;

    private ILogger<MemberAddedEventEventHandler> _logger;

    public MemberAddedEventEventHandler(DiscordEventRepository discordEventRepository, ISender sender, GatewayClient client, RootConfig config, ILogger<MemberAddedEventEventHandler> logger)
    {
        _discordEventRepository = discordEventRepository;
        _sender = sender;
        _client = client;
        _config = config;
        _logger = logger;
    }

    public async Task Handle(MemberAddedEventEvent request, CancellationToken cancellationToken)
    {
        Guild guild = _client.Cache.Guilds[request.GuildId];
        var @event = guild.ScheduledEvents[request.EventId];
        var discordEvent = await _discordEventRepository.FindWithRestrictionsByDiscordId(request.EventId);
        if (discordEvent is null)
        {
            if (@event.CreatorId != request.UserId)
            {
                throw new Exception("Event not found");
            }

            _logger.LogInformation("Event not found for DiscordId: {DiscordId}, but it is the creator of the event",
                @event.Id);

            return;

        }

        GuildUser user = _client.Cache.Guilds[_config.Discord.MainDiscordServerId].Users[request.UserId];

        await _sender.Send(new CheckForUserPreferenceOnEventInterestedEvent()
        {
            DiscordUser = user
        }, cancellationToken);
        
        var checkResult = await _sender.Send(new CheckFskRestrictionOnUserEvent()
        {
            Event = discordEvent, User = user
        }, cancellationToken);

        if (!checkResult.Success)
        {
            DMChannel channelAsync = await guild.Users[request.UserId].GetDMChannelAsync(cancellationToken: cancellationToken);
            await channelAsync.SendMessageAsync(
                $"Hallo\nLeider wurde beim Überprüfen deines Alters festgestellt, dass du nicht für dieses Event zugelassen bist, der Grund dafür ist: \"{checkResult.ErrorMessage}\"\nDu bist aber sehr gerne an einem anderen Event willkommen", cancellationToken: cancellationToken);
        }
    }
}