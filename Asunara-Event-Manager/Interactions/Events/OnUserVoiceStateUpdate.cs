using EventManager.Events.CheckVoiceActivityForChannel;
using EventManager.Events.MemberJoinedChannel;
using EventManager.Events.MemberLeftChannel;
using EventManager.Extensions;
using EventManager.Services;
using MediatR;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnUserVoiceStateUpdate(
    ISender sender,
    AudioService audioService,
    VoiceStateHistoryService voiceStateHistoryService,
    GatewayClient client) : IVoiceStateUpdateGatewayHandler
{
    public async ValueTask HandleAsync(VoiceState currentVoiceState)
    {
        var prevVoiceState = voiceStateHistoryService.GetLastVoiceState(currentVoiceState.UserId);
        voiceStateHistoryService.AddLastVoiceState(currentVoiceState);
        var guild = client.Cache.Guilds[currentVoiceState.GuildId];
        var currentUser = guild.Users[currentVoiceState.UserId];

        if (currentUser.IsBot)
        {
            await ProcessVoiceChannelKickOnBot(prevVoiceState, currentVoiceState);

            return;
        }

        if (currentVoiceState.ChannelId is not null && prevVoiceState?.ChannelId is not null &&
            currentVoiceState.ChannelId != prevVoiceState.ChannelId)
        {
            await sender.Send(new MemberLeftChannelEvent()
            {
                Channel = guild.GetVoiceGuildChannel(prevVoiceState.ChannelId.Value), User = currentUser,
            });

            await sender.Send(new MemberJoinedChannelEvent()
            {
                Channel = guild.GetVoiceGuildChannel(currentVoiceState.ChannelId.Value), User = currentUser
            });
        }
        else if (currentVoiceState.ChannelId is null)
        {
            await sender.Send(new MemberLeftChannelEvent()
            {
                Channel = guild.GetVoiceGuildChannel(prevVoiceState.ChannelId.Value), User = currentUser
            });
        }
        else if (prevVoiceState?.ChannelId is null)
        {
            await sender.Send(new MemberJoinedChannelEvent()
            {
                Channel = guild.GetVoiceGuildChannel(currentVoiceState.ChannelId.Value), User = currentUser
            });
        }
        else
        {
            await sender.Send(new CheckVoiceActivityForChannelEvent()
            {
                ChannelId = currentVoiceState.ChannelId.Value
            });
        }
    }


    private async Task ProcessVoiceChannelKickOnBot(VoiceState? prevVoiceState, VoiceState currentVoiceState)
    {
        if (prevVoiceState?.ChannelId is null)
        {
            return;
        }

        if (currentVoiceState.ChannelId is null ||
            prevVoiceState.ChannelId != audioService.GetConnectedVoiceChannelId())
        {
            return;
        }

        await audioService.DisconnectFromVoiceChannelAsync();
        await audioService.ConnectToVoiceChannelAsync(prevVoiceState.GuildId, prevVoiceState.ChannelId.Value);
    }
}