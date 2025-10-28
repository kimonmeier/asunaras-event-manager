namespace EventManager.Extensions;

public static class StringExtensions
{
    public static string WithMaxLength(this string value, int maxLength)
    {
        return value[..Math.Min(value.Length, maxLength)];
    }
}