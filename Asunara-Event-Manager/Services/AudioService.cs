using System.ComponentModel;
using NetCord.Gateway;
using NetCord.Gateway.Voice;

namespace EventManager.Services;

public class AudioService(GatewayClient client)
{
    private VoiceClient? _voiceClient;
    private ulong? _audioChannelId;
    private bool _isPlaying;

    public async Task PlayAudioAsync(string url)
    {
        if (_voiceClient is null)
        {
            throw new InvalidAsynchronousStateException("There is no audio client connected");
        }

        if (_isPlaying)
        {
            throw new InvalidAsynchronousStateException("There is already a song playing");
        }
        
        _isPlaying = true;

        if (_voiceClient.Status == WebSocketStatus.Disconnected)
        {
            await _voiceClient.StartAsync();
        }

        await _voiceClient.EnterSpeakingStateAsync(new SpeakingProperties(SpeakingFlags.Microphone));
        
        await using var fileStream = File.OpenRead(url);
        
        await using Stream audioStream = _voiceClient.CreateOutputStream();
        await using OpusEncodeStream stream = new(audioStream, PcmFormat.Short, VoiceChannels.Stereo, OpusApplication.Audio);
        await fileStream.CopyToAsync(stream);
        await stream.FlushAsync();
        
        _isPlaying = false;
    }

    public async Task ConnectToVoiceChannelAsync(ulong guildId, ulong channelId)
    {
        _voiceClient = await client.JoinVoiceChannelAsync(guildId, channelId);
        _audioChannelId = channelId;
    }

    public async Task DisconnectFromVoiceChannelAsync()
    {
        if (_voiceClient is null)
        {
            return;
        }

        try
        {
            await _voiceClient.CloseAsync();
        }
        catch (Exception _)
        {
            
        }// ignored

        await client.UpdateVoiceStateAsync(new VoiceStateProperties(_voiceClient.GuildId, null));

        _audioChannelId = null;
        _voiceClient.Dispose();
        _voiceClient = null;
    }
    
    public ulong? GetConnectedVoiceChannelId()
    {
        return _audioChannelId;
    }
}