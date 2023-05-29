using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Hanekawa.Application;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Services.Grpc;
using Hanekawa.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddApplicationLayer();

builder.Host.ConfigureDiscordBot((_, bot) =>
{
    bot.Token = builder.Configuration["botToken"];
    bot.ApplicationId = new Snowflake(ulong.Parse(builder.Configuration["applicationId"]));
    bot.UseMentionPrefix = true;
    bot.ReadyEventDelayMode = ReadyEventDelayMode.Guilds;
    bot.Intents |= GatewayIntents.Unprivileged | GatewayIntents.Members;
});
builder.Host.UseDefaultServiceProvider(x =>
{
    x.ValidateScopes = true;
    x.ValidateOnBuild = true;
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
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