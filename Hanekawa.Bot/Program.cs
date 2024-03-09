using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Hanekawa.Application;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Bot;
using Hanekawa.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Host.ConfigureDiscordBot<Bot>((_, bot) =>
{
    bot.Token = builder.Configuration["botToken"];
    bot.ApplicationId = new Snowflake(ulong.Parse(builder.Configuration["applicationId"]!));
    bot.UseMentionPrefix = true;
    bot.ReadyEventDelayMode = ReadyEventDelayMode.Guilds;
    bot.Intents |= GatewayIntents.All;
});

builder.Host.UseDefaultServiceProvider(x =>
{
    x.ValidateScopes = true;
    x.ValidateOnBuild = true;
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<IDbContext>()
        .MigrateDatabaseAsync();

app.Run();