using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using EventManager.Configuration;
using EventManager.Events.EventCompleted;
using EventManager.Events.EventCreated;
using EventManager.Events.EventDeleted;
using EventManager.Events.MemberAddedEvent;
using EventManager.Events.MemberJoinedChannel;
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
        _client.Ready += ClientOnReady;
        _client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;
        _client.JoinedGuild += ClientOnJoinedGuild;
        _client.GuildScheduledEventCancelled += ClientOnGuildScheduledEventCancelled;
        _client.GuildScheduledEventCreated += ClientOnGuildScheduledEventCreated;
        _client.GuildScheduledEventUserAdd += ClientOnGuildScheduledEventUserAdd;
        _client.GuildScheduledEventCompleted += ClientOnGuildScheduledEventCompleted;
        
        await _client.LoginAsync(TokenType.Bot, _config.Discord.Token);
        await _client.StartAsync();
        IScheduler scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.StartDelayed(TimeSpan.FromSeconds(5));

        await host.WaitForShutdownAsync();

        await scheduler.Shutdown();
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    private Task ClientOnGuildScheduledEventCompleted(SocketGuildEvent socketEvent)
    {
        return _sender.Send(new EventCompletedEvent()
        {
            DiscordEventId = socketEvent.Id
        });
    }

    private async Task ClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState prevVoiceState, SocketVoiceState currentVoiceState)
    {
        if (currentVoiceState.VoiceChannel is null)
        {
            return;
        }

        await _sender.Send(new MemberJoinedChannelEvent()
        {
            Channel = currentVoiceState.VoiceChannel, User = currentVoiceState.VoiceChannel.Guild.GetUser(user.Id),
        });
    }

    private async Task ClientOnGuildScheduledEventUserAdd(Cacheable<SocketUser, RestUser, IUser, ulong> eventUser, SocketGuildEvent @event)
    {
        IUser user = await eventUser.GetOrDownloadAsync();

        await _sender.Send(new MemberAddedEventEvent()
        {
            Event = @event, User = user,
        });
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
            Datum = guildEvent.StartTime.DateTime, DiscordId = guildEvent.Id, EventName = guildEvent.Name
        });
    }

    private Task ClientOnGuildScheduledEventCancelled(SocketGuildEvent guildEvent)
    {
        return _sender.Send(new EventDeletedEvent()
        {
            DiscordId = guildEvent.Id
        });
    }

    private async Task ClientOnReady()
    {
        Game activity = new(
            _config.Discord.Activity,
            ActivityType.Watching,
            ActivityProperties.Instance
        );
        
        _logger.LogInformation("Start to download");
        await _client.DownloadUsersAsync([_client.GetGuild(_config.Discord.MainDiscordServerId), _client.GetGuild(_config.Discord.TeamDiscordServerId)]);
        _logger.LogInformation("Download complete");
        await _client.SetActivityAsync(activity);
        var interactionService = new InteractionService(_client.Rest, new InteractionServiceConfig()
        {
            AutoServiceScopes = true,
            DefaultRunMode = RunMode.Async,
            EnableAutocompleteHandlers = true,
            ThrowOnError = true,
            LogLevel = LogSeverity.Info,
        });
        interactionService.Log += ClientOnLog;

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

            await HandleSlashCommandErrorAsync(context, result);
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

    private async Task HandleSlashCommandErrorAsync(Discord.IInteractionContext context, IResult result)
    {
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
        embedBuilder.Title = "Error occurred during Command";
        embedBuilder.Description = "While executing a command an unexpected issue occurred.";
        embedBuilder.AddField("Error-Message", message);
        embedBuilder.AddField("Time", DateTime.Now.ToString("O"));
        embedBuilder.AddField("Weitere Schritte", "Bitte Kimon mit einem Screenshot kontaktieren, damit er den Fehler beheben kann c:");
        embedBuilder.Author = new EmbedAuthorBuilder().WithName("Asunara-Event-Manager");

        await context.Interaction.ModifyOriginalResponseAsync(x =>
        {
            x.Embed = embedBuilder.Build();
        });
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