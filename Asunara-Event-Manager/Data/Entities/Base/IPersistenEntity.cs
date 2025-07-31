namespace EventManager.Data.Entities.Base;

public interface IPersistentEntity : IEntity
{
    public bool IsDeleted { get; set; }
}
