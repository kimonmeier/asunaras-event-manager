using EventManager.Data.Entities.Base;

namespace EventManager.Data.Entities.QOTD;

public class QotdQuestion : IEntity
{
    public Guid Id { get; set; }
    
    public string Question { get; set; }
    
    public ulong AuthorId { get; set; }
}