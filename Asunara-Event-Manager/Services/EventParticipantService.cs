namespace EventManager.Services;

public class EventParticipantService
{
    private readonly Dictionary<Guid, List<ulong>> _participants = new();
    
    public void AddParticipant(Guid eventId, ulong userId)
    {
        if (!_participants.ContainsKey(eventId))
        {
            _participants.Add(eventId, new List<ulong>());   
        }

        if (_participants[eventId].Contains(userId))
        {
            return;
        }
        _participants[eventId].Add(userId);
    }
    
    public List<ulong> GetParticipants(Guid eventId)
    {
        return !_participants.TryGetValue(eventId, out var participants) ? new List<ulong>() : participants;
    }

    public bool HasParticipant(Guid eventId, ulong userId)
    {
        return _participants.TryGetValue(eventId, out var participants) && participants.Contains(userId);   
    }
}