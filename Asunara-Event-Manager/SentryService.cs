using EventManager.Configuration;

namespace EventManager;

public static class SentryService
{
    public static void Initialize(RootConfig config)
    {
        SentrySdk.Init(x =>
        {
            x.Dsn = config.Sentry.Dsn;
            x.Environment = config.Sentry.Environment;
            
            x.SampleRate = config.Sentry.SampleRate;
            x.TracesSampleRate = config.Sentry.TraceSampleRate;
            x.ProfilesSampleRate = config.Sentry.ProfilingSampleRate;

#if DEBUG
            x.Debug = false;
#endif
            
            x.AddDiagnosticSourceIntegration();
        });
    }
}