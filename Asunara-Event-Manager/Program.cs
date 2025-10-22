// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Net.Mime;
using EventManager;
using EventManager.Configuration;
using EventManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using Quartz;

Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    SentrySdk.CaptureException(eventArgs.ExceptionObject as Exception);
};

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true);

builder.Services.AddEventManagerServices(builder.Configuration);

using IHost host = builder.Build();


// Add component interactions using minimal APIs
host.AddComponentInteraction<ButtonInteractionContext>("ping", () => "Pong!");
host.AddComponentInteraction<StringMenuInteractionContext>("string", (StringMenuInteractionContext context) => string.Join("\n", context.SelectedValues));
host.AddComponentInteraction<UserMenuInteractionContext>("user", (UserMenuInteractionContext context) => string.Join("\n", context.SelectedValues));
host.AddComponentInteraction<RoleMenuInteractionContext>("role", (RoleMenuInteractionContext context) => string.Join("\n", context.SelectedValues));
host.AddComponentInteraction<MentionableMenuInteractionContext>("mentionable", (MentionableMenuInteractionContext context) => string.Join("\n", context.SelectedValues));
host.AddComponentInteraction<ChannelMenuInteractionContext>("channel", (ChannelMenuInteractionContext context) => string.Join("\n", context.SelectedValues));
host.AddComponentInteraction<ModalInteractionContext>("modal", (ModalInteractionContext context) => ((TextInput)context.Components[0]).Value);

host.AddModules(typeof(Program).Assembly);

SentryService.Initialize(host.Services.GetRequiredService<RootConfig>());

ApplicationDbContext dbContext = host.Services.GetRequiredService<ApplicationDbContext>();
dbContext.Database.Migrate();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    host.StopAsync().GetAwaiter().GetResult();
};

await host.RunAsync();


