using EventManager.Data.Entities.Events.Base;

namespace EventManager.Data.Entities.Events.QOTD;

public class QotdQuestion : IEntity
{
    public Guid Id { get; set; }
    
    public string Question { get; set; }
    
    public ulong AuthorId { get; set; }
}