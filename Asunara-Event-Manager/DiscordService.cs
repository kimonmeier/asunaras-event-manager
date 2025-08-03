﻿using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Events.EventCreated;
using EventManager.Events.EventDeleted;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using IResult = Discord.Interactions.IResult;
using RunMode = Discord.Interactions.RunMode;

namespace EventManager;

public class DiscordService
{
    private readonly ILogger<DiscordService> _logger;
    private readonly RootConfig _config;
    private readonly DiscordSocketClient _client;
    private readonly ISender _sender;
    private readonly IServiceProvider _services;
    private readonly ISchedulerFactory _schedulerFactory;
    
    public DiscordService(ILogger<DiscordService> logger, RootConfig config, DiscordSocketClient client, ISender sender, IServiceProvider services, ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        _config = config;
        _client = client;
        _sender = sender;
        _services = services;
        _schedulerFactory = schedulerFactory;
    }

    public async Task RunAsync(IHost host)
    {
        if (string.IsNullOrWhiteSpace(_config.Discord?.Token))
        {
            _logger.LogCritical("Discord Token fehlt, Programm wird beendet!");
            throw new Exception("Discord token is missing");
        }

        _client.Log += ClientOnLog;
        _client.Connected += ClientOnConnected;
        _client.JoinedGuild += ClientOnJoinedGuild;
        _client.GuildScheduledEventCancelled += ClientOnGuildScheduledEventCancelled;
        _client.GuildScheduledEventCreated += ClientOnGuildScheduledEventCreated;
        
        await _client.LoginAsync(TokenType.Bot, _config.Discord.Token);
        await _client.StartAsync();
        IScheduler scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.StartDelayed(TimeSpan.FromSeconds(5));

        await host.WaitForShutdownAsync();

        await scheduler.Shutdown();
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    private async Task ClientOnJoinedGuild(SocketGuild guild)
    {
        await guild.Owner.SendMessageAsync($"Sorry aber ich bin ein Teambot für den Asunara Discord und kann deswegen nicht joinen!");
        await guild.LeaveAsync();
    }

    private Task ClientOnGuildScheduledEventCreated(SocketGuildEvent guildEvent)
    {
        return _sender.Send(new EventCreatedEvent()
        {
            Datum = guildEvent.StartTime.DateTime, DiscordId = guildEvent.Id
        });
    }

    private Task ClientOnGuildScheduledEventCancelled(SocketGuildEvent guildEvent)
    {
        return _sender.Send(new EventDeletedEvent()
        {
            DiscordId = guildEvent.Id
        });
    }

    private async Task ClientOnConnected()
    {
        Game activity = new(
            _config.Discord.Activity,
            ActivityType.Watching,
            ActivityProperties.Instance
        );

        await _client.SetActivityAsync(activity);

        var interactionService = new InteractionService(_client.Rest, new InteractionServiceConfig()
        {
            AutoServiceScopes = true,
            DefaultRunMode = RunMode.Async,
            EnableAutocompleteHandlers = true,
            ThrowOnError = true,
            LogLevel = LogSeverity.Info,
        });
        interactionService.Log += InteractionServiceOnLog;
        
        await interactionService.AddModulesAsync(typeof(Program).Assembly, _services);

#if DEBUG
        await interactionService.RegisterCommandsToGuildAsync(679367558809255938, true);
#else
        await interactionService.RegisterCommandsToGuildAsync(_config.Discord.TeamDiscordServerId, true);
#endif

        interactionService.SlashCommandExecuted += async (SlashCommandInfo commandInfo, Discord.IInteractionContext context, IResult result) =>
        {
            if (result.IsSuccess)
            {
                return;
            }

            string message;
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    message = $"Unmet Precondition: {result.ErrorReason}";

                    break;
                case InteractionCommandError.UnknownCommand:
                    message = "Unknown command";

                    break;
                case InteractionCommandError.BadArgs:
                    message = "Invalid number or arguments";

                    break;
                case InteractionCommandError.Exception:
                    message = $"Command exception: {result.ErrorReason}";

                    break;
                case InteractionCommandError.Unsuccessful:
                    message = "Command could not be executed";

                    break;
                default:
                    return;
            }

            EmbedBuilder embedBuilder = new();
            embedBuilder.Color = Color.Red;
            embedBuilder.Title = "Error occured during Command";
            embedBuilder.Description = "While executing a command an unexpected issue occured.";
            embedBuilder.AddField("Error-Message", message);
            embedBuilder.AddField("Time", DateTime.Now.ToString("O"));
            embedBuilder.AddField("Weitere Schritte", "Bitte Kimon mit einem Screenshot kontaktieren, damit er den Fehler beheben kann c:");
            embedBuilder.Author = new EmbedAuthorBuilder().WithName("Asunara-Event-Manager");
            
            await context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embedBuilder.Build();
            });
        };
        
        _client.InteractionCreated += async interaction =>
        {
            _logger.LogInformation("Started interaction");
            var ctx = new SocketInteractionContext(_client, interaction);
            try
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await ctx.Interaction.DeferAsync(true);
                }

                await interactionService.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error executing command");
            }
        };
    }

    private Task InteractionServiceOnLog(LogMessage arg)
    {
        return ClientOnLog(arg);
    }

    private Task ClientOnLog(LogMessage arg)
    {
        _logger.Log(GetLogLevelByDiscordLevel(arg.Severity), arg.Exception, arg.Message);

        return Task.CompletedTask;
    }

    private LogLevel GetLogLevelByDiscordLevel(LogSeverity level)
    {
        return level switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.None,
        };
    }
}