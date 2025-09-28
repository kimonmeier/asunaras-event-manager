namespace EventManager.Data.Entities.Activity;

public enum ActivityType
{
    MessageCreated,
    VoiceChannelJoined,
    VoiceChannelLeft,
    VoiceChannelAfk,
    VoiceChannelNonAfk
}