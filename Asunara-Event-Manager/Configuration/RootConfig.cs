namespace EventManager.Configuration;

public class RootConfig
{
    public DatabaseConfig Database { get; set; }
    
    public DiscordConfig Discord { get; set; }
    
    public SentryConfig Sentry { get; set; }
}