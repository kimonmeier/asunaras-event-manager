using EventManager.Events.CheckVoiceActivityForChannel;
using EventManager.Events.MemberJoinedChannel;
using EventManager.Events.MemberLeftChannel;
using EventManager.Extensions;
using EventManager.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EventManager.Interactions.Events;

public class OnUserVoiceStateUpdate(
    ISender sender,
    AudioService audioService,
    VoiceStateHistoryService voiceStateHistoryService,
    GatewayClient client,
    ILogger<OnUserVoiceStateUpdate> logger) : IVoiceStateUpdateGatewayHandler
{
    public async ValueTask HandleAsync(VoiceState currentVoiceState)
    {
        var prevVoiceState = voiceStateHistoryService.GetLastVoiceState(currentVoiceState.UserId);
        voiceStateHistoryService.AddLastVoiceState(currentVoiceState);
        var guild = client.Cache.Guilds[currentVoiceState.GuildId];
        GuildUser? currentUser = currentVoiceState.User
                                 ?? await guild.GetOrFetchUserAsync(currentVoiceState.UserId);

        if (currentUser is null)
        {
            logger.LogWarning("Voice state update for user {UserId} who is no longer a member of the guild {GuildId}",
                currentVoiceState.UserId, currentVoiceState.GuildId);
            return;
        }

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
        if (!audioService.GetConnectedVoiceChannelId().HasValue)
        {
            return;
        }
        
        if (prevVoiceState?.ChannelId is null)
        {
            return;
        }
        
        if (currentVoiceState.ChannelId is null &&
            currentVoiceState.ChannelId == audioService.GetConnectedVoiceChannelId())
        {
            return;
        }

        await audioService.DisconnectFromVoiceChannelAsync();
        await audioService.ConnectToVoiceChannelAsync(prevVoiceState.GuildId, prevVoiceState.ChannelId.Value);
    }
}