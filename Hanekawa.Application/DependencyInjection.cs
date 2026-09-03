using Hanekawa.Application.Contracts;
using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Extensions;
using Hanekawa.Application.Handlers.Commands.Account;
using Hanekawa.Application.Handlers.Commands.Administration;
using Hanekawa.Application.Handlers.Commands.Boost;
using Hanekawa.Application.Handlers.Commands.Club;
using Hanekawa.Application.Handlers.Commands.Settings;
using Hanekawa.Application.Handlers.Services.Internal;
using Hanekawa.Application.Handlers.Services.Levels;
using Hanekawa.Application.Handlers.Services.Logs;
using Hanekawa.Application.Handlers.Services.Metrics;
using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Pipelines;
using Hanekawa.Application.Services;
using Hanekawa.Application.Services.Images;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Hanekawa.Entities.Settings.Images;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors;
using Prometheus.Client.DependencyInjection;
using SixLabors.Fonts;

namespace Hanekawa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<ImageSettings>(configuration.GetSection("ImageSettings"));
        serviceCollection.AddScoped<ILevelService, LevelService>();
        serviceCollection.AddScoped<IDropService, DropService>();
        serviceCollection.AddScoped<IImageService, ImageService>();
        serviceCollection.AddScoped<IConfigService, ConfigService>();
        serviceCollection.AddScoped<IModuleService, ModuleService>();

        serviceCollection.AddScoped<IAdministrationCommandService, AdministrationCommandService>();
        serviceCollection.AddScoped<ILogService, LogSettingService>();
        serviceCollection.AddScoped<IGreetService, GreetService>();
        serviceCollection.AddScoped<IStreamService, StreamService>();
        serviceCollection.AddScoped<IClubCommandService, ClubCommandService>();
        serviceCollection.AddScoped<ILevelCommandService, LevelCommandService>();
        serviceCollection.AddScoped<IBoostCommandService, BoostCommands>();
        serviceCollection.AddScoped<IAccountCommandService, AccountCommandService>();
        serviceCollection.AddScoped<IBotService, BotService>();
        //serviceCollection.AddScoped<IWarningCommandService>();

        var fontCollection = new FontCollection();
        fontCollection.Add(@"Data/Fonts/ARIAL.TTF");
        fontCollection.Add(@"Data/Fonts/TIMES.TTF");
        fontCollection.AddSystemFonts();
        serviceCollection.AddSingleton(fontCollection);

        serviceCollection.AddSingleton<IRequestDispatcher, RequestDispatcher>();
        serviceCollection.AddSingleton<IEventPublisher, EventPublisher>();

        // Request handlers
        serviceCollection.AddDecoratedRequestHandler<Ban, BanHandler>();
        serviceCollection.AddDecoratedRequestHandler<Kick, KickHandler>();
        serviceCollection.AddDecoratedRequestHandler<Mute, MuteHandler>();
        serviceCollection.AddDecoratedRequestHandler<Unmute, UnmuteHandler>();
        serviceCollection.AddDecoratedRequestHandler<Unban, UnbanHandler>();
        serviceCollection.AddDecoratedRequestHandler<Prune, PruneHandler>();
        serviceCollection.AddDecoratedRequestHandler<LevelUp, LevelUpRoleHandler>();
        serviceCollection.AddDecoratedRequestHandler<CommandMetric, CommandMetricHandler>();
        serviceCollection.AddDecoratedRequestHandler<ProfileCommand, ProfileCommandResult, ProfileCommandHandler>();
        serviceCollection.AddDecoratedRequestHandler<WarningList, Response<Pagination<Message>>, WarningListHandler>();
        serviceCollection.AddDecoratedRequestHandler<WarningClear, Response<Message>, WarningClearHandler>();
        serviceCollection.AddDecoratedRequestHandler<WarningReceived, Response<Message>, WarningReceivedHandler>(typeof(WarningAdded));

        // Notification handlers
        serviceCollection.AddScoped<INotificationHandler<UserJoin>, UserJoinedHandler>();
        serviceCollection.AddScoped<INotificationHandler<UserLeave>, UserLeftHandler>();
        serviceCollection.AddScoped<INotificationHandler<UserBanned>, UserBannedHandler>();
        serviceCollection.AddScoped<INotificationHandler<UserUnbanned>, UserUnbannedHandler>();
        serviceCollection.AddScoped<INotificationHandler<MessageReceived>, MessageReceivedExperienceHandler>();

        serviceCollection.AddMetricFactory(new CollectorRegistry());
        serviceCollection.AddSingleton<IMetrics, Metrics>();

        return serviceCollection;
    }
}