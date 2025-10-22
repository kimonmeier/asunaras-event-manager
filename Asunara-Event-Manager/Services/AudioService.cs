using System.ComponentModel;
using Discord.Audio;
using Discord.WebSocket;
using NetCord;

namespace EventManager.Services;

public class AudioService
{
    private IAudioClient? _audioClient;
    private SocketVoiceChannel? _audioChannel;
    private bool _isPlaying;

    public async Task PlayAudioAsync(string url)
    {
        if (_audioClient is null)
        {
            throw new InvalidAsynchronousStateException("There is no audio client connected");
        }

        if (_isPlaying)
        {
            throw new InvalidAsynchronousStateException("There is already a song playing");
        }
        
        _isPlaying = true;
        
        await using var fileStream = File.OpenRead(url);
        await using AudioOutStream audioStream = _audioClient.CreatePCMStream(AudioApplication.Music);
        await fileStream.CopyToAsync(audioStream);
        await audioStream.FlushAsync();
        
        await _audioClient.SetSpeakingAsync(false);
        _isPlaying = false;
    }

    public async Task ConnectToVoiceChannelAsync(IVoiceGuildChannel channel)
    {
        _audioClient = await channel.ConnectAsync(selfDeaf: true);
        _audioChannel = channel;
    }
    
    public async Task DisconnectFromVoiceChannelAsync()
    {
        if (_audioClient is null)
        {
            return;
        }
        
        await _audioClient.StopAsync();
        try
        {
            await _audioChannel!.DisconnectAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        _audioChannel = null;
        _audioClient.Dispose();
        _audioClient = null;
    }
    
    public SocketVoiceChannel? GetConnectedVoiceChannel()
    {
        return _audioChannel;
    }
}