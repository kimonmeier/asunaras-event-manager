using Discord;
using Discord.WebSocket;
using EventManager.Data.Entities.Events;
using EventManager.Data.Repositories;
using EventManager.Events.CheckFskRestrictionOnUser;
using EventManager.Models.Restrictions;
using MediatR;

namespace EventManager.Events.MemberJoinedChannel;

public class MemberJoinedChannelEventHandler : IRequestHandler<MemberJoinedChannelEvent>
{
    private readonly DiscordEventRepository _discordEventRepository;
    private readonly ISender _sender;

    public MemberJoinedChannelEventHandler(DiscordEventRepository discordEventRepository, ISender sender)
    {
        _discordEventRepository = discordEventRepository;
        _sender = sender;
    }

    public async Task Handle(MemberJoinedChannelEvent request, CancellationToken cancellationToken)
    {
        var guild = request.Channel.Guild;
        var events = await _discordEventRepository.GetAllUncompleted();

        DiscordEvent? @event = events.SingleOrDefault(x => guild.GetEvent(x.DiscordId)?.Status == GuildScheduledEventStatus.Active);

        if (@event is null)
        {
            return;
        }
        
        var checkResult = await _sender.Send(new CheckFskRestrictionOnUserEvent()
        {
            User = request.User,
            Event = (await _discordEventRepository.FindWithRestrictionsByDiscordId(@event.DiscordId))!
        });

        if (checkResult.Success)
        {
            return;
        }
        
        await request.User.ModifyAsync(x => x.Channel = null);
        IDMChannel dmChannel = await request.User.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync($"Hallöchen, leider entsprichst du nicht den Altersregeln für dieses Event: \"{checkResult.ErrorMessage}\"");
    }
}