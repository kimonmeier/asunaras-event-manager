using EventManager.Data.Entities.Events.Base;

namespace EventManager.Data.Entities.Birthday;

public class UserBirthday : IPersistentEntity
{
    public Guid Id { get; set; }

    public ulong DiscordId { get; set; }
    
    public DateOnly Birthday { get; set; }
    
    public DateOnly CreationDate { get; set; }
    
    public bool IsDeleted { get; set; }
}