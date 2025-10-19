using EventManager.Data.Entities.Base;

namespace EventManager.Data.Entities.Users;

public class DiscordUser : IPersistentEntity
{
    public Guid Id { get; set; }
    
    public ulong DiscordUserId { get; set; }
    
    public string Username { get; set; }
    
    public string DisplayName { get; set; }
    
    public string AvatarUrl { get; set; }
    
    public bool IsDeleted { get; set; }
}