using NetCord.Gateway;

namespace EventManager.Services;

public class VoiceStateHistoryService
{
    private readonly Dictionary<ulong, VoiceState>  _lastVoiceStates = new();

    public void AddLastVoiceState(VoiceState voiceState)
    {
        _lastVoiceStates[voiceState.UserId] = voiceState;
    }

    public VoiceState? GetLastVoiceState(ulong userId)
    {
        return _lastVoiceStates.GetValueOrDefault(userId);
    }
}