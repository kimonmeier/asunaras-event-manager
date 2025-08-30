namespace EventManager;

public static class DateTimeExtensions
{
    public static DateTime FromUtc(this DateTime dateTime)
    {
        TimeSpan timeSpanDifference = DateTime.Now - DateTime.UtcNow;

        return dateTime.Add(timeSpanDifference);
    }
    
}