namespace EventManager.Services;

public class EventReminderService
{
    private readonly List<Guid> _announcedEvents = new();
    
    public void AnnounceEvent(Guid eventId)
    {
        _announcedEvents.Add(eventId);
    }
    
    public bool HasAnnouncedEvent(Guid eventId)
    {
        return _announcedEvents.Contains(eventId);
    }
}