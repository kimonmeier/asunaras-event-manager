namespace EventManager;

public static class DateTimeExtensions
{
    public static DateTime FromUtc(this DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"));
    }
    
}