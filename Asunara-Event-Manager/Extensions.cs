namespace EventManager;

public static class Extensions
{
    public static string WithMaxLength(this string value, int maxLength)
    {
        return value[..Math.Min(value.Length, maxLength)];
    }
    
}