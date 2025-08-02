using Discord.WebSocket;
using EFCoreSecondLevelCacheInterceptor;
using EventManager.Background;
using EventManager.Commands;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EventManager;

public static class ConfigureServices
{
    public static void AddEventManagerServices(this IServiceCollection services, IConfiguration configuration)
    {
        RootConfig? rootConfig = configuration.Get<RootConfig>();

        if (rootConfig == null)
        {
            throw new Exception("Configuration is empty");
        }
        
        services.AddSingleton(rootConfig);
        
        services.AddCustomLogging(configuration);
        services.AddDatabase(rootConfig);
        services.AddRepositories();
        services.AddDiscord();
        services.AddInteractions();
        services.AddServices();
        services.AddPipeline();
        services.AddBackgroundTasks(rootConfig);
    }

    private static void AddCustomLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration);
            builder.AddDebug();
        });
    }

    private static void AddDatabase(this IServiceCollection services, RootConfig configuration)
    {
        services.AddDbContext<ApplicationDbContext>(config =>
        {
            config.UseSqlite(configuration.Database.ConnectionString, op => op.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });
        services.AddEFSecondLevelCache(options =>
        {
            options.Settings.EnableLogging = true;
            options.UseMemoryCacheProvider();
        });

        services.AddScoped<DbTransactionFactory>();
        services.AddScoped<DbContext>(op => op.GetRequiredService<ApplicationDbContext>());
    }

    private static void AddDiscord(this IServiceCollection services)
    {
        services.AddSingleton<DiscordSocketClient>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<DiscordService>();
    }

    private static void AddPipeline(this IServiceCollection services)
    {
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<DiscordEventRepository>();
        services.AddTransient<QotdMessageRepository>();
        services.AddTransient<QotdQuestionRepository>();
    }

    private static void AddInteractions(this IServiceCollection services)
    {
        services.AddTransient<QotdQuestionAutocompleteHandler>();
        services.AddTransient<QotdInteraction>();
    }

    private static void AddBackgroundTasks(this IServiceCollection services, RootConfig rootConfig)
    {
        services.AddQuartz(options =>
        {
            options.UseInMemoryStore();
            options.UseDefaultThreadPool();
            options.UseSimpleTypeLoader();

            options.ScheduleJob<QotdPostJob>(trigger =>
            {
                trigger
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(rootConfig.Discord.Qotd.Time.ToTimeSpan().Hours, rootConfig.Discord.Qotd.Time.Minute)
                        .WithMisfireHandlingInstructionFireAndProceed());
            });
            
            options.ScheduleJob<QotdCheckQuestionsJob>(trigger =>
            {
                trigger
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(rootConfig.Discord.Qotd.Time.AddHours(-12).ToTimeSpan().Hours, rootConfig.Discord.Qotd.Time.Minute)
                        .WithMisfireHandlingInstructionFireAndProceed());
            });
            

        });

        services.AddTransient<QotdPostJob>();
        services.AddTransient<QotdCheckQuestionsJob>();
    }
}