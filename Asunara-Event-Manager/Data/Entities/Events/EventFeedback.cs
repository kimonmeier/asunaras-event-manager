using EventManager.Data.Entities.Events.Base;

namespace EventManager.Data.Entities.Events;

public class EventFeedback : IEntity
{
    public Guid Id { get; set; }
    
    public Guid DiscordEventId { get; set; }
    
    public DiscordEvent DiscordEvent { get; set; }
    
    public ulong UserId { get; set; }
    
    public int Score { get; set; }

    public bool Anonymous { get; set; } = true;
    
    public string? Good { get; set; }
    
    public string? Critic { get; set; }
    
    public string? Suggestion { get; set; }
    
    public ulong? FeedbackMessageId { get; set; }
}