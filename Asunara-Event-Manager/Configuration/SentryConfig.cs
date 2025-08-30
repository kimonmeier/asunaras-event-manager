namespace EventManager.Configuration;

public class SentryConfig
{
    public string Dsn { get; set; }
    
    public string Environment { get; set; }
    
    public float SampleRate { get; set; }
    
    public float TraceSampleRate { get; set; }
    
    public float ProfilingSampleRate { get; set; }
}