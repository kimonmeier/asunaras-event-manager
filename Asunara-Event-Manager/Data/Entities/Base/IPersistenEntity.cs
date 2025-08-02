namespace EventManager.Data.Entities.Events.Base;

public interface IPersistentEntity : IEntity
{
    public bool IsDeleted { get; set; }
}
