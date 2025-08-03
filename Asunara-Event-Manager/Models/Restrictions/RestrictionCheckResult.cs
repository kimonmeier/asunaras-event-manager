namespace EventManager.Models.Restrictions;

public class RestrictionCheckResult
{
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }

    public RestrictionCheckResult(bool success, string? errorMessage = null)
    {
        if (!success && errorMessage is null)
        {
            throw new ArgumentNullException(nameof(errorMessage));
        }
        
        Success = success;
        ErrorMessage = errorMessage;
    }
}