using Discord;
using Discord.WebSocket;
using EFCoreSecondLevelCacheInterceptor;
using EventManager.Background;
using EventManager.Behaviour;
using EventManager.Commands;
using EventManager.Configuration;
using EventManager.Data;
using EventManager.Data.Repositories;
using EventManager.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Quartz;
using Sentry.Extensions.Logging.Extensions.DependencyInjection;

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
            builder.AddSentry();
            builder.AddSimpleConsole(x =>
            {
                x.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
            });
        });
    }

    private static void AddDatabase(this IServiceCollection services, RootConfig configuration)
    {
        services.AddEFSecondLevelCache(options =>
        {
            options.UseMemoryCacheProvider();
            options.UseCacheKeyPrefix("EF_");
            options.CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(30));
            options.ConfigureLogging(true);
        });
        services.AddDbContext<ApplicationDbContext>((servicesProvider, config) =>
        {
            config.UseSqlite(configuration.Database.ConnectionString, op => op.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
            config.AddInterceptors(servicesProvider.GetRequiredService<SecondLevelCacheInterceptor>());
        });

        services.AddSingleton<DbTransactionLock>();
        services.AddScoped<DbTransactionFactory>();
        services.AddScoped<DbContext>(op => op.GetRequiredService<ApplicationDbContext>());
    }

    private static void AddDiscord(this IServiceCollection services)
    {
        services.AddSingleton<DiscordSocketClient>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton(new DiscordSocketConfig()
        {
            AlwaysDownloadUsers = true,
            AlwaysDownloadDefaultStickers = true,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages,
        });
        services.AddSingleton<DiscordService>();
        services.AddSingleton<EventParticipantService>();
        services.AddSingleton<EventReminderService>();
    }

    private static void AddPipeline(this IServiceCollection services)
    {
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssembly(typeof(Program).Assembly);
            
            x.AddOpenBehavior(typeof(ValidationBehavior<,>));
            x.AddOpenBehavior(typeof(SentryTracingBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SentryTracingBehavior<,>));
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<DiscordEventRepository>();
        services.AddTransient<QotdMessageRepository>();
        services.AddTransient<QotdQuestionRepository>();
        services.AddTransient<EventRestrictionRepository>();
        services.AddTransient<UserPreferenceRepository>();
        services.AddTransient<EventFeedbackRepository>();
        services.AddTransient<UserBirthdayRepository>();
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

            options.ScheduleJob<CheckEventReminderJob>(trigger =>
            {
                trigger
                    .WithSimpleSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(10).WithMisfireHandlingInstructionFireNow());
            });

            options.ScheduleJob<CheckBirthdayJob>(trigger =>
            {
                trigger
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0));
            });

        });

        services.AddTransient<QotdPostJob>();
        services.AddTransient<QotdCheckQuestionsJob>();
        services.AddTransient<CheckEventReminderJob>();
        services.AddTransient<CheckBirthdayJob>();
    }
}