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

SentryService.Initialize(host.Services.GetRequiredService<RootConfig>());

ApplicationDbContext dbContext = host.Services.GetRequiredService<ApplicationDbContext>();
dbContext.Database.Migrate();

var discordService = host.Services.GetRequiredService<DiscordService>();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    host.StopAsync().GetAwaiter().GetResult();
};


discordService.RunAsync(host).GetAwaiter().GetResult();


