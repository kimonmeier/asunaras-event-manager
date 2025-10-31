namespace EventManager.Services;

public class HalloweenService
{
    private readonly Dictionary<ulong, DateTime> _halloweenUsers = new();
    private readonly Dictionary<ulong, DateTime> _halloweenChannels = new();

    public void Scared(ulong channel, params ulong[] users)
    {
        _halloweenChannels[channel] = DateTime.UtcNow;
        
        foreach (ulong user in users)
        {
            if (!_halloweenUsers.ContainsKey(user))
            {
                _halloweenUsers.Add(user, DateTime.UtcNow);
                continue;
            }
            
            _halloweenUsers[user] = DateTime.UtcNow;
        }
    }

    public TimeSpan GetTimedDifferenceBetweenScaredUser(ulong user)
    {
        if (!_halloweenUsers.ContainsKey(user))
        {
            return TimeSpan.MaxValue;
        }
        
        return DateTime.UtcNow - _halloweenUsers[user];
    }
    
    public TimeSpan GetTimedDifferenceBetweenScaredChannel(ulong channel)
    {
        if (!_halloweenChannels.ContainsKey(channel))
        {
            return TimeSpan.MaxValue;
        }
        
        return DateTime.UtcNow - _halloweenChannels[channel];
    }
}