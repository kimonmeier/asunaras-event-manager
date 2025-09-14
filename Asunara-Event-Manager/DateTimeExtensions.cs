namespace EventManager;

public static class DateTimeExtensions
{
    public static DateTime FromUtc(this DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"));
    }

    public static int GetAge(this DateOnly d1)
    {
        var timeSpan = DateTime.Today - d1.ToDateTime(TimeOnly.MinValue);

        int age = 0;
        while (timeSpan.Days > 365)
        {
            timeSpan = timeSpan.Add(TimeSpan.FromDays(-365));
            age++;
        }
        
        return age;
    }
}