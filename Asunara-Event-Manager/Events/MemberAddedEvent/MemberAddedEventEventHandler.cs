using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Data.Repositories;
using EventManager.Events.CheckForUserPreferenceOnEventInterested;
using EventManager.Events.CheckFskRestrictionOnUser;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManager.Events.MemberAddedEvent;

public class MemberAddedEventEventHandler : IRequestHandler<MemberAddedEventEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly RootConfig _config;
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ISender _sender;

    private ILogger<MemberAddedEventEventHandler> _logger;

    public MemberAddedEventEventHandler(DiscordEventRepository discordEventRepository, ISender sender, DiscordSocketClient client, RootConfig config, ILogger<MemberAddedEventEventHandler> logger)
    {
        _discordEventRepository = discordEventRepository;
        _sender = sender;
        _client = client;
        _config = config;
        _logger = logger;
    }

    public async Task Handle(MemberAddedEventEvent request, CancellationToken cancellationToken)
    {
        var discordEvent = await _discordEventRepository.FindWithRestrictionsByDiscordId(request.Event.Id);
        if (discordEvent is null)
        {
            if (request.Event.Creator.Id != request.User.Id)
            {
                throw new Exception("Event not found");
            }

            _logger.LogInformation("Event not found for DiscordId: {DiscordId}, but it is the creator of the event", request.Event.Id);

            return;

        }

        SocketGuildUser user = _client.GetGuild(_config.Discord.MainDiscordServerId).GetUser(request.User.Id);

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
            IDMChannel channelAsync = await request.User.CreateDMChannelAsync();
            await channelAsync.SendMessageAsync(
                $"Hallo\nLeider wurde beim Überprüfen deines Alters festgestellt, dass du nicht für dieses Event zugelassen bist, der Grund dafür ist: \"{checkResult.ErrorMessage}\"\nDu bist aber sehr gerne an einem anderen Event willkommen");
        }
    }
}