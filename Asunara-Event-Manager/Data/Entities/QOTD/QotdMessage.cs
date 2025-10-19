using EventManager.Data.Entities.Base;

namespace EventManager.Data.Entities.QOTD;

public class QotdMessage : IEntity
{
    public Guid Id { get; set; }
    
    public Guid QuestionId { get; set; }

    public QotdQuestion Question { get; set; } = null!;
    
    public ulong MessageId { get; set; }
    
    public DateTime PostedOn { get; set; }
}