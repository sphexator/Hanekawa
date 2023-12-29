using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Hanekawa.Application;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Commands.Slash.Administration;
using Hanekawa.Bot.Commands.Slash.Club;
using Hanekawa.Bot.Commands.Slash.Setting;
using Hanekawa.Bot.Services.Grpc;
using Hanekawa.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddSingleton<Metrics<AdministrationCommands>>();
builder.Services.AddSingleton<Metrics<ClubCommands>>();
builder.Services.AddSingleton<Metrics<GreetCommands>>();
builder.Services.AddSingleton<Metrics<LevelCommands>>();

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

builder.Host.ConfigureDiscordBot((_, bot) =>
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

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();