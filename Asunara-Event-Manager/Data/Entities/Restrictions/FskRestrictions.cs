using EventManager.Data.Entities.Events;
using EventManager.Data.Enum;

namespace EventManager.Data.Entities.Restrictions;

public class FskRestrictions : EventRestriction
{
    public override RestrictionType Type { get; set; } = RestrictionType.Fsk;
    
    public int? MinAlter { get; set; }
    
    public int? MaxAlter { get; set; }
}